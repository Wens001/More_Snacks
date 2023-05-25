using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AssetBundleInfos
{
    public AssetBundle assetBundle { get; private set; }
    public int referencedCount { get; private set; }//引用数
    public void Unload()
    {
        referencedCount--;
        if (referencedCount <= 0)
        {
            if (BundleManager.bundleDict.ContainsValue(this))
                BundleManager.bundleDict.RemoveByValue(this);
            assetBundle?.Unload(false);
            assetBundle = null;
        }
    }

    public AssetBundleInfos(AssetBundle bundle)
    {
        assetBundle = bundle;
        referencedCount = 1;
    }
}
public static class BundleManager 
{
    static BundleManager(){ }


    public static bool IsIinit { get; private set; } = false;
    private static Action initAction ;
    private static Action initFailAction;
    /// <summary>
    /// 初始化，Load All Bundle
    /// </summary>
    /// <returns></returns>
    public static async Task Init()
    {
        if (IsIinit)
            return;
        try
        {
            var manifest = LoadMainManifest(true);
            foreach (var name in manifest.GetAllAssetBundles())
            {
                var bundle = await LoadBundle(name, true);
            }
        }
        catch (Exception )
        {
            initFailAction?.Invoke();
            throw ;
        }
        IsIinit = true;
        initAction?.Invoke();
    }

    public static void OnInit(Action action)
    {
        if (action == null)
            return;
        initAction += action;
    }
    public static void OnInitFail(Action action)
    {
        if (action == null)
            return;
        initFailAction += action;
    }

    #region Path

    public static string StreamingAssetPath(bool isFile = false)
    {
        string pre = "file://";
#if UNITY_EDITOR
        pre = "file://";
#elif UNITY_ANDROID
        pre = "";
#elif UNITY_IPHONE
	    pre = "file://";
#endif
        if (isFile)
            return Application.streamingAssetsPath;
        return $"{pre}{Application.streamingAssetsPath}";
    }

    public static string BundleFolderName()
    {
        return "ABundles";
    }

    public static string StreamingAssetBundlePath(bool isFile = false)
    {
        return $"{StreamingAssetPath(isFile)}/{BundleFolderName()}/";
    }

    public static string PersistentPath(bool isFile = false)
    {
        string pre = "file://";
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        pre = "file:///";
#elif UNITY_ANDROID
        pre = "file://";
#elif UNITY_IPHONE
        pre = "file://";
#endif
        if (isFile)
            return Application.persistentDataPath ;
        return $"{pre}{Application.persistentDataPath}";
    }

    public static string PersistentBundlePath(bool isFile = false)
    {
        return $"{PersistentPath(isFile)}/{BundleFolderName()}/";
    }

    #endregion

    public static bool IsError(UnityWebRequest req)
    {
        if (req == null || req.isNetworkError || req.isHttpError || !string.IsNullOrEmpty(req.error))
            return true;
        return false;
    }

    public static async Task<UnityWebRequest> GetRequest(string path)
    {
        var request = UnityWebRequest.Get(path);
        await request.SendWebRequest();
        if (IsError(request))
            return null;
        return request;
    }

    public static IEnumerator GetRequest(string path, Action<UnityWebRequest> action)
    {
        var request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();
        if (IsError(request))
            yield break;
        action?.Invoke(request);
    }

    public static async Task<string> GetText(string path)
    {
        path = $"{StreamingAssetPath()}/{path}";
        var req = await GetRequest(path);
        if (IsError(req))
            return string.Empty;
        return req.downloadHandler.text;
    }
    public static async Task<byte[]> GetBytes(string path)
    {
        path = $"{StreamingAssetPath()}/{path}";
        var req = await GetRequest(path);
        if (IsError(req))
            return null;
        return req.downloadHandler.data;
    }

    public static DoubleMap<string, AssetBundleInfos> bundleDict { get; private set; } = new DoubleMap<string, AssetBundleInfos>();

    #region Save Data

    /// <summary>
    /// Streaming Data Copy To Persistent
    /// </summary>
    /// <param name="fileName"></param>
    public static async Task CopyToPersistent(string fileName)
    {
        var des = PersistentBundlePath() + fileName;
        using (var req = await GetRequest(des))
        {
            if (File.Exists(des))
                File.Delete(des);
            using (FileStream fsDes = File.Create(des))
            {
                var data = req.downloadHandler.data;
                fsDes.Write(data, 0, data.Length);
            }
        }
    }

