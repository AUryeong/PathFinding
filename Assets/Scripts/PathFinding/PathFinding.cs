using System.Collections.Generic;

public abstract class PathFinding
{
    public abstract void StartPathFinding(NodeObject startNodeData, NodeObject endNodeData, List<List<NodeObject>> nodeGraph);
}