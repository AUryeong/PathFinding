using UnityEngine;

public class NodeManager : SingletonBehavior<NodeManager>
{
    public NodeData startNodeData;
    public NodeData endNodeData;

    public PaintGraph paintGraph;

    [Header("Value")]
    public bool isPathFinding;

    public Graph originGraph;

    public int visitDelay;
    public int discoveredDelay;

    private const int NODE_CREATE_RANGE = 2;

    public override void Init()
    {
        isPathFinding = false;

        var rect = CameraManager.Instance.screenRectInt;
        originGraph = new Graph(Mathf.Max(rect.width + 13, rect.height + 13));
        originGraph.FillAll();

        paintGraph.Init(originGraph);
        paintGraph.UpdatePaint();

        UpdateNodeByCamera();
        ResetPathFinding();
    }

    public void UpdateNodeByCamera()
    {
        if (isPathFinding) return;

        var rectInt = CameraManager.Instance.screenRectInt;
        var prevGraphSize = originGraph.Size;

        for (int i = 0; i < originGraph.StartPos.x - rectInt.x + NODE_CREATE_RANGE; i++)
            originGraph.AddX(AddType.First);

        for (int i = 0; i < rectInt.x + rectInt.width - (originGraph.StartPos.x + originGraph.Size.x) + NODE_CREATE_RANGE; i++)
            originGraph.AddX(AddType.Last);

        for (int i = 0; i < originGraph.StartPos.y - rectInt.y + NODE_CREATE_RANGE; i++)
            originGraph.AddY(AddType.First);

        for (int i = 0; i < rectInt.y + rectInt.height - (originGraph.StartPos.y + originGraph.Size.y) + NODE_CREATE_RANGE; i++)
            originGraph.AddY(AddType.Last);

        if (!prevGraphSize.Equals(originGraph.Size))
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

        if (startNodeData == null) return;
        if (endNodeData == null) return;

        isPathFinding = true;
        paintGraph.ResetLineRenderers();
        paintGraph.UpdatePaint();

        foreach (var pathFinding in InputManager.Instance.GetSelectPathFinding())
        {
            var copyGraph = originGraph.Copy();
            var copyStartNodeData = copyGraph.GetNodeData(startNodeData.pos.x, startNodeData.pos.y);
            var copyEndNodeData = copyGraph.GetNodeData(endNodeData.pos.x, endNodeData.pos.y);

            pathFinding.StartPathFinding(copyGraph, copyStartNodeData, copyEndNodeData).Forget();
        }
    }

    public void ResetPathFinding()
    {
        if (isPathFinding)
        {
            foreach (var pathFinding in InputManager.Instance.GetSelectPathFinding())
            {
                pathFinding.Stop();
            }
            
            for (int i = 0; i < originGraph.Size.y; i++)
            {
                for (int j = 0; j < originGraph.Size.x; j++)
                {
                    originGraph.GetNodeDataByIndex(j, i).Reset();
                }
            }
            isPathFinding = false;
        }

        UpdateNodeByCamera();


        paintGraph.ResetLineRenderers();
        paintGraph.UpdatePaint();
    }
}