using Cysharp.Threading.Tasks;
using UnityEngine;


public class PathFindingAStar : PathFindingDijkstra
{
    public override string Name => "A*";
    protected override Color Color => Color.green;

    protected override async UniTask CheckAndEnqueueNode(NodeData originData, Vector2Int pos)
    {
        if (!nodeGraph.IsContainsPos(pos)) return;

        var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);

        if (nodeData.nodeType == NodeType.Wall) return;

        float weight = originData.Weight + 1;
        if (nodeData.gWeight > weight)
        {
            nodeData.parent = originData;
            nodeData.gWeight = originData.gWeight + 1;
            nodeData.hWeight = GetHeuristicWeight(nodeData);
            nodeDataQueue.Enqueue(nodeData);
        }

        await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);
    }

    private float GetHeuristicWeight(NodeData nodeData)
    {
        float weight;
        switch (inputManager.heuristicType)
        {
            default:
            case HeuristicType.Euclidean:
                weight = Vector2Int.Distance(nodeData.pos, nodeEndData.pos);
                break;
            case HeuristicType.Manhattan:
                weight = Mathf.Abs(nodeData.pos.x - nodeEndData.pos.x) + Mathf.Abs(nodeData.pos.y - nodeEndData.pos.y);
                break;
            case HeuristicType.ChebyShev:
                weight = Mathf.Max(nodeData.pos.x - nodeEndData.pos.x, nodeData.pos.y - nodeEndData.pos.y);
                break;
        }
        return weight * inputManager.weight;
    }
}