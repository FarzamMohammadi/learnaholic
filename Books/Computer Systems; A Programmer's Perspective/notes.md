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
  
# 1.4 Processors Read and Interpret Instructions Stored in Memory

## Running the Hello Executable
- Continuing with our hello file example, it's time to execute hello.
- In Unix, we open an application program called "shell".
- The shell then prompts for a command line.
- To run hello, we type "./hello" and press Enter.
- Then the following message is printed: Greetings, universe.
  - Shell has built-in commands.
  - When shell receives a command that starts with a word other than its built-in commands, it assumes the user is running an executable.
  - After typing "./hello" and pressing Enter, the shell loads the file and runs it.
  - It then waits for the program to terminate.
- After the hello program runs and terminates, it then prompts the user for the next command line.

## Hardware in Use
- Some of the hardware in use during this process includes the following:
  - Buses: They carry information back and forth between components. They typically transfer fixed-size chunks of bytes known as words. The number of bytes depends on the system. Most machines today are either 4 bytes, meaning 32-bits, or 8 bytes, meaning 64-bits.
  - I/O Devices: Input/output (I/O) devices are the system’s connections to the external world, like a keyboard or a mouse. Each device is connected to the I/O bus by either a controller or an adapter. Regardless of the type, the purpose of both is to transfer information between the I/O bus and the device.
    - Controllers are chipsets in the device itself or on the main printed circuit board, aka motherboard.
    - Adapters are cards that plug into slots on the motherboard.
  - Main Memory: A temporary storage device that holds both the program and the data it manipulates while the processor is executing the program.
    - Physically, main memory consists of dynamic random-access memory, aka DRAM, chips.
    - Logically, memory is organized as a linear array of bytes, each with its own unique address (array index) starting at 0.
    - Each machine instruction that constitutes a program is made up of a variable number of bytes.
      - Even for C alone, the size of data items can vary based on the machine.
        - On an IA32 machine, for instance, data of type short requires two bytes, types int, float, and long four bytes, and type double eight bytes.
  - Processor: The central processing unit, aka CPU, aka processor, is the engine that interprets (or executes) instructions in main memory. From the moment power is applied to the system until the last moment, the processor is executing instructions. It is constantly executing what is pointed at by the program counter, aka PC, and then updating the program counter to point to the next instruction.
    - "A processor appears to operate according to a very simple instruction execution model, defined by its instruction set architecture."
    - The described instruction set architecture is simple, but in reality, most instruction set architectures are extremely complex and have been designed to optimize the speed of program execution.
    - The instruction set architecture can be found out based on the processor's microarchitecture.
  - Examples of some instructions that the CPU may carry out:
    - Load: Copy a byte or a word from main memory into a register, overwriting the previous contents of the register.
    - Store: Copy a byte or a word from a register to a location in main memory, overwriting the previous contents of that location.
    - Operate: Copy the contents of two registers to the ALU, perform an arithmetic operation on the two words, and store the result in a register, overwriting the previous contents of that register.
    - Jump: Extract a word from the instruction itself and copy that word into the program counter (PC), overwriting the previous value of the PC.

## Hardware Processes Behind the Scenes
- Now, we'll describe what happens with our hardware as we run the hello program.
  - Initially, the shell program executes its instructions and waits for us to type a command.
  - As we type "./hello" at the keyboard, the shell program reads each character into a register and then stores it in memory.
  - As we hit Enter, the shell knows we've finished typing the command.
  - The shell then loads the executable hello file by executing a sequence of instructions that copies the code and data from the object file from disk to main memory.
  - The data includes the string of characters “Greetings, universe\n” that will eventually be printed out.
  - Using a technique called direct memory access (or DMA), the data goes directly from the disk to main memory without passing through the processor.
    - Direct memory access (DMA) is a method that allows an input/output (I/O) device to send or receive data directly to or from the main memory, bypassing the CPU to speed up memory operations.
  - Once the code and data from the hello object file are loaded into memory, the processor begins executing the machine-language instructions in the hello program's main routine.
  - These instructions copy the bytes in the “Greetings, universe\n” string from memory to the register file, and from there to the display device where it's displayed on the screen.

Note: The illustrations in the text provide a detailed visualization of the program's execution flow within the hardware components.

# 1.5 Caches Matter

