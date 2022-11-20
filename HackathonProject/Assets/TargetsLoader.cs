using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Vuforia;
using System;

public class TargetsLoader: MonoBehaviour
{
    public static TargetsLoader Instance;

    Texture2D imageFromWeb;

    bool isReady;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
    }

    void OnVuforiaInitialized(VuforiaInitError error)
    {
        if (error == VuforiaInitError.NONE)
            isReady = true;
    }

    //public void CreateTarget(string uri, string targetName, Action<ImageTargetBehaviour> OnLoadedAction)
    //{
    //    StartCoroutine(RetrieveTextureFromWeb(uri, targetName, OnLoadedAction));
    //}

   public IEnumerator RetrieveTextureFromWeb(string uri, string targetName, Action<ImageTargetBehaviour, Texture2D> OnLoadedAction)
    {
        if(!isReady)
            yield break;

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded texture once the web request completes
                var texture = DownloadHandlerTexture.GetContent(uwr);
                imageFromWeb = texture;
                Debug.Log("Image downloaded " + uwr);
                CreateImageTargetFromDownloadedTexture(targetName, OnLoadedAction);
            }
        }
    }
    void CreateImageTargetFromDownloadedTexture(string targetName, Action<ImageTargetBehaviour, Texture2D> OnLoadedAction)
    {
        var mTarget = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(
    imageFromWeb,
    0.1f,
    targetName);
        // Add the DefaultObserverEventHandler to the newly created game object
        mTarget.gameObject.AddComponent<DefaultObserverEventHandler>();

        Debug.Log("Target created and active" + mTarget);

        OnLoadedAction?.Invoke(mTarget, imageFromWeb);
    }
}