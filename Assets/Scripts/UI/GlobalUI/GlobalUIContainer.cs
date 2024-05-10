using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Chat;
using UI;


/// <summary>
/// Singleton class which contains the GlobalUIContainer
/// </summary>
public class GlobalUIContainer
{
    private static GlobalUIContainer instance;
    private GlobalUIController gUIController;
    private TextChatUI textChatUI;

    private GlobalUIContainer() {
        GameObject container = GameObject.Find(UIUtils.GlobalUIContainerName);
        textChatUI = GameObject.Find(UIUtils.TextChatName).GetComponent<TextChatUI>();
        if (container == null) {
            Debug.LogError("GlobalUIContainer 'constructor' attempted to get component GlobalUIController from null gameobject. Ensure there is a GameObject called 'GlobalUIController' in the scene");
            return;
        }
        gUIController = container.GetComponent<GlobalUIController>();
    }
    public static GlobalUIContainer getInstance() {
        if (instance == null) {
            instance = new GlobalUIContainer();
        }
        return instance;
    }
    public GlobalUIController getUiController() {
        if (gUIController == null) {
            Debug.LogError("GlobalUIController is null!\nIs there a gameobject called 'GlobalUIController' in the current scene?\nDid the scene change and you forgot to refresh it?");
        }
        return gUIController;
    }
    public TextChatUI getTextChatUI() {
        return textChatUI;
    }
    public bool isActive() {
        return gUIController.isActive;
    }
    public static void refresh() {
        instance = new GlobalUIContainer();
    }
    public static void reset() {
        instance = null;
    }
}

