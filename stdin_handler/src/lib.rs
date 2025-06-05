use std::collections::hash_map::Values;
#[cfg(target_family = "unix")]
use nix::sys::termios::{SetArg, Termios, tcgetattr, cfmakeraw, tcsetattr};
#[cfg(target_family = "windows")]
use win32console::console::{WinConsole, HandleType};
use std::io;
use std::io::Read;
use std::str::Matches;

pub fn add(left: u64, right: u64) -> u64 {
    left + right
}

#[cfg(target_family = "unix")]
fn internal_init() -> i32{
    let stdin = io::stdin();
    let mut termios: Termios;
    match tcgetattr(stdin) {
        Result::Ok(T) => termios = T,
        Result::Err(_) => return 1,
    }
    cfmakeraw(&mut termios);
    let stdin = io::stdin();
    match tcsetattr(stdin, SetArg::TCSANOW, &termios) {
        Result::Err(_) => return -2,
        _ => {}
    }
    0
}

struct InputConsoleMode;
impl InputConsoleMode {
    pub const ENABLE_PROCESSED_INPUT: u32 = 1u32;
    pub const ENABLE_LINE_INPUT: u32 = 2u32;
    pub const ENABLE_ECHO_INPUT: u32 = 4u32;
    pub const ENABLE_WINDOW_INPUT: u32 = 8u32;
    pub const ENABLE_MOUSE_INPUT: u32 = 16u32;
    pub const ENABLE_INSERT_MODE: u32 = 32u32;
    pub const ENABLE_QUICK_EDIT_MODE: u32 = 64u32;
    pub const ENABLE_VIRTUAL_TERMINAL_INPUT: u32 = 512u32;
}

struct OutputConsoleMode;
impl OutputConsoleMode {
    pub const ENABLE_PROCESSED_OUTPUT: u32 = 1u32;
    pub const ENABLE_WRAP_AT_EOL_OUTPUT: u32 = 2u32;
    pub const ENABLE_VIRTUAL_TERMINAL_PROCESSING: u32 = 4u32;
    pub const DISABLE_NEWLINE_AUTO_RETURN: u32 = 8u32;
    pub const ENABLE_LVB_GRID_WORLDWIDE: u32 = 16u32;
}

#[cfg(target_family = "windows")]
fn internal_init() -> i32 {
    let mut value = match WinConsole::input().get_mode() {
        Err(_) => return -1001,
        Ok(T) => T
    };
    value |= InputConsoleMode::ENABLE_MOUSE_INPUT;
    value |= InputConsoleMode::ENABLE_WINDOW_INPUT;
    value |= InputConsoleMode::ENABLE_VIRTUAL_TERMINAL_INPUT;
    value &= !InputConsoleMode::ENABLE_QUICK_EDIT_MODE;
    match WinConsole::input().set_mode(value) {
        Err(_) => return -1002,
        Ok(_) => {}
    }
    
    let mut value = match WinConsole::output().get_mode() { 
        Err(_) => return -1003,
        Ok(T) => T
    };
    value |= OutputConsoleMode::ENABLE_PROCESSED_OUTPUT;
    value |= OutputConsoleMode::ENABLE_VIRTUAL_TERMINAL_PROCESSING;
    match WinConsole::output().set_mode(value) { 
        Err(_) => return -1004,
        Ok(_) => {}
    }
    0
}

#[unsafe(no_mangle)]
pub  extern "C" fn init() -> i32 {
    if cfg!(target_family = "windows") || cfg!(target_family = "unix") {
        return internal_init();
    }
    -2147483648
}

#[unsafe(no_mangle)]
pub extern "C" fn read_stdin() -> u8 {
    let mut stdin = io::stdin();
    let mut buffer = [0 as u8;1];
    stdin.read(&mut buffer).unwrap();
    buffer[0]
}


// #[cfg(test)]
// mod tests {
//     use super::*;
// 
//     #[test]
//     fn it_works() {
//         let result = add(2, 2);
//         assert_eq!(result, 4);
//     }
// }
