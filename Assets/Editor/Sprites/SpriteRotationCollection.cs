using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "SpriteRotationCollection", menuName = "Editor/SpriteRotationCollection")]
public class SpriteRotationCollection : ScriptableObject
{
    public ReadAndCopySprite[] Sprites;

    public Sprite GetDefaultReadSprite()
    {
        return Sprites[0].ReadSprite;
    }
}

[System.Serializable]
public class ReadAndCopySprite
{
    public Sprite CopySprite;
    public Sprite ReadSprite;
}
#endif