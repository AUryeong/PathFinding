using System.Collections.Generic;

public class ClassPool<T> where T : class, new()
{
    private static ClassPool<T> classPools;
    private readonly Stack<T> poolableQueue;

    public ClassPool()
    {
        poolableQueue = new Stack<T>();
    }

    public static ClassPool<T> Get()
    {
        if (classPools == null)
            classPools = new ClassPool<T>();
        return classPools;
    }

    public virtual void PushPool(T poolObj)
    {
        poolableQueue.Push(poolObj);
    }

    public virtual T PopPool()
    {
        return poolableQueue.Count > 0 ? poolableQueue.Pop() : new T();
    }
}