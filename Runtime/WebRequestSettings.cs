﻿using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public enum WebRequestMethod
    {
        NotSet,
        Get,
        Post
    }

    public class WebRequestSettings
    {
        public Dictionary< string, string > Headers = new Dictionary< string, string >();
        public WebRequestMethod Method;
        public float Timeout;
        public int RedirectsLimit = 32;
    }
}
