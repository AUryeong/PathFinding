using UnityEngine;

public class NodeManager : SingletonBehavior<NodeManager>
{
    [HideInInspector] public Vector2Int startNodePos;
    [HideInInspector] public NodeData startNodeData;

    [HideInInspector] public Vector2Int endNodePos;
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

    public Vector2Int GetTilePosByWorldPoint(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x - 0.5f);
        int y = Mathf.RoundToInt(pos.y - 0.5f);
        return new Vector2Int(x, y);
    }

    public void StartPathFinding(PathFinding selectPathFinding)
    {
        isPathFinding = true;
        selectPathFinding.StartPathFinding(graph, startNodePos, endNodePos).Forget();
    }

    public void ResetPathFinding(PathFinding selectPathFinding)
    {
        if (isPathFinding)
            selectPathFinding.Stop();

        for (int i = 0; i < graph.Size.y; i++)
        {
            for (int j = 0; j < graph.Size.x; j++)
            {
                graph.GetNodeData(j + graph.StartPos.x, i + graph.StartPos.y).stateType = NodeStateType.None;
            }
        }

        paintGraph.UpdatePaint();
        isPathFinding = false;
    }
}