using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class CanvasComponent : Entity
{
    Dictionary<UILayer, Transform> layers = new();
    public override void OnStart()
    {
        base.OnStart();

        var root = transform.GetChild(0);
        foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
        {
            var layerTransform = root.Find(layer.ToString());
            if (layerTransform != null)
            {
                layers[layer] = layerTransform;
            }
        }
    }
    internal void SetUILayer<T>(T uiEntity, UILayer layer) where T : Entity, new()
    {
        uiEntity.transform.SetParent(layers[layer], false);
        InitFullScreen(uiEntity.transform);
    }
    private void InitFullScreen(Transform uiTransform)
    {
        var rectTransform = uiTransform as RectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.SetAsLastSibling();
    }
}
