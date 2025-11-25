using System;
using System.Collections.Generic;
using UnityEngine;

public class MonoFacade : MonoBehaviour
{
    public string uiname;
    public List<UIElement> uielements;
    private Dictionary<string, UIElement> dElements;
    public UIElement GetUIElement(string uiname)
    {
        Init();
        dElements.TryGetValue(uiname, out var element);
        return element;
    }
    public bool Contains(Transform tr)
    {
        Init();
        foreach (var ele in uielements)
        {
            if (ele.component == null)
                continue;
            if (ele.component.gameObject.transform == tr)
                return true;
        }
        return false;
    }

    private void Init()
    {
        if (dElements != null)
            return;
        dElements = new();
        foreach (var ele in uielements)
        {
            dElements[ele.name] = ele;
        }
    }
}

[Serializable]
public class UIElement
{
    public Component component;
    public string comtype;
    public string name;
    public string originalName;
}
