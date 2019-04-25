#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS_TEST_MODE
using System;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
    public class DebugToolsDemoData
    {
        public static List<IResourceStoageDebug> FakeData
        {
            get
            {
                if (_fakeData == null)
                {
                    GenerateFakeData();
                }

                return _fakeData;
            }
        }

        private static List<IResourceStoageDebug> _fakeData;

        public static void GenerateFakeData()
        {
            _fakeData = new List<IResourceStoageDebug>();

            _fakeData.Add(new RSFake1());
            _fakeData.Add(new RSFake1());
        }
    }

    public class RSFake1 : IResourceStoageDebug
    {
        public List<ILoaderDebugger> DebugAllLoaders
        {
            get { return _debugAllLoaders; }
        }

        public IWorkersQueueDebug DebugWorkersQueue
        {
            get { return _debugWorkersQueue; }
        }

        private List<ILoaderDebugger> _debugAllLoaders;
        private IWorkersQueueDebug _debugWorkersQueue;

        public RSFake1()
        {
            _debugAllLoaders = new List<ILoaderDebugger>();
            _debugAllLoaders.Add(new FakeLoader1());
            _debugAllLoaders.Add(new FakeLoader2());

            _debugWorkersQueue = new FakeWorkersQueue1();
        }
    }

    public class FakeLoader1 : ILoaderDebugger
    {
        public List<ICacheDebugger> DebugCaches
        {
            get { return _debugCaches; }
        }

        public string SupportsMask
        {
            get { return "WebRequestLoader"; }
        }

        private List<ICacheDebugger> _debugCaches;

        public FakeLoader1()
        {
            _debugCaches = new List<ICacheDebugger>();
            _debugCaches.Add(new FakeCache1());
        }
    }

    public class FakeCache1 : ICacheDebugger
    {
        public CacheObjectDebugInfo[] GetCachedObjectsDebugInfo()
        {
            return _cachedObjectsDebugInfo;
        }


        private CacheObjectDebugInfo[] _cachedObjectsDebugInfo;

        public FakeCache1()
        {
            _cachedObjectsDebugInfo = new CacheObjectDebugInfo[3];
            _cachedObjectsDebugInfo[0] = new CacheObjectDebugInfo()
            {
                key = "MyIcon1.png",
                owners = new List<string>()
                {
                    "SuperClass1",
                    "SuperClass2",
                },
                resourceType = typeof(Texture2D).ToString()
            };

            _cachedObjectsDebugInfo[1] = new CacheObjectDebugInfo()
            {
                key = "MyIcon2.png",
                owners = new List<string>()
                {
                    "SuperClass1",
                    "SuperClass2",
                },
                resourceType = typeof(Texture2D).ToString()
            };

            _cachedObjectsDebugInfo[2] = new CacheObjectDebugInfo()
            {
                key = "MyIcon3.png",
                owners = new List<string>()
                {
                    "SuperClass1",
                    "SuperClass2",
                },
                resourceType = typeof(Texture2D).ToString()
            };
        }
    }

    public class FakeWorkersQueue1 : IWorkersQueueDebug
    {
        public int DebugMaxSimultaneousResouceWorkers
        {
            get { return 3; }
        }

        public List<IResourceWorkerDebug> DebugWorkersInWaitingState
        {
            get { return _debugWorkersInWaitingState; }
        }

        public List<IResourceWorkerDebug> DebugWorkersInProcessState
        {
            get { return _debugWorkersInProcessState; }
        }

        private List<IResourceWorkerDebug> _debugWorkersInWaitingState;
        private List<IResourceWorkerDebug> _debugWorkersInProcessState;


        public FakeWorkersQueue1()
        {
            _debugWorkersInWaitingState = new List<IResourceWorkerDebug>();

            _debugWorkersInWaitingState.Add(new FakeResourceWorker());
            _debugWorkersInWaitingState.Add(new FakeResourceWorker());
            _debugWorkersInWaitingState.Add(new FakeResourceWorker());
            _debugWorkersInWaitingState.Add(new FakeResourceWorker());

            _debugWorkersInProcessState = new List<IResourceWorkerDebug>();
            _debugWorkersInProcessState.Add(new FakeResourceWorker());
            _debugWorkersInProcessState.Add(new FakeResourceWorker());
            _debugWorkersInProcessState.Add(new FakeResourceWorker());
            _debugWorkersInProcessState.Add(new FakeResourceWorker());
            _debugWorkersInProcessState.Add(new FakeResourceWorker());
            _debugWorkersInProcessState.Add(new FakeResourceWorker());
            _debugWorkersInProcessState.Add(new FakeResourceWorker());
        }
    }

    public class FakeResourceWorker : IResourceWorkerDebug
    {
        public string Uri
        {
            get { return _uri; }
            private set { }
        }

        public bool IsWaitDependentResource
        {
            get { return isWait; }
            private set { }
        }

        public List<IDebugLoadingOperation> LoadingOperations
        {
            get { return _loadingOperations; }
            private set { }
        }

        private string _uri;
        private bool isWait;

        private List<IDebugLoadingOperation> _loadingOperations;


        public FakeResourceWorker()
        {
            _uri = "http://mySuperStatic.com/myFilder" + Random.Range(1, 1000) + "/" + Guid.NewGuid().ToString() + ".json";
            isWait = Random.Range(0, 100) > 50;
            _loadingOperations = new List<IDebugLoadingOperation>();
            for (int i = 0; i < Random.Range(0, 20); i++)
            {
                _loadingOperations.Add(new FakeLoadingOperation(_uri));
            }
        }
    }

    public class FakeLoadingOperation : IDebugLoadingOperation
    {
        public string Uri { get; private set; }
        public float Progress { get; private set; }
        public bool IsCompleted { get; private set; }

        public object Owner
        {
            get { return _owner; }
            private set { }
        }

        private string _owner;

        public FakeLoadingOperation(string uri)
        {
            Uri = uri;
            Progress = Random.Range(0f, 1f);
            IsCompleted = Random.Range(0, 100) > 50;

            _owner = Guid.NewGuid().ToString();
        }
    }


    public class FakeLoader2 : ILoaderDebugger
    {
        public List<ICacheDebugger> DebugCaches
        {
            get { return _debugCaches; }
        }

        public string SupportsMask
        {
            get { return "BundlesLoader"; }
        }

        private List<ICacheDebugger> _debugCaches;

        public FakeLoader2()
        {
            _debugCaches = new List<ICacheDebugger>();
            _debugCaches.Add(new FakeCache1());
            _debugCaches.Add(new FakeCache1());
        }
    }
}
#endif
