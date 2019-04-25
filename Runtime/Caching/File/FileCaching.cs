#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CrazyPanda.UnityCore.Serialization;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class FileCaching : ICache<byte[]>
    {
        #region Private Fields

        private CacheInfo _cacheInfo;
        private UnityJsonSerializer _jsonSerializer;
        private FileLocksManager _fileLocksManager;

        private string _cachingDirectory;
        private string _cacheInfoPath;

        #endregion

        #region Properties

        public IEnumerable<string> CachedKeys
        {
            get { return _cacheInfo.Files.Select(f => f.Key); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Use different directories for different FileCachings
        /// You need provide full path
        /// </summary>
        public FileCaching(string directory)
        {
            _cachingDirectory = directory;
            if (!Directory.Exists(_cachingDirectory))
            {
                Directory.CreateDirectory(_cachingDirectory);
            }

            _jsonSerializer = new UnityJsonSerializer();
            _fileLocksManager = new FileLocksManager();

            _cacheInfoPath = GetPathForKey("cacheinfo.txt");
            if (File.Exists(_cacheInfoPath))
            {
                _cacheInfo = _jsonSerializer.Deserialize<CacheInfo>(File.ReadAllBytes(_cacheInfoPath));
            }
            else
            {
                _cacheInfo = new CacheInfo();
            }
        }

        #endregion


        #region Public Members

        public bool Contains(string key)
        {
            return _cacheInfo.Contains(key);
        }

        public List<string> GetAllResorceNames()
        {
            var result = new List<string>();
            foreach (var info in _cacheInfo.Files)
            {
                result.Add(info.Key);
            }

            return result;
        }

        public void Add(object owner, string key, byte[] data)
        {
            _fileLocksManager.AddWriteLock(key);

            try
            {
                InternalSave(key, data);

                AddKeyAndSaveMetaFile(key, data);
            }
            finally
            {
                _fileLocksManager.RemoveWriteLock(key);
            }
        }

        public byte[] Get(object owner, string key)
        {
            _fileLocksManager.AddReadLock(key);

            byte[] bytes;

            try
            {
                var cachedFileInfo = GetCachedFileInfoStrict(key);
                bytes = InternalLoad(key, cachedFileInfo);
            }
            finally
            {
                _fileLocksManager.RemoveReadLock(key);
            }

            return bytes;
        }

#if !CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_NET_4_6
        public IEnumerator AddAsync(object owner, string key, byte[] resource)
        {
#warning FileCaching set async not implemented
            throw new NotImplementedException();
        }

        public ICacheGettingOperation<byte[]> GetAsync(object owner, string key)
        {
#warning FileCaching get async not implemented
            throw new NotImplementedException();
        }
#endif

        public List<string> GetUnusedResourceNames()
        {
            return new List<string>();
        }

        public List<string> GetOwnerResourcesNames(object owner)
        {
            return new List<string>(0);
        }

        public byte[] ForceReleaseResource(string key)
        {
            return ReleaseResource(null, key);
        }

        public byte[] ReleaseResource(object owner, string key)
        {
            if (!_cacheInfo.Contains(key))
            {
                return null;
            }

            if (_fileLocksManager.HasAnyLock(key))
            {
                return null;
            }

            InternalDelete(key);
            _cacheInfo.Remove(key);

            SaveMetaFile();
            return null;
        }

        public List<byte[]> ReleaseResources(object owner, List<string> keys)
        {
            return ForceReleaseResources(keys);
        }

        public List<byte[]> ForceReleaseResources(List<string> keys)
        {
            foreach (var key in keys)
            {
                if (!_cacheInfo.Contains(key))
                {
                    continue;
                }

                if (_fileLocksManager.HasAnyLock(key))
                {
                    continue;
                }

                InternalDelete(key);
                _cacheInfo.Remove(key);
            }

            SaveMetaFile();
            return null;
        }

        public List<byte[]> ReleaseAllResources()
        {
            List<CachedFileInfo> infoForRemove = new List<CachedFileInfo>();
            foreach (var file in _cacheInfo.Files.ToArray())
            {
                if (_fileLocksManager.HasAnyLock(file.Key))
                {
                    continue;
                }

                InternalDelete(file.Key);
                infoForRemove.Add(file);
            }

            foreach (var removedKey in infoForRemove)
            {
                _cacheInfo.Files.Remove(removedKey);
            }

            SaveMetaFile();
            return null;
        }

//        public void LockResource(string key)
//        {
//            if (Contains(key))
//            {
//                _fileLocksManager.AddManualLock(key);
//            }
//        }
//
//        public void UnlockResource(string key)
//        {
//            if (Contains(key))
//            {
//                _fileLocksManager.RemoveManualLock(key);
//            }
//        }
//
//        public void UnlockAllResources(string key)
//        {
//            foreach (var file in _cacheInfo.Files.ToArray())
//            {
//                _fileLocksManager.RemoveManualLock(key);
//            }
//        }

        private void InternalSave(string key, byte[] file)
        {
            var path = GetPathForKey(key);
            File.WriteAllBytes(path, file);
        }

        private byte[] InternalLoad(string key, CachedFileInfo cachedFileInfo)
        {
            byte[] bytes;
            var path = GetPathForKey(key);
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                throw new FileCachingException(string.Format("Exception while reading a key {0} with path {1}", key, path), e);
            }

            var actualMd5 = CalculateMd5(bytes);
            if (cachedFileInfo.Hash != actualMd5)
            {
                throw new
                    InvalidHashException(string.Format("Cached key {0} with path {1} hash {2} is not equals hash from disk {3}. Please remove or override file!",
                        key, path, cachedFileInfo.Hash, actualMd5));
            }

            return bytes;
        }

        private void InternalDelete(string key)
        {
            var path = GetPathForKey(key);
            File.Delete(path);
        }

        private string GetPathForKey(string key)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var filename = new string(key.Select(ch => invalidFileNameChars.Contains(ch) ? '_' : ch).ToArray());
            return string.Format("{0}/{1}", _cachingDirectory, filename);
        }

        private CachedFileInfo GetCachedFileInfoStrict(string key)
        {
            if (!_cacheInfo.Contains(key))
            {
                throw new CachedFileNotFoundException(string.Format("Not found cached resource with key {0}", key));
            }

            return _cacheInfo.Get(key);
        }

        private void AddKeyAndSaveMetaFile(string key, byte[] file)
        {
            _cacheInfo.AddOrUpdate(key, CalculateMd5(file));
            SaveMetaFile();
        }

        private void SaveMetaFile()
        {
            File.WriteAllBytes(_cacheInfoPath, _jsonSerializer.Serialize(_cacheInfo));
        }

        private static string CalculateMd5(byte[] file)
        {
            return ToPrettyString(MD5.Create().ComputeHash(file));
        }

        private static string ToPrettyString( byte[] bytes)
        {
            var sb = new StringBuilder();
            for( var i = 0; i < bytes.Length; i++ )
            {
                sb.Append( bytes[ i ].ToString( "x2" ) );
            }
            return sb.ToString();
        }

#endregion
    }
}
#endif