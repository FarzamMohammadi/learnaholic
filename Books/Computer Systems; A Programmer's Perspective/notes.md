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

# 1.2 Programs Are Translated by Other Programs into Different Forms

- Hello begins as a high-level C program, which is easier to understand by humans.
- Then it gets translated by other programs into a sequence of low-level machine-language instructions.
- Then these instructions are packed in a form called an executable object program and stored as a binary disk file.
- Object programs are also referred to as executable object files.

## The Compilation System (Preprocessor, Compiler, Assembler, and Linker)

```yaml
Source Program: hello.c (text)
  |
  V
Pre-processor: cpp
  |
  V
Modified Source Program: hello.i (text)
  |
  V
Compiler: cc1
  |
  V
Assembly Program: hello.s (text)
  |
  V
Assembler: as
  |
  V
Relocatable Object Programs: hello.o (binary)
  |
  +-------------------+
  |                   |
  V                   |
Linker: ld            |
  |                   |
  V                   V
Executable Object Program: hello (binary)  External Library: printf.o
```

"Preprocessing phase. The preprocessor (cpp) modifies the original C program according to directives that begin with the # character. For example, the #include <stdio.h> command in line 1 of hello.c tells the preprocessor to read the contents of the system header file stdio.h and insert it directly into the program text. The result is another C program, typically with the .i suffix."

- In this stage, the code is prepared for compilation. Here, which includes external files, replacing macros, and processing conditional compilation instructions.

"Compilation phase. The compiler (cc1) translates the text file hello.i into the text file hello.s, which contains an assembly-language program. Each statement in an assembly-language program exactly describes one low-level machine-language instruction in a standard text form. Assembly language is useful because it provides a common output language for different compilers for different high-level languages. For example, C compilers and Fortran compilers both generate output files in the same assembly language."

- The compiler converts the preprocessed code into assembly language, providing a low-level representation of the original code that's common across different languages.

"Assembly phase. Next, the assembler (as) translates hello.s into machine-language instructions, packages them in a form known as a relocatable object program, and stores the result in the object file hello.o. The hello.o file is a binary file whose bytes encode machine language instructions rather than characters. If we were to view hello.o with a text editor, it would appear to be gibberish."

- The assembler turns assembly code into machine language, creating a binary object file that computers can execute.

"Linking phase.Notice that our hello program calls the printf function, which is part of the standard C library provided by every C compiler. The printf function resides in a separate precompiled object file called printf.o, which must somehow be merged with our hello.o program. The linker (ld) handles this merging. The result is the hello file, which is an executable object file (or simply executable) that is ready to be loaded into memory and executed by the system."

- The linker combines the binary object file with other binary files, like libraries, to produce the final executable file.

Summary:
1. Preprocessing: Imports and macros are processed, creating a complete source file without external libraries.
2. Compilation: The complete source file is compiled into assembly language, which is a common output for different high-level languages.
3. Assembly: The assembly code is translated into machine code, producing a binary file with low-level instructions.
4. Linking: The binary object files are linked with external libraries to produce the final executable program.

## The GNU Project

- The aforementioned GCC is one of the many tools developed by the GNU (short for GNU’s Not Unix) project. This project is a tax-exempt charity started by Richard Stallman. He wanted to create a Unix-like system that could be shared and modified without restriction.
- The modern open-source movement started with GNU, with the project's notion of free software (“free” as in “free speech,” not “free beer”). Linux owes a lot of its popularity to GNU tools.
- "Free as in speech, not as in beer" distinguishes between freedom and price. It means something is free in terms of rights and liberty (like freedom of speech), not free of cost (like a free beer). This often refers to open-source software, where "free" means you're allowed to use, modify, and share the software, not that it's necessarily without cost.

# 1.3 It Pays to Understand How Compilation Systems Work

Reasons why programmers need to understand how compilation systems work:

- Optimizing program performance.
  - "For example, is a switch statement always more efficient than a sequence of if-else statements? How much overhead is incurred by a function call? Is a while loop more efficient than a for loop? Are pointer references more efficient than array indexes? Why does our loop run so much faster if we sum into a local variable instead of an argument that is passed by reference? How can a function run faster when we simply rearrange the parentheses in an arithmetic expression?"

- Understanding link-time errors.
  - Lots of complex errors come from linker operations, especially in large systems.
  - "For example, what does it mean when the linker reports that it cannot resolve a reference? What is the difference between a static variable and a global variable? What happens if you define two global variables in different C files with the same name? What is the difference between a static library and a dynamic library? Why does it matter what order we list libraries on the command line? And scariest of all, why do some linker-related errors not appear until run time?"

- Avoiding security holes.
  - For a long time, Buffer Overflow vulnerabilities accounted for the majority of security holes in network and internet servers.
    - Mainly because too few programmers understand the need to restrict the quantity and forms of data they accept from untrusted sources.
    - Secure programming starts by understanding the consequences of the way data and control information are stored on the program stack.