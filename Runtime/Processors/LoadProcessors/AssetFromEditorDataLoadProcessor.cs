using CrazyPanda.UnityCore.MessagesFlow;
using UnityEditor;
using UnityEngine;
using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetFromEditorDataLoadProcessor : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< UnityEngine.Object >, UrlLoadingRequest >
    {
        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
#if UNITY_EDITOR

            UnityEngine.Object asset = null;
            
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath( body.Url );
                var subAssetName = header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET );

                foreach( var subAsset in subAssets )
                {
                    if( subAsset.name == subAssetName )
                    {
                        asset = subAsset;
                        break;
                    }
                }
            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath( body.Url, body.AssetType );
            }

            if( asset == null )
            {
                header.AddException( new AssetNotLoadedException( "Asset not loaded", this, header, body ) );
                SendException( header, body );
                return;
            }

            SendOutput( header, new AssetLoadingRequest< UnityEngine.Object >( body.Url, body.AssetType, body.ProgressTracker, asset ) );
#else
            ProcessException( header, body, new AssetNotLoadedException( "Asset not loaded", this, header, body ) );
#endif
        }
    }
}
