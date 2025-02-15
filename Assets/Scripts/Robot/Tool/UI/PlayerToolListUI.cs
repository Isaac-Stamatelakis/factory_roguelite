using System.Collections;
using System.Collections.Generic;
using Player.Tool;
using Player.Tool.UI;
using PlayerModule;
using Robot.Tool;
using Robot.Tool.UI;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToolListUI : MonoBehaviour
{
    [SerializeField] private PlayerToolUIIndicator primaryIndicator;
    [SerializeField] private PlayerToolUIIndicator secondaryIndicator;
    private int offset;
    PlayerToolListElementUI[] elements;
    private List<IRobotToolInstance> tools;
    private Image image;
    [SerializeField] private Color highlightColor;
    private Color defaultColor;
    private PlayerInventory playerInventory;
    public void Start()
    {
        elements = GetComponentsInChildren<PlayerToolListElementUI>();
        image = GetComponent<Image>();
        defaultColor = image.color;
    }

    public void Initialize(List<IRobotToolInstance> tools, PlayerInventory playerInventory)
    {
        this.tools = tools;
        this.playerInventory = playerInventory;
        Display();
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            playerInventory?.CurrentTool.ModeSwitch(MoveDirection.Left,Input.GetKey(KeyCode.LeftControl));
            playerInventory?.PlayerRobotToolUI.Display();
        }
    }
    public void Display()
    {
        if (tools.Count == 0) return;
        for (int i = 0; i < elements.Length; i++)
        {
            int index = (i + offset) % tools.Count;
            elements[i].Display(index,tools[index],playerInventory.ChangeSelectedTool);
        }
        
        IRobotToolInstance current = tools[offset];
        primaryIndicator.Display(current);
        if (current is ISubModeRobotToolInstance)
        {
            secondaryIndicator.Display(current);
            secondaryIndicator.gameObject.SetActive(true);
        }
        else
        {
            secondaryIndicator.gameObject.SetActive(false);
        }

    }
    
    public void IterateSelectedTool(int iterator)
    {
        offset = iterator;
        if (offset > tools.Count) offset -= tools.Count;
        if (offset < 0) offset += tools.Count;
        Display();
    }

    public void SetOffset(int newOffset)
    {
        offset = newOffset;
        if (offset > tools.Count) offset -= tools.Count;
        if (offset < 0) offset += tools.Count;
        Display();
    }
    public void Highlight(bool state)
    {
        image.color = state ? highlightColor : defaultColor;
    }
}
