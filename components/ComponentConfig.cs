using ui.utils;
using ui.core;

namespace ui.components
{
    public struct ComponentConfig
    {
        public ActiveStatusHandler activeStatusHandler;

        public ComponentConfig(ActiveStatusHandler statusHandler)
        {
            activeStatusHandler = statusHandler;
        }
    }
}