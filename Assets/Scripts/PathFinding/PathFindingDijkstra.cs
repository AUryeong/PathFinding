using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingDijkstra : PathFinding
{
    private Graph nodeGraph;
    private Queue<Vector2Int> nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;
    public override string Name => "Dijkstra";

    public override void Stop()
    {
        base.Stop();
        nodeGraph = null;
        nodeDataHashSet.Clear();
        nodeDataQueue.Clear();
    }

    public override async UniTaskVoid StartPathFinding(Graph graph, Vector2Int startPos, Vector2Int endPos)
    {
        nodeGraph = graph;

        cancellation = new CancellationTokenSource();
        nodeDataHashSet ??= new HashSet<NodeData>();
        nodeDataQueue ??= new Queue<Vector2Int>();

        nodeDataQueue.Enqueue(startPos);
        nodeDataHashSet.Add(nodeGraph.GetNodeData(startPos.x, startPos.y));

        while (nodeDataQueue.Count > 0)
        {
            var pos = nodeDataQueue.Dequeue();
            if (pos == endPos)
                break;

            var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);

            nodeData.stateType = NodeStateType.Discovered;
            NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y, nodeData);

            foreach (var neighborPos in GetNeighBor(pos))
                await CheckAndEnqueueNode(neighborPos);

            await UniTask.Delay(NodeManager.Instance.visitDelay, cancellationToken: cancellation.Token);
        }

        Stop();
    }

    private async UniTask CheckAndEnqueueNode(Vector2Int pos)
    {
        if (!nodeGraph.IsContainsPos(pos)) return;

        var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);
        if (nodeDataHashSet.Contains(nodeData)) return;

        nodeDataHashSet.Add(nodeData);

        if (nodeData.nodeType == NodeType.Wall) return;

        nodeData.stateType = NodeStateType.Visited;
        NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y, nodeData);

        nodeDataQueue.Enqueue(pos);

        await UniTask.Delay(NodeManager.Instance.discoveredDelay, cancellationToken: cancellation.Token);
    }
}