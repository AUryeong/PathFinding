using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class PathFindingAStar : PathFindingDijkstra
{
    public override string Name => "AStar";

    private void SetWeight(int value)
    {
        weight = value;
    }

    private void SetHeuristicType(HeuristicType type)
    {
        heuristicType = type;
    }

    protected override float GetWeight(NodeData nodeData)
    {
        if (heuristicType == HeuristicType.Euclidean)
        {
            return Vector2Int.Distance(nodeData.pos, nodeEndData.pos) * weight;
        }

        return (Mathf.Abs(nodeData.pos.x - nodeEndData.pos.x) + Mathf.Abs(nodeData.pos.y - nodeEndData.pos.y)) * weight;
    }
}