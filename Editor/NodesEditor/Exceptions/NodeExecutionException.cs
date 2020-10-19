using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class NodeExecutionException : Exception
    {
        public string NodeModelId { get; }

        public NodeExecutionException( string message, string nodeModelId, Exception innerException ) : base( message, innerException )
        {
            NodeModelId = nodeModelId;
        }
    }
}