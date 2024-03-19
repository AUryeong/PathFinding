using System;
using UnityEngine;

public class PriorityQueue
{
    private NodeData[] priorityNodes;
    private int Capacity => priorityNodes.Length-1;

    public int Size { get; private set; }
    private int RootIndex => 1;

    public PriorityQueue(int capacity)
    {
        priorityNodes = new NodeData[capacity+1];
    }

    public void Clear()
    {
        if (Size != 0)
            Array.Clear(priorityNodes, 1, Size);

        Size = 0;
    }

    public void Enqueue(NodeData priorityNodeData)
    {
        if (Size >= Capacity)
        {
            var newArray = new NodeData[Capacity * 2+1];
            Array.Copy(priorityNodes, 1, newArray, 1, Size);
            priorityNodes = newArray;
        }

        Size++;
        priorityNodes[Size] = priorityNodeData;
        int index = Size;

        while (true)
        {
            int parentNode = index / 2;
            if (parentNode == 0) break;
            if (priorityNodes[parentNode].Weight <= priorityNodeData.Weight) break;

            priorityNodes[index] = priorityNodes[parentNode];
            priorityNodes[parentNode] = priorityNodeData;
            index = parentNode;
        }
    }

    public NodeData Dequeue()
    {
        var priorityNodeData = priorityNodes[Size];
        var result = priorityNodes[RootIndex];
        priorityNodes[RootIndex] = priorityNodeData;
        Size--;
        
        int index = RootIndex;

        while (true)
        {
            int findIndex = index * 2;
            if (findIndex >= Size) break;

            int leftChildNodeIndex = findIndex;
            int rightChildNodeIndex = findIndex + 1;

            var leftChildNode = priorityNodes[leftChildNodeIndex];
            var rightChildNode = priorityNodes[rightChildNodeIndex];

            if (leftChildNode.Weight > priorityNodeData.Weight && rightChildNode.Weight > priorityNodeData.Weight) break;
            int childNodeIndex = leftChildNode.Weight < rightChildNode.Weight ? leftChildNodeIndex : rightChildNodeIndex;

            priorityNodes[index] = priorityNodes[childNodeIndex];
            priorityNodes[childNodeIndex] = priorityNodeData;
            index = childNodeIndex;
        }

        return result;
    }
}