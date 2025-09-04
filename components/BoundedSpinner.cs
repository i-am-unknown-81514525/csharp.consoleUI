using System;
using ui.components.chainExt;
using ui.fmt;
using ui.utils;

namespace ui.components {
    public class DisableStore : ComponentStore
    {
        public bool disabled { get; set; } = false;
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
        public Action<int> onChange = (value) => { };

        //Reactive of amount with type int and default value: `0`, Trigger: SetHasUpdate();
        private int _amount = 0;
        public int amount
        {
            get
            {
                return _amount;
            }
            set
            {
                valueLabel.text = value.ToString();
                _amount = value;
                SetHasUpdate();
                UpdateColor();
                onChange(amount);
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

        public string content { get => textLabel.text; set { textLabel.text = value; SetHasUpdate(); } }

        public TextLabel textLabel = new TextLabel("");

        public DisableButton descButton = new DisableButton("[-]");

        public TextLabel valueLabel = new TextLabel("");

        public DisableButton ascButton = new DisableButton("[+]");

        private HorizontalGroupComponent group = new HorizontalGroupComponent();

        private void LoadComponents()
        {
            descButton.onClickHandler = (button, loc) =>
            {
                if (!button.store.disabled)
                {
                    amount--;
                }
            };
            ascButton.onClickHandler = (button, loc) =>
            {
                if (!button.store.disabled)
                {
                    amount++;
                }
            };
            group.Add(textLabel);
            group.Add((descButton, 3));
            group.Add(valueLabel);
            group.Add((ascButton, 3));
            Add(group);
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
                descButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.RED);
                descButton.store.disabled = true;
            }
            else
            {
                descButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.WHITE);
                descButton.store.disabled = false;
            }

            if (amount >= upper)
            {
                ascButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.RED);
                ascButton.store.disabled = true;
            }
            else
            {
                ascButton.WithBackground<DisableStore, DisableButton>(BackgroundColorEnum.WHITE);
                ascButton.store.disabled = false;
            }
        }
    }
}