using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PathFinding
{
    protected CancellationTokenSource cancellation;
    public abstract string Name { get; }

    protected Graph nodeGraph;

    protected NodeData nodeEndData;
    protected NodeData nodeStartData;

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

            NodeManager.Instance.paintGraph.lineRenderer.positionCount = points.Count;
            NodeManager.Instance.paintGraph.lineRenderer.SetPositions(points.ToArray());
        }

        if (cancellation != null)
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }

        NodeManager.Instance.isPathFinding = false;
    }

    protected virtual IEnumerable<Vector2Int> GetNeighBor(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y - 1);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
    }
}