using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : SingletonBehavior<InputManager>
{
    private readonly List<NodeObject> drewNodeObjects = new();
    private NodeType selectNodeType = NodeType.Wall;
    private PathFinding selectPathFinding;

    [Header("Left UI")]
    [SerializeField] private Button[] paletteButtons;

    [Header("Right UI")]
    private PathFinding[] pathFindings = new PathFinding[]
    {
        new PathFindingBFS()
    };
    [SerializeField] private List<Toggle> pathFindingToggles;
    [Space(10f)]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button startButton;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < (int)NodeType.Finish; i++)
        {
            var button = paletteButtons[i];
            var paletteType = (NodeType)(i + 1);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ChangePalette(paletteType));
        }

        for (int i = 0; i < pathFindingToggles.Count; i++)
        {
            int index = i;
            Toggle toggle = pathFindingToggles[i];
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                    ChangePathFinding(index);
            });
        }
        pathFindingToggles[0].isOn = true;

        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(NodeManager.Instance.ResetNodeState);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartPathFinding);
    }

    private void ChangePalette(NodeType nodeType)
    {
        selectNodeType = nodeType;
    }

    private void ChangePathFinding(int index)
    {
        selectPathFinding = pathFindings[index];
    }

    private void StartPathFinding()
    {
        NodeManager.Instance.StartPathFinding(selectPathFinding);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        CheckDraw();
        CheckErase();
    }

    private void CheckDraw()
    {
        if (!Input.GetMouseButton(0)) return;
        if (Input.GetMouseButtonDown(0))
            drewNodeObjects.Clear();

        var vector = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var rect = NodeManager.Instance.lastRect;
        int x = Mathf.RoundToInt(vector.x) - rect.x + 1;
        int y = Mathf.RoundToInt(vector.y) - rect.y + 1;
        var nodeObject = NodeManager.Instance.GetNodeObject(x, y);

        if (drewNodeObjects.Contains(nodeObject)) return;

        if (selectNodeType == NodeType.Finish)
        {
            var startNode = NodeManager.Instance.endNodeObject;
            if (startNode != null)
                startNode.NodeType = NodeType.None;
        }

        nodeObject.NodeType = selectNodeType;
        drewNodeObjects.Add(nodeObject);
    }

    private void CheckErase()
    {
        if (!Input.GetMouseButton(1)) return;
        if (Input.GetMouseButtonDown(1))
            drewNodeObjects.Clear();

        var vector = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var rect = NodeManager.Instance.lastRect;
        int x = Mathf.RoundToInt(vector.x) - rect.x + 1;
        int y = Mathf.RoundToInt(vector.y) - rect.y + 1;
        var nodeObject = NodeManager.Instance.GetNodeObject(x, y);

        if (drewNodeObjects.Contains(nodeObject)) return;

        nodeObject.NodeType = NodeType.None;
        drewNodeObjects.Add(nodeObject);
    }
}