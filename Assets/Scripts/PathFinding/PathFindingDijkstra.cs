using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingDijkstra : PathFinding
{
    protected PriorityQueue nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;
    public override string Name => "Dijkstra";

    protected override Color Color => Color.red;

    public override void Stop()
    {
        base.Stop();
        nodeDataHashSet.Clear();
        nodeDataQueue.Clear();
    }
    protected override async UniTask StartPathFinding()
    {
        nodeDataHashSet ??= new HashSet<NodeData>();
        nodeDataQueue ??= new PriorityQueue();
        
        nodeStartData.gWeight = 0;
        nodeEndData.hWeight = 0;

        nodeDataQueue.Enqueue(nodeStartData);

        while (nodeDataQueue.Size > 0)
        {
            var nodeData = nodeDataQueue.Dequeue();
            if (nodeData == nodeEndData)
            {
                ShowPath();
                break;
            }

            if (nodeDataHashSet.Contains(nodeData)) continue;

            nodeDataHashSet.Add(nodeData);

            foreach (var neighborPos in GetNeighBor(nodeData.pos))
                await CheckAndEnqueueNode(nodeData, neighborPos);

            AddLine(nodeData.pos);

            await UniTask.Delay(nodeManager.visitDelay, cancellationToken: cancellation.Token);
        }
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