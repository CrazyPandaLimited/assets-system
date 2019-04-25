#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_NET_4_6

using System.Collections;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class FileCaching
    {
        #region Public Members

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

        #endregion

        #region Nested Types

        private class FileCacheCacheLoadingOperation : ICacheGettingOperation<byte[]>
        {
            #region Private Fields

            private FileCaching _fileCaching;

            #endregion

            #region Properties

            public string ResourceName { get; private set; }
            public byte[] Result { get; private set; }
            public bool IsCompleted { get; private set; }

            #endregion

            #region Constructors

            public FileCacheCacheLoadingOperation(FileCaching fileCaching, string resourceName)
            {
                _fileCaching = fileCaching;
                ResourceName = resourceName;
            }

            #endregion

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

            #endregion
        }
    }
}
#endif