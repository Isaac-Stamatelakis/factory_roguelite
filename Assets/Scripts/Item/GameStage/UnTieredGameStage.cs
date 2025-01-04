using UnityEngine;

namespace Item.GameStage
{
    [CreateAssetMenu(fileName ="New Tiered Game Stage",menuName="Item/Game Stages/UnTiered")]
    public class UnTieredGameStage : GameStageObject
    {
        public override string GetGameStageId()
        {
            return name.ToLower().Replace(" ", "_");
        }
    }
}
