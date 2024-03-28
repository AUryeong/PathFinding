using System.Collections.Generic;
using UnityEngine;

public class NodeData
{
    public NodeType nodeType = NodeType.None;
    
    public Vector2Int pos;
    public NodeData parent;

    public float Weight => gWeight + hWeight;
    public float gWeight = float.MaxValue;
    public float hWeight = 0;
}

public enum NodeType
{
    None,
    Wall,
    Start,
    End
}

public enum AddType
{
    First,
    Last
}