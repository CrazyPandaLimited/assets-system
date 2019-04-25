#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_EXAMPLES
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;
using AssetBundleManifest = CrazyPanda.UnityCore.ResourcesSystem.AssetBundleManifest;

public class LoadResourceFromRemoteBundle : MonoBehaviour
{
    private CoroutineManager coroutineManager;
    private ResourceStorage resourceStorage;

    private string manifestName = "CustomManifestAndroid";
    private string bundlesFolderPath = "bundlersexample_0";
    private string resourceName = "ResourceFromBundle://Boxes.prefab";

    private void Awake()
    {
        GameObject timeProvidersGo = new GameObject("-TimeProvider");
        SimpleTimeProvider mainCoroutineTimeProvider = timeProvidersGo.AddComponent<SimpleTimeProvider>();

        coroutineManager = new CoroutineManager();
        coroutineManager.TimeProvider = mainCoroutineTimeProvider;
        coroutineManager.StartCoroutine(this, WorkProcess());
    }

    private IEnumerator WorkProcess()
    {
        string manifestPath = String.Empty;
        string bundlesPath = String.Empty;
#if UNITY_EDITOR

        foreach (var findAsset in AssetDatabase.FindAssets(manifestName))
        {
            if (AssetDatabase.GUIDToAssetPath(findAsset).Contains(".json"))
            {
                manifestPath = AssetDatabase.GUIDToAssetPath(findAsset);
                break;
            }
        }

        foreach (var findAsset in AssetDatabase.FindAssets(bundlesFolderPath))
        {
            if (AssetDatabase.GUIDToAssetPath(findAsset).Contains(".u3d"))
            {
                bundlesPath = AssetDatabase.GUIDToAssetPath(findAsset).Replace(bundlesFolderPath + ".u3d", "");

                bundlesPath = Application.dataPath.Replace("/Assets", "/") + bundlesPath;
                break;
            }
        }

        resourceStorage = new ResourceStorage(100);

        UnityResourceFromBundleLoader resFromBundlesLoader = new UnityResourceFromBundleLoader(coroutineManager);
        LocalFolderBundlesLoader bundlesLoader = new LocalFolderBundlesLoader(bundlesPath, coroutineManager);

        resourceStorage.RegisterResourceLoader(resFromBundlesLoader);
        resourceStorage.RegisterResourceLoader(bundlesLoader);


        var manifest = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestPath);

        var jsonSerializer =
            new JsonSerializer(new JsonSerializerSettings {Formatting = Formatting.Indented}, Encoding.UTF8);
        var manifestAsJson = jsonSerializer.DeserializeString<AssetBundleManifest>(manifest.text);
        bundlesLoader.Manifest.AddManifestPart(manifestAsJson);
#endif

        var loadPrefab = resourceStorage.LoadResource<GameObject>(this, resourceName);
        yield return loadPrefab;

        Instantiate(loadPrefab.Resource);
        
        //Del me
        resourceStorage.ReleaseFromCache(this, resourceName);
        
        loadPrefab = resourceStorage.LoadResource<GameObject>(this, resourceName);
        yield return loadPrefab;
        Instantiate(loadPrefab.Resource);
        
    }
}
#endif