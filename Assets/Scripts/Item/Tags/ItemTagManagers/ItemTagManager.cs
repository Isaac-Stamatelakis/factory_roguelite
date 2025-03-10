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

    public interface IItemTagViewable
    {
        public ItemTagVisualLayer GetLayer();
    }
    public interface IToolTipTagViewable
    {
        public string GetToolTip(object obj);
    }
    
    public interface IItemTagUIViewable : IItemTagViewable {
        public GameObject GetUITagObject(object obj);
    }

    public interface IItemTagWorldViewable : IItemTagViewable
    {
        public GameObject GetWorldTagObject(object obj);
    }

    public interface IItemTagReferencedType
    {
        public object CreateDeepCopy(object obj);
    }

    public interface IItemTagStackable
    {
        public bool AreStackable(object first, object second);
    }
    public abstract class ItemTagManager : ISerializableTag
    {
        public abstract string Serialize(object obj);
        public abstract object Deserialize(string data);
    }
}
