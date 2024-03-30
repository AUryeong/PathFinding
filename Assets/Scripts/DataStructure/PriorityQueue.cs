using System;

public class PriorityQueue
{
    private WeightNodeData[] priorityNodes;
    private int Capacity => priorityNodes.Length - 1;

    public int Size { get; private set; }
    
    private const int ROOT_INDEX = 1;

    public PriorityQueue(int capacity = 10)
    {
        priorityNodes = new WeightNodeData[capacity + 1];
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
            var newArray = new WeightNodeData[Capacity * 2 + 1];
            Array.Copy(priorityNodes, 1, newArray, 1, Size);
            priorityNodes = newArray;
        }
        
        var weightNodeData = new WeightNodeData()
        {
            value = priorityNodeData,
            weight = priorityNodeData.Weight
        };

        Size++;
        priorityNodes[Size] = weightNodeData;
        
        int index = Size;

        while (true)
        {
            int parentNode = index / 2;
            if (parentNode == 0) break;
            if (priorityNodes[parentNode].weight <= weightNodeData.weight) break;

            priorityNodes[index] = priorityNodes[parentNode];
            priorityNodes[parentNode] = weightNodeData;
            index = parentNode;
        }
    }

    public NodeData Dequeue()
    {
        var weightNodeData = priorityNodes[Size];
        var result = priorityNodes[ROOT_INDEX];
        priorityNodes[ROOT_INDEX] = weightNodeData;
        Size--;

        int index = ROOT_INDEX;
        while (true)
        {
            int findIndex = index * 2;
            if (findIndex >= Size) break;

            int leftChildNodeIndex = findIndex;
            int rightChildNodeIndex = findIndex + 1;

            var leftChildWeightNodeData = priorityNodes[leftChildNodeIndex];
            var rightChildWeightNodeData = priorityNodes[rightChildNodeIndex];

            if (leftChildWeightNodeData.weight > weightNodeData.weight && rightChildWeightNodeData.weight > weightNodeData.weight) break;
            int childNodeIndex = leftChildWeightNodeData.weight < rightChildWeightNodeData.weight ? leftChildNodeIndex : rightChildNodeIndex;

            priorityNodes[index] = priorityNodes[childNodeIndex];
            priorityNodes[childNodeIndex] = weightNodeData;
            index = childNodeIndex;
        }

        return result.value;
    }

    private struct WeightNodeData
    {
        public NodeData value;
        public float weight;
    }
}