using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class NodeData
{
    public NodeType nodeType = NodeType.None;
    public NodeStateType stateType = NodeStateType.None;
    public int x;
    public int y;
}

public enum NodeType
{
    None,
    Wall,
    Start,
    Finish
}

public enum NodeStateType
{
    None,
    Discovered,
    Visited
}

public class NodeManager : SingletonBehavior<NodeManager>
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private NodeObject originNodeObject;
    private ObjectPool<NodeObject> nodeObjectPool;

    private NodeData[,] nodeGraph;

    private List<List<NodeObject>> nodeGraphObject;
    [FormerlySerializedAs("endNodeObjectList")]
    [HideInInspector] public NodeObject endNodeObject;
    [HideInInspector] public List<NodeObject> startNodeObjetList = new();

    public RectInt lastRect;

    public void Init()
    {
        nodeObjectPool = new ListableObjectPool<NodeObject>(originNodeObject);
        nodeObjectPool
            .AddAction(ObjectPoolActionType.Pop, (nodeObject) => { nodeObject.gameObject.SetActive(true); })
            .AddAction(ObjectPoolActionType.Pool, (nodeObject) => { nodeObject.gameObject.SetActive(false); })
            .CreatePoolObject(100);

        InstantiateNodeByCamera();
    }

    public void UpdateNodeByCamera()
    {
        // if (cameraManager.screenRect == lastRect) return;
        //
        // lastRect = cameraManager.screenRect;
        //
        // int startIndexX = Mathf.FloorToInt(cameraManager.screenRect.x) - 1;
        // int lengthX = Mathf.CeilToInt(cameraManager.screenRect.width) + 3;
        //
        // int startIndexY = Mathf.FloorToInt(cameraManager.screenRect.y) - 1;
        // int lengthY = Mathf.CeilToInt(cameraManager.screenRect.height) + 3;
        //
        // int countX = startIndexX - lastStartIndexX;
        // if (countX != 0)
        // {
        //     if (countX > 0)
        //     {
        //         for (int i = 0; i < countX; i++)
        //         {
        //             for (int j = 0; j < lengthY; j++)
        //             {
        //                 nodeObjectPool.PushPool(nodeGraphObject[j][i]);
        //                 nodeGraphObject[j].RemoveAt(i);
        //                 
        //                 var node = nodeGraphObject[j][i];
        //                 nodeGraphObject[j].Insert(0, node);
        //             }
        //         }
        //
        //         lastStartIndexX = startIndexX;
        //     }
        // }
    }

    private void InstantiateNodeByCamera()
    {
        lastRect = cameraManager.screenRectInt;

        int startIndexX = lastRect.x - 1;
        int lengthX = lastRect.width + 3;

        int startIndexY = lastRect.y - 1;
        int lengthY = lastRect.height + 3;

        nodeGraph = new NodeData[lengthY, lengthX];
        nodeGraphObject = new List<List<NodeObject>>(lengthY);
        for (int i = 0; i < lengthY; i++)
        {
            var list = new List<NodeObject>(lengthX);
            nodeGraphObject.Add(list);
            for (int j = 0; j < lengthX; j++)
            {
                var nodeData = new NodeData()
                {
                    y = i,
                    x = j
                };
                nodeGraph[i, j] = nodeData;

                var nodeObject = nodeObjectPool.PopPool();
                nodeObject.SetData(startIndexX + j, startIndexY + i);
                nodeObject.NodeData = nodeData;

                list.Add(nodeObject);
            }
        }
    }

    public NodeObject GetNodeObject(int x, int y)
    {
       return nodeGraphObject[y][x];
    }

    public void ResetNodeState()
    {
        foreach (var node in nodeGraphObject.SelectMany(nodeList => nodeList))
        {
            node.NodeStateType = NodeStateType.None;
        }
    }

    public void StartPathFinding(PathFinding selectPathFinding)
    {
        selectPathFinding.StartPathFinding(startNodeObjetList[0], endNodeObject, nodeGraphObject);
    }
}