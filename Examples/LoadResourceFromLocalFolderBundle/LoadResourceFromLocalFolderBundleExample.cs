#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_EXAMPLES
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;
using AssetBundleManifest = CrazyPanda.UnityCore.ResourcesSystem.AssetBundleManifest;

public class LoadResourceFromLocalFolderBundleExample : MonoBehaviour
{

    [SerializeField] private string serverStaticPath;
    [SerializeField] private string manifestName;
    [SerializeField] private string prefabName1;
    [SerializeField] private string prefabName2;
    [SerializeField] private string prefabName3;

    [SerializeField] private string bundleName;
    
    private CoroutineManager coroutineManager;
    private ResourceStorage resourceStorage;

    private PrefabsResourceHolder _prefabsResourceHolder;

    private List<GameObject> goInstances;

    private void Awake()
    {
        GameObject timeProvidersGo = new GameObject("-TimeProvider");
        SimpleTimeProvider mainCoroutineTimeProvider = timeProvidersGo.AddComponent<SimpleTimeProvider>();
        
        goInstances = new List<GameObject>();

        coroutineManager = new CoroutineManager();
        coroutineManager.TimeProvider = mainCoroutineTimeProvider;
        coroutineManager.StartCoroutine(this, InitProcess());
    }


    public void OnPressCreatePrefab1()
    {
        coroutineManager.StartCoroutine(this, CreatePrefabProcess(prefabName1));
    }
    
    public void OnPressCreatePrefab2()
    {
        coroutineManager.StartCoroutine(this, CreatePrefabProcess(prefabName2));
    }
    
    public void OnPressCreatePrefab3()
    {
        coroutineManager.StartCoroutine(this, CreatePrefabProcess(prefabName3));
    }
    
    public void OnPressDeletePrefab1()
    {
        DeleteInstance(prefabName1);
    }
    
    public void OnPressDeletePrefab2()
    {
        DeleteInstance(prefabName2);
    }
    
    public void OnPressDeletePrefab3()
    {
        DeleteInstance(prefabName3);
    }
    

    public void OnPressReleasePrefabs()
    {
        _prefabsResourceHolder = null;
        //resourceStorage.ReleaseAllOwnerResourcesFromCache(_prefabsResourceHolder);
    }
    
    public void OnPressUnloadUnusedRS()
    {
        resourceStorage.UnloadUnusedResources(true);
    }
    
    public void OnPressUnloadUnusedUnity3d()
    {
        Resources.UnloadUnusedAssets();
    }
    
    public void OnPressUnloadBundle()
    {
        resourceStorage.ForceReleaseFromCache(bundleName, true);
    }

    public void OnPressPrintLoadedBundles()
    {
        var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();

        Debug.Log("LoadedBundles:");
        foreach (var loadedBundle in loadedBundles)
        {
            Debug.Log("Name:" + loadedBundle.name);
        }
        Debug.Log("--------------------");
    }


    private IEnumerator InitProcess()
    {
        Debug.Log("Start Init process");

        resourceStorage = new ResourceStorage(100);

        var manifestLoader = new WebRequestLoader(coroutineManager);
        manifestLoader.RegisterResourceCreator(new StringDataCreator());
        
        UnityResourceFromBundleLoader resFromBundlesLoader = new UnityResourceFromBundleLoader(coroutineManager);
        WebRequestBundlesLoader bundlesLoader = new WebRequestBundlesLoader(serverStaticPath, coroutineManager);

        resourceStorage.RegisterResourceLoader(manifestLoader);
        resourceStorage.RegisterResourceLoader(resFromBundlesLoader);
        resourceStorage.RegisterResourceLoader(bundlesLoader);


        //Load Manifest
        ManifestResourceHolder manifestHolder = new ManifestResourceHolder();
        var manifestLoadingOperation = resourceStorage.LoadResource<string>(manifestHolder, Path.Combine(serverStaticPath, manifestName));

        yield return manifestLoadingOperation;
        
        Debug.Log("Manifest loaded: " + manifestLoadingOperation.Resource);
        
        var jsonSerializer =
            new JsonSerializer(new JsonSerializerSettings {Formatting = Formatting.Indented}, Encoding.UTF8);
        var manifestAsJson = jsonSerializer.DeserializeString<AssetBundleManifest>(manifestLoadingOperation.Resource);
        bundlesLoader.Manifest.AddManifestPart(manifestAsJson);
        
        resourceStorage.ReleaseAllOwnerResourcesFromCache(manifestHolder);
        
        _prefabsResourceHolder = new PrefabsResourceHolder();
        Debug.Log("Init process pass");

    }


    private IEnumerator CreatePrefabProcess(string prefabName)
    {
        Debug.Log("Start loading process for " +prefabName);

        if (_prefabsResourceHolder == null)
        {
            _prefabsResourceHolder = new PrefabsResourceHolder();
        }
        
        var prefabLoadingOperation = resourceStorage.LoadResource<GameObject>(_prefabsResourceHolder, prefabName);

        yield return prefabLoadingOperation;
        
        Debug.Log("Done loading process for " + prefabName + " isResourceExist:{prefabLoadingOperation.Resource != null}");

        if (prefabLoadingOperation.Resource == null)
        {
           // OnPressUnloadBundle();
            yield break;
        }

        var newInstance = Instantiate(prefabLoadingOperation.Resource);
        newInstance.name = prefabName;
        goInstances.Add(newInstance);
    }

    private void DeleteInstance(string prefabName)
    {
        List<GameObject> toDel = new List<GameObject>();
        foreach (var goInstance in goInstances)
        {
            if (goInstance.name == prefabName)
            {
                toDel.Add(goInstance);
            }
        }

        foreach (var d in toDel)
        {
            goInstances.Remove(d);
            Destroy(d);
        }
    }
}


public class ManifestResourceHolder
{
    
}

public class PrefabsResourceHolder
{
    
}
#endif