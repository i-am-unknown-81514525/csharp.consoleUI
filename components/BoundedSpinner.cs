using System;
using ui.components.chainExt;
using ui.fmt;

namespace ui.components
{
    public class DisableStore : ComponentStore
    {
        public bool disabled { get; set; }
    }

    public class DisableButton : Button<DisableStore>
    {

        public DisableButton(string label = null) : base(label) { }

        public override DisableStore ComponentStoreConstructor()
        {
            return new DisableStore();
        }
    }

    public class BoundedSpinner : Container
    {
        public Action<int> OnChange = value => { };

        //Reactive of amount with type int and default value: `0`, Trigger: SetHasUpdate();
        private int _amount;
        public int amount
        {
            get
            {
                return _amount;
            }
            set
            {
                ValueLabel.text = value.ToString();
                _amount = value;
                SetHasUpdate();
                UpdateColor();
                OnChange(amount);
            }
        }

        //Reactive of lower with type int and default value: `int.MinValue`, Trigger: SetHasUpdate();
        private int _lower = int.MinValue;
        public int lower
        {
            get => _lower;
            set
            {
                _lower = value;
                SetHasUpdate();
                UpdateColor();
            }
        }

        //Reactive of upper with type int and default value: `int.MaxValue`, Trigger: SetHasUpdate();
        private int _upper = int.MaxValue;
        public int upper { get => _upper; set { _upper = value; SetHasUpdate(); UpdateColor(); } }

        public string content { get => TextLabel.text; set { TextLabel.text = value; SetHasUpdate(); } }

        public TextLabel TextLabel = new TextLabel("");

        public DisableButton DescButton = new DisableButton("[-]");

        public TextLabel ValueLabel = new TextLabel("");

        public DisableButton AscButton = new DisableButton("[+]");

        private HorizontalGroupComponent _group = new HorizontalGroupComponent();

        public void HiddenChange(int value)
        {
            if (_amount != value)
            {
                ValueLabel.text = value.ToString();
                _amount = value;
                SetHasUpdate();
                UpdateColor();
            }

        }

        private void LoadComponents()
        {
            DescButton.OnClickHandler = (button, loc) =>
            {
                if (!button.Store.disabled)
                {
                    amount--;
                }
            };
            AscButton.OnClickHandler = (button, loc) =>
            {
                if (!button.Store.disabled)
                {
                    amount++;
                }
            };
            _group.Add(TextLabel);
            _group.Add((DescButton, 3));
            _group.Add(ValueLabel);
            _group.Add((AscButton, 3));
            Add(_group);
        }

        public BoundedSpinner(int amount = 0, int lower = int.MinValue, int upper = int.MaxValue)
        {
            this.amount = amount;
            this.lower = lower;
            this.upper = upper;
            LoadComponents();
        }

        public BoundedSpinner(string content = "", int amount = 0, int lower = int.MinValue, int upper = int.MaxValue)
        {
            this.content = content;
            this.amount = amount;
            this.lower = lower;
            this.upper = upper;
            LoadComponents();
        }

        private void UpdateColor()
        {
            if (amount <= lower)
            {
                DescButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.RED);
                DescButton.Store.disabled = true;
            }
            else
            {
                DescButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.WHITE);
                DescButton.Store.disabled = false;
            }

            if (amount >= upper)
            {
                AscButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.RED);
                AscButton.Store.disabled = true;
            }
            else
            {
                AscButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.WHITE);
                AscButton.Store.disabled = false;
            }
        }
    }
}
