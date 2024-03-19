using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingBFS : PathFinding
{
    private Queue<NodeData> nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;
    public override string Name => "BFS";

    public override void Stop()
    {
        base.Stop();
        
        nodeGraph = null;
        nodeDataHashSet.Clear();
        nodeDataQueue.Clear();
    }

    public override async UniTaskVoid StartPathFinding(Graph graph, NodeData startData, NodeData endData)
    {
        nodeGraph = graph;

        nodeStartData = startData;
        nodeEndData = endData;

        cancellation = new CancellationTokenSource();
        nodeDataHashSet ??= new HashSet<NodeData>();
        nodeDataQueue ??= new Queue<NodeData>();

        nodeDataQueue.Enqueue(startData);
        nodeDataHashSet.Add(startData);

        while (nodeDataQueue.Count > 0)
        {
            var nodeData = nodeDataQueue.Dequeue();
            if (nodeData == nodeEndData)
                break;

            nodeData.stateType = NodeStateType.Discovered;
            NodeManager.Instance.paintGraph.UpdateUV(nodeData.pos.x, nodeData.pos.y, nodeData);

            foreach (var neighborPos in GetNeighBor(nodeData.pos))
                await CheckAndEnqueueNode(nodeData, neighborPos);

            await UniTask.Delay(NodeManager.Instance.visitDelay, cancellationToken: cancellation.Token);
        }

        isFind = true;
        Stop();
    }

    private async UniTask CheckAndEnqueueNode(NodeData originData, Vector2Int pos)
    {
        if (!nodeGraph.IsContainsPos(pos)) return;

        var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);
        if (nodeDataHashSet.Contains(nodeData)) return;

        nodeDataHashSet.Add(nodeData);

        if (nodeData.nodeType == NodeType.Wall) return;

        nodeData.stateType = NodeStateType.Visited;
        NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y, nodeData);

        nodeData.parent = originData;
        nodeDataQueue.Enqueue(nodeData);

        await UniTask.Delay(NodeManager.Instance.discoveredDelay, cancellationToken: cancellation.Token);
    }
}