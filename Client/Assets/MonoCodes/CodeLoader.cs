using System;
using UnityEngine;

public static class CodeLoader
{
    public const string DllOutputPath = "DllOutput/";
    public readonly static string[] ExtraFiles = new string[] { };

    public static Action UpdateAction;
    public static Action ApplicationQuitAction;
    public static Action<bool> ApplicationPauseAction;
    public static Action<bool> ApplicationFocusAction;
}
