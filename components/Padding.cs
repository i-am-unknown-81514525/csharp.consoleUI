namespace ui.components
{
    public class Padding : TextLabel
    {
        public new string text
        {
            get => base.text;
        }
        public Padding() : base(" ") { }
        public Padding(ComponentConfig config) : base(" ", config) { }

    }
}
