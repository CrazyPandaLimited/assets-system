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
using UnityEngine.UI;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;

public class LoadVersionedResourceFromUrlWithManifestExample : MonoBehaviour
{
    private CoroutineManager coroutineManager;
    private ResourceStorage resourceStorage;

    private string serverUrl;
    private string imageName = "f1/bender";


    private string manifestName = "VersionedFilesManifestExample";
    private string manifestPath;
    private string loaderPrefix;

    [SerializeField] private string discCacheFolderPath = "/cachedVersionedFiles";
    [SerializeField] private RawImage _image = null;
    [SerializeField] private Button _loadImageVersion1 = null;
    [SerializeField] private Button _loadImageVersion2 = null;

    private void Awake()
    {
        discCacheFolderPath = Application.dataPath + discCacheFolderPath;
        GameObject timeProvidersGo = new GameObject("-TimeProvider");
        SimpleTimeProvider mainCoroutineTimeProvider = timeProvidersGo.AddComponent<SimpleTimeProvider>();

        coroutineManager = new CoroutineManager();
        coroutineManager.TimeProvider = mainCoroutineTimeProvider;

#if UNITY_EDITOR

        foreach (var findAsset in AssetDatabase.FindAssets(manifestName))
        {
            if (AssetDatabase.GUIDToAssetPath(findAsset).Contains(".json") && AssetDatabase.GUIDToAssetPath(findAsset).Contains("/Sources/"))
            {
                serverUrl = Application.dataPath.Replace("/Assets", "/") + AssetDatabase.GUIDToAssetPath(findAsset).Replace(manifestName + ".json", "");
                break;
            }
        }

        discCacheFolderPath = serverUrl.Replace("/Sources/", "/FileCacheForExample");


        foreach (var findAsset in AssetDatabase.FindAssets(manifestName))
        {
            if (AssetDatabase.GUIDToAssetPath(findAsset).Contains(".json"))
            {
                manifestPath = AssetDatabase.GUIDToAssetPath(findAsset);
                break;
            }
        }
#endif
        resourceStorage = new ResourceStorage(100);

        var textureCreator = new TextureDataCreator();

        var versionedResourceLoader = new VersionedDiskCachedResourceLoader(discCacheFolderPath, "file://" + serverUrl, coroutineManager);
        versionedResourceLoader.RegisterResourceCreator(textureCreator);
        resourceStorage.RegisterResourceLoader(versionedResourceLoader);

        loaderPrefix = versionedResourceLoader.SupportsMask;
        TextAsset manifest = null;
#if UNITY_EDITOR
        manifest = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestPath);

#endif

        var jsonSerializer =
            new JsonSerializer(new JsonSerializerSettings {Formatting = Formatting.Indented}, Encoding.UTF8);
        var manifestAsJson = jsonSerializer.DeserializeString<AssetFilesManifest>(manifest.text);

        versionedResourceLoader.Manifest.AddManifestPart(manifestAsJson);

        _loadImageVersion1.onClick.AddListener(() => { coroutineManager.StartCoroutine(this, LoadImageVer1Process()); });

        _loadImageVersion2.onClick.AddListener(() => { coroutineManager.StartCoroutine(this, LoadImageVer2Process()); });
    }

    private IEnumerator LoadImageVer1Process()
    {
        var loadImageProcess = resourceStorage.LoadResource<Texture2D>(this, loaderPrefix + "://" + imageName);
        yield return loadImageProcess;
        _image.texture = loadImageProcess.Resource;
        _image.SetNativeSize();
    }

    private IEnumerator LoadImageVer2Process()
    {
        var fileInfoFromManifest = resourceStorage.GetResourceLoader<VersionedDiskCachedResourceLoader>().Manifest.GetAssetByName(imageName);
        fileInfoFromManifest.version = 1;

        var loadImageProcess = resourceStorage.LoadResource<Texture2D>(this, loaderPrefix + "://" + imageName);
        yield return loadImageProcess;
        _image.texture = loadImageProcess.Resource;
        _image.SetNativeSize();
    }
}
#endif