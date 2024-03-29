using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public enum HeuristicType
{
    Euclidean,
    Manhattan
}

public class InputManager : SingletonBehavior<InputManager>
{
    private NodeType selectNodeType = NodeType.Wall;
    private List<PathFinding> selectPathFindings;

    [Header("Value")]
    public HeuristicType heuristicType = HeuristicType.Euclidean;
    public int weight = 1;

    [Header("Left UI")]
    [SerializeField] private Button[] paletteButtons;

    [Header("Right UI")]
    [SerializeField] private Toggle pathFindingToggle;

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
        selectPathFindings = new List<PathFinding>(PathFinding.pathFindings.Length);

        for (int i = 0; i < (int)NodeType.End; i++)
        {
            var button = paletteButtons[i];
            var paletteType = (NodeType)(i + 1);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ChangePalette(paletteType));
        }

        for (int i = 0; i < PathFinding.pathFindings.Length; i++)
        {
            int index = i;
            var toggle = Instantiate(pathFindingToggle, pathFindingToggle.transform.parent);

            toggle.gameObject.SetActive(true);

            toggle.GetComponentInChildren<TextMeshProUGUI>().text = PathFinding.pathFindings[i].Name;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((value) =>
            {
                ChangePathFinding(value, index);
            });

            toggle.isOn = true;
        }

        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(NodeManager.Instance.ResetPathFinding);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(NodeManager.Instance.StartPathFinding);

        discoveryDelayInput.text = NodeManager.Instance.discoveredDelay.ToString();
        discoveryDelayInput.onValueChanged.RemoveAllListeners();
        discoveryDelayInput.onValueChanged.AddListener(ChangeDiscoveredDelay);

        visitDelayInput.text = NodeManager.Instance.visitDelay.ToString();
        visitDelayInput.onValueChanged.RemoveAllListeners();
        visitDelayInput.onValueChanged.AddListener(ChangeVisitDelay);

        weightInput.text = weight.ToString();
        weightInput.onValueChanged.RemoveAllListeners();
        weightInput.onValueChanged.AddListener((value) => weight = int.Parse(value));

        dropDown.value =(int)heuristicType;
        dropDown.onValueChanged.RemoveAllListeners();
        dropDown.onValueChanged.AddListener((value) => heuristicType = (HeuristicType)value);
    }

    private void ChangePathFinding(bool isToggle, int index)
    {
        NodeManager.Instance.ResetPathFinding();

        if (isToggle)
            selectPathFindings.Add(PathFinding.pathFindings[index]);
        else
            selectPathFindings.Remove(PathFinding.pathFindings[index]);
    }

    public List<PathFinding> GetSelectPathFinding()
    {
        return selectPathFindings;
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
        var nodeData = NodeManager.Instance.originGraph.GetNodeData(pos.x, pos.y);

        if (nodeData == null) return;
        if (nodeData.nodeType == nodeType) return;

        switch (nodeType)
        {
            case NodeType.Start:
                if (NodeManager.Instance.startNodeData != null)
                {
                    var prevData = NodeManager.Instance.startNodeData;
                    prevData.nodeType = NodeType.None;
                    NodeManager.Instance.paintGraph.UpdateUV(prevData.pos.x, prevData.pos.y);
                }

                NodeManager.Instance.startNodeData = nodeData;
                break;
            case NodeType.End:
                if (NodeManager.Instance.endNodeData != null)
                {
                    var prevData = NodeManager.Instance.endNodeData;
                    prevData.nodeType = NodeType.None;
                    NodeManager.Instance.paintGraph.UpdateUV(prevData.pos.x, prevData.pos.y);
                }

                NodeManager.Instance.endNodeData = nodeData;
                break;
        }

        nodeData.nodeType = nodeType;
        NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y);
    }
}