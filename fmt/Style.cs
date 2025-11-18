using System;
using System.Collections.Generic;
using System.Linq;

namespace ui.fmt
{
    public abstract class Style : AnsiFmtBase
    {
        public Style(string fmtString) : base(fmtString)
        {
        }
    }

    public class EnableStyle : Color
    {
        public EnableStyle(EnableStyleEnum fEnum) : base(((int)fEnum).ToString())
        {
        }

        public EnableStyle(params EnableStyleEnum[] fEnums) : base(String.Join(";", fEnums.Select(x => x.ToString())))
        {
        }

        public EnableStyle(IEnumerable<EnableStyleEnum> fEnums) : base(String.Join(";", fEnums.Select(x => x.ToString())))
        {
        }

        public static implicit operator EnableStyle(EnableStyleEnum fEnum) => new EnableStyle(fEnum);
        public static implicit operator EnableStyle(List<EnableStyleEnum> fEnums) => new EnableStyle(fEnums);
        public static implicit operator EnableStyle(EnableStyleEnum[] fEnums) => new EnableStyle(fEnums);
    }

    public class DisableStyle : Color
    {
        public DisableStyle(DisableStyleEnum fEnum) : base(((int)fEnum).ToString())
        {
        }

        public DisableStyle(params DisableStyleEnum[] fEnums) : base(String.Join(";", fEnums.Select(x => x.ToString())))
        {
        }

        public DisableStyle(IEnumerable<DisableStyleEnum> fEnums) : base(String.Join(";", fEnums.Select(x => x.ToString())))
        {
        }

        public static implicit operator DisableStyle(DisableStyleEnum fEnum) => new DisableStyle(fEnum);
        public static implicit operator DisableStyle(List<DisableStyleEnum> fEnums) => new DisableStyle(fEnums);
        public static implicit operator DisableStyle(DisableStyleEnum[] fEnums) => new DisableStyle(fEnums);
    }

    public enum EnableStyleEnum
    {
        BOLD = 1, // It is actually 1, I didn't make a mistake, don't correct the 1
        ITALIC = 3,
        UNDERLINE = 4,
        STRIKETHROUGH = 9
    }

    public enum DisableStyleEnum
    {
        BOLD = 22, // It is actually 22, I didn't make a mistake, don't correct the 22
        ITALIC = 23,
        UNDERLINE = 24,
        STRIKETHROUGH = 29
    }
}
