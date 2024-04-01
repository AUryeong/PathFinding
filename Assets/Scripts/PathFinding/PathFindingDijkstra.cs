using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingDijkstra : PathFinding
{
    public override string Name => "Dijkstra";

    protected override Color Color => Color.red;

    protected PriorityQueue nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;

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

        float weight = originData.gWeight + GetWeight(originData, nodeData);
        if (nodeData.gWeight > weight)
        {
            nodeData.parent = originData;
            nodeData.gWeight = weight;
            nodeDataQueue.Enqueue(nodeData);
        }

        await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);
    }
    protected float GetWeight(NodeData startNodeData, NodeData endNodeData)
    {
        float weight;
        switch (inputManager.heuristicType)
        {
            default:
            case HeuristicType.Euclidean:
                weight = Vector2Int.Distance(startNodeData.pos, endNodeData.pos);
                break;
            case HeuristicType.Manhattan:
                weight = Mathf.Abs(startNodeData.pos.x - endNodeData.pos.x) + Mathf.Abs(startNodeData.pos.y - endNodeData.pos.y);
                break;
            case HeuristicType.ChebyShev:
                weight = Mathf.Max(startNodeData.pos.x - endNodeData.pos.x, startNodeData.pos.y - endNodeData.pos.y);
                break;
        }
        return weight * inputManager.weight;
    }
}