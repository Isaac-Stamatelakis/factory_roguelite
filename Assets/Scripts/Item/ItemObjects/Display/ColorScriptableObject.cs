using UnityEngine;

namespace Item.ItemObjects.Display
{
    [CreateAssetMenu(fileName ="New Animated Item",menuName="Item/ColorObject")]
    public class ColorScriptableObject : ScriptableObject
    {
        public Color Color;
    }
}
