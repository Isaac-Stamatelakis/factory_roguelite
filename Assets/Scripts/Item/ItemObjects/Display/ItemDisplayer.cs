using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Items {
    public static class ItemDisplayer 
    {
        private static readonly string stackDisplayContainerName = "SpriteStack";

        public static string StackDisplayContainerName => stackDisplayContainerName;

        public static void displayUISingle(Image image, ItemObject itemObject) {
            displayItemSpriteOnImage(image,itemObject.getSprite());
        }
        public static void displayUIAnimated(Image image, ItemObject itemObject, int animationCounter) {
            Sprite[] sprites = itemObject.getSprites();
            int animationIndex = animationCounter % sprites.Length;
            displayItemSpriteOnImage(image,sprites[animationIndex]);
        }

        public static void displayUIStack(UIItemSpriteStackContainer stackContainer, ItemObject itemObject) {
            
        }

        public static void displayItemSpriteOnImage(Image image, Sprite sprite) {
            image.sprite = sprite;
            
        }
        public static void displayOnUI(
            ItemObject itemObject, 
            GameObject gameObject, 
            Image image = null, 
            int animationIndex = 0, 
            UIItemSpriteStackContainer spriteStackContainer = null
        ) {
            ItemDisplayType? displayType = itemObject.getDisplayType();
            switch (displayType) {
                case ItemDisplayType.Single:
                    image.sprite = itemObject.getSprite();
                    break;
                case ItemDisplayType.Animated:
                    image.sprite = itemObject.getSprites()[animationIndex];
                    break;
                case ItemDisplayType.Stack:
                    if (spriteStackContainer == null) {
                        
                    }
                    /*
                    Sprite[] sprites = itemObject.getSprites();
                    Transform stackContainer = gameObject.transform.Find("SpriteStack");
                    if (stackContainer == null) {
                        GameObject containerObject = new GameObject();
                        containerObject.name = stackDisplayContainerName;
                        containerObject.transform.SetParent(gameObject.transform);
                        stackContainer = containerObject.transform;
                    }
                    if (stackContainer.childCount < )
                    */
                    break;
            }
        }
        public static void displayInWorld(ItemObject itemObject, GameObject gameObject, SpriteRenderer spriteRenderer = null) {
            ItemDisplayType? displayType = itemObject.getDisplayType();
            switch (displayType) {
                case ItemDisplayType.Single:
                    Sprite[] sprites = itemObject.getSprites();
                    if (sprites.Length == 0) {
                        Debug.LogError("Cannot display single sprite item with no sprite");
                        return;
                    }
                    if (spriteRenderer == null) {
                        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        if (spriteRenderer == null) {
                            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                        }
                    }
                    spriteRenderer.sprite = sprites[0];
                    break;
                case ItemDisplayType.Animated:

                    break;
                case ItemDisplayType.Stack:
                    Transform stackContainer = gameObject.transform.Find("SpriteStack");
                    if (stackContainer == null) {
                        GameObject containerObject = new GameObject();
                        containerObject.name = stackDisplayContainerName;
                        containerObject.transform.SetParent(gameObject.transform);
                        stackContainer = containerObject.transform;
                    }
                    
                    break;
            }
        }
    }
}

