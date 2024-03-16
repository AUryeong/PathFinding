using UnityEngine;

public class NodeManager : SingletonBehavior<NodeManager>
{
    [HideInInspector] public Vector2Int startNodePos;
    [HideInInspector] public Vector2Int endNodePos;

    private RectInt lastRect;

    public Graph graph;
    public PaintGraph paintGraph;

    public int visitDelay;
    public int discoveredDelay;

    public override void Init()
    {
        var rect = CameraManager.Instance.screenRectInt;
        graph = new Graph(Mathf.Max(rect.width + 13, rect.height + 13));
        graph.FillAll();

        paintGraph.Init();
        paintGraph.UpdatePaint();

        lastRect = rect;
    }

    #region Prev

    public void UpdateNodeByCamera()
    {
        var cameraManager = CameraManager.Instance;
        if (lastRect.Equals(cameraManager.screenRectInt)) return;

        var sizeX = graph.Size.x / 2;
        var sizeY = graph.Size.y / 2;

        var newRectInt = cameraManager.screenRectInt;

        int countX = Mathf.Abs(newRectInt.x) - sizeX + 1;
        if (countX > 0)
        {
            for (int i = 0; i < countX; i++)
            {
                graph.AddX(AddType.First);
            }

            paintGraph.UpdatePaint();
            return;
        }

        countX = newRectInt.x + newRectInt.width - sizeX + 2;
        if (countX > 0)
        {
            for (int i = 0; i < countX; i++)
            {
                graph.AddX(AddType.Last);
            }

            paintGraph.UpdatePaint();
            return;
        }
    }

    #endregion

    public Vector2Int GetTilePosByWorldPoint(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x + graph.Size.x / 2f + 0.5f);
        int y = Mathf.RoundToInt(pos.y + graph.Size.y / 2f + 0.5f);
        return new Vector2Int(x, y);
    }

    public NodeData GetNodeData(int x, int y)
    {
        return graph.GetNodeData(x, y);
    }

    public void StartPathFinding(PathFinding selectPathFinding)
    {
        selectPathFinding.StartPathFinding(graph, startNodePos, endNodePos).Forget();
    }

    public void ResetPathFinding(PathFinding selectPathFinding)
    {
        selectPathFinding.Stop();

        for (int i = 1; i <= graph.Size.y; i++)
        {
            for (int j = 1; j <= graph.Size.x; j++)
            {
                graph.GetNodeData(j, i).stateType = NodeStateType.None;
            }
        }
        
        paintGraph.UpdatePaint();
    }
}