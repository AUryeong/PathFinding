using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingDijkstra : PathFinding
{
    protected PriorityQueue nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;
    public override string Name => "Dijkstra";

    public override Color Color => Color.red;

    public override void Stop()
    {
        base.Stop();
        nodeDataHashSet.Clear();
        nodeDataQueue.Clear();
    }

    public override async UniTaskVoid StartPathFinding(Graph graph, NodeData startData, NodeData endData)
    {
        nodeManager = NodeManager.Instance;

        nodeGraph = graph;

        nodeStartData = startData;
        nodeEndData = endData;

        lineRenderer = nodeManager.paintGraph.GetLineRenderer(this);

        cancellation = new CancellationTokenSource();
        nodeDataHashSet ??= new HashSet<NodeData>();
        nodeDataQueue ??= new PriorityQueue();
        
        startData.gWeight = 0;
        startData.hWeight = 0;

        nodeDataQueue.Enqueue(startData);

        while (nodeDataQueue.Size > 0)
        {
            var nodeData = nodeDataQueue.Dequeue();
            if (nodeData == endData)
            {
                isFind = true;
                break;
            }

            if (nodeDataHashSet.Contains(nodeData)) continue;

            nodeDataHashSet.Add(nodeData);

            foreach (var neighborPos in GetNeighBor(nodeData.pos))
                await CheckAndEnqueueNode(nodeData, neighborPos);

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, nodeManager.TilePosToGetWorldPoint(nodeData.pos));

            await UniTask.Delay(nodeManager.visitDelay, cancellationToken: cancellation.Token);
        }

        cancellation = null;
        Stop();
    }

    protected virtual async UniTask CheckAndEnqueueNode(NodeData originData, Vector2Int pos)
    {
        if (!nodeGraph.IsContainsPos(pos)) return;

        var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);

        if (nodeData.nodeType == NodeType.Wall) return;

        float weight = originData.gWeight + 1;
        if (nodeData.gWeight > weight)
        {
            nodeData.parent = originData;
            nodeData.gWeight = weight;
            nodeDataQueue.Enqueue(nodeData);
        }

        await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);
    }
}