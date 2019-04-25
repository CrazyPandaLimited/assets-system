#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.AssetGraph;
using UnityEngine.AssetGraph.DataModel.Version2;

[ CustomNode( "Bundles/Extract Dependencies", 2000 ) ]
public class ExtractDependencies : Node
{
	#region Properties
	public override string ActiveStyle { get { return "node 3 on"; } }

	public override string InactiveStyle { get { return "node 3"; } }

	public override string Category { get { return "Bundles"; } }

	public override NodeOutputSemantics NodeInputType { get { return NodeOutputSemantics.AssetBundleConfigurations; } }

	public override NodeOutputSemantics NodeOutputType { get { return NodeOutputSemantics.AssetBundleConfigurations; } }
	#endregion

	#region Public Members
	public override void OnInspectorGUI( NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged )
	{
	}

	public override void Initialize( NodeData data )
	{
		data.AddDefaultInputPoint();
		data.AddDefaultOutputPoint();
	}

	public override Node Clone( NodeData newData )
	{
		var newNode = new ExtractDependencies();
		newData.AddDefaultInputPoint();
		return newNode;
	}

	public override void Prepare( BuildTarget target, NodeData nodeData, IEnumerable< PerformGraph.AssetGroups > incoming, IEnumerable< ConnectionData > connectionsToOutput, PerformGraph.Output outputFunc )
	{
		if( outputFunc != null )
		{
			var destination = connectionsToOutput == null || !connectionsToOutput.Any() ? null : connectionsToOutput.First();

			if( incoming != null )
			{
				var dependencyCollector = new Dictionary< string, List< AssetReference > >();

				// build dependency map
				foreach( var ag in incoming )
				{
					foreach( var key in ag.assetGroups.Keys )
					{
						var assets = ag.assetGroups[ key ];

						foreach( var a in assets )
						{
							CollectDependencies( key, new[ ]
							{
								a.importFrom
							}, ref dependencyCollector );
						}
					}
				}

				var result = new Dictionary< string, List< AssetReference > >();

				foreach( var ag in incoming )
				{
					foreach( var assetGroup in ag.assetGroups )
					{
						result.Add( assetGroup.Key, assetGroup.Value );
					}
				}

				foreach( var pair in dependencyCollector )
				{
					if( !result.ContainsKey( pair.Key ) )
					{
						result.Add( pair.Key, new List< AssetReference >() );
					}

					result[ pair.Key ].AddRange( pair.Value );
				}

				outputFunc( destination, result );
			}
			else
			{
				outputFunc( destination, new Dictionary< string, List< AssetReference > >() );
			}
		}
	}
	#endregion

	#region Private Members
	private void CollectDependencies( string groupKey, string[ ] assetPaths, ref Dictionary< string, List< AssetReference > > collector )
	{
		var dependencies = AssetDatabase.GetDependencies( assetPaths );
		foreach( var d in dependencies )
		{
			if( TypeUtility.GetMainAssetTypeAtPath( d ) == typeof ( MonoScript ) )
			{
				continue;
			}

			if( d == assetPaths[ 0 ] )
			{
				continue;
			}

			if( !collector.ContainsKey( groupKey ) )
			{
				collector[ groupKey ] = new List< AssetReference >();
			}

			if( collector[ groupKey ].All( reference => reference.importFrom != d ) )
			{
				var assetReference = AssetReference.CreateReference( d );
				collector[ groupKey ].Add( assetReference );
			}
		}
	}
	#endregion
}
#endif