use std::arch::x86_64::_mm_test_mix_ones_zeros;use nix::sys::termios;
use nix::sys::termios::{SetArg, Termios};
use std::io;
use std::io::Read;

pub fn add(left: u64, right: u64) -> u64 {
    left + right
}

#[unsafe(no_mangle)]
#[cfg(target_family = "unix")]
pub extern "C" fn init() -> i32{
    let stdin = io::stdin();
    let mut termios: Termios;
    match termios::tcgetattr(stdin) {
        Result::Ok(T) => termios = T,
        Result::Err(_) => return 1,
    }
    termios::cfmakeraw(&mut termios);
    let stdin = io::stdin();
    match termios::tcsetattr(stdin, SetArg::TCSANOW, &termios) {
        Result::Err(_) => return -2,
        _ => {}
    }
    0
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
