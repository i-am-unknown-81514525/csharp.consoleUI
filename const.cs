namespace ui
{
    public enum KeyCode : byte
    {
        INTERRUPT = 3,
        TAB = 9,
        NEWLINE = 13,
        SPACE = 32,
        BACKSPACE = 127,
        ARR_UP = 252, // SPECIAL: \x1b[A
        ARR_DOWN = 253, // SPECIAL: \x1b[B
        ARR_LEFT = 254, // SPECIAL: \x1b[C
        ARR_RIGHT = 255, // SPECIAL: \x1b[D
        SPECIAL_UNFOCUS = 250, // SPECIAL: \x1b[O
        SPECIAL_FOCUS = 251, // SPECIAL: \x1b[I
        DEL=249 // SPECIAL: \x1b[3~
    }
}