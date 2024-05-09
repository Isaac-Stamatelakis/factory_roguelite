using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI {
    public class DynamicColorTextUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup list;
        [SerializeField] private DynamicTextCharacter charPrefab;
        private int fixedUpdatesForChange;
        private int frame;
        private List<Color> colors;
        private List<VerticalAlignmentOptions> positions;
        private DynamicTextCharacter[] characters;
        public void init(List<Color> colors, List<VerticalAlignmentOptions> positions, string text, int fixedUpdatesForChange) {
            this.colors = colors;
            this.positions = positions;
            this.fixedUpdatesForChange = fixedUpdatesForChange;
            characters = new DynamicTextCharacter[text.Length];
            int i = 0;
            foreach (char c in text) {
                DynamicTextCharacter character = GameObject.Instantiate(charPrefab);
                character.init(c);
                character.transform.SetParent(list.transform,false);
                characters[i] = character;
                i++;
            }
        }
        public void FixedUpdate() {
            frame ++;
            if (frame >= fixedUpdatesForChange) {
                frame = 0;
                switchColors();
                move();
            }
        }

        public void switchColors() {
            Color first = colors[colors.Count-1];
            colors.RemoveAt(colors.Count-1);
            colors.Insert(0,first);
            for (int i = 0; i < characters.Length; i++) {
                int index = i % colors.Count;
                characters[i].setColor(colors[index]);
            }
        }
        public void move() {
            VerticalAlignmentOptions first = positions[positions.Count-1];
            positions.RemoveAt(positions.Count-1);
            positions.Insert(0,first);
            for (int i = 0; i < characters.Length; i++) {
                int index = i % positions.Count;
                characters[i].setPosition(positions[index]);
            }
        }
    }
}

