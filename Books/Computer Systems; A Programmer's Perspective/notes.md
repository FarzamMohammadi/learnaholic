# Computer Systems: A Programmer's Perspective by Randal E. Bryant & David R. O'Hallaron

# 1.1 Information Is Bits + Context

- hello.c = a source program / source file
  - Created via an editor.
  - Saved as a text file.
  - Source code is a sequence of bits, each with a value of 0 or 1.
    - Organized in 8-bit chunks called bytes.

```c
#include <stdlib.h>

int main()
{
    printf("Greetings, universe\n");
    return 0;
}
```

- Most modern systems represent text characters from the ASCII standard.
  - Where each character is represented via a unique byte-sized integer value.
  - ASCII files are either text files (such as hello.c) or binary files.

```c
#include <stdlib.h> // # 35, i 105, n 110, c 99, l 108, u 117, d 100, e 101, <sp> 32, < 60, s 115, t 116, d 100, l 108, i 105, b 98, > 62, \n 10
int main()         // i 105, n 110, t 116, <sp> 32, m 109, a 97, i 105, n 110, ( 40, ) 41, \n 10, { 123, \n 10
{                  // <sp> 32, { 123, \n 10
    printf("Greetings, universe\n"); // <sp> 32, <sp> 32, <sp> 32, <sp> 32, p 112, r 114, i 105, n 110, t 116, f 102, ( 40, " 34, G 71, r 114, e 101, e 101, t 116, i 105, n 110, g 103, s 115, <sp> 32, u 117, n 110, i 105, v 118, e 101, r 114, s 115, e 101, \ 92, n 110, " 34, ) 41, ; 59, \n 10
    return 0;       // <sp> 32, <sp> 32, <sp> 32, <sp> 32, r 114, e 101, t 116, u 117, r 114, n 110, <sp> 32, 0 48, ; 59, \n 10
}                  // <sp> 32, } 125, \n 10
```

## Origins of the C Programming Language

- C was developed from 1969 to 73 by Dennis Ritchie of Bell Labs.
- The American National Standards Institute (ANSI) ratified the ANSI C standard in 1989.
  - Then, the standardization became the responsibility of ISO.
- C is “quirky, flawed, and an enormous success.”
- C was closely tied with Unix.
  - Almost all of Unix was written in C.
  - Many universities and people adopted Unix, which also meant learning and working with C.
- C is small and simple.
  - The entire language was written by Dennis.
    - Which means consistent design with "little baggage."
  - It's good for system-level programming.
  - May not be as good for other programming.
    - Lacks support for useful abstractions like classes, objects, and exceptions.
    - Which C++ and Java were designed to take care of.