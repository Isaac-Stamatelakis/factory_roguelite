using System.Collections;
using System.Collections.Generic;
using UnityEngine;


abstract public class ItemObject : ScriptableObject
{
    [Header("Unique identifier for this item")]
    public string id;
    public abstract Sprite getSprite();
}

public interface ISingleSpriteObject {
    public Sprite getSprite();
}

public interface IMultipleSpriteObject {
    public Sprite[] getSprites();
}

public interface IAnimatedSpriteObject {
    public Sprite[] getSprites();
}
