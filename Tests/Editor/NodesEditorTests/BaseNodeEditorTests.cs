using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    public abstract class BaseNodeEditorTests
    {
        protected static readonly string _pathToGeneratedDll;
        private static readonly string _pathToGeneratedDllsFolder = Path.Combine( Application.dataPath, "..", "Temp", "GeneratedDlls" );

        static BaseNodeEditorTests()
        {
            _pathToGeneratedDll = Path.Combine( _pathToGeneratedDllsFolder, "Test.dll" );
        }

        [ SetUp ]
        public void Initialize()
        {
            Directory.CreateDirectory( _pathToGeneratedDllsFolder );
        }

        [ TearDown ]
        public void Shutdown()
        {
            Directory.Delete( _pathToGeneratedDllsFolder, true );
        }
    }
}