using ui.utils;
using ui.core;

namespace ui.components
{
    public struct ComponentConfig
    {
        public ActiveStatusHandler ActiveStatusHandler;

        public ComponentConfig(ActiveStatusHandler statusHandler)
        {
            ActiveStatusHandler = statusHandler;
        }
    }
}