using System.Collections.Generic;

public class ClassPool<T> where T : class, new()
{
    private readonly Stack<T> poolableQueue;

    public ClassPool()
    {
        poolableQueue = new Stack<T>();
    }

    public ClassPool<T> CreatePoolObject(int count = 0)
    {
        if (poolableQueue.Count >= count) return this;

        for (int i = 0; i < count - poolableQueue.Count; i++)
            poolableQueue.Push(new T());

        return this;
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