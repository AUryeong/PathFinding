using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class PathFindingAStar : PathFindingDijkstra
{
    public override string Name => "A*";
    private InputManager inputManager;
    public override Color Color => Color.green;

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

            if (nodeData.stateType != NodeStateType.Discovered)
                nodeData.stateType = NodeStateType.Visited;

            nodeManager.paintGraph.UpdateUV(pos.x, pos.y, nodeData);
        }

        await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);
    }

    private float GetHeuristicWeight(NodeData nodeData)
    {
        if (inputManager == null)
            inputManager = InputManager.Instance;

        if (inputManager.heuristicType == HeuristicType.Euclidean)
            return Vector2Int.Distance(nodeData.pos, nodeEndData.pos) * inputManager.weight;
        else
            return (Mathf.Abs(nodeData.pos.x - nodeEndData.pos.x) + Mathf.Abs(nodeData.pos.y - nodeEndData.pos.y)) * inputManager.weight;
    }
}