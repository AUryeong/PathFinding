using UnityEngine;

public class NodeData
{
    public NodeType nodeType = NodeType.None;
    public NodeStateType stateType = NodeStateType.None;
    
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

public enum NodeStateType
{
    None,
    Discovered,
    Visited,
}

public enum AddType
{
    First,
    Last
}