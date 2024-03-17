using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PathFinding
{
    protected CancellationTokenSource cancellation;
    public abstract string Name { get; }
    public abstract UniTaskVoid StartPathFinding(Graph graph, Vector2Int startPos, Vector2Int endPos);

    public virtual void Stop()
    {
        if (cancellation != null)
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }

        NodeManager.Instance.isPathFinding = false;
    }

    protected virtual IEnumerable<Vector2Int> GetNeighBor(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y - 1);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
    }
}