using MEBCGF;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class Entity
{
    public GameObject gameObject { get; private set; }
    protected MonoFacade MonoFacade => gameObject.GetComponentInChildren<MonoFacade>();
    public Transform transform => (gameObject != null && gameObject) ? gameObject.transform : null;
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

    public T AddChild<T>(Transform tr, bool DestroyWhenDispose = false) where T : Entity, new()
    {
        return AddChild<T>(tr.gameObject, DestroyWhenDispose);
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
    public async Task<T> AddChild<T>(string resourcePath) where T : Entity, new()
    {
        try
        {
            var instanceRequest = await ResourceHelper.InstantiateAsync(resourcePath);
            instanceRequest.gameObject.transform.SetParent(transform, true);
            instanceRequest.gameObject.transform.localPosition = Vector3.zero;
            return AddChild<T>(instanceRequest, true);
        }
        catch (Exception ex)
        {
            Log.Error($"AddChild error \r\n{ex}, 资源 {resourcePath}");
            return null;
        }
    }

    partial void DestroyGameObject()
    {
        if (gameObject != null && gameObject)
        {
            if (destroyWhenDispose)
                GameObject.Destroy(gameObject);
        }
        gameObject = null;
    }
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

    private CanvasGroup _canvasGroup = null;

    public CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = gameObject?.GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }

    private LayoutElement _layoutElement = null;

    public LayoutElement layoutElement
    {
        get
        {
            if (_layoutElement == null)
                _layoutElement = gameObject.GetComponent<LayoutElement>();
            return _layoutElement;
        }
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = alpha;
    }

    public void SetAlphaActive(bool active) => SetAlphaActive(active, false);

    public void SetAlphaActive(bool active, bool includeLayout)
    {
        SetAlpha(active ? 1f : 0f);
        if (includeLayout)
            SetIgnoreLayout(!active);
    }

    public void SetIgnoreLayout(bool ignoreLayout)
    {
        if (layoutElement == null)
            return;

        layoutElement.ignoreLayout = ignoreLayout;
    }

}
