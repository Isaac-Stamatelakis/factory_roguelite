namespace Player.Controls
{
    public enum PlayerControl
    {
        Jump = 0,
        MoveLeft = 1,
        MoveRight = 2,
        MoveDown  = 3,
        MoveUp = 4,
        Teleport = 5,
        SwitchToolMode = 7,
        OpenConduitOptions = 10,
        SwitchPlacementMode = 11,
        TerminateConduitGroup = 12,
        SwitchConduitPortView = 13,
        ChangeConduitViewMode = 14,
        HideUI = 15,
        SwapToolLoadOut = 17,
        SwapRobotLoadOut = 18,
        OpenQuestBook = 19,
        OpenInventory = 20,
        OpenSearch = 21,
        AutoSelect = 22,
        OpenRobotLoadOut = 23,
        OpenToolLoadOut = 24,
        SwitchPlacementSubMode = 25,
        PlacePreview = 26,
        ShowItemRecipes = 27,
        ShowItemUses = 28,
        EditItemTag = 29,
    }

    public enum ModifierKeyCode
    {
        Ctrl,
        Shift,
        Alt
    }

    public enum PlayerControlGroup
    {
        Movement = 0,
        Gameplay = 1,
        Tools = 2,
        Utils = 3,
        Misc = 4,
    }
}