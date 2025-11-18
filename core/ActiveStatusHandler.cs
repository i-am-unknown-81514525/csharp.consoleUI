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
        private ICanActive _activeItem;

        public ICanActive GetCurrActive() => _activeItem;

        public bool IsActive() => _activeItem != null;

        public bool SetActive(ICanActive item)
        {
            if (item == null) throw new NullReferenceException("The provided item is null");
            if (!item.IsRequestingActive()) throw new InvalidOperationException("The item doesn't request to be active");
            if (_activeItem == null)
            {
                _activeItem = item;
                return true;
            }
            bool result = _activeItem.Deactive(_activeItem.ActiveRequest());
            if (!result) return false;
            _activeItem = item;
            return true;
        }

        public void SetInactive(ICanActive item)
        {
            if (item == null) throw new NullReferenceException("The provided item is null");
            if (!item.IsRequestingDeactive()) throw new InvalidOperationException("The item doesn't request to be deactive");
            if (item != _activeItem && _activeItem != null) throw new InvalidOperationException("The item is not the current active item");
            _activeItem = null;
        }

    }
}
