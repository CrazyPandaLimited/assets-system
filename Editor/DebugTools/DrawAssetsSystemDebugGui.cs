using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
	public class DrawAssetsSystemDebugGui
	{
		private const string NamespacePrefix = "CrazyPanda.UnityCore.AssetssSystem.";

		private AssetsSystemDebugToolAssets _settings;

		private bool _loadersInfoFoldout;
		private bool _queueInfoFoldout;
		private bool _waitingQueueInfoDetailsFoldout;
		private bool _executingQueueInfoDetailsFoldout;
		private bool[ ] _loaderInfoFoldouts;
		private Vector2 _scrollPosition;


		private DebugUIQueue _debugUiQueue;
		private DebugUICombiners _debugUiCombiners;
		private DebugUICaches _debugUiCaches;
		private DebugUIPromises _debugUiPromises;
		private DebugUIRequestsHistory _debugUiRequestsHistory;

		public DrawAssetsSystemDebugGui( AssetsSystemDebugToolAssets settings )
		{
			_settings = settings;
			_debugUiQueue = new DebugUIQueue();
			_debugUiCombiners = new DebugUICombiners();
			_debugUiCaches = new DebugUICaches();
			_debugUiPromises = new DebugUIPromises();
			_debugUiRequestsHistory = new DebugUIRequestsHistory();
		}

		public void DrawSystemGui( AssetsSystemDebugInfo assetsStorageInstance, Rect windowSize )
		{
			_scrollPosition = GUILayout.BeginScrollView( _scrollPosition, GUILayout.Width( windowSize.width ) );

			// var tmpRect = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_LoadersTogle_Title_Height);
			// GUI.Label(tmpRect, string.Empty, _settings.PlayMode_LoadersTogleBg_Style);
			// tmpRect.x += _settings.PlayMode_LoadersTogleBg_Style.contentOffset.x;
			// tmpRect.width -= _settings.PlayMode_LoadersTogleBg_Style.contentOffset.x;
			//
			// tmpRect.y += _settings.PlayMode_LoadersTogleBg_Style.contentOffset.y;
			// tmpRect.height -= _settings.PlayMode_LoadersTogleBg_Style.contentOffset.y + _settings.PlayMode_LoadersTogleBg_Style.border.bottom;
			//
			// _loadersInfoFoldout = GUI.Toggle(tmpRect, _loadersInfoFoldout, _settings.PlayMode_LoadersTogle_Title, _settings.PlayMode_LoadersTogle_Style);

			// if (_loaderInfoFoldouts == null || _loaderInfoFoldouts.Length != assetsStorageInstance.DebugAllLoaders.Count)
			// {
			//     _loaderInfoFoldouts = new bool[assetsStorageInstance.DebugAllLoaders.Count];
			// }
			//
			// if (_loadersInfoFoldout)
			// {
			//     LoadersLayoutCalculation(assetsStorageInstance, windowSize);
			//     LoadersDraw(assetsStorageInstance);
			// }

			// tmpRect = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_QueueTogle_Title_Height);
			// GUI.Label(tmpRect, string.Empty, _settings.PlayMode_QueueTogleBg_Style);
			// tmpRect.x += _settings.PlayMode_QueueTogleBg_Style.contentOffset.x;
			// tmpRect.width -= _settings.PlayMode_QueueTogleBg_Style.contentOffset.x;
			//
			// tmpRect.y += _settings.PlayMode_QueueTogleBg_Style.contentOffset.y;
			// tmpRect.height -= _settings.PlayMode_QueueTogleBg_Style.contentOffset.y + _settings.PlayMode_QueueTogleBg_Style.border.bottom;
			//
			// _queueInfoFoldout = GUI.Toggle(tmpRect, _queueInfoFoldout, _settings.PlayMode_QueueTogle_Title, _settings.PlayMode_QueueTogle_Style);

			// if (_queueInfoFoldout)
			// {
			//     QueueLayoutCalculation(assetsStorageInstance, windowSize);
			//     QueueDraw(assetsStorageInstance);
			// }

			_debugUiQueue.Draw( assetsStorageInstance.RequestsQueue );
			_debugUiCombiners.Draw( assetsStorageInstance.MessageCombineNodesInfos );
			_debugUiCaches.Draw( assetsStorageInstance.Caches );
			_debugUiCaches.Draw( assetsStorageInstance.RefcountCacheControllers );
			_debugUiPromises.Draw(assetsStorageInstance.PromiseMap);
			_debugUiRequestsHistory.Draw( assetsStorageInstance.RequestsHistoryInfo );
			GUILayoutUtility.GetRect( windowSize.width, 20 );
			GUILayout.EndScrollView();
		}

		private Rect _loadersSectionAllBgRect;
		private Rect[ ] _loadersSectionLoaderTitleBgRects;
		private Rect[ ] _loadersSectionLoaderTitleFgRects;
		private Rect[ ] _loadersSectionAlLcachingBgRects;
		private Rect[ ][ ] _loadersSectionCachingBgRects;
		private Rect[ ][ ] _loadersSectionCachingTitleRects;
		private Rect[ ][ ][ /* row */ ][ /* col */ ] _loadersSectionCachingAssetTableRects;

		private void LoadersLayoutCalculation( AssetsSystemDebugInfo assetsStorageInstance, Rect windowSize )
		{
			// _loadersSectionAllBgRect = new Rect();
			// _loadersSectionLoaderTitleBgRects = new Rect[assetsStorageInstance.DebugAllLoaders.Count];
			// _loadersSectionLoaderTitleFgRects = new Rect[assetsStorageInstance.DebugAllLoaders.Count];
			//
			//
			// _loadersSectionAlLcachingBgRects = new Rect[assetsStorageInstance.DebugAllLoaders.Count];
			// _loadersSectionCachingBgRects = new Rect[assetsStorageInstance.DebugAllLoaders.Count][];
			// _loadersSectionCachingAssetTableRects = new Rect[assetsStorageInstance.DebugAllLoaders.Count][][][];
			//
			// for (int loaderIdx = 0; loaderIdx < assetsStorageInstance.DebugAllLoaders.Count; loaderIdx++)
			// {
			//     var h1 = _settings.PlayMode_Loader_Togle_Title_Offsets.height + _settings.PlayMode_Loader_Togle_Title_Offsets.y;
			//     _loadersSectionLoaderTitleBgRects[loaderIdx] = GUILayoutUtility.GetRect(windowSize.width, h1);
			//
			//     if (loaderIdx == 0)
			//     {
			//         _loadersSectionAllBgRect = _loadersSectionLoaderTitleBgRects[loaderIdx];
			//     }
			//
			//     _loadersSectionLoaderTitleBgRects[loaderIdx].x += _settings.PlayMode_Loader_Togle_Title_Offsets.x;
			//     _loadersSectionLoaderTitleBgRects[loaderIdx].y += _settings.PlayMode_Loader_Togle_Title_Offsets.y;
			//     _loadersSectionLoaderTitleBgRects[loaderIdx].width -= _settings.PlayMode_Loader_Togle_Title_Offsets.x * 2;
			//
			//
			//     _loadersSectionLoaderTitleFgRects[loaderIdx].x =
			//         _loadersSectionLoaderTitleBgRects[loaderIdx].x + _settings.PlayMode_Loader_TogleBg_Style.contentOffset.x;
			//     _loadersSectionLoaderTitleFgRects[loaderIdx].width =
			//         _loadersSectionLoaderTitleBgRects[loaderIdx].width - _settings.PlayMode_Loader_TogleBg_Style.contentOffset.x;
			//
			//     _loadersSectionLoaderTitleFgRects[loaderIdx].y =
			//         _loadersSectionLoaderTitleBgRects[loaderIdx].y + _settings.PlayMode_Loader_TogleBg_Style.contentOffset.y;
			//     _loadersSectionLoaderTitleFgRects[loaderIdx].height = _loadersSectionLoaderTitleBgRects[loaderIdx].height -
			//                                                            _settings.PlayMode_Loader_TogleBg_Style.contentOffset.y +
			//                                                            _settings.PlayMode_Loader_TogleBg_Style.border.bottom;
			//
			//     if (_loaderInfoFoldouts[loaderIdx])
			//     {
			//         _loadersSectionCachingBgRects[loaderIdx] = new Rect[assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches.Count];
			//         _loadersSectionCachingAssetTableRects[loaderIdx] = new Rect[assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches.Count][][];
			//
			//         for (int loaderCacheIdx = 0; loaderCacheIdx < assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches.Count; loaderCacheIdx++)
			//         {
			//             _loadersSectionCachingBgRects[loaderIdx][loaderCacheIdx] =
			//                 GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_Loader_CacheTitle_Height);
			//
			//             _loadersSectionCachingBgRects[loaderIdx][loaderCacheIdx].x += _settings.PlayMode_Loader_Togle_Title_Offsets.x;
			//             _loadersSectionCachingBgRects[loaderIdx][loaderCacheIdx].y += _settings.PlayMode_Loader_Togle_Title_Offsets.y;
			//             _loadersSectionCachingBgRects[loaderIdx][loaderCacheIdx].width -= _settings.PlayMode_Loader_Togle_Title_Offsets.x * 2;
			//
			//             if (loaderCacheIdx == 0)
			//             {
			//                 _loadersSectionAlLcachingBgRects[loaderIdx] = _loadersSectionCachingBgRects[loaderIdx][loaderCacheIdx];
			//             }
			//
			//             var cacheFilesInfos = assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches[loaderCacheIdx].GetCachedObjectsDebugInfo();
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx] = new Rect[cacheFilesInfos.Length + 1][];
			//
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0] = new Rect[3];
			//             var row = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_Loader_CacheCol_Height);
			//             row.x += _settings.PlayMode_Loader_Togle_Title_Offsets.x + _settings.PlayMode_Loader_CacheElement_ColumsPositionCorrection.x;
			//
			//             row.width = row.width - (_settings.PlayMode_Loader_Togle_Title_Offsets.x * 2 +
			//                                      _settings.PlayMode_Loader_CacheElement_ColumsPositionCorrection.x * 2);
			//
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][0] = row;
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][0].width =
			//                 row.width * _settings.PlayMode_Loader_CacheElement_ColumsPercent[0];
			//
			//
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][1] = row;
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][1].x +=
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][0].width;
			//
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][1].width =
			//                 row.width * _settings.PlayMode_Loader_CacheElement_ColumsPercent[1];
			//
			//
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][2] = row;
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][2].x +=
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][0].width +
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][1].width;
			//
			//             _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][2].width =
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][2].width -
			//                 (_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][0].width +
			//                  _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][1].width);
			//
			//             for (int cacheFileIdx = 0; cacheFileIdx < cacheFilesInfos.Length; cacheFileIdx++)
			//             {
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1] = new Rect[3];
			//                 row = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_Loader_CacheElement_Height *
			//                                                                  (cacheFilesInfos[cacheFileIdx].owners.Count == 0
			//                                                                      ? 1
			//                                                                      : cacheFilesInfos[cacheFileIdx].owners.Count));
			//
			//                 row.x += _settings.PlayMode_Loader_Togle_Title_Offsets.x + _settings.PlayMode_Loader_CacheElement_ColumsPositionCorrection.x;
			//
			//                 row.width = row.width - (_settings.PlayMode_Loader_Togle_Title_Offsets.x * 2 +
			//                                          _settings.PlayMode_Loader_CacheElement_ColumsPositionCorrection.x * 2);
			//
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][0] = row;
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][0].width =
			//                     row.width * _settings.PlayMode_Loader_CacheElement_ColumsPercent[0];
			//
			//
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][1] = row;
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][1].x +=
			//                     _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][0].width;
			//
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][1].width =
			//                     row.width * _settings.PlayMode_Loader_CacheElement_ColumsPercent[1];
			//
			//
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][2] = row;
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][2].x +=
			//                     _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][0].width +
			//                     _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][1].width;
			//
			//                 _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][2].width =
			//                     _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][2].width -
			//                     (_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][0].width +
			//                      _loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][1].width);
			//             }
			//
			//
			//             if (loaderCacheIdx == assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches.Count - 1)
			//             {
			//                 GUILayoutUtility.GetRect(windowSize.width, 5);
			//
			//                 _loadersSectionAlLcachingBgRects[loaderIdx].height =
			//                     (GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height) - _loadersSectionAlLcachingBgRects[loaderIdx].y;
			//
			//                 _loadersSectionAlLcachingBgRects[loaderIdx].x = _loadersSectionCachingBgRects[loaderIdx][loaderCacheIdx].x;
			//                 _loadersSectionAlLcachingBgRects[loaderIdx].width = _loadersSectionLoaderTitleBgRects[loaderIdx].width;
			//             }
			//         }
			//     }
			//
			//     if (assetsStorageInstance.DebugAllLoaders.Count > 1 && loaderIdx == assetsStorageInstance.DebugAllLoaders.Count - 1)
			//     {
			//         var fromLastDelay = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_LoadersTogle_Buttom_Height);
			//         _loadersSectionAllBgRect.height = fromLastDelay.y + fromLastDelay.height;
			//     }
			// }
			//
			//
			// GUILayoutUtility.GetRect(windowSize.width, 40);
		}

		private void LoadersDraw( AssetsSystemDebugInfo assetsStorageInstance )
		{
			// GUI.Label(_loadersSectionAllBgRect, string.Empty, _settings.PlayMode_LoadersTogleButtom_Style);
			//
			// for (int loaderIdx = 0; loaderIdx < assetsStorageInstance.DebugAllLoaders.Count; loaderIdx++)
			// {
			//     GUI.Label(_loadersSectionLoaderTitleBgRects[loaderIdx], string.Empty, _settings.PlayMode_Loader_TogleBg_Style);
			//
			//     var newState = GUI.Toggle(_loadersSectionLoaderTitleFgRects[loaderIdx], _loaderInfoFoldouts[loaderIdx], String.Format(
			//         _settings.PlayMode_Loader_Togle_TitleTemplate,
			//         assetsStorageInstance.DebugAllLoaders[loaderIdx].GetType().ToString().Replace(NamespacePrefix, ""),
			//         assetsStorageInstance.DebugAllLoaders[loaderIdx].SupportsMask), _settings.PlayMode_Loader_Togle_Style);
			//
			//     if (_loaderInfoFoldouts[loaderIdx] != newState)
			//     {
			//         _loaderInfoFoldouts[loaderIdx] = newState;
			//         return;
			//     }
			//
			//     if (_loaderInfoFoldouts[loaderIdx])
			//     {
			//         GUI.Label(_loadersSectionAlLcachingBgRects[loaderIdx], "", _settings.PlayMode_Loader_Buttom_BG_Style);
			//
			//
			//         for (int loaderCacheIdx = 0; loaderCacheIdx < assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches.Count; loaderCacheIdx++)
			//         {
			//             GUI.Label(_loadersSectionCachingBgRects[loaderIdx][loaderCacheIdx],
			//                 string.Format(_settings.PlayMode_Loader_CacheTitle_Name,
			//                     assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches[loaderCacheIdx].GetType()), _settings.PlayMode_Loader_CacheTitle_Style);
			//
			//
			//             EditorGUI.DrawRect(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][0],
			//                 _settings.PlayMode_Loader_CacheCol_Style.hover.textColor);
			//             GUI.Label(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][0], _settings.PlayMode_Loader_CacheColumnsNames[0],
			//                 _settings.PlayMode_Loader_CacheCol_Style);
			//
			//             EditorGUI.DrawRect(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][1],
			//                 _settings.PlayMode_Loader_CacheCol_Style.hover.textColor);
			//             GUI.Label(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][1], _settings.PlayMode_Loader_CacheColumnsNames[1],
			//                 _settings.PlayMode_Loader_CacheCol_Style);
			//
			//             EditorGUI.DrawRect(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][2],
			//                 _settings.PlayMode_Loader_CacheCol_Style.hover.textColor);
			//             GUI.Label(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][0][2], _settings.PlayMode_Loader_CacheColumnsNames[2],
			//                 _settings.PlayMode_Loader_CacheCol_Style);
			//
			//
			//             var cacheFilesInfos = assetsStorageInstance.DebugAllLoaders[loaderIdx].DebugCaches[loaderCacheIdx].GetCachedObjectsDebugInfo();
			//             for (int cacheFileIdx = 0; cacheFileIdx < cacheFilesInfos.Length; cacheFileIdx++)
			//             {
			//                 GUIStyle usedStyle = cacheFileIdx % 2 == 0 ? _settings.PlayMode_Loader_CacheElement_Style : _settings.PlayMode_Loader_CacheElement_Style2;
			//                 EditorGUI.DrawRect(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][0],
			//                     usedStyle.hover.textColor);
			//                 EditorGUI.DrawRect(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][1],
			//                     usedStyle.hover.textColor);
			//                 EditorGUI.DrawRect(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][2],
			//                     usedStyle.hover.textColor);
			//
			//                 GUI.Label(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][0], cacheFilesInfos[cacheFileIdx].key,
			//                     usedStyle);
			//                 GUI.Label(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][1],
			//                     cacheFilesInfos[cacheFileIdx].assetType,
			//                     usedStyle);
			//
			//
			//                 var ownersFormated = cacheFilesInfos[cacheFileIdx].owners.Count == 0 ? "------" : string.Empty;
			//                 for (int i = 0; i < cacheFilesInfos[cacheFileIdx].owners.Count; i++)
			//                 {
			//                     ownersFormated += cacheFilesInfos[cacheFileIdx].owners[i] + Environment.NewLine;
			//                 }
			//
			//                 GUI.Label(_loadersSectionCachingAssetTableRects[loaderIdx][loaderCacheIdx][cacheFileIdx + 1][2], ownersFormated, usedStyle);
			//             }
			//         }
			//     }
			// }
		}


		private Rect _queueSectionAllBgRect;
		private Rect[ ] _queueSectionLoaderTitleBgRects;
		private Rect[ ] _queueSectionLoaderTitleFgRects;
		private Rect[ /* row */ ][ /* col */ ] _queueSectionWaitingTableRects;
		private Rect[ /* row */ ][ /* col */ ] _queueSectionExecutionTableRects;

		private void QueueLayoutCalculation( AssetsSystemDebugInfo assetsStorageInstance, Rect windowSize )
		{
			// _queueSectionAllBgRect = new Rect();
			// _queueSectionLoaderTitleBgRects = new Rect[2];
			// _queueSectionLoaderTitleFgRects = new Rect[2];
			//
			// //Waiting queue
			//
			// var h1 = _settings.PlayMode_Queue_Togle_Title_Offsets.height + _settings.PlayMode_Queue_Togle_Title_Offsets.y;
			// _queueSectionLoaderTitleBgRects[0] = GUILayoutUtility.GetRect(windowSize.width, h1);
			//
			// _queueSectionAllBgRect = _queueSectionLoaderTitleBgRects[0];
			//
			// _queueSectionLoaderTitleBgRects[0].x += _settings.PlayMode_Queue_Togle_Title_Offsets.x;
			// _queueSectionLoaderTitleBgRects[0].y += _settings.PlayMode_Queue_Togle_Title_Offsets.y;
			// _queueSectionLoaderTitleBgRects[0].width -= _settings.PlayMode_Queue_Togle_Title_Offsets.x * 2;
			//
			// _queueSectionLoaderTitleFgRects[0].x =
			//     _queueSectionLoaderTitleBgRects[0].x + _settings.PlayMode_Queue_TogleBg_Style.contentOffset.x;
			//
			// _queueSectionLoaderTitleFgRects[0].width =
			//     _queueSectionLoaderTitleBgRects[0].width - _settings.PlayMode_Queue_TogleBg_Style.contentOffset.x;
			//
			// _queueSectionLoaderTitleFgRects[0].y =
			//     _queueSectionLoaderTitleBgRects[0].y + _settings.PlayMode_Queue_TogleBg_Style.contentOffset.y;
			// _queueSectionLoaderTitleFgRects[0].height = _queueSectionLoaderTitleBgRects[0].height -
			//                                              _settings.PlayMode_Queue_TogleBg_Style.contentOffset.y +
			//                                              _settings.PlayMode_Queue_TogleBg_Style.border.bottom;
			//
			// int countQueueColumns = _settings.PlayMode_Queue_CacheElement_ColumsPercent.Length + 1;
			// if (_waitingQueueInfoDetailsFoldout)
			// {
			//     _queueSectionWaitingTableRects = new Rect[assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState.Count + 1][];
			//
			//     _queueSectionWaitingTableRects[0] = new Rect[countQueueColumns];
			//
			//     var row = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_Queue_CacheCol_Height);
			//     row.x += _settings.PlayMode_Queue_Togle_Title_Offsets.x + _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x;
			//
			//     row.width = row.width - (_settings.PlayMode_Queue_Togle_Title_Offsets.x * 2 +
			//                              _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x * 2);
			//
			//     float xOffset = 0;
			//     for (int i = 0; i < countQueueColumns; i++)
			//     {
			//         _queueSectionWaitingTableRects[0][i] = row;
			//         _queueSectionWaitingTableRects[0][i].x = row.x + xOffset;
			//
			//         if (i == countQueueColumns - 1)
			//         {
			//             _queueSectionWaitingTableRects[0][i].width = row.width - xOffset;
			//             continue;
			//         }
			//
			//         _queueSectionWaitingTableRects[0][i].width = row.width * _settings.PlayMode_Queue_CacheElement_ColumsPercent[i];
			//         xOffset += _queueSectionWaitingTableRects[0][i].width;
			//     }
			//
			//
			//     for (int workerId = 0; workerId < assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState.Count; workerId++)
			//     {
			//         _queueSectionWaitingTableRects[workerId + 1] = new Rect[countQueueColumns];
			//
			//         float calculatedHeight = assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations == null ||
			//                                  assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations.Count == 0
			//             ? _settings.PlayMode_Queue_CacheElement_Height
			//             : _settings.PlayMode_Queue_CacheElement_Height *
			//               assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations.Count;
			//
			//         row = GUILayoutUtility.GetRect(windowSize.width, calculatedHeight);
			//         row.x += _settings.PlayMode_Queue_Togle_Title_Offsets.x + _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x;
			//
			//         row.width = row.width - (_settings.PlayMode_Queue_Togle_Title_Offsets.x * 2 +
			//                                  _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x * 2);
			//         xOffset = 0;
			//         for (int i = 0; i < countQueueColumns; i++)
			//         {
			//             _queueSectionWaitingTableRects[workerId + 1][i] = row;
			//             _queueSectionWaitingTableRects[workerId + 1][i].x = row.x + xOffset;
			//
			//             if (i == countQueueColumns - 1)
			//             {
			//                 _queueSectionWaitingTableRects[workerId + 1][i].width = row.width - xOffset;
			//                 continue;
			//             }
			//
			//             _queueSectionWaitingTableRects[workerId + 1][i].width = row.width * _settings.PlayMode_Queue_CacheElement_ColumsPercent[i];
			//             xOffset += _queueSectionWaitingTableRects[workerId + 1][i].width;
			//         }
			//     }
			// }
			//
			//
			// //Execution queue
			//
			// h1 = _settings.PlayMode_Queue_Togle_Title_Offsets.height + _settings.PlayMode_Queue_Togle_Title_Offsets.y;
			// _queueSectionLoaderTitleBgRects[1] = GUILayoutUtility.GetRect(windowSize.width, h1);
			//
			// _queueSectionLoaderTitleBgRects[1].x += _settings.PlayMode_Queue_Togle_Title_Offsets.x;
			// _queueSectionLoaderTitleBgRects[1].y += _settings.PlayMode_Queue_Togle_Title_Offsets.y;
			// _queueSectionLoaderTitleBgRects[1].width -= _settings.PlayMode_Queue_Togle_Title_Offsets.x * 2;
			//
			//
			// _queueSectionLoaderTitleFgRects[1].x =
			//     _queueSectionLoaderTitleBgRects[1].x + _settings.PlayMode_Queue_TogleBg_Style.contentOffset.x;
			// _queueSectionLoaderTitleFgRects[1].width =
			//     _queueSectionLoaderTitleBgRects[1].width - _settings.PlayMode_Queue_TogleBg_Style.contentOffset.x;
			//
			// _queueSectionLoaderTitleFgRects[1].y =
			//     _queueSectionLoaderTitleBgRects[1].y + _settings.PlayMode_Queue_TogleBg_Style.contentOffset.y;
			// _queueSectionLoaderTitleFgRects[1].height = _queueSectionLoaderTitleBgRects[1].height -
			//                                              _settings.PlayMode_Queue_TogleBg_Style.contentOffset.y +
			//                                              _settings.PlayMode_Queue_TogleBg_Style.border.bottom;
			//
			// if (_executingQueueInfoDetailsFoldout)
			// {
			//     _queueSectionExecutionTableRects = new Rect[assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState.Count + 1][];
			//
			//     _queueSectionExecutionTableRects[0] = new Rect[countQueueColumns];
			//
			//     var row = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_Queue_CacheCol_Height);
			//     row.x += _settings.PlayMode_Queue_Togle_Title_Offsets.x + _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x;
			//
			//     row.width = row.width - (_settings.PlayMode_Queue_Togle_Title_Offsets.x * 2 +
			//                              _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x * 2);
			//     
			//     
			//     float xOffset = 0;
			//     for (int i = 0; i < countQueueColumns; i++)
			//     {
			//         _queueSectionExecutionTableRects[0][i] = row;
			//         _queueSectionExecutionTableRects[0][i].x = row.x + xOffset;
			//
			//         if (i == countQueueColumns - 1)
			//         {
			//             _queueSectionExecutionTableRects[0][i].width = row.width - xOffset;
			//             continue;
			//         }
			//
			//         _queueSectionExecutionTableRects[0][i].width = row.width * _settings.PlayMode_Queue_CacheElement_ColumsPercent[i];
			//         xOffset += _queueSectionExecutionTableRects[0][i].width;
			//     }
			//
			//     for (int workerId = 0; workerId < assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState.Count; workerId++)
			//     {
			//         _queueSectionExecutionTableRects[workerId + 1] = new Rect[countQueueColumns];
			//
			//         float calculatedHeight = assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations == null ||
			//                                  assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations.Count == 0
			//             ? _settings.PlayMode_Queue_CacheElement_Height
			//             : _settings.PlayMode_Queue_CacheElement_Height *
			//               assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations.Count;
			//         
			//         row = GUILayoutUtility.GetRect(windowSize.width, calculatedHeight);
			//         row.x += _settings.PlayMode_Queue_Togle_Title_Offsets.x + _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x;
			//
			//         row.width = row.width - (_settings.PlayMode_Queue_Togle_Title_Offsets.x * 2 +
			//                                  _settings.PlayMode_Queue_CacheElement_ColumsPositionCorrection.x * 2);
			//
			//         xOffset = 0;
			//         for (int i = 0; i < countQueueColumns; i++)
			//         {
			//             _queueSectionExecutionTableRects[workerId + 1][i] = row;
			//             _queueSectionExecutionTableRects[workerId + 1][i].x = row.x + xOffset;
			//
			//             if (i == countQueueColumns - 1)
			//             {
			//                 _queueSectionExecutionTableRects[workerId + 1][i].width = row.width - xOffset;
			//                 continue;
			//             }
			//
			//             _queueSectionExecutionTableRects[workerId + 1][i].width = row.width * _settings.PlayMode_Queue_CacheElement_ColumsPercent[i];
			//             xOffset += _queueSectionExecutionTableRects[workerId + 1][i].width;
			//         }
			//     }
			// }
			//
			//
			// var fromLastDelay = GUILayoutUtility.GetRect(windowSize.width, _settings.PlayMode_Queue_CacheTitle_Height);
			// _queueSectionAllBgRect.height = fromLastDelay.y + fromLastDelay.height - _queueSectionAllBgRect.y;
		}

		private void QueueDraw( AssetsSystemDebugInfo assetsStorageInstance )
		{
			// GUI.Label(_queueSectionAllBgRect, string.Empty, _settings.PlayMode_QueueTogleButtom_Style);
			//
			// int countQueueColumns = _settings.PlayMode_Queue_CacheElement_ColumsPercent.Length + 1;
			//
			// //Waiting
			//
			// GUI.Label(_queueSectionLoaderTitleBgRects[0], string.Empty, _settings.PlayMode_Queue_TogleBg_Style);
			//
			// var newState = GUI.Toggle(_queueSectionLoaderTitleFgRects[0], _waitingQueueInfoDetailsFoldout,
			//     String.Format(_settings.PlayMode_Queue_Togle_TitleTemplate_Waiting, assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState.Count),
			//     _settings.PlayMode_Queue_Togle_Style);
			//
			// if (_waitingQueueInfoDetailsFoldout != newState)
			// {
			//     _waitingQueueInfoDetailsFoldout = newState;
			//     return;
			// }
			//
			// if (_waitingQueueInfoDetailsFoldout)
			// {
			//     for (int i = 0; i < countQueueColumns; i++)
			//     {
			//         EditorGUI.DrawRect(_queueSectionWaitingTableRects[0][i], _settings.PlayMode_Queue_CacheCol_Style.hover.textColor);
			//         GUI.Label(_queueSectionWaitingTableRects[0][i], _settings.PlayMode_Queue_CacheColumnsNames[i], _settings.PlayMode_Queue_CacheCol_Style);
			//     }
			//
			//     for (int workerId = 0; workerId < assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState.Count; workerId++)
			//     {
			//         GUIStyle usedStyle = workerId % 2 == 0 ? _settings.PlayMode_Queue_CacheElement_Style : _settings.PlayMode_Queue_CacheElement_Style2;
			//
			//         for (int i = 0; i < countQueueColumns; i++)
			//         {
			//             EditorGUI.DrawRect(_queueSectionWaitingTableRects[workerId + 1][i], usedStyle.hover.textColor);
			//         }
			//
			//         GUI.Label(_queueSectionWaitingTableRects[workerId + 1][0], assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].Uri,
			//             usedStyle);
			//         GUI.Label(_queueSectionWaitingTableRects[workerId + 1][1],
			//             assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].IsWaitDependentAsset.ToString(), usedStyle);
			//
			//         if (assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations != null &&
			//             assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations.Count > 0)
			//         {
			//             GUI.Label(_queueSectionWaitingTableRects[workerId + 1][2],
			//                 assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations[0].Progress.ToString(),
			//                 usedStyle);
			//         }
			//
			//         string ownersLable = string.Empty;
			//         if (assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations != null &&
			//             assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations.Count > 0)
			//         {
			//             foreach (var loadingOperation in assetsStorageInstance.DebugWorkersQueue.DebugWorkersInWaitingState[workerId].LoadingOperations)
			//             {
			//                 ownersLable += loadingOperation.MetaData.Owner.ToString() + Environment.NewLine;
			//             }
			//         }
			//         else
			//         {
			//             ownersLable = "Null";
			//         }
			//
			//         GUI.Label(_queueSectionWaitingTableRects[workerId + 1][3], ownersLable, usedStyle);
			//     }
			// }
			//
			//
			// //Execution
			//
			// GUI.Label(_queueSectionLoaderTitleBgRects[1], string.Empty, _settings.PlayMode_Queue_TogleBg_Style);
			//
			// newState = GUI.Toggle(_queueSectionLoaderTitleFgRects[1], _executingQueueInfoDetailsFoldout,
			//     String.Format(_settings.PlayMode_Queue_Togle_TitleTemplate_Execution, assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState.Count),
			//     _settings.PlayMode_Queue_Togle_Style);
			//
			// if (_executingQueueInfoDetailsFoldout != newState)
			// {
			//     _executingQueueInfoDetailsFoldout = newState;
			//     return;
			// }
			//
			// if (_executingQueueInfoDetailsFoldout)
			// {
			//     for (int i = 0; i < countQueueColumns; i++)
			//     {
			//         EditorGUI.DrawRect(_queueSectionExecutionTableRects[0][i], _settings.PlayMode_Queue_CacheCol_Style.hover.textColor);
			//         GUI.Label(_queueSectionExecutionTableRects[0][i], _settings.PlayMode_Queue_CacheColumnsNames[i], _settings.PlayMode_Queue_CacheCol_Style);
			//     }
			//
			//
			//     for (int workerId = 0; workerId < assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState.Count; workerId++)
			//     {
			//         GUIStyle usedStyle = workerId % 2 == 0 ? _settings.PlayMode_Queue_CacheElement_Style : _settings.PlayMode_Queue_CacheElement_Style2;
			//
			//         for (int i = 0; i < countQueueColumns; i++)
			//         {
			//             EditorGUI.DrawRect(_queueSectionExecutionTableRects[workerId + 1][i], usedStyle.hover.textColor);
			//         }
			//
			//         GUI.Label(_queueSectionExecutionTableRects[workerId + 1][0], assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].Uri,
			//             usedStyle);
			//         GUI.Label(_queueSectionExecutionTableRects[workerId + 1][1],
			//             assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].IsWaitDependentAsset.ToString(),
			//             usedStyle);
			//
			//         if (assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations != null &&
			//             assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations.Count > 0)
			//         {
			//             GUI.Label(_queueSectionExecutionTableRects[workerId + 1][2],
			//                 assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations[0].Progress.ToString(),
			//                 usedStyle);
			//         }
			//
			//         string ownersLable = string.Empty;
			//         if (assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations != null &&
			//             assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations.Count > 0)
			//         {
			//             foreach (var loadingOperation in assetsStorageInstance.DebugWorkersQueue.DebugWorkersInProcessState[workerId].LoadingOperations)
			//             {
			//                 ownersLable += loadingOperation.MetaData.Owner.ToString() + Environment.NewLine;
			//             }
			//         }
			//         else
			//         {
			//             ownersLable = "Null";
			//         }
			//
			//         GUI.Label(_queueSectionExecutionTableRects[workerId + 1][3], ownersLable, usedStyle);
			//     }
			// }
		}
	}
}
