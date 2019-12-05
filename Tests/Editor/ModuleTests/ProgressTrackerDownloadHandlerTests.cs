using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public sealed class ProgressTrackerDownloadHandlerTests
    {
        private const float MaxDownloadProgress = 1.0f;
        private MockProgressTrackerDownloadHandler _downloadHandler;

        [ SetUp ]
        public void Initialize() => _downloadHandler = new MockProgressTrackerDownloadHandler();

        [ Test ]
        public void CheckAnyDataReceivedAfterForcedSetTest()
        {
            var expectedBytes = new byte[ ] { 1, 2, 3, 4, 5, 6 };
            var expectedLength = expectedBytes.Length;
            var contentLengthHeader = Convert.ToUInt64( expectedLength );

            Assert.LessOrEqual( _downloadHandler.Progress, default );
            Assert.IsEmpty( _downloadHandler.data );

            _downloadHandler.ReceiveContentLengthHeader( contentLengthHeader );
            Assert.IsTrue( _downloadHandler.ReceiveData( expectedBytes, expectedLength ) );

            Assert.AreEqual( MaxDownloadProgress, _downloadHandler.Progress );
            Assert.AreEqual( expectedLength, _downloadHandler.Received );
            Assert.AreEqual( contentLengthHeader, _downloadHandler.ContentLength );
            Assert.AreEqual( expectedLength, _downloadHandler.data.Length );
            Assert.AreEqual( expectedBytes, _downloadHandler.data );
        }

        [ Test ]
        public void CheckDownloadingProgressFailedAfterForcedSetWrongDataTest()
        {
            var expectedBytes = new byte[ ] { 1, 2, 3, 4, 5, 6 };
            var expectedLength = expectedBytes.Length;
            var contentLengthHeader = 10u;

            _downloadHandler.ReceiveContentLengthHeader( contentLengthHeader );
            Assert.IsTrue( _downloadHandler.ReceiveData( expectedBytes, expectedLength ) );
            Assert.Less( _downloadHandler.Progress, MaxDownloadProgress );
        }

        [ Test ]
        public void CheckReceiveDataFailedAfterForcedEmptyValuesTest()
        {
            var expectedBytes = new byte[ ] { };
            var expectedLength = expectedBytes.Length;
            Assert.IsFalse( _downloadHandler.ReceiveData( expectedBytes, expectedLength ) );
        }

        [ UnityTest ]
        public IEnumerator CheckProgressEventInvokedFailedAfterForcedSetDataTest()
        {
            var progressEventInvoked = false;
            var expectedBytes = new byte[ ] { 0 };
            var expectedLength = expectedBytes.Length;

            _downloadHandler.ProgressTrackerEvent += _ => progressEventInvoked = true;
            _downloadHandler.ReceiveData( expectedBytes, expectedLength );

            yield return null;

            Assert.IsTrue( progressEventInvoked );
        }
        
        private sealed class MockProgressTrackerDownloadHandler : ProgressTrackerDownloadHandler
        {
            #region Public Fields
            public float Progress => GetProgress();
            public int Received => _received;
            public ulong ContentLength => _contentLength;
            #endregion

            #region Public Members
        
            public new bool ReceiveData( byte[ ] data, int dataLength ) => base.ReceiveData( data, dataLength );

            public new void ReceiveContentLengthHeader( ulong contentLength ) => base.ReceiveContentLengthHeader( contentLength );
            #endregion
        }
    }
}
