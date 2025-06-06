using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;
using Items.Tags;

namespace Items {
    public abstract class PresetItemObject : ItemObject
    {
        [SerializeField]  protected Sprite[] sprites;
        public GameStageObject GameStageObject;
        public Sprite[] Sprites {get => sprites; set => sprites = value;}
        [Header("Changing this modifies the format of sprite you can input")]
        [SerializeField]  protected ItemDisplayType displayType;
        public override ItemDisplayType? getDisplayType()
        {
            return displayType;
        }

        public override Sprite GetSprite()
        {
            if (sprites == null || sprites.Length < 1) {
                return null;
            }
            return sprites[0];
        }

        public override Sprite[] GetSprites()
        {
            return sprites;
        }

        public override GameStageObject GetGameStageObject()
        {
            return GameStageObject;
        }
        public override void SetGameStageObject(GameStageObject gameStageObject)
        {
            GameStageObject = gameStageObject;
        }
    }
}

