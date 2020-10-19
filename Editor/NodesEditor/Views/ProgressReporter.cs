using UnityEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class ProgressReporter : IProgressReporter
    {
        public void Report( string message, float progress ) => EditorUtility.DisplayProgressBar( string.Empty, message, progress );
        
        public void Dispose() => EditorUtility.ClearProgressBar();
    }

    sealed class NullableProgressReporter : IProgressReporter
    {
        public void Report( string message, float progress )
        {
        }
        
        public void Dispose()
        {
            
        }
    }
}