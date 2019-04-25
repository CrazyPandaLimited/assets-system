using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class AnticacheNotFoundException:Exception
    {
        public AnticacheNotFoundException(string message) : base(message)
        {
        }
    }
}