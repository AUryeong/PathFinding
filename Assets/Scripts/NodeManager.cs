using UnityEngine;

public class NodeManager : SingletonBehavior<NodeManager>
{
    [HideInInspector] public NodeData startNodeData;
    [HideInInspector] public NodeData endNodeData;

    public PaintGraph paintGraph;

    [Header("Value")]
    public bool isPathFinding;

    public Graph graph;

    public int visitDelay;
    public int discoveredDelay;

    private const int NODE_CREATE_RANGE = 2;

    public override void Init()
    {
        isPathFinding = false;

        var rect = CameraManager.Instance.screenRectInt;
        graph = new Graph(Mathf.Max(rect.width + 13, rect.height + 13));
        graph.FillAll();

        paintGraph.Init(graph);
        paintGraph.UpdatePaint();

        UpdateNodeByCamera();
        ResetPathFinding();
    }

    public void UpdateNodeByCamera()
    {
        var rectInt = CameraManager.Instance.screenRectInt;
        var prevGraphSize = graph.Size;

        for (int i = 0; i < graph.StartPos.x - rectInt.x + NODE_CREATE_RANGE; i++)
            graph.AddX(AddType.First);

        for (int i = 0; i < rectInt.x + rectInt.width - (graph.StartPos.x + graph.Size.x) + NODE_CREATE_RANGE; i++)
            graph.AddX(AddType.Last);

        for (int i = 0; i < graph.StartPos.y - rectInt.y + NODE_CREATE_RANGE; i++)
            graph.AddY(AddType.First);

        for (int i = 0; i < rectInt.y + rectInt.height - (graph.StartPos.y + graph.Size.y) + NODE_CREATE_RANGE; i++)
            graph.AddY(AddType.Last);

        if (!prevGraphSize.Equals(graph.Size))
            paintGraph.UpdatePaint();
    }

    public Vector2Int GetWorldPointToTilePos(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x - 0.5f);
        int y = Mathf.RoundToInt(pos.y - 0.5f);
        return new Vector2Int(x, y);
    }
    public Vector3 TilePosToGetWorldPoint(Vector2Int pos)
    {
        return new Vector3(pos.x + 0.5f, pos.y + 0.5f);
    }

    public void StartPathFinding()
    {
        if (isPathFinding) return;

        isPathFinding = true;
        paintGraph.ResetLineRenderers();

        if (startNodeData == null) return;
        if (endNodeData == null) return;

        foreach (var pathFinding in InputManager.Instance.GetSelectPathFinding())
        {
            pathFinding.StartPathFinding(graph, startNodeData, endNodeData).Forget();
        }
    }

    public void ResetPathFinding()
    {
        if (isPathFinding)
        {
            foreach (var pathFinding in InputManager.Instance.GetSelectPathFinding())
            {
                Debug.Log(pathFinding.Name);
                pathFinding?.Stop();
            }
        }

        paintGraph.ResetLineRenderers();

        for (int i = 0; i < graph.Size.y; i++)
        {
            for (int j = 0; j < graph.Size.x; j++)
            {
                var nodeData = graph.GetNodeDataByIndex(j, i);
                nodeData.stateType = NodeStateType.None;
                nodeData.gWeight = int.MaxValue;
                nodeData.parent = null;
                nodeData.pos = new Vector2Int(j + graph.StartPos.x, i + graph.StartPos.y);
            }
        }

        paintGraph.UpdatePaint();
        isPathFinding = false;
    }
}