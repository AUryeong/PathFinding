using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PathFinding
{
    protected CancellationTokenSource cancellation;
    public abstract UniTaskVoid StartPathFinding(Graph graph, Vector2Int startPos, Vector2Int endPos);

    public virtual void Stop()
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }
}