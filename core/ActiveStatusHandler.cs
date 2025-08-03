using System;

namespace ui.core
{
    public interface ICanActive
    {
        bool Deactive(Event deactiveEvent);
        bool IsRequestingActive();
        bool IsRequestingDeactive();

        Event ActiveRequest();
    }

    public class ActiveStatusHandler
    {
        private ICanActive activeItem;

        public ICanActive GetCurrActive() => activeItem;

        public bool IsActive() => activeItem != null;

        public bool SetActive(ICanActive item)
        {
            if (item == null) throw new NullReferenceException("The provided item is null");
            if (!item.IsRequestingActive()) throw new InvalidOperationException("The item doesn't request to be active");
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

        public void SetInactive(ICanActive item)
        {
            if (item == null) throw new NullReferenceException("The provided item is null");
            if (!item.IsRequestingDeactive()) throw new InvalidOperationException("The item doesn't request to be deactive");
            if (item != activeItem && activeItem != null) throw new InvalidOperationException("The item is not the current active item");
            activeItem = null;
        }

    }
}
