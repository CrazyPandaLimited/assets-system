#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class WorkersQueue
    {
        #region Private Fields

        private int _maxSimultaneousResouceWorkers;
        private List<IResourceWorker> _workersInWaitingState = new List<IResourceWorker>();
        private List<IResourceWorker> _workersInProcessState = new List<IResourceWorker>();

        #endregion

        #region Constructors

        public WorkersQueue(int maxSimultaneousResouceWorkers)
        {
            _maxSimultaneousResouceWorkers = maxSimultaneousResouceWorkers;
        }

        #endregion

        #region Public Members

        public IEnumerable<IResourceWorker> GetAllWorkersInWaitingSate()
        {
            foreach (var w in _workersInWaitingState)
            {
                yield return w;
            }
        }

        public IEnumerable<IResourceWorker> GetAllWorkersInProcessSate()
        {            
            foreach (var w in _workersInProcessState)
            {
                yield return w;
            }
        }

        public T GetExistResourceWorker<T>(string uri) where T : IResourceWorker
        {
            foreach (var worker in _workersInWaitingState)
            {
                if (worker.Uri.Equals(uri))
                {
                    return (T) worker;
                }
            }

            foreach (var worker in _workersInProcessState)
            {
                if (worker.Uri.Equals(uri))
                {
                    return (T) worker;
                }
            }

            return default(T);
        }

        public void AddToQueue(IResourceWorker worker)
        {
            _workersInWaitingState.Add(worker);
            TryToStartLoading();
        }

        public void LoadingComplete(IResourceWorker worker)
        {
            _workersInWaitingState.Remove(worker);
            _workersInProcessState.Remove(worker);
            TryToStartLoading();
        }

        public void CancelAllRequests(object owner)
        {
            foreach (var waitLoader in _workersInWaitingState.ToArray())
            {
                waitLoader.RemoveLoadingOperation(owner);
            }

            foreach (var waitLoader in _workersInProcessState.ToArray())
            {
                waitLoader.RemoveLoadingOperation(owner);
            }
        }

        public void CancelAllRequests()
        {
            foreach (var waitLoader in _workersInWaitingState.ToArray())
            {
                waitLoader.CancelLoading();
            }

            foreach (var waitLoader in _workersInProcessState.ToArray())
            {
                waitLoader.CancelLoading();
            }

            _workersInWaitingState.Clear();
            _workersInProcessState.Clear();
        }

        #endregion

        #region Private Members

        private void TryToStartLoading()
        {
            if (_workersInProcessState.Count == _maxSimultaneousResouceWorkers || _workersInWaitingState.Count == 0)
            {
                return;
            }

            var workersToStartLoading = new List<IResourceWorker>();
            for (var i = _workersInWaitingState.Count - 1; i >= 0; i--)
            {
                var task = _workersInWaitingState[i];

                if (!task.IsWaitDependentResource)
                {
                    workersToStartLoading.Add(task);
                }

                if (_workersInProcessState.Count + workersToStartLoading.Count == _maxSimultaneousResouceWorkers)
                {
                    break;
                }
            }

            for (var j = 0; j < workersToStartLoading.Count; j++)
            {
                _workersInWaitingState.Remove(workersToStartLoading[j]);
                _workersInProcessState.Add(workersToStartLoading[j]);
                workersToStartLoading[j].StartLoading();
            }
        }

        #endregion
    }
}
#endif