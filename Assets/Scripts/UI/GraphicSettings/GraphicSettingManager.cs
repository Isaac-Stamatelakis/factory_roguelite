using System.Collections.Generic;

namespace UI.GraphicSettings
{
    internal interface IDropDownGraphicManager
    {
        public List<string> GetStringValues();
    }
    public interface IScrollBarGraphicManager
    {
        public int GetSteps();
    }
    internal abstract class GraphicSettingManager
    {
        public abstract void ApplyGraphicSettings(int value);
        public abstract string GetValueName(int value);
    }
    
    internal abstract class BooleanGraphicSettingManager : GraphicSettingManager
    {
        public override string GetValueName(int value)
        {
            return value == 0 ? "Off" : "On";
        }
    }
}