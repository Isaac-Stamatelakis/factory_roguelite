using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "SpriteRotationCollection", menuName = "Editor/SpriteRotationCollection")]
public class SpriteRotationCollection : ScriptableObject
{
    public Sprite R0;
    public Sprite R1;
    public Sprite R2;
    public Sprite R3;
}

#endif