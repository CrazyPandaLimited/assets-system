#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_EXAMPLES
using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem;
using UnityEngine;

public class LoadResourceFromUrlExample : MonoBehaviour
{
    private string loadingUrlForText;
    private string loadingUrlForImage;
    private string loadedText;
    private Texture2D loadedImage;

    private ResourceStorage resourceStorage;

    private void Awake()
    {
        GameObject timeProvidersGo = new GameObject("-TimeProvider");
        SimpleTimeProvider mainCoroutineTimeProvider = timeProvidersGo.AddComponent<SimpleTimeProvider>();

        CoroutineManager coroutineManager = new CoroutineManager();
        coroutineManager.TimeProvider = mainCoroutineTimeProvider;

        resourceStorage = new ResourceStorage(100);
        WebRequestLoader webLoader = new WebRequestLoader(coroutineManager);
        webLoader.RegisterResourceCreator(new TextureDataCreator());
        webLoader.RegisterResourceCreator(new StringDataCreator());
        resourceStorage.RegisterResourceLoader(webLoader);
    }
    
    private void OnGUI()
    {
        loadingUrlForText = GUILayout.TextField(loadingUrlForText);

        if (GUILayout.Button("Start loading text"))
        {
            resourceStorage.LoadResource<string>(this, loadingUrlForText, (a) => { loadedText = a.Resource; }, null);
        }

        GUILayout.Space(30);

        loadingUrlForImage = GUILayout.TextField(loadingUrlForImage);
        if (GUILayout.Button("Start loading image"))
        {
            resourceStorage.LoadResource<Texture2D>(this, loadingUrlForImage, (a) => { loadedImage = a.Resource; }, null);
        }

        GUILayout.Label("Loaded text: " + loadedText);
        GUILayout.Label(loadedImage);
    }
}

public class SimpleTimeProvider : MonoBehaviour, ITimeProvider
{
    #region Properties

    public float deltaTime
    {
        get { return Time.deltaTime; }
    }

    #endregion

    #region Events

    public event Action<object, Exception> OnError;
    public event Action OnUpdate;

    #endregion

    #region Public Members

    public void Update()
    {
        try
        {
            if (OnUpdate != null)
            {
                OnUpdate();
            }
        }
        catch (Exception exception)
        {
            if (OnError != null)
            {
                OnError.Invoke(this, exception);
            }
            else
            {
                Debug.LogException(exception);
            }
        }
    }

    #endregion
}
#endif