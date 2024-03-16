using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFindingBFS : PathFinding
{
    private Graph nodeGraph;
    private Queue<Vector2Int> nodeDataQueue;
    private HashSet<NodeData> nodeDataHashSet;

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

        while (nodeDataQueue.Count > 0)
        {
            var pos = nodeDataQueue.Dequeue();
            var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);

            if (nodeData.nodeType == NodeType.Finish)
                break;

            nodeData.stateType = NodeStateType.Discovered;
            NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y, nodeData);

            await CheckAndEnqueueNode(new Vector2Int(pos.x + 1, pos.y));
            await CheckAndEnqueueNode(new Vector2Int(pos.x, pos.y - 1));
            await CheckAndEnqueueNode(new Vector2Int(pos.x - 1, pos.y));
            await CheckAndEnqueueNode(new Vector2Int(pos.x, pos.y + 1));

            await UniTask.Delay(NodeManager.Instance.visitDelay, cancellationToken:cancellation.Token);
        }
    }

    private async UniTask CheckAndEnqueueNode(Vector2Int pos)
    {
        if (pos.x > nodeGraph.Size.x || pos.x <= 0) return;
        if (pos.y > nodeGraph.Size.y || pos.y <= 0) return;

        var nodeData = nodeGraph.GetNodeData(pos.x, pos.y);
        if (nodeDataHashSet.Contains(nodeData)) return;

        nodeDataHashSet.Add(nodeData);
        if (nodeData == null)
        {
            Debug.Log($"{pos.x},{pos.y}");
        }

        if (nodeData.nodeType == NodeType.Wall) return;

        nodeData.stateType = NodeStateType.Visited;
        NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y, nodeData);

        nodeDataQueue.Enqueue(pos);

        await UniTask.Delay(NodeManager.Instance.discoveredDelay, cancellationToken:cancellation.Token);
    }
}