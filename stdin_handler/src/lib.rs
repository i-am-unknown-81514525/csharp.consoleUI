#[cfg(target_family = "unix")]
use nix::sys::termios::{SetArg, Termios, tcgetattr, cfmakeraw, tcsetattr, ControlFlags, InputFlags, LocalFlags, OutputFlags};
#[cfg(target_family = "windows")]
use win32console::console::{HandleType, WinConsole};
use std::fs::File;
use std::io::{self, BufRead};
use std::io::{Read, Write};
use std::ffi::{CString, c_char};

#[cfg(target_family = "windows")]
struct InputConsoleMode;
#[cfg(target_family = "windows")]
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

#[cfg(target_family = "windows")]
struct OutputConsoleMode;
#[cfg(target_family = "windows")]
impl OutputConsoleMode {
    pub const ENABLE_PROCESSED_OUTPUT: u32 = 1u32;
    pub const ENABLE_WRAP_AT_EOL_OUTPUT: u32 = 2u32;
    pub const ENABLE_VIRTUAL_TERMINAL_PROCESSING: u32 = 4u32;
    pub const DISABLE_NEWLINE_AUTO_RETURN: u32 = 8u32;
    pub const ENABLE_LVB_GRID_WORLDWIDE: u32 = 16u32;
}

#[cfg(target_family = "unix")]
fn internal_init() -> i32{
    let stdin = io::stdin();
    let mut termios: Termios;
    match tcgetattr(stdin) {
        Result::Ok(t) => {
            termios = t
        },
        Result::Err(_) => return -1,
    }
    cfmakeraw(&mut termios);
    let stdin = io::stdin();
    match tcsetattr(stdin, SetArg::TCSANOW, &termios) {
        Result::Err(_) => return -2,
        _ => {}
    }
    0
}

#[cfg(target_family = "unix")]
fn internal_reset() -> i32 {
    let c_iflag = InputFlags::from_bits_truncate(27906);
    let c_oflag = OutputFlags::from_bits_truncate(5);
    let c_cflag = ControlFlags::from_bits_truncate(1215);
    let c_lflag = LocalFlags::from_bits_truncate(35387);
    let stdin = io::stdin();
    let mut termios: Termios;
    match tcgetattr(stdin) {
        Result::Ok(t) => {
            termios = t
        },
        Result::Err(_) => return -1,
    }
    termios.input_flags = c_iflag;
    termios.output_flags = c_oflag;
    termios.control_flags = c_cflag;
    termios.local_flags = c_lflag;
    let stdin = io::stdin();
    match tcsetattr(stdin, SetArg::TCSANOW, &termios) {
        Result::Err(_) => return -2,
        _ => {}
    }
    0
}

#[cfg(target_family = "windows")]
fn internal_init() -> i32 {
    let mut value = match WinConsole::input().get_mode() {
        Err(_) => return -1025,
        Ok(T) => T
    };
    value |= InputConsoleMode::ENABLE_MOUSE_INPUT;
    value |= InputConsoleMode::ENABLE_WINDOW_INPUT;
    value |= InputConsoleMode::ENABLE_VIRTUAL_TERMINAL_INPUT;
    value |= InputConsoleMode::ENABLE_INSERT_MODE;
    value &= !InputConsoleMode::ENABLE_ECHO_INPUT;
    value &= !InputConsoleMode::ENABLE_QUICK_EDIT_MODE;
    value &= !InputConsoleMode::ENABLE_LINE_INPUT;
    value &= !InputConsoleMode::ENABLE_PROCESSED_INPUT;
    match WinConsole::input().set_mode(value) {
        Err(_) => return -1026,
        Ok(_) => {}
    }
    
    let mut value = match WinConsole::output().get_mode() { 
        Err(_) => return -1027,
        Ok(T) => T
    };
    value |= OutputConsoleMode::ENABLE_PROCESSED_OUTPUT;
    value |= OutputConsoleMode::ENABLE_VIRTUAL_TERMINAL_PROCESSING;
    match WinConsole::output().set_mode(value) { 
        Err(_) => return -1028,
        Ok(_) => {}
    }
    0
}

