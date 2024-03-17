using System;
using UnityEngine;

public class NodeData
{
    public NodeType nodeType = NodeType.None;
    public NodeStateType stateType = NodeStateType.None;
}

public enum NodeType
{
    None,
    Wall,
    Start,
    End
}

public enum NodeStateType
{
    None,
    Discovered,
    Visited
}

public enum AddType
{
    First,
    Last
}

public class Graph // 앞, 뒤 / 위, 아래 삽입이 자유로워야함
{
    private NodeData[][] nodeGraph;
    private readonly ClassPool<NodeData> nodePool;

    public int Capacity
    {
        get => nodeGraph.Length;
        set
        {
            int beforeCapacity = Capacity;
            if (value <= beforeCapacity) return;

            var newNodeGraph = new NodeData[value][];
            for (int index = 0; index < value; index++)
                newNodeGraph[index] = new NodeData[value];

            for (int i = 0; i < size.y; i++)
            {
                Array.Copy(nodeGraph[(startIndex.y + i) % Capacity], startIndex.x, newNodeGraph[i], 0, size.x - startIndex.x);
                if (startIndex.x != 0)
                    Array.Copy(nodeGraph[(startIndex.y + i) % Capacity], 0, newNodeGraph[i], size.x - startIndex.x, startIndex.x);
            }

            nodeGraph = newNodeGraph;

            startIndex = Vector2Int.zero;

            endIndex.x = Mathf.Min(beforeCapacity, size.x);
            endIndex.y = Mathf.Min(beforeCapacity, size.y);

            nodePool.CreatePoolObject(value * value - beforeCapacity * beforeCapacity);
        }
    }

    public Vector2Int Size => size;
    private Vector2Int size;

    public Vector2Int StartPos => startPos;
    private Vector2Int startPos;

    private Vector2Int startIndex;
    private Vector2Int endIndex;

    public Graph(int capacity)
    {
        nodeGraph = new NodeData[capacity][];
        for (int index = 0; index < capacity; index++)
            nodeGraph[index] = new NodeData[capacity];

        startIndex = Vector2Int.zero;
        endIndex = Vector2Int.zero;
        startPos = Vector2Int.zero;

        nodePool = new ClassPool<NodeData>()
            .CreatePoolObject(capacity * capacity);
        Capacity = capacity;
    }

    public void FillAll()
    {
        for (int i = 0; i < Capacity; i++)
        {
            for (int j = 0; j < Capacity; j++)
            {
                nodeGraph[i][j] = nodePool.PopPool();
            }
        }

        startPos.x = -Mathf.CeilToInt(Capacity / 2f);
        startPos.y = -Mathf.CeilToInt(Capacity / 2f);

        size.x = Capacity;
        size.y = Capacity;
    }

    public bool IsContainsPos(Vector2Int pos)
    {
        if (pos.x >= size.x + startPos.x || pos.x < startPos.x)
            return false;

        if (pos.y >= size.y + startPos.y || pos.y < startPos.y)
            return false;

        return true;
    }

    public NodeData GetNodeData(int x, int y)
    {
        return nodeGraph[(y + startIndex.y - startPos.y) % Capacity][(x + startIndex.x - startPos.x) % Capacity];
    }

    public void AddX(AddType type)
    {
        if (size.x >= Capacity)
            Capacity *= 2;

        switch (type)
        {
            case AddType.First:
            {
                startIndex.x = startIndex.x - 1 < 0 ? Capacity - 1 : startIndex.x - 1;
                for (int i = 0; i < size.y; i++)
                    nodeGraph[(i + startIndex.y) % Capacity][startIndex.x] = nodePool.PopPool();

                startPos.x--;
                break;
            }
            case AddType.Last:
            {
                for (int i = 0; i < size.y; i++)
                    nodeGraph[(i + startIndex.y) % Capacity][endIndex.x] = nodePool.PopPool();

                endIndex.x = (endIndex.x + 1) % Capacity;
                break;
            }
            default:
                return;
        }

        size.x++;
    }

    public void AddY(AddType type)
    {
        if (size.y >= Capacity)
            Capacity *= 2;

        switch (type)
        {
            case AddType.First:
            {
                startIndex.y = startIndex.y - 1 < 0 ? Capacity - 1 : startIndex.y - 1;
                for (int i = 0; i < size.x; i++)
                    nodeGraph[startIndex.y][(i + startIndex.x) % Capacity] = nodePool.PopPool();

                startPos.y--;
                break;
            }
            case AddType.Last:
            {
                for (int i = 0; i < size.x; i++)
                    nodeGraph[endIndex.y][(i + startIndex.x) % Capacity] = nodePool.PopPool();

                endIndex.y = (endIndex.y + 1) % Capacity;
                break;
            }
            default:
                return;
        }

        size.y++;
    }
}