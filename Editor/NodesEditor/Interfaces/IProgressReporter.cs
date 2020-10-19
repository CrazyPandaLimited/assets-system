using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    interface IProgressReporter : IDisposable
    {
        void Report( string message, float progress );
    }
}