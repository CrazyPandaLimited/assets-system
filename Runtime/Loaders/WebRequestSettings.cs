#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public enum WebRequestMethod
    {
        NotSet,
        Get,
        Post
    }

    public class WebRequestSettings
    {
        #region Public Fields

        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public WebRequestMethod Method;
        public float Timeout;
        public int RedirectsLimit = 32;
        public bool ChunkTransfer = true;

        #endregion
    }
}
#endif