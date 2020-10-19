using System;
using System.Reflection;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    [Serializable]
    public class ProcessorLinkInformation
    {
        public string ProcessorsLinkerName { get; set; } = string.Empty;
        public Type ExpectedLinkType { get; set; }
        public PropertyInfo ProcessorLinkProperty { get; set; }
        public ProcessorModel ProcessorToLink { get; set; }
        public bool IsCorrect => ProcessorLinkProperty != null && !string.IsNullOrEmpty( ProcessorToLink.Name ) && !string.IsNullOrEmpty( ProcessorsLinkerName );
    }
}