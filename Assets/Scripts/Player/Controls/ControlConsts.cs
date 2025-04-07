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
        Recall = 6,
        SwitchToolMode = 7,
        OpenConduitOptions = 10,
        SwitchConduitPlacementMode = 11,
        TerminateConduitGroup = 12,
        SwitchConduitPortView = 13,
        ChangeConduitViewMode = 14,
        HideUI = 15,
        // ReSharper disable once InconsistentNaming
        SwapToolLoadOut = 17,
        SwapRobotLoadOut = 18,
        OpenQuestBook = 19,
        OpenInventory = 20,
        OpenSearch = 21,
        AutoSelect = 22,
    }
}