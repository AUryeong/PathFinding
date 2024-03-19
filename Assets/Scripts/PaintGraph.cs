using UnityEngine;

public class PaintGraph : MonoBehaviour
{
    public LineRenderer lineRenderer;
    
    private Graph graph;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private Vector2[] uv; // 타일 변동을 위한 uv 저장
    private Vector2 textureSize;

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

        mesh.uv = uv;
    }

    public void UpdatePaint() // 타일 길이 변경에 따른 다시 그리기
    {
        int width = graph.Size.x;
        int height = graph.Size.y;

        Vector2Int graphStartPos = graph.StartPos;
        var pos = new Vector2(graphStartPos.x * TILE_SIZE, graphStartPos.y * TILE_SIZE);

        int tileCount = width * height;

        var verticals = new Vector3[4 * tileCount];
        var triangles = new int[6 * tileCount];
        uv = new Vector2[4 * tileCount];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                var nodeData = graph.GetNodeDataByIndex(x, y);

                verticals[index + 0] = new Vector3(TILE_SIZE * x + pos.x, TILE_SIZE * y + pos.y);
                verticals[index + 1] = new Vector3(TILE_SIZE * x + pos.x, TILE_SIZE * (y + 1) + pos.y);
                verticals[index + 2] = new Vector3(TILE_SIZE * (x + 1) + pos.x, TILE_SIZE * (y + 1) + pos.y);
                verticals[index + 3] = new Vector3(TILE_SIZE * (x + 1) + pos.x, TILE_SIZE * y + pos.y);

                SetUV(index, nodeData);

                int trianglesIndex = (y * width + x) * 6;

                triangles[trianglesIndex + 0] = index + 0;
                triangles[trianglesIndex + 1] = index + 1;
                triangles[trianglesIndex + 2] = index + 2;

                triangles[trianglesIndex + 3] = index + 0;
                triangles[trianglesIndex + 4] = index + 2;
                triangles[trianglesIndex + 5] = index + 3;
            }
        }

        mesh.vertices = verticals;
        mesh.uv = uv;
        mesh.triangles = triangles;

        meshFilter.mesh = mesh;
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
        uv[startIndex + 0] = new Vector2(x / textureSize.x, y / textureSize.y);
        uv[startIndex + 1] = new Vector2(x / textureSize.x, (y + 1) / textureSize.y);
        uv[startIndex + 2] = new Vector2((x + 1) / textureSize.x, (y + 1) / textureSize.y);
        uv[startIndex + 3] = new Vector2((x + 1) / textureSize.x, y / textureSize.y);
    }
}