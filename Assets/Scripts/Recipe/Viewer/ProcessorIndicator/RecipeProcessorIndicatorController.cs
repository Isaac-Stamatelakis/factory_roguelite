using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Items;
using Recipe;
using Recipe.Processor;
using Recipe.Viewer;

namespace RecipeModule.Viewer {
    public class RecipeProcessorIndicatorController : MonoBehaviour
    {
        private Dictionary<RecipeProcessorPosition, List<RecipeProcessorIndicator>> indicators;
        private int length;
        private int currentIndex;
        private List<RecipeProcessor> processors;
        private Dictionary<RecipeProcessor, Sprite[]> processorSprites;
        private RecipeViewer recipeViewer;

        public RecipeViewer RecipeViewer { get => recipeViewer; set => recipeViewer = value; }

        public void Initialize(RecipeViewer recipeViewer, List<RecipeProcessor> processors, RecipeProcessor inital) {
            if (processors == null || processors.Count == 0) {
                return;
            }
            this.recipeViewer = recipeViewer;
            InitIndicators();
            processorSprites = new Dictionary<RecipeProcessor, Sprite[]>();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (RecipeProcessor recipeProcessor in processors)
            {
                RecipeProcessorInstance processorInstance = RecipeRegistry.GetProcessorInstance(recipeProcessor);
                List<TileItem> tileItems = ItemRegistry.getTileEntitiesOfProcessor(processorInstance);
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
            Display();
        }

        private void InitIndicators() {
            indicators = new Dictionary<RecipeProcessorPosition, List<RecipeProcessorIndicator>>
                {
                    [RecipeProcessorPosition.Center] = new List<RecipeProcessorIndicator>(),
                    [RecipeProcessorPosition.Left] = new List<RecipeProcessorIndicator>(),
                    [RecipeProcessorPosition.Right] = new List<RecipeProcessorIndicator>()
                };

            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                RecipeProcessorIndicator indicator = child.GetComponent<RecipeProcessorIndicator>();
                if (indicator == null) {
                    Debug.LogError(child.name + " did not have recipe processor indicator");
                    continue;
                }
                indicators[indicator.position].Add(indicator);
                indicator.init(this);
            }
            length = indicators[RecipeProcessorPosition.Left].Count;
            indicators[RecipeProcessorPosition.Left] = indicators[RecipeProcessorPosition.Left].OrderBy(i => i.index).ToList();
            indicators[RecipeProcessorPosition.Right] = indicators[RecipeProcessorPosition.Right].OrderBy(i => i.index).ToList();
            if (indicators[RecipeProcessorPosition.Left].Count != indicators[RecipeProcessorPosition.Right].Count) {
                Debug.LogError("Left and right sides of recipe processor indicator do not match");
                return;
            }
        }
        
        public void Display() {
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
        public void MoveLeft() {
            currentIndex = Global.modInt(currentIndex-1, processors.Count);
            Display();
        }

        public void MoveRight() {
            currentIndex = Global.modInt(currentIndex+1, processors.Count);
            Display();
        }

        public void MoveByAmount(int amount) {
            currentIndex = Global.modInt(currentIndex+amount, processors.Count);
            Display();
        }
    }

}
