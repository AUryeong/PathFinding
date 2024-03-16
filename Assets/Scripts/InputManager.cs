using System.Collections.Generic;
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
    private readonly PathFinding[] pathFindings = 
    {
        new PathFindingBFS()
    };
    [SerializeField] private List<Toggle> pathFindingToggles;
    
    [Space(10f)]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button startButton;
    
    public override void Init()
    {
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
        resetButton.onClick.AddListener(ResetPathFinding);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartPathFinding);
    }

    private void ChangePathFinding(int index)
    {
        selectPathFinding = pathFindings[index];
    }

    private void StartPathFinding()
    {
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

    private void FixedUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButton(0))
            SetNodeTypeByMouse(selectNodeType);
        
        if (Input.GetMouseButton(1))
            SetNodeTypeByMouse(NodeType.None);
    }

    private void SetNodeTypeByMouse(NodeType nodeType)
    {
        var vector = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var pos = NodeManager.Instance.GetTilePosByWorldPoint(vector);
        var nodeData = NodeManager.Instance.GetNodeData(pos.x, pos.y);

        if (nodeData.nodeType == nodeType) return;

        switch (nodeType)
        {
            case NodeType.Start:
                NodeManager.Instance.startNodePos = pos;
                break;
            case NodeType.Finish:
                NodeManager.Instance.endNodePos = pos;
                break;
        }
        
        nodeData.nodeType = nodeType;
        NodeManager.Instance.paintGraph.UpdateUV(pos.x, pos.y);
    }

}