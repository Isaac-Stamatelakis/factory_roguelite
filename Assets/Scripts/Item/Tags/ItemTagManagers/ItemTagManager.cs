using System.Diagnostics.CodeAnalysis;
using Items;
using UnityEngine;

namespace Item.Tags.ItemTagManagers
{
    public enum ItemTagVisualLayer
    {
        Front,
        Back
    }
    public interface ISerializableTag {
        public string Serialize(object obj);
        public object Deserialize(string data);
    }
    
    public interface IToolTipTagViewable
    {
        public string GetToolTip(object obj);
    }
    
    public interface IItemTagUIViewable {
        public GameObject GetUITagObject(object obj, ItemObject containerObject);
    }

    public interface IItemTagWorldViewable 
    {
        public GameObject GetWorldTagObject(object obj, ItemObject containerObject);
    }

    public interface IItemTagReferencedType
    {
        public object CreateDeepCopy(object obj);
    }

    public interface IItemTagStackable
    {
        public bool AreStackable(object first, object second);
    }

    public interface IItemTagNullStackable : IItemTagStackable
    {
        public bool IsStackableWithNullObject([NotNull] object nonNullObject);
    }
    public abstract class ItemTagManager : ISerializableTag
    {
        public abstract string Serialize(object obj);
        public abstract object Deserialize(string data);
    }
}
