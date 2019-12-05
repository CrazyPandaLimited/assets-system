using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class ProgressTrackerDownloadHandler : DownloadHandlerScript
    {
        #region Public Fields
        public event Action< float > ProgressTrackerEvent;
        #endregion

        #region Private Fields
        protected int _received;
        protected ulong _contentLength;
        private readonly List<byte> _receivedData = new List< byte >();
        #endregion
        
        #region Protected Members
        protected override bool ReceiveData( byte[ ] data, int dataLength )
        {
            if( data == null || data.Length == 0 )
            {
                return false;
            }

            _receivedData.AddRange( data );
            _received += dataLength;
            ProgressTrackerEvent?.Invoke( GetProgress() );

            return true;
        }

        protected override float GetProgress() => _contentLength == 0 ? 0 : Mathf.Clamp01( ( float ) _received / _contentLength );

        protected override void ReceiveContentLengthHeader( ulong contentLength )
        {
            base.ReceiveContentLengthHeader( contentLength );
            _contentLength = contentLength;
        }
        
        protected override byte[ ] GetData() => _receivedData.ToArray();
        
        #endregion
    }
}
