using MEBCGF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum UILayer
{
    Background = 0,
    Normal = 1,
    Front = 2,
    Topmost = 3,
}
internal static class UIHelper
{
    public static async Task<T> LoadUI<T>(string path = null, UILayer layer = UILayer.Normal, Entity parent = null) where T : Entity, new()
    {
        if (parent == null)
            parent = Game.world;
        var uiEntity = parent.GetChild<T>();
        if (uiEntity == null)
            uiEntity = await parent.AddChild<T>(path ?? $"Prefab/UI/{typeof(T).Name}");
        Game.world.GetChild<CanvasComponent>().SetUILayer(uiEntity, layer);
        return uiEntity;
    }
}
