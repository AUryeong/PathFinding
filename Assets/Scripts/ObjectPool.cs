using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectPoolActionType
{
    Instantiate,
    Pop,
    Pool
}

public class ObjectPool<T> where T : Component
{
    public readonly T originPoolObject;
    private readonly Stack<T> poolableQueue;

    private Transform parent;
    
    // Action
    public delegate void PoolAction(T obj);

    private readonly Dictionary<ObjectPoolActionType, PoolAction> poolActionDict;

    public ObjectPool(T origin)
    {
        originPoolObject = origin;
        parent = origin.transform.parent;
        poolableQueue = new Stack<T>();
        poolActionDict = new Dictionary<ObjectPoolActionType, PoolAction>(3);
    }
    
    private T Instantiate()
    {
        var instantiateObj = Object.Instantiate(originPoolObject, parent);
        poolActionDict.TryGetValue(ObjectPoolActionType.Instantiate, out var instantiateAction);
        instantiateAction?.Invoke(instantiateObj);
        return instantiateObj;
    }

    public ObjectPool<T> CreatePoolObject(int count = 0)
    {
        for (int i = 0; i < count; i++)
            poolableQueue.Push(Instantiate());

        return this;
    }

    public ObjectPool<T> AddAction(ObjectPoolActionType type, PoolAction action)
    {
        if (!poolActionDict.ContainsKey(type))
        {
            poolActionDict.Add(type, action);
        }
        else
        {
            poolActionDict[type] += action;
        }

        return this;
    }

    public virtual ObjectPool<T> SetParent(Transform parent)
    {
        this.parent = parent;
        foreach (var obj in poolableQueue)
            obj.transform.SetParent(parent);

        return this;
    }

    public virtual void PushPool(T poolObj)
    {
        poolableQueue.Push(poolObj);
        poolActionDict.TryGetValue(ObjectPoolActionType.Pool, out var poolAction);
        poolAction?.Invoke(poolObj);
    }

    public virtual T PopPool()
    {
        var popObj = poolableQueue.Count > 0 ? poolableQueue.Pop() : Instantiate();
        poolActionDict.TryGetValue(ObjectPoolActionType.Pop, out var popAction);
        popAction?.Invoke(popObj);
        return popObj;
    }
}

public class ListableObjectPool<T> : ObjectPool<T>, IEnumerable<T> where T : Component
{
    private readonly List<T> activeObjectList;
    public ListableObjectPool(T origin) : base(origin)
    {
        activeObjectList = new List<T>();
    }

    public override void PushPool(T poolObj)
    {
        base.PushPool(poolObj);
        activeObjectList.Remove(poolObj);
    }

    public override T PopPool()
    {
        var poolObject = base.PopPool();
        activeObjectList.Add(poolObject);
        return poolObject;
    }

    public override ObjectPool<T> SetParent(Transform parent)
    {
        base.SetParent(parent);
        foreach (var obj in activeObjectList)
            obj.transform.SetParent(parent);

        return this;
    }

    public List<T> GetList()
    {
        return activeObjectList;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return activeObjectList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class StaticObjectPool<T> : ListableObjectPool<T> where T : Component
{
    private static StaticObjectPool<T> staticObjectPoolInstance;

    public static StaticObjectPool<T> Create(T origin)
    {
        return staticObjectPoolInstance ??= new StaticObjectPool<T>(origin);
    }

    public static StaticObjectPool<T> Get()
    {
        Debug.Assert(staticObjectPoolInstance != null);
        return staticObjectPoolInstance;
    }
    
    public StaticObjectPool(T origin) : base(origin)
    {
    }
}