using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingBFS : PathFinding
{
    private Queue<NodeData> nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;
    public override string Name => "BFS";
    protected override Color Color => Color.yellow;

    public override void Stop()
    {
        base.Stop();
        nodeDataHashSet.Clear();
        nodeDataQueue.Clear();
    }

    protected override async UniTask StartPathFinding()
    {
        nodeDataHashSet ??= new HashSet<NodeData>();
        nodeDataQueue ??= new Queue<NodeData>();

        nodeDataQueue.Enqueue(nodeStartData);
        nodeDataHashSet.Add(nodeStartData);

        while (nodeDataQueue.Count > 0)
        {
            var nodeData = nodeDataQueue.Dequeue();
            if (nodeData == nodeEndData)
            {
                ShowPath();
                break;
            }

            foreach (var neighborPos in GetNeighBor(nodeData.pos))
                await CheckAndEnqueueNode(nodeData, neighborPos);

            AddLine(nodeData.pos);

            await UniTask.Delay(nodeManager.visitDelay, cancellationToken: cancellation.Token);
        }
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