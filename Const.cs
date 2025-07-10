namespace ui
{
    public enum KeyCode : byte
    {
        INTERRUPT = 3,
        TAB = 9,
        NEWLINE = 13,
        PASTE = 22,
        ESC = 27,
        SPACE = 32,
        BACKSPACE = 127,
        INSERT = 245, // SPECIAL: \x1b[2~
        PG_UP = 246, // SPECIAL: \x1b[5~
        PG_DOWN = 247, // SPECIAL: \x1b[6~
        HOME = 244, // SPECIAL: \x1b[H
        END = 248, // SPECIAL: \x1b[F
        DEL = 249, // SPECIAL: \x1b[3~
        SPECIAL_UNFOCUS = 250, // SPECIAL: \x1b[O
        SPECIAL_FOCUS = 251, // SPECIAL: \x1b[I
        ARR_UP = 252, // SPECIAL: \x1b[A, \x1bOA
        ARR_DOWN = 253, // SPECIAL: \x1b[B, \x1bOB
        ARR_RIGHT = 254, // SPECIAL: \x1b[C, \x1bOC
        ARR_LEFT = 255 // SPECIAL: \x1b[D, \x1bOD
    }
}