using System;
using TMPro;
using UnityEngine;

public class NodeObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshPro text;
    public NodeData NodeData
    {
        get => nodeDataData;
        set
        {
            nodeDataData = value;
            text.text = $"{nodeDataData.pos.x},{nodeDataData.pos.y}";
        }
    }
    private NodeData nodeDataData;

    public NodeType NodeType
    {
        get => nodeDataData.nodeType;
        set
        {
            NodeType prevNodeType = NodeData.nodeType;
            if (prevNodeType == value) return;
            
            switch (prevNodeType)
            {
                case NodeType.Start:
                    NodeManager.Instance.startNodeObjetList.Remove(this);
                    break;
                case NodeType.Finish:
                    NodeManager.Instance.endNodeObject = null;
                    break;
            }
            
            NodeData.nodeType = value;
            switch (NodeData.nodeType)
            {
                case NodeType.Wall:
                    spriteRenderer.color = Color.gray;
                    break;
                case NodeType.Start:
                    spriteRenderer.color = Color.green;
                    NodeManager.Instance.startNodeObjetList.Add(this);
                    break;
                case NodeType.Finish:
                    spriteRenderer.color = Color.red;
                    NodeManager.Instance.endNodeObject = this;
                    break;
                case NodeType.None:
                default:
                    spriteRenderer.color = Color.white;
                    break;
            }
        }
    }

    public NodeStateType NodeStateType
    {
        get => nodeDataData.stateType;
        set
        {
            if (NodeData.stateType == value) return;
            
            NodeData.stateType = value;
            switch (NodeData.stateType)
            {
                case NodeStateType.Discovered:
                    spriteRenderer.color = Color.yellow;
                    break;
                case NodeStateType.Visited:
                    spriteRenderer.color = Color.cyan;
                    break;
                default:
                case NodeStateType.None:
                    switch (NodeData.nodeType)
                    {
                        case NodeType.Wall:
                            spriteRenderer.color = Color.gray;
                            break;
                        case NodeType.Start:
                            spriteRenderer.color = Color.green;
                            NodeManager.Instance.startNodeObjetList.Add(this);
                            break;
                        case NodeType.Finish:
                            spriteRenderer.color = Color.red;
                            NodeManager.Instance.endNodeObject = this;
                            break;
                        case NodeType.None:
                        default:
                            spriteRenderer.color = Color.white;
                            break;
                    }
                    break;
            }
        }
    }

    public void SetData(int x, int y)
    {
        text.text = $"({x},{y})";
        transform.position = new Vector3(x, y);
    }
}