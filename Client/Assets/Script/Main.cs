using MEBCGF;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        LoadDll();
    }

    WWW www;
    private async void LoadDll()
    {
        www = new WWW(Path.Combine(Application.dataPath, "../", CodeLoader.DllOutputPath, "Logic.dll"));
        tcs = new();
        var success = await tcs.Task;
        if (!success)
        {
            Debug.LogError($"Load Logic.dll failed: {www.error}");
            return;
        }
        
        var a = Assembly.Load(www.bytes);
        var t = a.GetType("MEBCGF.Startup");
        var m = t.GetMethod("Start");
        m.Invoke(null, null);
    }

    TaskCompletionSource<bool> tcs;
    void Update()
    {
        CodeLoader.UpdateAction?.Invoke();
        if (www == null)
            return;
        if (!www.isDone)
            return;
        if (string.IsNullOrEmpty(www.error))
            tcs.TrySetResult(true);
        else
            tcs.TrySetResult(false);
    }
    private void OnApplicationQuit()
    {
        CodeLoader.ApplicationQuitAction?.Invoke();
    }
    private void OnApplicationPause(bool pause)
    {
        CodeLoader.ApplicationPauseAction?.Invoke(pause);
    }
    private void OnApplicationFocus(bool focus)
    {
        CodeLoader.ApplicationFocusAction?.Invoke(focus);
    }
}
