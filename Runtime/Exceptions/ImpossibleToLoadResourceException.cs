using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class ImpossibleToLoadResourceException:Exception
    {
        public ImpossibleToLoadResourceException(string message) : base(message)
        {
        }
    }
}