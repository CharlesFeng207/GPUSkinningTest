using System.Collections;
using System.Collections.Generic;
using PerfToolkit;
using UnityEngine;

public class ShowPower : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    float e = 0;
 
    private void OnGUI()
    {
        GUILayout.Label(string.Format($"<size=80>电池总容量{Power.capacity}毫安,电压{Power.voltage}伏</size>"));
        GUILayout.Label(string.Format($"<size=80>实时电流{e}毫安,实时功率{(int)(e * Power.voltage)},满电量能玩{((Power.capacity /e).ToString("f2"))}小时</size>"));
    }
 
 
    float t = 0f;
    private void Update()
    {
        if(Time.time - t > 1f)
        {
            t = Time.time;
            e = Power.electricity;
        }
    }
#endif
}