    public static async Task SaveToPersistent(string url, string savePath)
    {
        savePath = PersistentPath() + savePath;
        using (var req = await GetRequest(url))
        {
            if (File.Exists(savePath))
                File.Delete(savePath);
            using (FileStream fsDes = File.Create(savePath))
            {
                var data = req.downloadHandler.data;
                fsDes.Write(data, 0, data.Length);
            }
        }
    }

    #endregion

    #region LoadAssets

    public static async Task<AssetBundleInfos> LoadBundle(string bundleName, bool IsStreaming)
    {
        if (bundleDict.ContainsKey(bundleName))
        {
            return bundleDict.GetValueByKey(bundleName);
        }
        string path = null;
        if (IsStreaming)
            path = StreamingAssetBundlePath() + bundleName;
        else
            path = PersistentBundlePath() + bundleName;
        using (var req = await GetRequest(path))
        {
            var bytes = req.downloadHandler.data;
            MyStream.WriteData(bytes);
            ////异步
            //AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(bytes);
            //await createRequest;
            //var bundle = createRequest.assetBundle;
            var bundle = AssetBundle.LoadFromMemory(bytes);
            if (bundle == null)
                throw new Exception($"Null AssetBundle ,Path : {path}");
            var bundleRef = new AssetBundleInfos(bundle);
            if (!bundleDict.ContainsKey(bundleName))
                bundleDict.Add(bundleName, bundleRef);
            return bundleRef;
        }
    }

    public static async Task<T> LoadAsset<T>(string bundleName, string name)
        where T : UnityEngine.Object
    {
        var bundle = await LoadBundle(bundleName, true);
        return bundle.assetBundle.LoadAsset<T>(name);
    }

    public static async Task<string[]> GetAllAssetNames(string bundleName)
    {
        var bundle = await LoadBundle(bundleName, true);
        return bundle.assetBundle.GetAllAssetNames();
    }

    public static async Task<T[]> LoadAllAssets<T>(string bundleName)
        where T : UnityEngine.Object
    {
        var bundle = await LoadBundle(bundleName, true);
        return bundle.assetBundle.LoadAllAssets<T>();
    }
    public static async Task LoadScene(string bundleName, string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        var bundle = await LoadBundle(bundleName, true);
        SceneManager.LoadScene(sceneName, mode);
    }

    #region Manifest

    public static AssetBundleManifest LoadManifest(string name)
    {
        var manifestAB = AssetBundle.LoadFromFile(StreamingAssetBundlePath(true) + name);
        return manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    private static AssetBundleManifest MainManifest; 
    public static AssetBundleManifest LoadMainManifest(bool IsStreaming)
    {
        if (MainManifest == null)
        {
            string path = null;
            if (IsStreaming)
                path = StreamingAssetBundlePath(true) + BundleFolderName();
            else
                path = PersistentBundlePath(true) + BundleFolderName();
            var manifestAB = AssetBundle.LoadFromFile(path);
            MainManifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        return MainManifest;
    }

    #endregion

    public static async Task<AsyncOperation> LoadSceneSync(string bundleName, string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        var bundle = await LoadBundle(bundleName, true);
        var aop = SceneManager.LoadSceneAsync(sceneName, mode);
        return aop;
    }

    public static bool Unload(string bundleName)
    {
        if (bundleDict.ContainsKey(bundleName))
        {
            bundleDict.GetValueByKey(bundleName).Unload();
            return true;
        }
        return false;
    }

    #endregion


}
/*
    var aop = await BundleManager.Instance.LoadSceneSync("ccc.unity3d", "xxx");
    await Awaiters.Until(() => aop.isDone);
    var bundleInfo = await BundleManager.LoadBundle("aaa.bbb",true);
    var go = bundleInfo.assetBundle.LoadAsset<GameObject>("aaa.prefab");
    Instantiate(go);
    bundleInfo.OnUnload();

    await Awaiters.Until(() => aop.isDone);
    await Awaiters.EndOfFrame;
    await new WaitForSeconds(.1f);
 */