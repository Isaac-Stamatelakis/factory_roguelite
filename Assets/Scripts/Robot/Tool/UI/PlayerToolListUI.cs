using System.Collections;
using System.Collections.Generic;
using Player.Tool;
using Player.Tool.UI;
using Robot.Tool;
using UnityEngine;

public class PlayerToolListUI : MonoBehaviour
{
    private int offset;
    PlayerToolListElementUI[] elements;
    private List<IRobotToolInstance> tools;

    public void Start()
    {
        elements = GetComponentsInChildren<PlayerToolListElementUI>();
    }

    public void Initialize(List<IRobotToolInstance> tools)
    {
        this.tools = tools;
        Display();
    }
    public void Display()
    {
        
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
}