- An important lesson from the learnings so far: The system spends a lot of time moving information from one place to another.
- The machine instructions in the hello program are originally stored on disk. When the program is loaded, they are copied to main memory. As the processor runs the program, instructions are copied from main memory into the processor. Similarly, the data string “Greetings, universe\n”, originally on disk, is copied to main memory and then from main memory to the display device.
- Due to physical laws, larger storage devices are slower than smaller storage devices.
- Faster devices are more expensive to build than their slower counterparts.
- A typical register file stores only a few hundred bytes of info, whereas main memory stores much more.
- The processor can read data from the register file almost 100 times faster than from memory.
- This processor-memory gap is continuously increasing due to the progression in semiconductor development.
- It's much easier and cheaper to make processors run faster than to do the same for main memory.
- To deal with this processor-memory gap, system designers include smaller and faster storage devices called cache memories (or caches) that serve as temporary staging areas for information the processor is likely to need in the near future.
- Caches typically have multiple levels.
- The L1 cache on the processor chip holds tens of thousands of bytes.
- The L2 cache holds tens of millions of bytes, which is why it might take 5 times longer for the processor to access the L2 cache than the L1.
- The L1 and L2 caches are implemented with a hardware technology known as static random-access memory (SRAM).
- In newer cache memories, there are 3 levels (L1, L2, L3).
- The idea behind caching is that a system can get the effect of both a very large memory and a very fast one by exploiting 'locality', the tendency for programs to access data and code in localized regions.
- Programmers can exploit their understanding of cache memories to improve the performance of their programs by an order of magnitude.

# 1.6 Storage Devices Form a Hierarchy

- The general idea: Inserting a smaller, faster storage device (e.g., cache memory) between the processor and a larger, slower device (e.g., main memory).
- The storage devices in every computer system are organized as a 'memory hierarchy':
  - As we move down the hierarchy, the devices become slower, larger, and less costly per byte.
  - The register file is at the very top, known as L0 or level 0.
  - Following this are L1 to L3 (or level 1 to level 3), which are the cache memories.
  - Main memory is positioned from level 4 downwards.
- The main concept of a memory hierarchy is that storage at one level acts as a cache for storage at the next lower level. For instance, the register file is a cache for the L1 cache. Caches L1 and L2 serve as caches for L2 and L3, respectively. The L3 cache is a cache for the main memory, which, in turn, is a cache for the disk. In certain networked systems with distributed file systems, the local disk functions as a cache for data stored on the disks of other systems.
  - Side note: In the context of memory hierarchy, the term "cache" refers to speed of access and proximity to the CPU, rather than the amount of data stored. Higher levels of the hierarchy (like the register file) are faster but hold less data, caching data from the lower (slower but larger) levels.
- As programmers, we can leverage our understanding of the memory hierarchy to write more efficient programs.

# 1.7 The Operating System Manages the Hardware

- In previous processes, it was never the program or shell that was accessing the keyboard, display, disk, or main memory directly; it was the 'operating system'.
- All attempts by an application to manipulate the hardware must go through the operating system.
- The operating system has 2 purposes:
  1. Protect hardware from misuse.
  2. Provide applications with a simple and uniform mechanism for manipulating complicated and widely different low-level hardware devices.

## Aside
- The 1960s was an era with complex operating systems.
  - IBM had OS/360.
  - Honeywell had Multics.
  - OS/360 was one of the most successful projects back then.
  - Multics was taking a long time to complete, and Bell Labs was helping them.
  - In 1969, it got super complex, and the progress was so little that Bell Labs, which included Dennis Ritchie, left the Multics team.
  - Bell Labs created their own OS written entirely in machine language, with many ideas borrowed from Multics.
  - They named the new system "Unix" as a pun on the complexity of "Multics".
  - The kernel was rewritten in C in 1973, and Unix was announced to the world in 1974.
  - In the 1980s, Unix vendors tried to differentiate themselves by adding new and often complicated features.
  - To combat this, IEEE sponsored the effort to standardize Unix, which was later dubbed as "Posix" by Richard Stallman.

## 1.7.1 Processes
- When running hello, it appears that all throughout the flow, hello has exclusive access to main memory and I/O devices.
- It appears that hello is running instructions one by one without disruption, completely in charge.
- All of this is an illusion created by the notion of 'process'.
- A process is the OS's abstraction for a running program.
- Multiple processes can run concurrently, and each will appear to have exclusive access to the hardware it uses.
  - Newer systems with multicore CPUs can execute multiple programs simultaneously.
- The concurrency is enabled via the OS's interleaving ability with a mechanism known as 'context switching'.
- The OS keeps track of all state information to handle concurrency.
  - This state is known as context.
- The OS performs context switching by saving the state of the currently executing program and its processes and switching and passing over control to the new one.
- The process then picks up exactly where it left off before.
- In our hello program example, there are 2 concurrent processes:
  - The shell starts by running alone, waiting for input.
  - When we run hello, the shell then carries out our request by invoking a special function known as 'system call' that passes control to the OS.
  - The OS saves the shell's context and then passes control to the new hello process.
  - After hello terminates, the OS restores the shell process, passes control back to it, and waits for the next command line input.
