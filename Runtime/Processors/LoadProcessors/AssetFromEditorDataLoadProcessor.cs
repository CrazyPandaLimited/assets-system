using UnityCore.MessagesFlow;
using UnityEditor;
using UnityEngine;
using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetFromEditorDataLoadProcessor : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< UnityEngine.Object >, UrlLoadingRequest >
    {
        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
#if UNITY_EDITOR

            var asset = AssetDatabase.LoadAssetAtPath( body.Url, body.AssetType );

            if( asset == null )
            {
                header.AddException(new AssetSystemException( "Asset not loaded from bundle" ) );
                _exceptionConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< UnityEngine.Object >( body.Url, body.AssetType, body.ProgressTracker, asset ) );
            return FlowMessageStatus.Accepted;
#else
            ProcessException( header, body, new AssetSystemException( "Asset not loaded from bundle" ) );
            return FlowMessageStatus.Rejected;
#endif
        }
        #endregion
    }
}
