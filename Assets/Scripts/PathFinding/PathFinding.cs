using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PathFinding
{
    public static readonly PathFinding[] pathFindings =
    {
        new PathFindingBFS(),
        new PathFindingDijkstra(),
        new PathFindingAStar()
    };
    protected NodeManager nodeManager;
    protected InputManager inputManager;

    protected LineRenderer lineRenderer;

    protected CancellationTokenSource cancellation;
    protected Graph nodeGraph;

    protected NodeData nodeEndData;
    protected NodeData nodeStartData;

    public abstract string Name { get; }
    protected abstract Color Color { get; }

    public PathFinding()
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
            lineRenderer.material.SetColor("_Color", color);
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

    protected virtual IEnumerable<Vector2Int> GetNeighBor(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y - 1);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
    }
}