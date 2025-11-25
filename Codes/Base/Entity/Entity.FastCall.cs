using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

public partial class Entity
{
    class CallCellArgs<T> : CallCellBase
    {
        public Action<T> actionWithT;
    }
    class CallCell : CallCellBase
    {
    }
    public abstract class CallCellBase
    {
        public bool unregisterred;
        public string eventName;
        public Action action;
    }

    private Dictionary<string, CallCellBase> _localCalls;
    private Dictionary<string, CallCellBase> localCalls
    {
        get
        {
            if (_localCalls == null)
            {
                _localCalls = new();
            }
            return _localCalls;
        }
    }
    private static Dictionary<string, List<CallCellBase>> dCalls = new();
    protected void RegisterCall(string eventName, Action call)
    {
        //Log.Assert(isActive, $"实例已经被回收，不应该再注册新的消息 {eventName}");
        if (!dCalls.ContainsKey(eventName))
        {
            dCalls[eventName] = new();
        }
        if (localCalls.ContainsKey(eventName))
            return;
        var one = new CallCell();
        one.unregisterred = false;
        one.action = call;
        one.eventName = eventName;
        dCalls[eventName].Add(one);
        localCalls.Add(eventName, one);
    }
    protected void RegisterCall<T>(string eventName, Action<T> call)
    {
        //Log.Assert(isActive, $"实例已经被回收，不应该再注册新的消息 {eventName}");
        if (!dCalls.ContainsKey(eventName))
        {
            dCalls[eventName] = new();
        }
        if (localCalls.ContainsKey(eventName))
            return;
        var one = new CallCellArgs<T>();
        one.unregisterred = false;
        one.actionWithT = call;
        one.action = null;
        one.eventName = eventName;
        dCalls[eventName].Add(one);
        localCalls.Add(eventName, one);
    }
    protected void UnregisterCall(string eventName)
    {
        if (!localCalls.TryGetValue(eventName, out var call))
            return;
        call.unregisterred = true;
        dCalls[eventName].Remove(call);
        localCalls.Remove(eventName);
    }

    // 注意这里注册的消息一定要手动注销
    public static CallCellBase RegisterCallStatic(string eventName, Action call)
    {
        if (!dCalls.ContainsKey(eventName))
        {
            dCalls[eventName] = new();
        }
        var one = new CallCell();
        one.unregisterred = false;
        one.action = call;
        one.eventName = eventName;
        dCalls[eventName].Add(one);
        return one;
    }
    // 注意这里注册的消息一定要手动注销
    public static CallCellBase RegisterCallStatic<T>(string eventName, Action<T> call)
    {
        if (!dCalls.ContainsKey(eventName))
        {
            dCalls[eventName] = new();
        }
        var one = new CallCellArgs<T>();
        one.unregisterred = false;
        one.actionWithT = call;
        one.action = null;
        one.eventName = eventName;
        dCalls[eventName].Add(one);
        return one;
    }
    public static void UnregisterCallStatic(string eventName, CallCellBase call)
    {
        if (!dCalls.TryGetValue(eventName, out var callList))
            return;

        if (callList.Contains(call))
        {
            callList.Remove(call);
        }
    }

    public static void FastCall<T>(string eventName, T t)
    {
        if (!dCalls.TryGetValue(eventName, out var list))
        {
            return;
        }
        //if (eventName != Events.Update
        //    && eventName != Events.BattleUpdate
        //    && eventName != Events.OnEntityDestroy
        //    && eventName != Events.Move
        //    && eventName != Events.Stop
        //    && eventName != Events.OnPlayerMove
        //    && eventName != Events.OnEntityCreated)
        //    Log.Debug($"FastCall {eventName} {t}");
        var count = list.Count;
        for (var i = count - 1; i >= 0; i--)
        {
            if (i >= list.Count)
                continue;
            var c = list[i];
            if (c.unregisterred)
            {
                list.Remove(c);
                continue;
            }

            if (c is CallCellArgs<T> cca)
            {
#if DEBUG
                Profiler.BeginSample($"{cca.actionWithT.Target.GetType()}.{c.eventName}");
#endif
                cca.actionWithT(t);
            }
            else
            {
#if DEBUG
                Profiler.BeginSample($"{c.action.Target.GetType()}.{c.eventName}");
#endif
                c.action();
            }
#if DEBUG
            Profiler.EndSample();
#endif
        }
    }
    public static void FastCall(string eventName)
    {
        if (!dCalls.TryGetValue(eventName, out var list))
        {
            return;
        }
        //if (eventName != Events.Update
        //    && eventName != Events.BattleUpdate
        //    && eventName != Events.OnEntityDestroy
        //    && eventName != Events.Move
        //    && eventName != Events.Stop
        //    && eventName != Events.OnPlayerMove
        //    && eventName != Events.OnEntityCreated)
        //    Log.Debug($"FastCall {eventName}");
        var count = list.Count;
        for (var i = count - 1; i >= 0; i--)
        {
            if (i >= list.Count)
                continue;
            var c = list[i];
            if (c.unregisterred)
            {
                list.Remove(c);
                continue;
            }
            c.action();
        }
    }
    protected void UnregisterAllCalls()
    {
        foreach (var s in localCalls)
        {
            s.Value.unregisterred = true;
            dCalls[s.Value.eventName].Remove(s.Value);
        }
        localCalls.Clear();
    }
}
