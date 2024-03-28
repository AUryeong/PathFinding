using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingBFS : PathFinding
{
    private Queue<NodeData> nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;
    public override string Name => "BFS";
    public override Color Color => Color.yellow;

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
        nodeDataQueue ??= new Queue<NodeData>();

        nodeDataQueue.Enqueue(startData);
        nodeDataHashSet.Add(startData);

        while (nodeDataQueue.Count > 0)
        {
            var nodeData = nodeDataQueue.Dequeue();
            if (nodeData == nodeEndData)
            {
                isFind = true;
                break;
            }

            foreach (var neighborPos in GetNeighBor(nodeData.pos))
                await CheckAndEnqueueNode(nodeData, neighborPos);

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, nodeManager.TilePosToGetWorldPoint(nodeData.pos));

            await UniTask.Delay(nodeManager.visitDelay, cancellationToken: cancellation.Token);
        }

        cancellation = null;
        Stop();
    }

    private async UniTask CheckAndEnqueueNode(NodeData originData, Vector2Int pos)
    {
        if (!nodeGraph.IsContainsPos(pos)) return;

        var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);
        if (nodeDataHashSet.Contains(nodeData)) return;

        nodeDataHashSet.Add(nodeData);

        if (nodeData.nodeType == NodeType.Wall) return;

        nodeData.parent = originData;
        nodeDataQueue.Enqueue(nodeData);

        await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);
    }
}