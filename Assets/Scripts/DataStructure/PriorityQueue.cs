using System.Collections.Generic;

public class PriorityQueue
{
    private List<PirorityNodeData> priorityNodes;
    private int TailIndex => priorityNodes.Count - 1;
    private int RootIndex => TailIndex / 2;

    public void Enqueue(PirorityNodeData priorityNodeData)
    {
        priorityNodes.Add(priorityNodeData);
        int index = TailIndex;

        while (true)
        {
            int parentNode = index / 2;
            if (priorityNodes[parentNode].pirority >= priorityNodeData.pirority) break;

            priorityNodes[index] = priorityNodes[parentNode];
            priorityNodes[parentNode] = priorityNodeData;
            index = parentNode;
        }
    }

    public struct PirorityNodeData
    {
        public NodeData nodeData;
        public int pirority;
    }
}