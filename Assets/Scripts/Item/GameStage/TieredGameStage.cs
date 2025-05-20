using TileEntity;
using UnityEngine;

namespace Item.GameStage
{
    [CreateAssetMenu(fileName ="New Tiered Game Stage",menuName="Item/Game Stages/Tiered")]
    public class TieredGameStage : GameStageObject
    {
        public Tier Tier;
        public override string GetGameStageId()
        {
            return ((int)Tier).ToString();
        }

        public override string GetGameStageName()
        {
            return Tier.ToString();
        }
    }
}
