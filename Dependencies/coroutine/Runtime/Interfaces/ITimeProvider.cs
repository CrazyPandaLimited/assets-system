using System;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
    public interface ITimeProvider
    {
        #region Properties
        float deltaTime { get; }
        #endregion

        #region Public Members
        event Action OnUpdate;
        #endregion
    }
}
