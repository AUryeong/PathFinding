using UnityEngine;

public class NodeData
{
    public NodeType nodeType = NodeType.None;
    
    public Vector2Int pos;
    public NodeData parent;

    public Vector2Int direction;

    public float Weight => gWeight + hWeight;
    public float gWeight;
    public float hWeight;

    public void Reset()
    {
        parent = null;
        
        gWeight = float.MaxValue;
        hWeight = 0;
    }
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