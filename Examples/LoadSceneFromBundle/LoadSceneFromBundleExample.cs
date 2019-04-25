#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_EXAMPLES
using System.Collections;
using System.Text;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;
using AssetBundleManifest = CrazyPanda.UnityCore.ResourcesSystem.AssetBundleManifest;

public class LoadSceneFromBundleExample : MonoBehaviour
{
    private ResourceStorage resourceStorage;
    private CoroutineManager coroutineManager;

    private string manifestURL = "http://server_static.com/Bundles manifest name.json";
    private string bundlesFolderPath = "Bundles location";
    private string bundleName = "LocalFolderBundle://MainScene";

    private void Awake()
    {
        GameObject timeProvidersGo = new GameObject("-TimeProvider");
        SimpleTimeProvider mainCoroutineTimeProvider = timeProvidersGo.AddComponent<SimpleTimeProvider>();

        coroutineManager = new CoroutineManager();
        coroutineManager.TimeProvider = mainCoroutineTimeProvider;
        
        resourceStorage = new ResourceStorage(100);

        //Loader for load any file from www
        WebRequestLoader webLoader = new WebRequestLoader(coroutineManager);

        //Creator for cast loaded by www byte[] to string
        webLoader.RegisterResourceCreator(new StringDataCreator());


        //Loader to load bundle from disk
        LocalFolderBundlesLoader bundlesLoader = new LocalFolderBundlesLoader(bundlesFolderPath, coroutineManager);

        //Loader to load bundle from server static folder
        //WebRequestBundlesLoader bundlesLoader = new WebRequestBundlesLoader(bundlesFolderPath, coroutineManager);

        resourceStorage.RegisterResourceLoader(webLoader);
        resourceStorage.RegisterResourceLoader(bundlesLoader);
        
        coroutineManager.StartCoroutine(this, WorkProcess());
    }

    IEnumerator WorkProcess()
    {
        var manifestRequest = resourceStorage.LoadResource<string>(this, manifestURL);

        yield return manifestRequest;

        var jsonSerializer =
            new JsonSerializer(new JsonSerializerSettings {Formatting = Formatting.Indented}, Encoding.UTF8);
        var manifestAsJson = jsonSerializer.DeserializeString<AssetBundleManifest>(manifestRequest.Resource);

        //Set loaded bundles manifest
        resourceStorage.GetResourceLoader<LocalFolderBundlesLoader>().Manifest.AddManifestPart(manifestAsJson);
        //For server bundle loader
        //resourceStorage.GetResourceLoader<WebRequestBundlesLoader>().Manifest.AddManifestPart(manifestAsJson);

        var sceneBundleRequest = resourceStorage.LoadResource<AssetBundle>(this, bundleName);
        yield return sceneBundleRequest;

        if (sceneBundleRequest.Resource.isStreamedSceneAssetBundle)
        {
            string[] scenePaths = sceneBundleRequest.Resource.GetAllScenePaths();
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
            SceneManager.LoadScene(sceneName);
        }
    }
}
#endif