using UnityEngine;

public class Power
{
    static public float electricity
    {
        get
        {
#if UNITY_ANDROID
            //获取电流（微安），避免频繁获取，取一次大概2毫秒
            float electricity = (float)manager.Call<int>("getIntProperty", PARAM_BATTERY);
            //小于1W就认为它的单位是毫安，否则认为是微安
            return ToMA(electricity);
#else
            return -1f;
#endif

        }
    }
    //获取电压 伏
    static public float voltage
    {
        get
        {
            var receive = currActivity.Call<AndroidJavaObject>("registerReceiver", new object[] { null, new AndroidJavaObject("android.content.IntentFilter", new object[] { "android.intent.action.BATTERY_CHANGED" }) });
            if (receive != null)
            {
                return (float)receive.Call<int>("getIntExtra", new object[] { "voltage", 0 }) / 1000f; //BatteryManager.EXTRA_VOLTAGE
            }
            return 0f;
        }
    }


    //获取电池总容量 毫安
    static public int capacity { get; private set; }

    //获取实时电流参数
    static object[] PARAM_BATTERY = new object[] { 2 }; //BatteryManager.BATTERY_PROPERTY_CURRENT_NOW)
    static AndroidJavaObject manager;
    static AndroidJavaObject currActivity;
    static Power()
    {
#if UNITY_ANDROID
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        manager = currActivity.Call<AndroidJavaObject>("getSystemService", new object[] { "batterymanager" });
        capacity = (int)(ToMA((float)manager.Call<int>("getIntProperty", new object[] { 1 })) / ((float)manager.Call<int>("getIntProperty", new object[] { 4 }) / 100f));   //BATTERY_PROPERTY_CHARGE_COUNTER 1 BATTERY_PROPERTY_CAPACITY 4
#endif
    }

    public static float ToMA(float maOrua)
    {
        return maOrua < 10000 ? maOrua : maOrua / 1000f;
    }
}