//using Cysharp.Threading.Tasks;
//using DG.Tweening;
//using GameFramework;
//using GameKit.Base;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
using MEBCGF;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class Entity
{

    //    public InstanceRequest InstanceRequest { get; private set; }
    public GameObject gameObject { get; private set; }
    protected MonoFacade MonoFacade => gameObject.GetComponentInChildren<MonoFacade>();
    public Transform transform => (gameObject != null && gameObject) ? gameObject.transform : null;
    //public override string ToString()
    //{
    //    return $"{EntityType ?? GetType().Name}_{uid}_{gameObject?.name}";
    //}
    //    TagTimes _tagTime;
    //    public int GetTimeByTag(string tag, int defaultTime)
    //    {
    //        if (_tagTime == null)
    //            _tagTime = gameObject.GetComponent<TagTimes>();
    //        if (_tagTime == null)
    //            return defaultTime;
    //        return _tagTime.GetTime(tag, defaultTime);
    //    }
    public T GetMonoComponent<T>(string name) where T : Component
    {
        if (gameObject == null || !gameObject)
            return null;

        if (MonoFacade == null)
            return null;

        var element = MonoFacade.GetUIElement(name);
        if (element == null)
            return null;
        if (element.component == null || !element.component)
            return null;
        if (element.component is T)
            return element.component as T;
        return element.component.GetComponent<T>();
    }
    bool destroyWhenDispose;
    private void AttachGameObject(GameObject obj, bool destroyWhenDispose = false)
    {
        if (obj == null) return;
        this.destroyWhenDispose = destroyWhenDispose;
        //InstanceRequest = null;
        gameObject = obj;
    }

    public T AddChild<T>(GameObject gameObject, bool DestroyWhenDispose = false) where T : Entity, new()
    {
        var child = new T();
        child.Parent = this;
        children.Add(child.uid, child);
        child.AttachGameObject(gameObject, DestroyWhenDispose);
        child.OnStart();
        return child;
    }

    //public void AttachGameObject(InstanceRequest req)
    //{
    //    if (req == null) return;
    //    InstanceRequest = req;
    //    gameObject = req.gameObject;
    //}

    //    public T AddChild<T>(InstanceRequest gameObject) where T : Entity, new()
    //    {
    //        var child = new T();
    //        child.Parent = this;
    //        children.Add(child.uid, child);
    //        child.AttachGameObject(gameObject);
    //        child.OnStart();
    //        return child;
    //    }

    public async Task<T> AddChild<T>(string resourcePath) where T : Entity, new()
    {
        try
        {
            var instanceRequest = await ResourceHelper.InstantiateAsync(resourcePath);
            instanceRequest.gameObject.transform.SetParent(transform, true);
            instanceRequest.gameObject.transform.localPosition = Vector3.zero;
            return AddChild<T>(instanceRequest);
        }
        catch (Exception ex)
        {
            Log.Error($"AddChild error \r\n{ex}, 资源 {resourcePath}");
            return null;
        }
    }

    //    public T AddChild<T>(Transform resource) where T : Entity, new()
    //    {
    //        return AddChild<T>(resource.gameObject);
    //    }

    //    partial void DestroyGameObject()
    //    {
    //        if (gameObject != null && gameObject)
    //        {
    //            InstanceRequest?.Destroy();
    //            if (destroyWhenDispose)
    //                GameObject.Destroy(gameObject);
    //        }
    //        InstanceRequest = null;
    //        gameObject = null;
    //    }

    //    [System.Diagnostics.Conditional("DEBUG")]
    //    public static void LogAssert(bool condition, string message)
    //    {
    //        if (condition)
    //            return;
    //        Log.Error($"Assertion Failed: {message} \r\n{new System.Diagnostics.StackTrace()}");
    //    }


    //    private CanvasGroup _canvasGroup = null;

    //    public CanvasGroup canvasGroup
    //    {
    //        get
    //        {
    //            if (_canvasGroup == null)
    //                _canvasGroup = gameObject?.GetComponent<CanvasGroup>();
    //            return _canvasGroup;
    //        }
    //    }

    //    private LayoutElement _layoutElement = null;

    //    public LayoutElement layoutElement
    //    {
    //        get
    //        {
    //            if (_layoutElement == null)
    //                _layoutElement = gameObject.GetComponent<LayoutElement>();
    //            return _layoutElement;
    //        }
    //    }

    //    public void SetAlpha(float alpha)
    //    {
    //        if (canvasGroup == null)
    //            return;

    //        canvasGroup.alpha = alpha;
    //    }

    //    public void SetAlphaActive(bool active) => SetAlphaActive(active, false);

    //    public void SetAlphaActive(bool active, bool includeLayout)
    //    {
    //        SetAlpha(active ? 1f : 0f);
    //        if (includeLayout)
    //            SetIgnoreLayout(!active);
    //    }

    //    public void SetIgnoreLayout(bool ignoreLayout)
    //    {
    //        if (layoutElement == null)
    //            return;

    //        layoutElement.ignoreLayout = ignoreLayout;
    //    }
    //    public async Task<Entity> AddEffect(string path, float lifeTime = 0, Vector3? pos = null)
    //    {
    //        if (transform == null)
    //            return null;
    //        var parent = this;
    //        if (lifeTime > 0)
    //        {
    //            parent = Game.world.BattleField;
    //            pos = pos ?? transform.position;
    //        }
    //        var ent = await parent.AddChild<EffectData>(path);
    //        ent.OnSet(path);
    //        if (lifeTime > 0f)
    //            ent.AddChild<LifeTimeComponent>().OnSet(lifeTime);

    //        ent.transform.SetParent(this.transform);
    //        ent.transform.localEulerAngles = Vector3.zero;
    //        if (pos.HasValue)
    //            ent.transform.position = pos.Value;
    //        else
    //            ent.transform.localPosition = Vector3.zero;
    //        return ent;
    //    }
    //    public async Task<Entity> AddEffect(string path, Transform parent, float lifeTime = 0)
    //    {
    //        var ent = await AddChild<EffectData>(path);
    //        ent.OnSet(path);
    //        ent.transform.SetParent(parent, true);
    //        ent.transform.localScale = Vector3.one;
    //        ent.transform.localPosition = Vector3.zero;
    //        if (lifeTime > 0f)
    //            ent.AddChild<LifeTimeComponent>().OnSet(lifeTime);
    //        return ent;
    //    }

    //    protected CancellationTokenSource _CancellationTokenSource;
    //    protected CancellationTokenSource CancellationTokenSource
    //    {
    //        get
    //        {
    //            if (_CancellationTokenSource == null)
    //                _CancellationTokenSource = new();
    //            return _CancellationTokenSource;
    //        }
    //    }
    //    /// <summary>
    //    /// delay function
    //    /// </summary>
    //    /// <param name="millisecondsDelay"></param>
    //    /// <param name="ignoreTimeScale"></param>
    //    /// <returns>if task has been cancelled, return true for cancelled.</returns>
    //    public async UniTask<bool> Delay(int millisecondsDelay, bool ignoreTimeScale = false)
    //    {
    //        return await Delay(millisecondsDelay, PlayerLoopTiming.Update, CancellationTokenSource.Token, ignoreTimeScale);
    //    }

    //    public async UniTask<bool> Delay(int millisecondsDelay, PlayerLoopTiming updateMode, bool ignoreTimeScale = false)
    //    {
    //        return await Delay(millisecondsDelay, updateMode, CancellationTokenSource.Token, ignoreTimeScale);
    //    }

    //    public async UniTask<bool> Delay(int millisecondsDelay, CancellationToken token, bool ignoreTimeScale = false)
    //    {
    //        return await Delay(millisecondsDelay, PlayerLoopTiming.Update, token, ignoreTimeScale);
    //    }

    //    public async UniTask<bool> Delay(int millisecondsDelay, PlayerLoopTiming updateMode, CancellationToken token, bool ignoreTimeScale = false)
    //    {
    //        return await UniTask.Delay(millisecondsDelay, ignoreTimeScale, updateMode, token).SuppressCancellationThrow();
    //    }

    //    public async Task<bool> DelayFrame(int frameCount)
    //    {
    //        return await DelayFrame(frameCount, PlayerLoopTiming.Update, CancellationTokenSource.Token);
    //    }

    //    public async Task<bool> DelayFrame(int frameCount, PlayerLoopTiming updateMode)
    //    {
    //        return await DelayFrame(frameCount, updateMode, CancellationTokenSource.Token);
    //    }

    //    public async Task<bool> DelayFrame(int frameCount, CancellationToken token)
    //    {
    //        return await DelayFrame(frameCount, PlayerLoopTiming.Update, token);
    //    }

    //    public async Task<bool> DelayFrame(int frameCount, PlayerLoopTiming updateMode, CancellationToken token)
    //    {
    //        return await UniTask.DelayFrame(frameCount, updateMode, token).SuppressCancellationThrow();
    //    }

    //    public async UniTask<bool> WaitUntil(Func<bool> predicate)
    //    {
    //        return await UniTask.WaitUntil(predicate, PlayerLoopTiming.Update, CancellationTokenSource.Token).SuppressCancellationThrow(); ;
    //    }
    protected void OnClearAllButtonListeners()
    {
        ClearAllButtonListeners();
    }
    partial void ClearAllButtonListeners()
    {
        if (gameObject == null)
            return;
        var monofacade = gameObject.GetComponent<MonoFacade>();
        if (monofacade == null)
            return;
        foreach (var e in monofacade.uielements)
        {
            if (e.component == null)
                continue;
            var button = e.component.GetComponent<Button>();
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }

    //    public async UniTask DoMove(Vector3 p, float speed)
    //    {
    //        var anim = GetChild<ViewAnimationComponent>();
    //        transform.LookAt(p);
    //        anim.PlayAnim("run");
    //        anim.SetBool("moving", true);
    //        var t = transform.DOMove(p, p.DistanceWithoutY(transform.position) / speed);
    //        await t.AsyncWaitForCompletion();
    //        anim.SetBool("moving", false);
    //        anim.PlayAnim("idle");
    //    }
    //    public async UniTask DoMove(float x, float y, float z, float speed)
    //    {
    //        var p = new Vector3(x, y, z);
    //        await DoMove(p, speed);
    //    }

    //    public async Task WaitForEvent(string eventName)
    //    {
    //        var tcs = new TaskCompletionSource<bool>();
    //        RegisterCall(eventName, () =>
    //        {
    //            UnregisterCall(eventName);
    //            tcs.TrySetResult(true);
    //        });
    //        await tcs.Task;
    //    }

    //    Type waitForCloseUIType;
    //    TaskCompletionSource<bool> waitForUICloseTcs;
    //    public async System.Threading.Tasks.Task WaitForUIClose<T>() where T : LWFW.UIBaseView
    //    {
    //        if (await Delay(100))
    //            return;
    //        if (!GameEntry.UI.IsWindowOpen(typeof(T)))
    //            return;
    //        if (waitForUICloseTcs != null)
    //        {
    //            await waitForUICloseTcs.Task;
    //            return;
    //        }
    //        GameEntry.Event.Unsubscribe(FEventId.GF_window_closed, GF_window_closed);
    //        GameEntry.Event.Subscribe(FEventId.GF_window_closed, GF_window_closed);

    //        waitForCloseUIType = typeof(T);
    //        waitForUICloseTcs = new();
    //        await waitForUICloseTcs.Task;
    //    }

    //    private void GF_window_closed(object obj)
    //    {
    //        if ((obj as Type) != waitForCloseUIType)
    //            return;
    //        GameEntry.Event.Unsubscribe(FEventId.GF_window_closed, GF_window_closed);

    //        if (waitForUICloseTcs == null)
    //            return;
    //        waitForUICloseTcs.SetResult(true);
    //        waitForUICloseTcs = null;
    //    }

    //    Type waitForOpenUIType;
    //    TaskCompletionSource<bool> waitForUIOpenTcs;
    //    public async System.Threading.Tasks.Task WaitForUIOpen<T>() where T : LWFW.UIBaseView
    //    {
    //        if (GameEntry.UI.IsWindowOpen(typeof(T)))
    //            return;
    //        if (waitForUIOpenTcs != null)
    //            await waitForUIOpenTcs.Task;
    //        GameEntry.Event.Unsubscribe(FEventId.GF_window_opened, GF_window_opened);
    //        GameEntry.Event.Subscribe(FEventId.GF_window_opened, GF_window_opened);

    //        waitForOpenUIType = typeof(T);
    //        waitForUIOpenTcs = new();
    //        await waitForUIOpenTcs.Task;
    //    }

    //    private void GF_window_opened(object obj)
    //    {
    //        if ((obj as Type) != waitForOpenUIType)
    //            return;
    //        GameEntry.Event.Unsubscribe(FEventId.GF_window_opened, GF_window_opened);

    //        if (waitForUIOpenTcs == null)
    //            return;
    //        waitForUIOpenTcs.SetResult(true);
    //        waitForUIOpenTcs = null;
    //    }

    //    public static string gettext(string msg, params object[] args)
    //    {
    //        if (string.IsNullOrEmpty(msg))
    //            return "";
    //        msg = LanguageHelper.GetText(msg);
    //        if (args.Length == 0)
    //            return msg;

    //        for (var i = 0; i < args.Length; i++)
    //        {
    //            args[i] = gettext(args[i].ToString());
    //        }
    //        return string.Format(msg, args);
    //    }
    //    public static async void ResizeContentSizeFitter(Transform tr)
    //    {
    //        if (tr == null)
    //            return;
    //        var l = tr.localScale;
    //        tr?.ShowThis(false);
    //        await Game.world.BattleField.DelayFrame(1);
    //        tr?.gameObject.SetActive(false);
    //        await Game.world.BattleField.DelayFrame(1);
    //        tr?.gameObject.SetActive(true);
    //        await Game.world.BattleField.DelayFrame(1);
    //        tr?.gameObject.SetActive(false);
    //        await Game.world.BattleField.DelayFrame(1);
    //        tr?.gameObject.SetActive(true);
    //        await Game.world.BattleField.DelayFrame(1);
    //        tr?.ShowThis(true);
    //    }
    //    ContentSizeFitter outerContentSizeFitter; // 最外围的ContentSizeFitter
    //    protected async System.Threading.Tasks.Task Resize(ContentSizeFitter ContentSizeFitter = null)
    //    {
    //        if (ContentSizeFitter != null)
    //            outerContentSizeFitter = ContentSizeFitter;
    //        if (outerContentSizeFitter == null)
    //            outerContentSizeFitter = gameObject.GetComponentInChildren<ContentSizeFitter>();
    //        if (outerContentSizeFitter == null)
    //            return;

    //        if (await DelayFrame(1))
    //            return;

    //        // 强制刷新所有ContentSizeFitter和VerticalLayoutGroup
    //        await RefreshContentSizeFitters();

    //        //再刷新一遍
    //        if (await DelayFrame(1))
    //            return;

    //        // 强制刷新所有ContentSizeFitter和VerticalLayoutGroup
    //        await RefreshContentSizeFitters();

    //    }
    //    /// <summary>
    //    /// 刷新所有ContentSizeFitter组件
    //    /// </summary>
    //    private async Task RefreshContentSizeFitters()
    //    {
    //        // 获取所有ContentSizeFitter组件
    //        ContentSizeFitter[] contentSizeFitters = gameObject.GetComponentsInChildren<ContentSizeFitter>(true);

    //        // 获取所有VerticalLayoutGroup组件
    //        VerticalLayoutGroup[] verticalLayouts = gameObject.GetComponentsInChildren<VerticalLayoutGroup>(true);

    //        // 先禁用所有ContentSizeFitter
    //        foreach (var csf in contentSizeFitters)
    //        {
    //            csf.enabled = false;
    //        }

    //        // 强制刷新所有VerticalLayoutGroup
    //        foreach (var vlg in verticalLayouts)
    //        {
    //            LayoutRebuilder.ForceRebuildLayoutImmediate(vlg.GetComponent<RectTransform>());
    //        }

    //        if (await DelayFrame(1))
    //            return;

    //        // 重新启用所有ContentSizeFitter
    //        foreach (var csf in contentSizeFitters)
    //        {
    //            csf.enabled = true;
    //            LayoutRebuilder.ForceRebuildLayoutImmediate(csf.GetComponent<RectTransform>());
    //        }
    //    }
    //    public static async Task HideNormalUI()
    //    {
    //        if (await Game.world.BattleField.DelayFrame(1))
    //            return;
    //        GameEntry.UI.DestroyWindowByLayer(LWFW.UILayer.Normal);
    //        FastCall(Events.ShowWorldField, false);
    //    }
}