- This process abstraction is enabled by close cooperation between low-level hardware and the OS.

## 1.7.2 Threads
- In modern systems, a process can consist of multiple execution units called 'threads'.
- It is easier to share data between multiple threads than multiple processes.

## 1.7.3 Virtual Memory
- Virtual memory is an abstraction that provides each process with the illusion that it has exclusive use of the main memory.
- Each process has the same uniform view of memory, which is known as its virtual address space.
- In Linux, the topmost region of the address space is reserved for code and data in the OS that's common to all processes.
- The low region of the address space holds the code and data defined by the user's process.

- Notes on addresses, from lowest going up:  
  - Program Code and Data: This is where your program's code (instructions) and global variables live. Every process starts with its code at a specific address, and right after the code, you have the global variables. This part doesn't change size while the program runs. It's set up when your program starts, based on the executable file.
  
  - Heap: Right after your program's code and global variables, there's the heap. Unlike the code and data, the heap's size can change while your program is running. When you create new objects or free up space (like using malloc and free in C), you're working with the heap. It grows when you allocate more memory and shrinks when you free memory.
  
  - Shared Libraries: In the middle of the address space, there's a section for shared libraries. These are common pieces of code, like the C standard library, that many programs use. Instead of having a copy in every program, they share this code to save space and resources. This part includes code and data that are reused across different programs.
  
  - Stack: Near the top of the address space, you have the stack. This is used for managing function calls. Every time you enter a function, a "layer" (or frame) is added to the stack with information like variables local to that function. When the function returns, that layer is removed. The stack grows and shrinks dynamically as functions are called and return.
  
  - Kernel Virtual Memory: The very top part of the address space is reserved for the operating system's kernel. This is the core part of the OS, and it controls everything. Applications can't access this area directly; they have to go through system calls to interact with the kernel.

### Personal notes:
To explain virtual memory and how it works in simpler terms:

Virtual memory is a system that lets your computer run programs that need more memory than what is physically available in the main memory (RAM). It does this by using a combination of hardware and software to manage memory more efficiently.

Here's how it basically works:

  1. Storing on Disk: The entire virtual memory a process might need is stored on the disk. This includes all the code, data, stack, and heap the process might use.

  2. Main Memory as a Cache: The main memory (RAM) acts like a cache for the disk. A cache is a smaller, faster storage area that keeps copies of the data from the most frequently used parts of the disk.

  3. Address Translation: Every time the processor needs to access memory, the hardware translates the virtual address (the address within the process's virtual memory space) to a physical address (the actual address in RAM). This translation is done using a special piece of hardware.

  4. Working Set: Only a portion of the process's virtual memory is kept in RAM at any time. This portion is what's currently being used or what's expected to be used soon (often called the "working set"). If the process needs to access data not in RAM, the system will load it from the disk into RAM, potentially replacing some of the existing data in RAM.

  5. Swap: The process of moving data between the disk and RAM to make space for new data is called swapping or paging. This allows the system to make efficient use of the available RAM and lets processes run as if they have more memory than what's physically available.

In summary, virtual memory allows your computer to use its hardware and operating system software to make it seem like there's much more memory available by storing most of the data on disk and keeping only the most used data in RAM. This requires constant address translation and moving data back and forth between RAM and disk as needed.

# 1.7.4 Files
- A file is a sequence of bytes.
- Every I/O device is modeled as a file.
- All input and output is performed by reading from and writing to files, typically using system calls known as Unix I/O.

## Aside
- Linux was created by Finnish graduate student Linus Torvalds.

# 1.8 Systems Communicate with Other Systems Using Networks

- Until now, we've considered the system as an isolated entity of hardware and software.
- In reality, systems are connected to other systems via networks.
- Network communication can be seen as another form of I/O process:
  - Outgoing: The system copies a sequence of bytes from main memory to the network adapter.
  - Incoming: The system reads data sent from other machines and copies it into main memory.
- To illustrate using our hello program example, let's look at a telnet session:
  - We run a telnet client on our local machine, connecting to a telnet server.
  - We execute the hello program, just as we did previously in section 1.2.
  - However, this time, the client sends the program across the network to the server on a remote machine.
  - The remote server receives the "hello" command and forwards it to the remote shell (command interpreter).
  - The remote shell executes the "hello" program.
  - Instead of the output being displayed on the remote machine, the remote shell sends the output back to the telnet server.
  - The telnet server then transmits this output across the network back to our local telnet client.
  - Our local client receives the output and displays it on our local terminal.
  - In conclusion: The command is executed on the server, but the output is routed back and displayed on the client's side.

Note: Telnet is a network protocol used to provide a two-way, interactive and text-based communication channel between two machines, allowing for virtual access to another computer.