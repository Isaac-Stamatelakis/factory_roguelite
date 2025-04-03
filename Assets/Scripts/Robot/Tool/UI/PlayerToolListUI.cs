using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
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
    private Image image;
    [SerializeField] private Color highlightColor;
    private Color defaultColor;
    private PlayerInventory playerInventory;
    [SerializeField] private InventoryUI mToolCollectionUI;
    public void Start()
    {
        image = GetComponent<Image>();
        defaultColor = image.color;
    }

    public void Initialize(List<IRobotToolInstance> tools, PlayerInventory playerInventory)
    {
        this.playerInventory = playerInventory;

        List<ItemSlot> toolItemSlots = new List<ItemSlot>();
        foreach (IRobotToolInstance robotToolInstance in tools)
        {
            toolItemSlots.Add(new ItemSlot(robotToolInstance.GetToolObject().ToolIconItem,1,null));
        }
        mToolCollectionUI.DisplayInventory(toolItemSlots,clear:false);
        mToolCollectionUI.InventoryInteractMode = InventoryInteractMode.OverrideAction;

        void SelectTool(int index)
        {
            playerInventory.ChangeSelectedTool(index);
            DisplayIndicators(index);

        }

        void DisplayIndicators(int index)
        {
            IRobotToolInstance current = tools[index];
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
        mToolCollectionUI.OverrideClickAction(SelectTool);
        List<string> topTexts = new List<string>();
        for (int i = 0; i < toolItemSlots.Count; i++)
        {
            topTexts.Add((i+1).ToString());
        }
        mToolCollectionUI.DisplayTopText(topTexts);
        mToolCollectionUI.SetAllPanelColors(new Color(174/255f,203/255f,221/255f,1f));
        mToolCollectionUI.SetHighlightColor(new Color(222/255f,218/255f,91/255f,1f));
        mToolCollectionUI.SetOnHighlight(DisplayIndicators);
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            playerInventory?.CurrentTool.ModeSwitch(MoveDirection.Left,Input.GetKey(KeyCode.LeftControl));
        }
    }
    
    public void Highlight(bool state)
    {
        image.color = state ? highlightColor : defaultColor;
    }

    public void HighLightTool(int index)
    {
        mToolCollectionUI.HighlightSlot(index);
    }
}
