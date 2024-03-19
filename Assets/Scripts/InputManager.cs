﻿using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : SingletonBehavior<InputManager>
{
    private NodeType selectNodeType = NodeType.Wall;
    private PathFinding selectPathFinding;

    [Header("Left UI")]
    [SerializeField] private Button[] paletteButtons;

    [Header("Right UI")]
    [SerializeField] private Toggle pathFindingToggle;
    private readonly PathFinding[] pathFindings =
    {
        new PathFindingBFS(),
        new PathFindingDijkstra(),
        new PathFindingAStar()
    };
    [Space(10f)]
    [SerializeField] private TMP_InputField weightInput;
    [SerializeField] private TMP_Dropdown dropDown;

    [Space(10f)]
    [SerializeField] private TMP_InputField discoveryDelayInput;
    [SerializeField] private TMP_InputField visitDelayInput;

    [Space(10f)]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button startButton;

    public override void Init()
    {
        for (int i = 0; i < (int)NodeType.End; i++)
        {
            var button = paletteButtons[i];
            var paletteType = (NodeType)(i + 1);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ChangePalette(paletteType));
        }

        for (int i = 0; i < pathFindings.Length; i++)
        {
            int index = i;
            var toggle = Instantiate(pathFindingToggle, pathFindingToggle.transform.parent);

            toggle.gameObject.SetActive(true);

            toggle.GetComponentInChildren<TextMeshProUGUI>().text = pathFindings[i].Name;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                    ChangePathFinding(index);
            });

            if (index == 0)
                toggle.isOn = true;
        }

        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(ResetPathFinding);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartPathFinding);

        discoveryDelayInput.text = NodeManager.Instance.discoveredDelay.ToString();
        discoveryDelayInput.onValueChanged.RemoveAllListeners();
        discoveryDelayInput.onValueChanged.AddListener(ChangeDiscoveredDelay);

        visitDelayInput.text = NodeManager.Instance.visitDelay.ToString();
        visitDelayInput.onValueChanged.RemoveAllListeners();
        visitDelayInput.onValueChanged.AddListener(ChangeVisitDelay);

        weightInput.onValueChanged.RemoveAllListeners();
        weightInput.onValueChanged.AddListener((value) => selectPathFinding.weight = int.Parse(value));

        dropDown.onValueChanged.RemoveAllListeners();
        dropDown.onValueChanged.AddListener((value) => selectPathFinding.heuristicType = (HeuristicType)value);
    }

    private void ChangePathFinding(int index)
    {
        if (NodeManager.Instance.isPathFinding) return;

        ResetPathFinding();
        selectPathFinding = pathFindings[index];
        weightInput.text = selectPathFinding.weight.ToString();
        dropDown.value = (int)selectPathFinding.heuristicType;
    }

    private void StartPathFinding()
    {
        if (NodeManager.Instance.isPathFinding) return;

        NodeManager.Instance.StartPathFinding(selectPathFinding);
    }

    private void ResetPathFinding()
    {
        NodeManager.Instance.ResetPathFinding(selectPathFinding);
    }

    private void ChangePalette(NodeType nodeType)
    {
        selectNodeType = nodeType;
    }

    private void ChangeDiscoveredDelay(string text)
    {
        int delay = int.Parse(text);
        NodeManager.Instance.discoveredDelay = delay;
    }

    private void ChangeVisitDelay(string text)
    {
        int delay = int.Parse(text);
        NodeManager.Instance.visitDelay = delay;
    }

    private void FixedUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (NodeManager.Instance.isPathFinding) return;

        if (Input.GetMouseButton(0))
            SetNodeTypeByMouse(selectNodeType);

        if (Input.GetMouseButton(1))
            SetNodeTypeByMouse(NodeType.None);
    }

    private void SetNodeTypeByMouse(NodeType nodeType)
    {
        var vector = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var pos = NodeManager.Instance.GetWorldPointToTilePos(vector);
        var nodeData = NodeManager.Instance.graph.GetNodeData(pos.x, pos.y);

        if (nodeData == null) return;
        if (nodeData.nodeType == nodeType) return;

        switch (nodeType)
        {
            case NodeType.Start:
                if (NodeManager.Instance.startNodeData != null)
                {
                    var prevData = NodeManager.Instance.startNodeData;
                    prevData.nodeType = NodeType.None;
                    NodeManager.Instance.paintGraph.UpdateUV(prevData.pos.x, prevData.pos.y, prevData);
                }

                NodeManager.Instance.startNodeData = nodeData;
                break;
            case NodeType.End:
                if (NodeManager.Instance.endNodeData != null)
                {
                    var prevData = NodeManager.Instance.endNodeData;
                    prevData.nodeType = NodeType.None;
                    NodeManager.Instance.paintGraph.UpdateUV(prevData.pos.x, prevData.pos.y, prevData);
                }

                NodeManager.Instance.endNodeData = nodeData;
                break;
        }

        nodeData.nodeType = nodeType;
        NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y);
    }
}