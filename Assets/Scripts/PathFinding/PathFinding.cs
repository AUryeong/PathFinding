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

    protected CancellationTokenSource cancellation;

    protected NodeManager nodeManager;

    public abstract string Name { get; }
    public abstract Color Color { get; }

    protected Graph nodeGraph;

    protected NodeData nodeEndData;
    protected NodeData nodeStartData;

    protected LineRenderer lineRenderer;

    protected bool isFind = false;

    public abstract UniTaskVoid StartPathFinding(Graph graph, NodeData startData, NodeData endData);

    public virtual void Stop()
    {
        if (isFind)
        {
            var nodeData = nodeEndData;
            var points = new List<Vector3>();
            while (true)
            {
                points.Add(NodeManager.Instance.TilePosToGetWorldPoint(nodeData.pos));
                if (nodeData == nodeStartData) break;

                nodeData = nodeData.parent;
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());

            isFind = false;
        }

        if (nodeGraph != null)
        {
            nodeGraph.PushPoolNodeData();
            nodeGraph = null;
        }

        if (cancellation != null)
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }
    }

    protected virtual IEnumerable<Vector2Int> GetNeighBor(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y - 1);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
    }
}