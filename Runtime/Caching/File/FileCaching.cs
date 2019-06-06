#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CrazyPanda.UnityCore.Serialization;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class FileCaching : ICache<byte[]>
    {
        private CacheInfo _cacheInfo;
        private UnityJsonSerializer _jsonSerializer;
        private FileLocksManager _fileLocksManager;

        private string _cachingDirectory;
        private string _cacheInfoPath;

        public IEnumerable<string> CachedKeys
        {
            get { return _cacheInfo.Files.Select(f => f.Key); }
        }

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

        public IEnumerator AddAsync(object owner, string key, byte[] resource)
        {
            _fileLocksManager.AddWriteLock(key);

            var t = Task.Run(() => { InternalSave(key, resource); });

            while (!t.IsCompleted)
            {
                yield return null;
            }

            _fileLocksManager.RemoveWriteLock(key);

            if (t.IsFaulted)
            {
                throw new FileCachingException("Exception in asyncronyous save task", t.Exception);
            }

            AddKeyAndSaveMetaFile(key, resource);
        }

        public ICacheGettingOperation<byte[]> GetAsync(object owner, string key)
        {
            return new FileCacheCacheLoadingOperation(this, key);
        }

        private class FileCacheCacheLoadingOperation : ICacheGettingOperation<byte[]>
        {
            private FileCaching _fileCaching;
            
            public string ResourceName { get; private set; }
            public byte[] Result { get; private set; }
            public bool IsCompleted { get; private set; }

            public FileCacheCacheLoadingOperation(FileCaching fileCaching, string resourceName)
            {
                _fileCaching = fileCaching;
                ResourceName = resourceName;
            }


            public IEnumerator StartProcess()
            {
                _fileCaching._fileLocksManager.AddReadLock(ResourceName);

                Task<byte[]> t;
                try
                {
                    var cachedFileInfo = _fileCaching.GetCachedFileInfoStrict(ResourceName);

                    t = Task.Run(() => _fileCaching.InternalLoad(ResourceName, cachedFileInfo));

                    while (!t.IsCompleted)
                    {
                        yield return null;
                    }
                }
                finally
                {
                    _fileCaching._fileLocksManager.RemoveReadLock(ResourceName);
                }

                Result = t.Result;
                IsCompleted = true;
                if (t.IsFaulted)
                {
                    throw new FileCachingException("Exception in asyncronyous save task", t.Exception);
                }
            }
        }


        public Dictionary<string, object> ReleaseResource(object owner, string uri)
        {
            return ForceReleaseResource(uri);
        }

        public Dictionary<string, object> ReleaseResources(object owner, List<string> uris)
        {
            return ForceReleaseResources(uris);
        }

        public Dictionary<string, object> ReleaseAllOwnerResources(object owner)
        {
            return new Dictionary<string, object>();
        }

        public Dictionary<string, object> ReleaseUnusedResources()
        {
            return new Dictionary<string, object>();
        }

        public List<string> GetUnusedResourceNames()
        {
            return new List<string>();
        }

        public List<string> GetOwnerResourcesNames(object owner)
        {
            return new List<string>(0);
        }

        public Dictionary<string, object> ForceReleaseResources(List<string> keys)
        {
            var result = new Dictionary<string, object>();

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
                result.Add(key, null);
            }

            SaveMetaFile();
            return result;
        }

        public Dictionary<string, object> ForceReleaseResource(string key)
        {
            var result = new Dictionary<string, object>();

            if (!_cacheInfo.Contains(key))
            {
                return result;
            }

            if (_fileLocksManager.HasAnyLock(key))
            {
                return result;
            }

            InternalDelete(key);
            _cacheInfo.Remove(key);
            result.Add(key, null);


            SaveMetaFile();
            return result;
        }

        public Dictionary<string, object> ReleaseAllResources()
        {
            var returnResult = new Dictionary<string, object>();
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
                returnResult.Add(removedKey.Key, null);
            }

            SaveMetaFile();
            return returnResult;
        }

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

        private static string ToPrettyString(byte[] bytes)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
#endif