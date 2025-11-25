using System.Threading.Tasks;
using UnityEngine;

public static class ResourceHelper
{
    public static async Task<GameObject> InstantiateAsync(string path)
    {
        return GameObject.Instantiate(Resources.Load<GameObject>(path));
    }
}
