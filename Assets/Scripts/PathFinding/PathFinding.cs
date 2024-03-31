using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PathFinding
{
    public static readonly PathFinding[] PATH_FINDINGS =
    {
        new PathFindingBFS(),
        new PathFindingDijkstra(),
        new PathFindingAStar(),
        new PathFindingJPS()
    };
    protected readonly NodeManager nodeManager;
    protected readonly InputManager inputManager;

    private LineRenderer lineRenderer;

    protected CancellationTokenSource cancellation;
    protected Graph nodeGraph;

    protected NodeData nodeEndData;
    protected NodeData nodeStartData;
    private static readonly int COLOR = Shader.PropertyToID("_Color");

    public abstract string Name { get; }
    protected abstract Color Color { get; }

    protected PathFinding()
    {
        nodeManager = NodeManager.Instance;
        inputManager = InputManager.Instance;
    }

    public async UniTaskVoid StartPathFinding(Graph graph, NodeData startData, NodeData endData)
    {
        nodeGraph = graph;

        nodeStartData = startData;
        nodeEndData = endData;

        if (lineRenderer == null)
        {
            lineRenderer = nodeManager.paintGraph.GetLineRenderer(this);
            var color = Color;
            color.a = 0.6f;
            lineRenderer.material.SetColor(COLOR, color);
        }

        cancellation = new CancellationTokenSource();

        await StartPathFinding();

        cancellation = null;
        Stop();
    }

    protected abstract UniTask StartPathFinding();

    protected void ShowPath()
    {
        ShowPathTask().Forget();
    }

    private async UniTaskVoid ShowPathTask()
    {
        var nodeData = nodeEndData;
        lineRenderer.positionCount = 0;
        while (true)
        {
            AddLine(nodeData.pos);
            await UniTask.Delay(nodeManager.visitDelay);

            if (nodeData == nodeStartData) break;
            nodeData = nodeData.parent;
        }
    }

    public virtual void Stop()
    {
        if (nodeGraph != null)
        {
            nodeGraph?.PushPoolNodeData();
            nodeGraph = null;
        }

        if (cancellation != null)
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }
    }

    protected void AddLine(Vector2Int pos)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, nodeManager.TilePosToGetWorldPoint(pos));
    }

    protected IEnumerable<Vector2Int> GetNeighBor(Vector2Int pos)
    {
        return GetNeighBor().Select(neighBor => neighBor + pos);
    }

    protected IEnumerable<Vector2Int> GetNeighBor()
    {
        yield return new Vector2Int(1, 0);
        yield return new Vector2Int(1, 1);
        yield return new Vector2Int(0, 1);
        yield return new Vector2Int(-1, 1);
        yield return new Vector2Int(0, -1);
        yield return new Vector2Int(-1,-1);
        yield return new Vector2Int(-1, 0);
        yield return new Vector2Int(1,-1);
    }
}