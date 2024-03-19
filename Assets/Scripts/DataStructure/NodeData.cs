using UnityEngine;

public class NodeData
{
    public NodeType nodeType = NodeType.None;
    public NodeStateType stateType = NodeStateType.None;
    
    public Vector2Int pos;
    public NodeData parent;
    public float weight = float.MaxValue;
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