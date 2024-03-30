using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingJPS : PathFinding
{
    private struct DirectionCondition
    {
        public readonly Vector2Int directionBlankPos;
        public readonly Vector2Int directionWallPos;

        private static readonly Vector2Int DIRECTION_UP = new(0, 1);
        private static readonly Vector2Int DIRECTION_RIGHT_UP = new(1, 1);
        private static readonly Vector2Int DIRECTION_RIGHT = new(1, 0);
        private static readonly Vector2Int DIRECTION_RIGHT_DOWN = new(1, -1);
        private static readonly Vector2Int DIRECTION_DOWN = new(0, -1);
        private static readonly Vector2Int DIRECTION_LEFT_DOWN = new(-1, -1);
        private static readonly Vector2Int DIRECTION_LEFT = new(-1, 0);
        private static readonly Vector2Int DIRECTION_LEFT_UP = new(-1, 1);

        public DirectionCondition(Vector2Int wall, Vector2Int blank)
        {
            directionBlankPos = blank;
            directionWallPos = wall;
        }

        public static DirectionCondition[] GetDirectionCondition(Vector2Int direction)
        {
            return CORNER_DIRECTION_LOOKUP[direction];
        }

        private static readonly Dictionary<Vector2Int, DirectionCondition[]> CORNER_DIRECTION_LOOKUP = new(8)
        {
            {
                DIRECTION_UP,
                new DirectionCondition[]
                {
                    new(DIRECTION_RIGHT, DIRECTION_RIGHT_UP),
                    new(DIRECTION_LEFT, DIRECTION_LEFT_UP)
                }
            },
            {
                DIRECTION_RIGHT_UP,
                new DirectionCondition[]
                {
                    new(DIRECTION_DOWN, DIRECTION_RIGHT_DOWN),
                    new(DIRECTION_LEFT, DIRECTION_LEFT_UP)
                }
            },
            {
                DIRECTION_RIGHT,
                new DirectionCondition[]
                {
                    new(DIRECTION_UP, DIRECTION_RIGHT_UP),
                    new(DIRECTION_DOWN, DIRECTION_RIGHT_DOWN)
                }
            },
            {
                DIRECTION_RIGHT_DOWN,
                new DirectionCondition[]
                {
                    new(DIRECTION_LEFT, DIRECTION_LEFT_DOWN),
                    new(DIRECTION_UP, DIRECTION_RIGHT_UP)
                }
            },
            {
                DIRECTION_DOWN,
                new DirectionCondition[]
                {
                    new(DIRECTION_LEFT, DIRECTION_LEFT_DOWN),
                    new(DIRECTION_RIGHT, DIRECTION_RIGHT_DOWN)
                }
            },
            {
                DIRECTION_LEFT_DOWN,
                new DirectionCondition[]
                {
                    new(DIRECTION_RIGHT, DIRECTION_RIGHT_DOWN),
                    new(DIRECTION_UP, DIRECTION_LEFT_UP)
                }
            },
            {
                DIRECTION_LEFT,
                new DirectionCondition[]
                {
                    new(DIRECTION_DOWN, DIRECTION_LEFT_DOWN),
                    new(DIRECTION_UP, DIRECTION_LEFT_UP)
                }
            },
            {
                DIRECTION_LEFT_UP,
                new DirectionCondition[]
                {
                    new(DIRECTION_RIGHT, DIRECTION_RIGHT_UP),
                    new(DIRECTION_DOWN, DIRECTION_LEFT_DOWN)
                }
            },
        };
    }

    public override string Name => "JPS";
    protected override Color Color => Color.blue;

    protected PriorityQueue cornerQueue;

    public override void Stop()
    {
        base.Stop();
        cornerQueue.Clear();
    }

    protected override async UniTask StartPathFinding()
    {
        cornerQueue ??= new PriorityQueue();

        nodeStartData.gWeight = 0;
        nodeStartData.hWeight = 0;
        
        foreach (var vector in GetNeighBor())
            await FindCorner(nodeStartData, vector);

        while (cornerQueue.Size > 0)
        {
            var corner = cornerQueue.Dequeue();
            if (corner == nodeEndData)
            {
                ShowPath();
                break;
            }


            await FindCorner(corner, corner.direction);
        }
    }

    private float GetHeuristicWeight(NodeData startNodeData, NodeData endNodeData)
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

    protected IEnumerable<Vector2Int> GetNeighBor()
    {
        yield return new Vector2Int(1, 0);
        yield return new Vector2Int(1, 1);
        yield return new Vector2Int(0, 1);
        yield return new Vector2Int(-1, 1);
        yield return new Vector2Int(0, -1);
        yield return new Vector2Int(-1,-1);
        yield return new Vector2Int(-1, 0);
        yield return new Vector2Int(1,-1);
    }

    private async UniTask CheckFindCornerStraight(NodeData nodeData, NodeData parentNodeData, Vector2Int direction)
    {
        var pos = nodeData.pos;
        while (true)
        {
            pos.Set(pos.x + direction.x, pos.y + direction.y);
            if (!nodeGraph.IsContainsPos(pos)) break;

            var findNodeData = nodeGraph.GetNodeData(pos.x, pos.y);
            if (findNodeData == nodeEndData)
            {
                float weight = parentNodeData.gWeight + GetHeuristicWeight(nodeData, parentNodeData);
                if (weight < nodeData.Weight)
                {
                    nodeData.parent = parentNodeData;
                    nodeData.direction = direction;
                    nodeData.gWeight = weight;
                    nodeData.hWeight = GetHeuristicWeight(findNodeData, nodeEndData);
                    cornerQueue.Enqueue(nodeData);
                    return;
                }
            }

            if (findNodeData.nodeType == NodeType.Wall) break;

            var conditions = DirectionCondition.GetDirectionCondition(direction);
            foreach (var condition in conditions)
            {
                var wallNodePos = condition.directionWallPos + pos;
                if (!nodeGraph.IsContainsPos(wallNodePos)) continue;
                
                if (nodeGraph.GetNodeData(wallNodePos.x, wallNodePos.y).nodeType != NodeType.Wall) continue;

                var blankNodePos = condition.directionBlankPos + pos;
                if (!nodeGraph.IsContainsPos(blankNodePos)) continue;
                
                if (nodeGraph.GetNodeData(blankNodePos.x, blankNodePos.y).nodeType == NodeType.Wall) continue;

                float weight = parentNodeData.gWeight + GetHeuristicWeight(nodeData, parentNodeData);
                if (weight < nodeData.Weight)
                {
                    nodeData.parent = parentNodeData;
                    nodeData.direction = direction;
                    nodeData.gWeight = weight;
                    nodeData.hWeight = GetHeuristicWeight(findNodeData, nodeEndData);
                    cornerQueue.Enqueue(nodeData);
                }
            }

            await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);
        }
    }

    private async UniTask FindCorner(NodeData parentNodeData, Vector2Int direction)
    {
        bool isStraight = direction.x == 0 || direction.y == 0;
        var pos = parentNodeData.pos;
        while (true)
        {
            if (!nodeGraph.IsContainsPos(pos)) break;

            var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);
            if (nodeData == nodeEndData)
            {
                nodeData.parent = parentNodeData;
                cornerQueue.Enqueue(nodeData);
                return;
            }

            if (nodeData.nodeType == NodeType.Wall) break;

            AddLine(pos);
            var conditions = DirectionCondition.GetDirectionCondition(direction);
            foreach (var condition in conditions)
            {
                var wallNodePos = condition.directionWallPos + pos;
                if (nodeGraph.GetNodeData(wallNodePos.x, wallNodePos.y).nodeType != NodeType.Wall) continue;

                var blankNodePos = condition.directionBlankPos + pos;
                if (nodeGraph.GetNodeData(blankNodePos.x, blankNodePos.y).nodeType == NodeType.Wall) continue;

                float weight = parentNodeData.gWeight + GetHeuristicWeight(nodeData, parentNodeData);
                if (weight < nodeData.gWeight)
                {
                    nodeData.parent = parentNodeData;
                    nodeData.gWeight = weight;
                    nodeData.direction = condition.directionBlankPos;
                    nodeData.hWeight = GetHeuristicWeight(nodeData, nodeEndData);
                    cornerQueue.Enqueue(nodeData);
                }
            }
            
            pos.Set(pos.x + direction.x, pos.y + direction.y);
            await UniTask.Delay(nodeManager.discoveredDelay, cancellationToken: cancellation.Token);

            if (isStraight) continue;
            
            await CheckFindCornerStraight(nodeData, parentNodeData, new Vector2Int(direction.x, 0));
            await CheckFindCornerStraight(nodeData, parentNodeData, new Vector2Int(0, direction.y));
        }
    }
}