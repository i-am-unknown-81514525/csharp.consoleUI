using ui.utils;
using ui.core;

namespace ui.components
{
    public struct ComponentConfig
    {
        public SplitConfig splitConfig;
        public ActiveStatusHandler activeStatusHandler;

        public ComponentConfig(SplitConfig config, ActiveStatusHandler statusHandler)
        {
            splitConfig = config;
            activeStatusHandler = statusHandler;
        }
    }
}