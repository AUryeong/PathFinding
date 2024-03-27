using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class PathFindingAStar : PathFindingDijkstra
{
    public override string Name => "A*";
    private InputManager inputManager;

    protected override float GetWeight(NodeData nodeData)
    {
        if (inputManager == null)
            inputManager = InputManager.Instance;

        if (inputManager.heuristicType == HeuristicType.Euclidean)
            return Vector2Int.Distance(nodeData.pos, nodeEndData.pos) * inputManager.weight;
        else
            return (Mathf.Abs(nodeData.pos.x - nodeEndData.pos.x) + Mathf.Abs(nodeData.pos.y - nodeEndData.pos.y)) * inputManager.weight;
    }
}