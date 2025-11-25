using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class Entity
{
    private static int s_uid;

    private Dictionary<string, List<int>> s_groups = new Dictionary<string, List<int>>();
    public int uid { get; private set; } = ++s_uid;
    public Entity Parent { get; set; }
    public Dictionary<int, Entity> children { get; set; } = new();
    /// <summary>
    /// Will be set with the first group name this entity joins.
    /// </summary>
    public string EntityType { get; private set; } = null;
    public void ForceSetEntityType(string type)
    {
        EntityType = type;
    }
    public int index { get; set; }

    public bool isActive { get; set; } = true;
    public T AddChild<T>() where T : Entity, new()
    {
        var child = new T();
        child.Parent = this;
        children.Add(child.uid, child);
        child.AttachGameObject(gameObject);
        child.OnStart();
        return child;
    }
    public T GetChild<T>() where T : Entity
    {
        foreach (var c in children.Values)
        {
            if (c is T)
                return c as T;
        }
        return null;
    }
    public T GetOrAddChild<T>() where T : Entity, new()
    {
        var child = GetChild<T>();
        if (child == null)
        {
            child = AddChild<T>();
        }
        return child;
    }
    public List<T> GetChildren<T>() where T : Entity
    {
        var l = new List<T>();
        foreach (var c in children.Values)
        {
            if (c is T)
                l.Add(c as T);
        }
        return l;
    }
    public Entity GetChild(int uid, bool ignoreActiveState = false)
    {
        if (children.TryGetValue(uid, out var child))
        {
            if (!ignoreActiveState && !child.isActive)
                return null;
            return child;
        }
        return null;
    }

    public void RemoveChild<T>() where T : Entity
    {
        children.Keys.Where(key => children[key] is T).ToList()
            .ForEach(key => RemoveChild(children[key]));
    }
    public void RemoveChild(Entity child)
    {
        if (child == null) return;
        children.Remove(child.uid);
        child.Internal_OnDestroy();
    }
    public void RemoveChildOnly(Entity child)
    {
        if (child == null) return;
        children.Remove(child.uid);
    }
    public void ClearChildren()
    {
        foreach (var child in children.Values)
        {
            child.Internal_OnDestroy();
        }
        children.Clear();
    }
    internal void OnDispose()
    {
        if (Parent == null)
        {
            Internal_OnDestroy();
            return;
        }
        Parent.RemoveChild(this);
    }
    private void AddToGroup(string groupName, int entityId)
    {
        if (Parent == null) return;

        if (!Parent.s_groups.ContainsKey(groupName))
        {
            Parent.s_groups[groupName] = new List<int>();
        }
        Parent.s_groups[groupName].Add(entityId);
    }

    public void RemoveFromGroup(string groupName, int entityId)
    {
        if (Parent == null) return;

        if (Parent.s_groups.ContainsKey(groupName))
        {
            Parent.s_groups[groupName].Remove(entityId);
        }
    }

    public List<int> GetGroup(string groupName)
    {
        if (!s_groups.TryGetValue(groupName, out var group))
        {
            group = new List<int>();
            s_groups[groupName] = group;
        }
        return group;
    }

    public void ClearGroup(string groupName)
    {
        if (s_groups.ContainsKey(groupName))
        {
            s_groups.Remove(groupName);
        }
    }

    public void ClearAllGroups()
    {
        s_groups.Clear();
    }

    /// <summary>
    /// 聚合到指定的集群中，第一个群集名称将被设置为 EntityType。
    /// </summary>
    /// <param name="groupName"></param>
    public void JoinGroup(string groupName, bool SetAsEntityType = false)
    {
        if (IsInGroup(groupName))
            return;
        if (SetAsEntityType || string.IsNullOrEmpty(EntityType))
            EntityType = groupName;
        AddToGroup(groupName, this.uid);
    }

    public void LeaveGroup(string groupName)
    {
        RemoveFromGroup(groupName, this.uid);
    }
    public bool IsInGroup(string groupName)
    {
        if (Parent == null) return false;

        return Parent.s_groups.ContainsKey(groupName) && Parent.s_groups[groupName].Contains(this.uid);
    }
    private void Internal_OnDestroy()
    {
        isActive = false;

        // Remove from all groups first, before clearing children
        if (Parent != null)
        {
            foreach (var group in Parent.s_groups.Values)
            {
                group.Remove(this.uid);
            }
        }

        ClearAllButtonListeners();
        UnregisterAllCalls();
        OnDestroy();
        ClearChildren();

        Parent = null;
    }

    public virtual void OnStart()
    {
        isActive = true;
    }
    protected CancellationTokenSource _CancellationTokenSource;
    protected CancellationTokenSource CancellationTokenSource
    {
        get
        {
            if (_CancellationTokenSource == null)
                _CancellationTokenSource = new();
            return _CancellationTokenSource;
        }
    }

    public virtual void OnDestroy()
    {
        // 释放 CancellationTokenSource
        if (_CancellationTokenSource != null)
        {
            _CancellationTokenSource.Cancel();
            _CancellationTokenSource.Dispose();
            _CancellationTokenSource = null;
        }

        DestroyGameObject();
        ClearAllGroups();
    }
    partial void DestroyGameObject();
}
