using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;
using Items.Tags;

namespace Items {
    public abstract class PresetItemObject : ItemObject
    {
        [SerializeField] [HideInInspector] protected Sprite[] sprites;
        public GameStageObject GameStageObject;
        #if UNITY_EDITOR
        public Sprite[] Sprites {get => sprites; set => sprites = value;}
        #endif
        [Header("Changing this modifies the format of sprite you can input")]
        [SerializeField]  protected ItemDisplayType displayType;
        public override ItemDisplayType? getDisplayType()
        {
            return displayType;
        }

        public override Sprite getSprite()
        {
            if (sprites == null || sprites.Length < 1) {
                return null;
            }
            return sprites[0];
        }

        public override Sprite[] getSprites()
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

