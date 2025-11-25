using MEBCGF;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class Main : MonoBehaviour
{
    async void Start()
    {
        foreach (var d in CodeLoader.ExtraFiles)
        {
            var a = await LoadDll(d);
            if (a == null)
                return;
        }
        var assembly = await LoadDll("Logic.dll");
        var t = assembly.GetType("MEBCGF.Startup");
        var m = t.GetMethod("Start");
        m.Invoke(null, null);
        Log.Debug("Logic.dll loaded and started");
    }

    WWW www;
    private async Task<Assembly> LoadDll(string d)
    {
        www = new WWW(Path.Combine(Application.dataPath, "../", CodeLoader.DllOutputPath, d));
        tcs = new();
        var success = await tcs.Task;
        if (!success)
        {
            Debug.LogError($"Load {d} failed: {www.error}");
            return null;
        }
        Log.Debug($"Load extra dll {d} success");
        var dllbytes = www.bytes;
        byte[] pdfbytes = null;
#if UNITY_EDITOR
        d = d.Replace(".dll", ".pdb");
        www = new WWW(Path.Combine(Application.dataPath, "../", CodeLoader.DllOutputPath, d));
        tcs = new();
        success = await tcs.Task;
        if (!success)
        {
            Debug.LogError($"Load {d} failed: {www.error}");
            return null;
        }
        Log.Debug($"Load extra dll {d} success");
        pdfbytes = www.bytes;
#endif
        return AppDomain.CurrentDomain.Load(dllbytes, pdfbytes);
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
