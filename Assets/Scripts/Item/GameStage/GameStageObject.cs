using UnityEngine;

namespace Item.GameStage
{
    public enum GameStageType
    {
        Tiered,
        UnTiered
    }
    public abstract class GameStageObject : ScriptableObject
    {
        public abstract string GetGameStageId();
    }
}
