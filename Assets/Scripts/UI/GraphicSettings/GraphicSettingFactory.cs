using UI.GraphicSettings.Managers;

namespace UI.GraphicSettings
{
    internal static class GraphicSettingFactory
    {
        internal static GraphicSettingManager GetManager(GraphicSetting graphicSetting)
        {
            switch (graphicSetting)
            {
                case GraphicSetting.View_Range:
                    return new ViewRangeGraphicManager();
                case GraphicSetting.VSync:
                    return new VSyncGraphicManager();
                case GraphicSetting.AntiAliasing:
                    return new AntiAliasingManager();
                case GraphicSetting.Particles:
                    return new ParticleSettingManager();
                case GraphicSetting.FullScreen:
                    return new FullScreenManager();
                case GraphicSetting.Resolution:
                    return new ResolutionManager();
                default:
                    return null;
            }
        }
    }
}