using System;
using System.Text;
using UnityEngine;

public class Graph // 앞, 뒤 / 위, 아래 삽입이 자유로워야함
{
    private NodeData[][] nodeGraph;
    private NodeDataPool nodePool;

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

        nodePool = NodeDataPool.Get();
    }

    public Graph Copy()
    {
        var graph = new Graph(Capacity);
        for (int i = 0; i < Capacity; i++)
        {
            for (int j = 0; j < Capacity; j++)
            {
                var copyNodeData = nodeGraph[i][j];
                if (copyNodeData == null) continue;

                var pasteNodeData = nodePool.PopPool();

                pasteNodeData.nodeType = copyNodeData.nodeType;
                pasteNodeData.pos = copyNodeData.pos;
                
                pasteNodeData.parent = null;
                pasteNodeData.hWeight = 0;
                pasteNodeData.gWeight = float.MaxValue;

                graph.nodeGraph[i][j] = pasteNodeData;
            }
        }

        graph.startPos = startPos;
        graph.startIndex = startIndex;
        graph.endIndex = endIndex;
        graph.size = size;

        return graph;
    }

    public void PushPoolNodeData()
    {
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                nodePool.PushPool(GetNodeDataByIndex(j, i));
            }
        }
    }

    public void FillAll()
    {
        startPos = Vector2Int.one * -Mathf.CeilToInt(Capacity / 2f);
        size = Vector2Int.one * Capacity;

        for (int i = 0; i < Capacity; i++)
        {
            for (int j = 0; j < Capacity; j++)
            {
                var nodeData = nodePool.PopPool();
                nodeData.pos = new Vector2Int(j + startPos.x, i + startPos.y);
                nodeGraph[i][j] = nodeData;
            }
        }
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

    public NodeData GetNodeDataByIndex(int x, int y)
    {
        return nodeGraph[(y + startIndex.y) % Capacity][(x + startIndex.x) % Capacity];
    }

    public void AddX(AddType type)
    {
        if (size.x >= Capacity)
            Capacity *= 2;

        switch (type)
        {
            case AddType.First:
                {
                    startPos.x--;
                    startIndex.x = startIndex.x - 1 < 0 ? Capacity - 1 : startIndex.x - 1;

                    for (int i = 0; i < size.y; i++)
                    {
                        var nodeData = nodePool.PopPool();
                        nodeData.pos = new Vector2Int(startPos.x, startPos.y + i);
                        nodeGraph[(i + startIndex.y) % Capacity][startIndex.x] = nodeData;
                    }
                    break;
                }
            case AddType.Last:
                {
                    for (int i = 0; i < size.y; i++)
                    {
                        var nodeData = nodePool.PopPool();
                        nodeData.pos = new Vector2Int(startPos.x + size.x, startPos.y+i);
                        nodeGraph[(i + startIndex.y) % Capacity][endIndex.x] = nodeData;
                    }

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
                    startPos.y--;
                    startIndex.y = startIndex.y - 1 < 0 ? Capacity - 1 : startIndex.y - 1;

                    for (int i = 0; i < size.x; i++)
                    {
                        var nodeData = nodePool.PopPool();
                        nodeData.pos = new Vector2Int(startPos.x + i, startPos.y);
                        nodeGraph[startIndex.y][(i + startIndex.x) % Capacity] = nodeData;
                    }
                    break;
                }
            case AddType.Last:
                {
                    for (int i = 0; i < size.x; i++)
                    {
                        var nodeData = nodePool.PopPool();
                        nodeData.pos = new Vector2Int(startPos.x + i, startPos.y + size.y);
                        nodeGraph[endIndex.y][(i + startIndex.x) % Capacity] = nodeData;
                    }

                    endIndex.y = (endIndex.y + 1) % Capacity;
                    break;
                }
            default:
                return;
        }
        size.y++;
    }
}