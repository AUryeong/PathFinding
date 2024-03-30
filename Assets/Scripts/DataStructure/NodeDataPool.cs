using System.Collections.Generic;

public class NodeDataPool
{
    private static NodeDataPool nodeDataPools;
    private readonly Stack<NodeData> poolableQueue;

    public NodeDataPool()
    {
        poolableQueue = new Stack<NodeData>();
    }

    public static NodeDataPool Get()
    {
        if (nodeDataPools == null)
            nodeDataPools = new NodeDataPool();
        return nodeDataPools;
    }

    public virtual void PushPool(NodeData poolObj)
    {
        poolableQueue.Push(poolObj);
    }

    public virtual NodeData PopPool()
    {
        return poolableQueue.Count > 0 ? poolableQueue.Pop() : new NodeData();
    }
}