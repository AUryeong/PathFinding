using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingAStar : PathFindingDijkstra
{
    public override string Name => "AStar";
    
    protected override float GetWeight(NodeData nodeData)
    {
        return Vector2Int.Distance(nodeData.pos, nodeEndData.pos);
    }
    
    protected override async UniTask CheckAndEnqueueNode(NodeData originData, Vector2Int pos)
    {
        if (!nodeGraph.IsContainsPos(pos)) return;

        var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);

        if (nodeData.nodeType == NodeType.Wall) return;

        if (nodeData.stateType != NodeStateType.Discovered)
            nodeData.stateType = NodeStateType.Visited;

        NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y, nodeData);

        float weight = GetWeight(nodeData);
        nodeData.weight = weight;
        nodeDataQueue.Enqueue(nodeData, weight);

        await UniTask.Delay(NodeManager.Instance.discoveredDelay, cancellationToken: cancellation.Token);
    }
}