using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ItemModule;

namespace RecipeModule.Viewer {
    public class RecipeProcessorIndicatorController : MonoBehaviour
    {
        private Dictionary<RecipeProcessorPosition, List<RecipeProcessorIndicator>> indicators;
        private int length;
        private int currentIndex;
        private List<RecipeProcessor> processors;
        private Dictionary<RecipeProcessor, Sprite[]> processorSprites;

        public void init(List<RecipeProcessor> processors, RecipeProcessor inital) {
            if (processors == null || processors.Count == 0) {
                return;
            }
            initIndicators();
            processorSprites = new Dictionary<RecipeProcessor, Sprite[]>();
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            foreach (RecipeProcessor recipeProcessor in processors) {
                List<TileItem> tileItems = itemRegistry.getTileEntitiesOfProcessor(recipeProcessor);
                Sprite[] sprites = new Sprite[tileItems.Count];
                for (int i = 0; i < tileItems.Count; i++) {
                    sprites[i] = tileItems[i].getSprite();
                }
                processorSprites[recipeProcessor] = sprites;
            }

            this.processors = processors;
            int currentIndex = processors.IndexOf(inital);
            if (currentIndex == -1) {
                Debug.LogError("could not find initalIndex");
                return;
            }
            /*
            int size = Mathf.Min(length,processors.Count-1);
            int removals = length-size;
            for (int i = removals-1; i >= 0; i--) {
                GameObject.Destroy(indicators[RecipeProcessorPosition.Left][i].gameObject);
                indicators[RecipeProcessorPosition.Left].RemoveAt(i);
                GameObject.Destroy(indicators[RecipeProcessorPosition.Right][i].gameObject);
                indicators[RecipeProcessorPosition.Right].RemoveAt(i);
            }
            length -= removals;
            */
            display();
        }

        private void initIndicators() {
            indicators = new Dictionary<RecipeProcessorPosition, List<RecipeProcessorIndicator>>();
            indicators[RecipeProcessorPosition.Center] = new List<RecipeProcessorIndicator>();
            indicators[RecipeProcessorPosition.Left] = new List<RecipeProcessorIndicator>();
            indicators[RecipeProcessorPosition.Right] = new List<RecipeProcessorIndicator>();

            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                RecipeProcessorIndicator indicator = child.GetComponent<RecipeProcessorIndicator>();
                if (indicator == null) {
                    Debug.LogError(child.name + " did not have recipe processor indicator");
                    continue;
                }
                indicators[indicator.position].Add(indicator);
            }
            length = indicators[RecipeProcessorPosition.Left].Count;
            indicators[RecipeProcessorPosition.Left] = indicators[RecipeProcessorPosition.Left].OrderBy(i => i.index).ToList();
            indicators[RecipeProcessorPosition.Right] = indicators[RecipeProcessorPosition.Right].OrderBy(i => i.index).ToList();
            if (indicators[RecipeProcessorPosition.Left].Count != indicators[RecipeProcessorPosition.Right].Count) {
                Debug.LogError("Left and right sides of recipe processor indicator do not match");
                return;
            }
        }
        
        public void display() {
            indicators[RecipeProcessorPosition.Center][0].image.sprite = getRandomSprite(processorSprites[processors[currentIndex]]);
            for (int i = 0; i < length; i++) {
                int index = Global.modInt(currentIndex-(i+1), processors.Count);
                indicators[RecipeProcessorPosition.Left][i].image.sprite = getRandomSprite(processorSprites[processors[index]]);
            }
            for (int i = 0; i < length; i++) {
                int index = Global.modInt(currentIndex+(i+1), processors.Count);
                indicators[RecipeProcessorPosition.Right][i].image.sprite = getRandomSprite(processorSprites[processors[index]]);
            }
        }

        private Sprite getRandomSprite(Sprite[] sprites) {
            if (sprites.Length == 0) {
                return null;
            }
            int ran = Random.Range(0,sprites.Length);
            return sprites[ran];
        }
        public void moveLeft() {
            currentIndex = Global.modInt(currentIndex-1, processors.Count);
            display();
        }

        public void moveRight() {
            currentIndex = Global.modInt(currentIndex+1, processors.Count);
            display();
        }
    }

}
