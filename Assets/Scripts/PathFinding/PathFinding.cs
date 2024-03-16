using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PathFinding
{
    public abstract UniTaskVoid StartPathFinding(Graph graph, Vector2Int startPos, Vector2Int endPos);

}