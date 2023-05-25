using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;


public class GetDeviceInfo : MonoBehaviour
{
    private void Start()
    {
        GetDeviceInformation();
    }

    public Text[] texts;

    //利用滑动条滚动条实现吧
    void GetDeviceInformation()
    {
        texts[0].text = "设备模型：" + SystemInfo.deviceModel;
        texts[1].text = "设备名称：" + SystemInfo.deviceName;
        texts[2].text = "设备类型：" + SystemInfo.deviceType;
        texts[3].text = "设备唯一标识符：" + SystemInfo.deviceUniqueIdentifier;
        texts[4].text = "是否支持纹理复制：" + SystemInfo.copyTextureSupport;
        texts[5].text = "显卡ID：" + SystemInfo.graphicsDeviceID;
        texts[6].text = "显卡名称：" + SystemInfo.graphicsDeviceName;
        texts[7].text = "显卡类型：" + SystemInfo.graphicsDeviceType;
        texts[8].text = "显卡供应商：" + SystemInfo.graphicsDeviceVendor;
        texts[9].text = "显卡供应商ID：" + SystemInfo.graphicsDeviceVendorID;
        texts[10].text = "显卡版本号：" + SystemInfo.graphicsDeviceVersion;
        texts[11].text = "显存大小（单位：MB）：" + SystemInfo.graphicsMemorySize;
        texts[12].text = "是否支持多线程渲染：" + SystemInfo.graphicsMultiThreaded;
        texts[13].text = "支持的渲染目标数量：" + SystemInfo.supportedRenderTargetCount;
        texts[14].text = "系统内存大小（单位：MB）：" + SystemInfo.systemMemorySize;
        texts[15].text = "操作系统：" + SystemInfo.operatingSystem;

        //GetMacAddress();
        //GetDeviceIMEI();
    }

    /// <summary>
    /// 获取mac地址
    /// </summary>
    void GetMacAddress()
    {
        NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface ni in nis)
        {
            Debug.Log("Name = " + ni.Name);
            Debug.Log("Des = " + ni.Description);
            Debug.Log("Type = " + ni.NetworkInterfaceType.ToString());
            Debug.Log("Mac地址 = " + ni.GetPhysicalAddress().ToString());
            texts[16].text += "   mac地址：" + ni.GetPhysicalAddress().ToString();
        }
    }




    /// <summary>
    /// 手机序列号是IMEI码的俗称。
    /// IMEI为TAC + FAC + SNR + SP。
    /// IMEI(International Mobile Equipment Identity)是国际移动设备身份码的缩写，
    /// 国际移动装备辨识码，是由15位数字组成的"电子串号"，
    /// 它与每台移动电话机一一对应，而且该码是全世界唯一的。
    /// </summary>
    #region 获得安卓手机上的IMEI号
    string imei0 = "";
    string imei1 = "";
    string meid = "";
    void GetDeviceIMEI()
    {
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var telephoneyManager = context.Call<AndroidJavaObject>("getSystemService", "phone");
        imei0 = telephoneyManager.Call<string>("getImei", 0);//如果手机双卡 双待  就会有两个MIEI号
        imei1 = telephoneyManager.Call<string>("getImei", 1);
        meid = telephoneyManager.Call<string>("getMeid");//电信的手机 是MEID
        texts[17].text = "IMEI0:" + imei0;
        texts[18].text = "IMEI1:" + imei1;
        texts[19].text = "MEID:" + meid;

    }
    #endregion

}