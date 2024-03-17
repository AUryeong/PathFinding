using System;
using UnityEngine;

public class PaintGraph : MonoBehaviour
{
    private Graph graph;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private Vector3[] verticals;
    private Vector2[] uvs; // 타일 변동을 위한 uv 저장
    private int[] triangles;
    private Vector2 textureSize;

    private RectInt screenRectInt;

    private const int TILE_SIZE = 1;

    public void Init(Graph nodeGraph)
    {
        graph = nodeGraph;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh();

        var texture = meshRenderer.material.mainTexture;
        textureSize = new Vector2(texture.width / 100f, texture.height / 100f);
    }


    public void UpdateUV(int x, int y, NodeData nodeData = null) // 특정 타일 위치만 색깔 다시 그리기
    {
        int index = ((y - graph.StartPos.y) * graph.Size.x + (x - graph.StartPos.x)) * 4; // 사용의 편의성을 위해 -1 해주는거 넣기
        nodeData ??= graph.GetNodeData(x, y);
        SetUV(index, nodeData);

        mesh.uv = uvs;
    }

    public void UpdatePaint() // 타일 길이 변경에 따른 다시 그리기
    {
        var newScreenRectInt = CameraManager.Instance.screenRectInt;

        int width = newScreenRectInt.width;
        int height = newScreenRectInt.height;

        int createCountX = width - screenRectInt.width;
        int createCountY = height - screenRectInt.height;

        Vector2Int graphStartPos = graph.StartPos;

        int tileCount = width * height;
        int startIndex = 0;
        int startTrianglesIndex = 0;

        if (createCountX > 0 || createCountY > 0)
        {
            var prevVerticals = verticals;
            var prevUvs = uvs;
            var prevTriangles = triangles;

            verticals = new Vector3[4 * tileCount];
            uvs = new Vector2[4 * tileCount];
            triangles = new int[6 * tileCount];

            if (prevVerticals != null)
            {
                startIndex = prevVerticals.Length;
                startTrianglesIndex = prevTriangles.Length;

                Array.Copy(prevVerticals, 0, verticals, 0, prevVerticals.Length);
                Array.Copy(prevUvs, 0, uvs, 0, prevUvs.Length);
                Array.Copy(prevTriangles, 0, triangles, 0, prevTriangles.Length);
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y < screenRectInt.height && x < screenRectInt.width) continue;
                int index = (y * createCountX + x) * 4 + startIndex;
                var nodeData = graph.GetNodeData(x + graphStartPos.x, y + graphStartPos.y);

                int up = y + 1;
                int right = x + 1;

                if (verticals.Length <= index)
                {
                    Debug.Log($"{index} , {tileCount} , {startIndex} , {createCountY}, {createCountX}");
                }

                verticals[index + 0] = new Vector3(x, y);
                verticals[index + 1] = new Vector3(x, up);
                verticals[index + 2] = new Vector3(right, up);
                verticals[index + 3] = new Vector3(right, y);

                SetUV(index, nodeData);

                int trianglesIndex = (y * createCountX + x) * 6 + startTrianglesIndex;

                triangles[trianglesIndex + 0] = index + 0;
                triangles[trianglesIndex + 1] = index + 1;
                triangles[trianglesIndex + 2] = index + 2;

                triangles[trianglesIndex + 3] = index + 0;
                triangles[trianglesIndex + 4] = index + 2;
                triangles[trianglesIndex + 5] = index + 3;
            }
        }

        mesh.vertices = verticals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        meshFilter.mesh = mesh;

        transform.position = new Vector2(newScreenRectInt.x, newScreenRectInt.y);

        screenRectInt = newScreenRectInt;
    }

    private void SetUV(int startIndex, NodeData nodeData) // UV는 여러곳에서도 쓰고 길기도 하니 따로 뺐음
    {
        if (nodeData == null)
        {
            SetUV(startIndex, 0, 2);
            return;
        }

        switch (nodeData.nodeType)
        {
            case NodeType.Wall:
                SetUV(startIndex, 0, 1);
                break;
            case NodeType.Start:
                SetUV(startIndex, 1, 1);
                break;
            case NodeType.End:
                SetUV(startIndex, 0, 2);
                break;
            default:
            case NodeType.None:
                switch (nodeData.stateType)
                {
                    case NodeStateType.Discovered:
                        SetUV(startIndex, 0, 0);
                        break;
                    case NodeStateType.Visited:
                        SetUV(startIndex, 1, 0);
                        break;
                    case NodeStateType.None:
                    default:
                        SetUV(startIndex, 1, 2);
                        break;
                }

                break;
        }
    }

    private void SetUV(int startIndex, int x, int y)
    {
        uvs[startIndex + 0] = new Vector2(x / textureSize.x, y / textureSize.y);
        uvs[startIndex + 1] = new Vector2(x / textureSize.x, (y + 1) / textureSize.y);
        uvs[startIndex + 2] = new Vector2((x + 1) / textureSize.x, (y + 1) / textureSize.y);
        uvs[startIndex + 3] = new Vector2((x + 1) / textureSize.x, y / textureSize.y);
    }
}