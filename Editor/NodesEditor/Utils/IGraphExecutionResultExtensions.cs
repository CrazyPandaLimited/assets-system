using System;
using System.Linq;
using CrazyPanda.UnityCore.NodeEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    static class IGraphExecutionResultExtensions
    {
        internal static Exception GetExceptionByNodeId( this IGraphExecutionResult result, string nodeId )
        {
            return result.Exceptions.FirstOrDefault( exception => exception is NodeExecutionException nodeException &&
                                                                  nodeException.NodeModelId == nodeId )?.InnerException;
        }
    }
}