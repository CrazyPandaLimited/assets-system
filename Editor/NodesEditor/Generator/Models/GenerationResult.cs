using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    public sealed class DllGenerationResult
    {
        public static DllGenerationResult Empty => new DllGenerationResult { CodeGenerationModel = new AssetsStorageModel() };
        
        private readonly HashSet< Diagnostic > _diagnostics = new HashSet< Diagnostic >();

        public AssetsStorageModel CodeGenerationModel { get; set; }
        public bool Success { get; set; }
        public string GeneratedContent { get; set; } = string.Empty;

        public IEnumerable< Diagnostic > Diagnostics
        {
            get => _diagnostics;
            set
            {
                _diagnostics.Clear();
                _diagnostics.UnionWith( value );
            }
        }
    }
}