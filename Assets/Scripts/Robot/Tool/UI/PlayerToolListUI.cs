using System.Collections;
using System.Collections.Generic;
using Player.Tool;
using Player.Tool.UI;
using Robot.Tool;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToolListUI : MonoBehaviour
{
    private int offset;
    PlayerToolListElementUI[] elements;
    private List<IRobotToolInstance> tools;
    private Image image;
    [SerializeField] private Color highlightColor;
    private Color defaultColor;

    public void Start()
    {
        elements = GetComponentsInChildren<PlayerToolListElementUI>();
        image = GetComponent<Image>();
        defaultColor = image.color;
    }

    public void Initialize(List<IRobotToolInstance> tools)
    {
        this.tools = tools;
    }
    public void Display()
    {
        if (tools.Count == 0) return;
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].Display(tools[(i+offset)%tools.Count]);
        }
    }
    
    public void IterateSelectedTool(int iterator)
    {
        offset = iterator;
        if (offset > tools.Count) offset -= tools.Count;
        if (offset < 0) offset += tools.Count;
        Display();
    }

    public void Highlight(bool state)
    {
        image.color = state ? highlightColor : defaultColor;
    }
}
