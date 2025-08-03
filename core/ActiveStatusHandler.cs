using System;

namespace ui.core
{
    public interface ICanActive
    {
        bool Deactive(Event deactiveEvent);
        bool isRequestingActive();
        bool isRequestingDeactive();

        Event ActiveRequest();
    }

    public class ActiveStatusHandler
    {
        private ICanActive activeItem;

        public ICanActive getCurrActive() => activeItem;

        public bool isActive() => activeItem != null;

        public bool setActive(ICanActive item)
        {
            if (item == null) throw new NullReferenceException("The provided item is null");
            if (!item.isRequestingActive()) throw new InvalidOperationException("The item doesn't request to be active");
            if (activeItem == null)
            {
                activeItem = item;
                return true;
            }
            bool result = activeItem.Deactive(activeItem.ActiveRequest());
            if (!result) return false;
            activeItem = item;
            return true;
        }

        public void setInactive(ICanActive item)
        {
            if (item == null) throw new NullReferenceException("The provided item is null");
            if (!item.isRequestingDeactive()) throw new InvalidOperationException("The item doesn't request to be deactive");
            if (item != activeItem && activeItem != null) throw new InvalidOperationException("The item is not the current active item");
            activeItem = null;
        }

    }
}
