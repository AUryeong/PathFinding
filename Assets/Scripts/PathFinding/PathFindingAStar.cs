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

        float weight = originData.gWeight + GetWeight(originData, nodeData);
        if (weight < nodeData.gWeight)
        {
            nodeData.parent = originData;
            nodeData.gWeight = weight;
            nodeData.hWeight = GetWeight(nodeData, nodeEndData);
            nodeDataQueue.Enqueue(nodeData);
        }

        await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);
    }
}