#[cfg(target_family = "windows")]
fn internal_reset() -> i32 {
    let mut value: u32 = 0;
    value |= InputConsoleMode::ENABLE_MOUSE_INPUT;
    value |= InputConsoleMode::ENABLE_INSERT_MODE;
    value |= InputConsoleMode::ENABLE_ECHO_INPUT;
    value |= InputConsoleMode::ENABLE_QUICK_EDIT_MODE;
    value |= InputConsoleMode::ENABLE_LINE_INPUT;
    value |= InputConsoleMode::ENABLE_PROCESSED_INPUT;
    value &= !InputConsoleMode::ENABLE_WINDOW_INPUT;
    value &= !InputConsoleMode::ENABLE_VIRTUAL_TERMINAL_INPUT;
    match WinConsole::input().set_mode(value) {
        Err(_) => return -1026,
        Ok(_) => {}
    }
    let mut value: u32 = 0;
    value |= OutputConsoleMode::ENABLE_PROCESSED_OUTPUT;
    value |= OutputConsoleMode::ENABLE_VIRTUAL_TERMINAL_PROCESSING;
    match WinConsole::output().set_mode(value) { 
        Err(_) => return -1028,
        Ok(_) => {}
    }
    0
}

fn ansi_reset() -> () {
    let _ = io::stdout()
        .write_all(b"\x1b[?1049l\x1b[H\x1b[0J\x1b[0m\x1b[?9l\x1b[?1000l\x1b[?1001l\x1b[?1002l\x1b[?1003l\x1b[?1006l\x1b[?1004l\x1b[?25h\x1b[?7h\x1b[H");
    let _ = io::stdout().flush();
}

#[unsafe(no_mangle)]
pub extern "cdecl" fn init() -> i32 {
    if cfg!(target_family = "windows") || cfg!(target_family = "unix") {
        return internal_init();
    }
    -2147483648
}

#[unsafe(no_mangle)]
pub extern "cdecl" fn reset() -> i32 {
    ansi_reset();
    if cfg!(target_family = "windows") || cfg!(target_family = "unix") {
        return internal_reset();
    } else {
        -2147483648
    }
}

#[unsafe(no_mangle)]
pub extern "cdecl" fn read_stdin() -> u8 {
    let mut stdin = io::stdin();
    let mut buffer = [0 as u8;1];
    stdin.read(&mut buffer).unwrap();
    buffer[0]
}

#[unsafe(no_mangle)]
pub extern "cdecl" fn stdin_data_remain() -> bool {
    let mut stdin = io::stdin().lock();
    let mut v1 = stdin.fill_buf()
                        .map(
                            |b| !b.is_empty()
                        )
                        .unwrap_or(false); // Experimental API implemented from https://github.com/rust-lang/rust/pull/85815
    drop(stdin);
    if v1 {
        let buf = (*io::stdin().lock().fill_buf().unwrap()).to_vec();
        v1 = buf.iter().count() > 0;
    }
    v1
}



// #[no_mangle]
// pub extern fn print_string(text_pointer: *const c_char) {
//     unsafe {
//         let text: String = CStr::from_ptr(text_pointer).to_str().expect("Can not read string argument.").to_string();
//         println!("{}", text);
//     }
// } // From https://stackoverflow.com/questions/66582380/pass-string-from-c-sharp-to-rust-using-ffi

const IS_DEBUG: bool = true;

#[unsafe(no_mangle)]
pub extern "cdecl" fn read_stdin_end()  -> *const c_char {
    let mut buf: Vec<u8> = vec![];
    if stdin_data_remain() {
        buf = (*io::stdin().lock().fill_buf().unwrap()).to_vec();
        if (IS_DEBUG) {
            let mut file = std::fs::OpenOptions::new() // https://stackoverflow.com/a/30684735
                .write(true)
                .append(true)
                .create(true)
                .open("read-log")
                .unwrap();

            if let Err(e) = writeln!(file, "{:?}", buf.iter().map(|v| v.to_string()).collect::<Vec<String>>().join(" ")) {
                eprintln!("Couldn't write to file: {}", e);
            }
        }
        if buf[0] == '\x1b' as u8 && buf.len() == 1 { // 
            io::stdin().lock().consume(buf.len() - 1); // ??? Somehow this work and don't question this
        } else {
            io::stdin().lock().consume(buf.len());
        }
    }
    let c_string: CString = CString::new(buf.as_slice()).unwrap();
    let ptr: *const c_char = c_string.as_ptr();
    std::mem::forget(c_string);
    ptr
}


// https://notes.huy.rocks/en/string-ffi-rust.html
// https://learn.microsoft.com/en-us/dotnet/framework/interop/default-marshalling-for-strings
