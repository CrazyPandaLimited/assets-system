#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH && POKEROLDIMPL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AssetGraph;
using UnityEngine.AssetGraph.DataModel.Version2;

[ CustomNode( "Custom/Copy to", 1000 ) ]
public class CopyToNode : Node
{
	#region Private Fields
	[ SerializeField ] private string _bundlesFolder;
	#endregion

	#region Properties
	public override string ActiveStyle { get { return "node 8 on"; } }

	public override string InactiveStyle { get { return "node 8"; } }

	public override string Category { get { return "Custom"; } }

	public override NodeOutputSemantics NodeInputType { get { return NodeOutputSemantics.AssetBundles; } }

	public override NodeOutputSemantics NodeOutputType { get { return NodeOutputSemantics.Any; } }
	#endregion

	#region Public Members
	public override void Initialize( NodeData data )
	{
		data.AddDefaultInputPoint();
		data.AddDefaultOutputPoint();
	}

	public override Node Clone( NodeData newData )
	{
		throw new Exception();
	}

	public override void OnInspectorGUI( NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged )
	{
		EditorGUILayout.HelpBox( "Local bundles path is:\n" + GetBundlesFolder() + "\nAt build machine this folder will be re-setted automatically.", MessageType.Info );
	}

	public override void Prepare( BuildTarget target, NodeData node, IEnumerable< PerformGraph.AssetGroups > incoming, IEnumerable< ConnectionData > connectionsToOutput, PerformGraph.Output Output )
	{
		if( Output != null )
		{
			var destination = connectionsToOutput == null || !connectionsToOutput.Any() ? null : connectionsToOutput.First();

			if( incoming != null )
			{
				foreach( var ag in incoming )
				{
					Output( destination, ag.assetGroups );
				}
			}
			else
			{
				Output( destination, new Dictionary< string, List< AssetReference > >() );
			}
		}
	}

	public override void Build( BuildTarget target, NodeData nodeData, IEnumerable< PerformGraph.AssetGroups > incoming, IEnumerable< ConnectionData > connectionsToOutput, PerformGraph.Output outputFunc, Action< NodeData, string, float > progressFunc )
	{
		_bundlesFolder = GetBundlesFolder();
		Debug.Log( "Copying bundles to folder: " + _bundlesFolder );
		CopyBundlesTo( incoming );
	}

	private string GetBundlesFolder()
	{
		if( InternalEditorUtility.inBatchMode )
		{
			return BundlesBuildAgent.ServerBundlesPath;
		}

		return Application.dataPath + "/Resources/" + LocalBundlesSettings.LocalBundlesPath;
	}
	#endregion

	#region Private Members
	private void CopyBundlesTo( IEnumerable< PerformGraph.AssetGroups > incoming )
	{
		foreach( var assetGroupse in incoming )
		{
			foreach( var assetGroup in assetGroupse.assetGroups )
			{
				foreach( var assetReference in assetGroup.Value )
				{
					var path = Path.Combine( _bundlesFolder, assetReference.fileNameAndExtension );
					Debug.Log( "Copying bundle " + assetReference.path + " to folder " + path );
					File.Copy( assetReference.path, path, true );
				}
			}
		}
	}
	#endregion
}
#endif