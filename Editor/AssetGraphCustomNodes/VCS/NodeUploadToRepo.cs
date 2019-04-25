#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CrazyPanda.UnityCore.ResourcesSystem;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using CustomManifest = CrazyPanda.UnityCore.ResourcesSystem.AssetBundleManifest;
using Debug = UnityEngine.Debug;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;

[ CustomNode( "VCS/Node UploadToHGRepo", 1000 ) ]
// TODO RENAME this class
public class MyNode : Node
{
	#region Private Fields
	[ SerializeField ]
	private SerializableMultiTargetString m_myValue;

	[ SerializeField ]
	private string _exportDirectory;

	[ SerializeField ]
	private HGAction _hgAction;
	#endregion

	#region Properties
	public override string ActiveStyle
	{
		get
		{
			return "node 8 on";
		}
	}

	public override string InactiveStyle
	{
		get
		{
			return "node 8";
		}
	}

	public override string Category
	{
		get
		{
			return "VCS";
		}
	}
	#endregion

	#region Public Members
	public override void Initialize( Model.NodeData data )
	{
		m_myValue = new SerializableMultiTargetString();
		data.AddDefaultInputPoint();
		data.AddDefaultOutputPoint();
	}

	public override Node Clone( Model.NodeData newData )
	{
		var newNode = new MyNode();
		newNode.m_myValue = new SerializableMultiTargetString( m_myValue );
		newData.AddDefaultInputPoint();
		newData.AddDefaultOutputPoint();
		return newNode;
	}

	public override void OnInspectorGUI( NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor,
										Action onValueChanged )
	{
		editor.UpdateNodeName( node );

		GUILayout.Space( 10f );

		editor.DrawPlatformSelector( node );
		using( new EditorGUILayout.VerticalScope( GUI.skin.box ) )
		{
			var disabledScope = editor.DrawOverrideTargetToggle( node, m_myValue.ContainsValueOf( editor.CurrentEditingGroup ),
																b =>
																{
																	using( new RecordUndoScope( "Remove Target Platform Settings", node, true ) )
																	{
																		if( b )
																		{
																			m_myValue[ editor.CurrentEditingGroup ] = m_myValue.DefaultValue;
																		}
																		else
																		{
																			m_myValue.Remove( editor.CurrentEditingGroup );
																		}
																		onValueChanged();
																	}
																} );

			using( disabledScope )
			{
				_exportDirectory = editor.DrawFolderSelector( "Export Directory", "Select Output Folder", _exportDirectory,
															Application.dataPath + "/../", folderSelected => folderSelected );
				_hgAction = ( HGAction ) EditorGUILayout.EnumPopup( "HGAction", _hgAction );
			}
		}
	}

	public override void Prepare( BuildTarget target, Model.NodeData node,
								IEnumerable< PerformGraph.AssetGroups > incoming, IEnumerable< Model.ConnectionData > connectionsToOutput,
								PerformGraph.Output Output )
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

	public override void Build( BuildTarget target, Model.NodeData node, IEnumerable< PerformGraph.AssetGroups > incoming,
								IEnumerable< Model.ConnectionData > connectionsToOutput, PerformGraph.Output outputFunc,
								Action< Model.NodeData, string, float > progressFunc )
	{
		if( _hgAction != HGAction.Nothing )
		{
			ExecuteCommandLine( "hg pull", progressFunc, 0, node );
		}

		progressFunc( node, "COPY FILES", 0.3f );
		foreach( var assetGroups in incoming )
		{
			foreach( var ag in assetGroups.assetGroups )
			{
				CopyFiles( ag.Value );
			}
		}

		var commitMessage = "Bundles_from_build_to";
		switch( _hgAction )
		{
			case HGAction.Nothing:
				break;

			case HGAction.Commit:
				ExecuteCommandLine( "hg add", progressFunc, 0.6f, node );
				ExecuteCommandLine( "hg commit -m " + commitMessage, progressFunc, 0.8f, node );
				break;
			case HGAction.CommitAndPush:
				ExecuteCommandLine( "hg add", progressFunc, 0.4f, node );
				ExecuteCommandLine( "hg commit -m " + commitMessage, progressFunc, 0.5f, node );
				ExecuteCommandLine( "hg push", progressFunc, 0.6f, node );
				break;
		}

		progressFunc( node, "DONE", 1f );
	}
	#endregion

	#region Private Members
	// TODO This method is dublicate from ArtTools/.../AtlasOptimizerController.cs
	// TODO we need extract this functionality to special utility class
	private void ExecuteCommandLine( string command, Action< Model.NodeData, string, float > progressFunc, float progress,
									Model.NodeData node )
	{
		// TODO remove from this method to outside
		{
			progressFunc( node, command.ToUpperInvariant(), progress );
		}

		var process = new Process();

#if UNITY_EDITOR_WIN
		process.StartInfo.FileName = "cmd.exe";
#else
		process.StartInfo.FileName = "/bin/bash";
#endif

		process.StartInfo.WorkingDirectory = _exportDirectory;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.RedirectStandardOutput = true;

#if UNITY_EDITOR_WIN
		process.StartInfo.Arguments = "/C" + command;
#else
		process.StartInfo.Arguments = "-c \"" + command + "\"";
#endif
		process.Start();
		process.WaitForExit();

		var output = "\n----\nOutput: " + process.StandardOutput.ReadToEnd() + "\n----\nErrors: " +
					process.StandardError.ReadToEnd();

		if( process.ExitCode != 0 )
		{
			Debug.LogError( "command " + command + " - exit code: " + process.ExitCode + output );
		}
		else
		{
			Debug.Log( command + output );
		}
	}

	private void CopyFiles( List< AssetReference > assetInfos )
	{
		var jsonSerializer =
			new JsonSerializer( new JsonSerializerSettings { Formatting = Formatting.Indented }, Encoding.UTF8 );
		var customManifest = new CustomManifest();
		foreach( var assetReference in assetInfos )
		{
			if( assetReference.extension == ".json" )
			{
				var manifestContent = File.ReadAllText( assetReference.absolutePath );
				customManifest = jsonSerializer.DeserializeString< CustomManifest >( manifestContent );
			}
		}

		foreach( var assetReference in assetInfos )
		{
			var apendToName = "";
			if( assetReference.extension != ".json" )
			{
				var crc32 = "";
				foreach( KeyValuePair< string, BundleInfo > bundleInfo in customManifest.BundleInfos )
				{
					if( bundleInfo.Value.Name == assetReference.fileName )
					{
						crc32 = bundleInfo.Value.CRC;
					}
				}
				apendToName = "_" + crc32;
			}

			var newPath = Path.Combine( _exportDirectory, assetReference.fileName + apendToName + assetReference.extension );
			if( !File.Exists( newPath ) )
			{
				File.Copy( assetReference.absolutePath, newPath );
			}
		}
	}
	#endregion

	#region Nested Types
	private enum HGAction
	{
		Nothing,
		Commit,
		CommitAndPush
	}
	#endregion
}
#endif