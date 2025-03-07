# Computer Systems: A Programmer's Perspective by Randal E. Bryant & David R. O'Hallaron

full course: https://www.cs.cmu.edu/afs/cs/academic/class/15213-f17/www/schedule.html

# 1 - A Tour of Computer Systems

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

# 1.9 Important Themes
Concluding chapter and discussions:
- Takeaway: A system is more than just hardware.
  - It is the collection of intertwined hardware and software that must cooperate to achieve the ultimate goal of running application programs.

To finish up, we will go over a few important aspects of computer systems:

## Concurrency and Parallelism
- Throughout the history of digital computers, two demands have driven improvement:
  1. We want to do MORE.
  2. We want to do it FASTER.
  - Both of these improve when the processor does more!
- Concurrency: A system that is performing multiple simultaneous activities.
- Parallelism: Using concurrency to make a system run faster.

## Parallelism Abstractions

### Thread-Level Concurrency
- Concurrency allows for multiple program executions at the same time.
- A Thread supports multiple executions within a single program process.
- Previously, this process was only simulated by having the computer switch between various processes (context-switching).
  - This allowed multiple users to interact with the system simultaneously.
  - For example, when users are getting pages from a Web server.
  - Or one user doing multiple things, like having multiple browser tabs open.
- Until most recently, most computing processes were done by a single processor that had to switch between multiple tasks.
  - This configuration is known as a 'uniprocessor system'.
- In contrast, when we have multiple processors under the control of a single OS kernel, we call it a 'multiprocessor system'.
  - Such systems have been available since the 1980s but have become more common with the advent of multi-core processors and 'hyperthreading'.
- Multi-core processors have several CPUs (referred to as 'cores') integrated onto a single integrated-circuit chip.
- The industry expects we will have hundreds of cores on a single chip in the future.
- Hyperthreading (also known as simultaneous multithreading) is a technique that allows a CPU to execute multiple flows of control. It involves having multiple copies of some of the CPU hardware, such as program counters and register files, while having only single copies of other parts of the hardware, such as units that perform floating-point arithmetic.
- A conventional processor requires around 20,000 clock cycles to switch between threads.
- A hyperthreaded processor decides which of its threads to execute on a cycle-by-cycle basis.
- Multiprocessing improves the system in 2 ways:
  1. Reduces the need to simulate concurrency when performing multiple tasks.
  2. Can run a single application program faster, but only if the program is expressed in terms of multiple threads that can effectively execute in parallel.
- We need to find ways to write programs that can exploit the thread-level parallelism available with the hardware.

## Instruction-Level Parallelism
- At a much lower level, modern processors can execute multiple instructions at one time, known as 'instruction-level' parallelism.
- For example, early microprocessors required multiple (3-10) clock cycles to execute a single instruction.
- More recent processors can execute 2-4 instructions per clock cycle.
- Later, we'll look at pipelining, where the actions required to execute an instruction are partitioned into different steps, and the processor hardware is organized as a series of stages, each performing one of these steps. The stages can operate in parallel, working on different parts of different instructions.
- Processors that can sustain execution rates faster than one instruction cycle are known as 'superscalar' processors. Most modern processors support superscalar operation. Later, we will look at how programmers can use their understanding of this to generate code that achieves higher degrees of instruction-level parallelism that run faster.

## Single-Instruction, Multiple-Data (SIMD) Parallelism
- At the lowest level, modern processors have special hardware that allows a single instruction to cause multiple operations to be performed in parallel, this mode is known as 'single-instruction, multiple-data' or 'SIMD' parallelism.
  - For example, a processor that can add 4 pairs of floating-point numbers in parallel.
- SIMD instructions are provided mostly to speed up applications that process image, sound, and video data.
- Although some compilers attempt to automatically extract SIMD parallelism in C programs, a more reliable method is using special 'vector' data types supported in compilers such as GCC. This style of programming is described as 'Web Aside opt:simd'.

## 1.9.2 The Importance of Abstractions in Computer Systems
- The use of abstractions is one of the most important concepts in computer science.
  - An example is APIs, allowing programmers to use code without having to dig deep into the nitty-gritty.
- On the processor side, the 'instruction set architecture' provides an abstraction of the actual processor hardware. It seems like only one instruction is being performed at a time, when in reality what's happening under the hood is much more elaborate with multiple instructions executing in parallel.
- On the operating system side, we talked about 4 abstractions:
  1. File: an abstraction of I/O.
  2. Virtual Memory: an abstraction of program memory.
  3. Processes: an abstraction of a running program.
  4. Virtual Machine: an abstraction of an entire computer system. VMs were introduced by IBM in the 1960s.

# 1.10
- Computer systems combine hardware and software to run applications.
- Information is represented as bits, interpreted differently based on context.
- Programs start as ASCII text and are translated into binary executables by compilers and linkers.
- Processors execute binary instructions from main memory, often moving data between memory, I/O devices, and CPU registers.
- Storage devices are arranged hierarchically: CPU registers, cache memories, DRAM, and disk storage, with higher levels being faster and costlier.
- Higher-level storage caches for lower levels; understanding this hierarchy helps optimize C program performance.
- The operating system kernel mediates between applications and hardware, providing key abstractions:
  1. Files for I/O devices.
  2. Virtual memory for main memory and disks.
  3. Processes for the processor, memory, and I/O devices.
- Networks enable system communication, acting as another I/O device.

# 2 - Representing and Manipulating Information

- Modern computers store and process information represented as 2-valued signals, aka 'bits'.
- Bits on their own don't mean much.
  - However, when they're grouped, we can apply some 'interpretation' to give them useful meanings.
- Representations of numbers:
  - Unsigned encodings: are based on traditional binary notation and represent numbers greater or equal to 0.
  - Two's-complement encodings: Represent 'signed' integers - numbers that may either be positive or negative.
  - Floating-point encodings: are Base-two version of scientific notation for representing real numbers.
- Computer representations use a limited number of bits to encode a number, and hence some operations can 'overflow' when the results are too large to be represented.
- Decimal representation, developed over 1000 years ago, is natural for humans but binary values are more efficient for machines.
- Binary signals can be easily stored and transmitted, enabling the creation of complex digital systems on silicon chips.
- Groups of bits can encode elements of any finite set, allowing for the representation of numbers, characters, and more.
- Important number representations in computing:
  - Unsigned for nonnegative numbers.
  - Two’s-complement for signed integers.
  - Floating-point for real numbers.
- Arithmetic operations in computers mimic those on integers and real numbers but are subject to overflow and precision limitations.
- Understanding the ranges of values and arithmetic operations properties is crucial for correct program operation and security.
- Numeric representations and operations in C are consistent with those in C++, whereas Java defines its own standards for numeric data.

# 2.1 Information Storage
- Computers operate on 8-bit blocks, known as bytes, rather than individual bits.
  - A byte is the smallest addressable memory unit.
- Each memory byte has a unique address.
- The 'virtual address space' encompasses all possible addresses.
  - It's a conceptual model for machine-level programs, implemented using RAM, disk storage, special hardware, and OS software to simulate a unified byte array.
- The compiler and runtime system manage memory, organizing it for data, instructions, and control information within this virtual space.
- In C, pointer values are virtual addresses pointing to storage blocks.
- The C compiler assigns type information to pointers, affecting the machine-level code it generates for data access.
- At the machine level, programs perceive objects and their sequences merely as byte blocks, disregarding data types.

# Aside
- Pointers are crucial in C, allowing for direct data structure element reference.
- A pointer includes a value (the object's location) and a type (the kind of object at that location, e.g., integer or floating-point number).

## 2.1.1 Hexadecimal Notation
- A byte consists of 8 bits, which can be represented in binary (base-2) or decimal (base-10) formats.
  - However, these formats are not always convenient for depicting bit patterns.
- Hexadecimal (base-16) notation is a more effective method for representing bit patterns.
  - It employs digits 0-9 and letters A-F to represent values from 0 to 15.
  - Each hexadecimal digit corresponds to a 4-bit binary sequence.

### Hexadecimal notation
Hex digit | Decimal value | Binary value
----------------------------------------
    0     |       0       |    0000
    1     |       1       |    0001
    2     |       2       |    0010
    3     |       3       |    0011
    4     |       4       |    0100
    5     |       5       |    0101
    6     |       6       |    0110
    7     |       7       |    0111
    8     |       8       |    1000
    9     |       9       |    1001
    A     |      10       |    1010
    B     |      11       |    1011
    C     |      12       |    1100
    D     |      13       |    1101
    E     |      14       |    1110
    F     |      15       |    1111

#### Example Conversion:

Hexadecimal to Binary:
Hex: 1    7    3    A    4    C
Bin: 0001 0111 0011 1010 0100 1100

Binary to Hexadecimal:
Bin: 1111 0010 1011 0111 0011
Hex:    F    2    B    7    3

## 2.1.2 Words
- A computer's 'word size' is crucial as it defines the virtual address space limit.
- With a word size of 'w' bits, the virtual address range is 0 to 2^w - 1, equating to 2^w accessible bytes.
- At the time of writing, many personal computers use a 32-bit word size, capping the virtual address space at 4GB (4 billion bytes).
- A shift towards 64-bit word sizes is occurring, vastly expanding the virtual address space.
- In essence, word size determines the max memory accessible by a program, and as technology evolves, word sizes increase to support more complex applications.

## 2.1.3 Data Sizes
- Computers and compilers manipulate various data formats with different encodings and sizes.
- C data types like 'char', 'int', 'float', and pointers have sizes that vary depending on the system and compiler.
- Program portability requires awareness of these size differences.
  - For instance, using an 'int' to store a pointer is fine on a 32-bit machine but problematic on a 64-bit one.
- As technology shifts towards 64-bit architectures, programmers must adapt to avoid bugs related to data size assumptions.

## Sizes (in bytes) of C numeric data types. 
The number of bytes allocated varies with machine and compiler. This chart shows the values typical of 32-bit and 64-bit
machines.

C declaration    | 32-bit | 64-bit
-----------------------------------
char             |    1   |    1
short int        |    2   |    2
int              |    4   |    4
long int         |    4   |    8
long long int    |    8   |    8
char *           |    4   |    8
float            |    4   |    4
double           |    8   |    8

# 2.1.4 Addressing and Byte Ordering
- Multi-byte objects in memory are contiguous and addressed by the smallest byte address.
- Byte order is either little endian (least significant byte first) or big endian (most significant byte first).
  - Little endian: Intel-compatible machines.
  - Big endian: IBM, Sun Microsystems machines.
  - Some machines are bi-endian (support both orders).
- Byte order choice is arbitrary but should be consistent.
- Networking and machine-level program inspection often involve byte order considerations.
- C code example shows printing byte representations using casting.
- Different machines may show different byte orderings for the same data.
- Integer and floating-point data encode numbers differently, leading to different byte patterns.

# 2.1.5 Representing Strings
- Strings in C are arrays of characters ending with a null character (0).
- Characters use standard encodings, commonly ASCII.
- Example: "12345" is represented as 31 32 33 34 35 00 in ASCII (including the null terminator).
- ASCII representation of decimal digits is 0x3x for digit x.
- Text data, due to ASCII encoding, is more platform-independent than binary data, unaffected by system's byte ordering or word size.

# 2.1.6 Representing Code
- Code Compilation and Machine Code Representation:
  - C functions compile into machine-specific code, varying significantly across systems.
  - Example Function: `int sum(int x, int y) { return x + y; }`
    - Linux 32 Byte Representation: `55 89 e5 8b 45 0c 03 45 08 c9 c3`
    - Windows Byte Representation: `55 89 e5 8b 45 0c 03 45 08 5d c3`
    - Sun Byte Representation: `81 c3 e0 08 90 02 00 09`
    - Linux 64 Byte Representation: `55 48 89 e5 89 7d fc 89 75 f8 03 45 fc c9 c3`
  - These byte representations show that even identical processors under different operating systems generate different machine codes.

- Compatibility and Portability:
  - Machine instructions and their encodings are specific to each system type, leading to incompatibilities across different hardware and software environments.
  - Binary compatibility and portability of code are limited across different systems, highlighting the non-universal nature of compiled binary code.

- Machine Perspective of Programs:
  - From the machine's viewpoint, a program is merely a sequence of bytes with no inherent meaning or structure derived from the original source code.
  - Debugging aids may include auxiliary tables to link back to the source, but the core executable lacks this contextual information.


# 2.1.7 Introduction to Boolean Algebra
- Boolean Algebra Basics
  - Binary values (0 and 1) form the core of computer data encoding, manipulation.
  - Founded by George Boole in the mid-19th century, defining True and False as 1 and 0.

- Fundamental Operations
  - NOT (`~` or `¬`): Inverts the value. ¬P is true if P is false, and vice versa.
  - AND (`&` or `∧`): True if both operands are true. p & q is 1 if both p and q are 1.
  - OR (`|` or `∨`): True if at least one operand is true. p | q is 1 if either p or q is 1.
  - XOR aka Exclusive or (`^` or `⊕`): True if exactly one operand is true. p ^ q is 1 if p ≠ q.

- Claude Shannon's Contribution
  - Linked Boolean algebra with digital logic, crucial for designing digital systems.

- Operations on Bit Vectors
  - Extend basic Boolean operations to operate on sequences of bits (bit vectors).
  - Operations applied element-wise to bit vectors of fixed length (w).
  - For a 4-bit vector to be considered `true` in a strict binary sense, all four bits must be 1 ([1111]).
    - Any bit vector that does not meet this criterion (i.e., contains any 0s) is considered `false`.
  - Example: 
    - Given bit vectors:
      - `a = [0110]` (In binary)
      - `b = [1100]` (In binary)
    - Operations, results, and implications:
      - AND (`&`): Compares each bit position in `a` and `b`. If both are 1, result is 1; otherwise, 0.
        - `a & b = [0100]` -> Only the second position is true, which means the result mostly represents false conditions except for one true condition.
          - To make the result entirely true, change either `a` or `b` to `[1111]`.
      - OR (`|`): Compares each bit position. If at least one is 1, result is 1; if both are 0, result is 0.
        - `a | b = [1110]` -> Most positions are true; only the last is false. This means the result is largely true with a minor false condition.
          - To make the result entirely false, change both `a` and `b` to `[0000]`.
      - XOR (`^`): Compares each bit position. If the bits are different, result is 1; if the same, result is 0.
        - `a ^ b = [1010]` -> This result has an equal mix of true and false conditions, indicating differences between `a` and `b`.
          - To reverse the result, swap the bits in either `a` or `b` to get `[0101]` or `[1011]`, respectively.
      - NOT (`~`) on `b`: Flips each bit in `b`. If it's 1, it becomes 0; if it's 0, it becomes 1.
        - `~b = [0011]` -> Flipping `b`'s bits makes the result true in the positions where `b` was false, and vice versa.
          - To get the opposite result, apply `~` again to return to the original `b` = `[1100]`.
      - `True`: If we're considering the entire 4-bit vector as a single truth value, then for it to be considered "true" in an all-or-nothing sense, all four bits must be 1 ([1111]).
      - `False`: Conversely, if even one bit of the 4-bit vector is 0, then the entire vector would not be considered wholly true in this strict interpretation. It would have at least one "false" condition, making the overall result less than entirely true ([1110], [1101], etc., would not be considered completely true).

- Boolean Algebra and Rings
  - Boolean operations form a structured algebra with properties like arithmetic.
  - Distribution: a & (b | c) = (a & b) | (a & c); also, a | (b & c) = (a | b) & (a | c).
  - Boolean rings: Each element is its own additive inverse (a ^ a = 0).
  
- Encoding Sets with Bit Vectors
  - Encode sets as bit vectors, where each bit represents the presence (1) or absence (0) of an element.
  - Operations | and & correspond to set union and intersection, respectively.
  - Example:
    - Consider the bit vector [01101001].
      - This means:
        - Slot 0 is filled (1 at the rightmost position).
        - Slot 1 is empty (0 next to it).
        - Slot 2 is filled.
        - Slot 3 is filled.
        - Slot 4 is empty.
        - Slot 5 is filled.
        - Slot 6 is empty.
        - Slot 7 is filled (1 at the leftmost position).
      - It encodes the set {0, 2, 3, 5, 7} (if we count slots from right to left, starting at 0).
  - Practical application: Using bit-vector masks to enable/disable signals in programs.

# 2.1.8 Bit-Level Operations in C
- C supports bit-wise Boolean operations:
  - `|` for Or
  - `&` for And
  - `~` for Not
  - `^` for Exclusive-Or
- Types include `char` or `int`, with qualifiers like `short`, `long`, `long long`, or `unsigned`.
- Convert hexadecimal arguments to binary, perform operations, then convert back to hexadecimal.

# 2.1.9 Logical Operations in C
- C includes logical operators `||`, `&&`, and `!`, corresponding to logical OR, AND, and NOT.
- Logical operators interpret any nonzero argument as `True` and `0` as `False`.
- Returns either `1` (True) or `0` (False).

### Examples
- `!0x41` results in `0x00` (False).
- `!0x00` results in `0x01` (True).
- `!!0x41` results in `0x01` (True).
- `0x69 && 0x55` results in `0x01` (True).
- `0x69 || 0x55` results in `0x01` (True).

### Important Distinctions
- Bit-wise (`&`, `|`) and logical (`&&`, `||`) operations yield similar outcomes when using `0` or `1` as arguments.
- Logical operators evaluate the second argument only if necessary. This prevents errors like division by zero (`a && 5/a`) or null pointer dereferencing (`p && *p++`).

# 2.1.10 Shift Operations in C
- C provides shift operations to shift bit patterns left (`<<`) or right (`>>`).
- Left shift `x << k`: Shifts `x` by `k` bits to the left, dropping the `k` most significant bits and filling the right end with `k` zeros.
  - Example: `[01100011] << 4` results in `[00110000]`.
- Right shift `x >> k` has two forms:
  - Logical Right Shift: Fills the left end with `k` zeros.
    - Example: `[01100011] >> 4` (logical) results in `[00000110]`.
  - Arithmetic Right Shift: Fills the left end with `k` repetitions of the most significant bit, useful for signed integer data.
    - Example: `[10010101] >> 4` (arithmetic) results in `[11111001]`.
- C standards are not precise about the right shift type for signed data:
  - Unsigned data must use logical shifts.
  - Signed data may use either, leading to portability issues.
- Most compilers/machines use arithmetic right shifts for signed data.
- Java defines right shifts precisely: `x >> k` (arithmetic), `x >>> k` (logical).

## Special Cases and Considerations
- Shifting by `k >= w` (where `w` is bit width):
  - C does not specify behavior; often results in `k mod w`.
  - Java specifies shifts should be modular.
- Operator precedence:
  - Addition/subtraction have higher precedence than shifts in C.
  - Common source of errors; use parentheses to clarify intentions.

# Practical Example
- Multiplying by a Power of 2:
- To multiply an integer by 2, you can simply left shift (<<) the number by 1. This operation moves all the bits to the left, effectively doubling the number.
  ```c
  - int num = 4; // 4 in binary: 0100
  - int result = num << 1; // Shift left by 1: 1000 in binary, which is 8 in decimal
  ```
- Dividing by a Power of 2:
- To divide an integer by 2, you can right shift (>>) the number by 1. This operation moves all the bits to the right, halving the number.
  ```c
  - int num = 4; // 4 in binary: 0100
  - int result = num >> 1; // Shift right by 1: 0010 in binary, which is 2 in decimal
  ```

# 2.2 Integer Representations

- Bits encode integers in two ways:
  1. Represent nonnegative numbers.
  2. Represent negative, zero, and positive numbers.
- Discusses resizing encoded integers for different bit lengths.

# 2.2.1 Integral Data Types in C

- C supports a variety of integral data types that represent a finite range of integers.
- Each type can specify a size with keywords `char`, `short`, `long`, or `long long`, and can be `unsigned` (all nonnegative numbers) or signed (possibly negative, which is the default).

- The number of bytes allocated for different sizes varies by the machine’s word size and compiler. This affects the range of values that can be represented.
- For example, most 64-bit machines use an 8-byte representation for `long`, offering a much wider range of values than the 4-byte representation used on 32-bit machines.
- The ranges of represented numbers are not symmetric: the range of negative numbers extends one more than positive numbers, which we'll understand better when considering how negative numbers are represented.
- The C standards define minimum ranges of values that each data type must be able to represent. These ranges are symmetric for positive and negative representations, a characteristic not always matched by typical implementations.
- The introduction of `long long` with ISO C99 requires at least an 8-byte representation, reflecting the trend towards supporting wider integral types for enhanced computational capabilities.

## Guaranteed ranges for C integral data types

| C data type             | Minimum                         | Maximum                         |
|-------------------------|---------------------------------|---------------------------------|
| char                    | -127                            | 127                             |
| unsigned char           | 0                               | 255                             |
| short [int]             | -32,767                         | 32,767                          |
| unsigned short [int]    | 0                               | 65,535                          |
| int                     | -32,767                         | 32,767                          |
| unsigned [int]          | 0                               | 65,535                          |
| long [int]              | -2,147,483,647                  | 2,147,483,647                   |
| unsigned long [int]     | 0                               | 4,294,967,295                   |
| long long [int]         | -9,223,372,036,854,775,807      | 9,223,372,036,854,775,807       |
| unsigned long long [int]| 0                               | 18,446,744,073,709,551,615      |

# 2.2.2 Unsigned Encodings

- Unsigned integers use a binary representation where each bit contributes a value based on its position: `value = Σ(bit * 2^position)`.

## Representation Examples
- `B2U4([0001])` equals `1` (`0*2^3 + 0*2^2 + 0*2^1 + 1*2^0`).
- `B2U4([0101])` equals `5` (`0*2^3 + 1*2^2 + 0*2^1 + 1*2^0`).
- `B2U4([1011])` equals `11` (`1*2^3 + 0*2^2 + 1*2^1 + 1*2^0`).
- `B2U4([1111])` equals `15` (`1*2^3 + 1*2^2 + 1*2^1 + 1*2^0`).

- The range of values for a w-bit unsigned integer is from 0 to `2^w - 1`.
- Every number in the range 0 to `2^w - 1` has a unique w-bit representation, making the mapping between bit vectors and integers a bijection.

# 2.2.3 Two’s-Complement Encodings

- Two’s-complement encoding is used for signed integers, interpreting the most significant bit as negative.
- Examples with a 4-bit two’s-complement integer:
  - `[0001]` represents 1.
  - `[0101]` represents 5.
  - `[1011]` represents -5.
  - `[1111]` represents -1.
- The least and greatest values for a w-bit two’s-complement number are `-2^(w-1)` and `2^(w-1) - 1`, respectively.
- This encoding scheme ensures every number within its range has a unique representation and supports bijection.
- Notable aspects:
  - The range of two’s-complement is asymmetric: the magnitude of the minimum negative value is one greater than the maximum positive value.
  - The maximum unsigned value is just over twice the maximum two’s-complement value.
  - Numeric value `-1` in two’s-complement has the same bit pattern as the maximum unsigned value (all bits set to 1).
  - Zero is represented by a string of all zeros in both signed and unsigned representations.
- C standards do not mandate two’s-complement for signed integers, but it's widely used. Ranges and representations can vary, so use `<limits.h>` for portability.
- Java specifies two’s-complement representation with exact ranges for its integer types, aiming for consistent behavior across platforms.

## Two's-Complement Representation Examples
- `B2T4([0001])` equals `1` (`-0*2^3 + 0*2^2 + 0*2^1 + 1*2^0 = 0 + 0 + 0 + 1`).
- `B2T4([0101])` equals `5` (`-0*2^3 + 1*2^2 + 0*2^1 + 1*2^0 = 0 + 4 + 0 + 1`).
- `B2T4([1011])` equals `-5` (`-1*2^3 + 0*2^2 + 1*2^1 + 1*2^0 = -8 + 0 + 2 + 1`).
- `B2T4([1111])` equals `-1` (`-1*2^3 + 1*2^2 + 1*2^1 + 1*2^0 = -8 + 4 + 2 + 1`).

## Important Numeric Values and Their Hexadecimal Representations

| Value  | 8-bit  | 16-bit       | 32-bit           | 64-bit                        |
|--------|--------|--------------|------------------|-------------------------------|
| UMaxw  | 0xFF   | 0xFFFF       | 0xFFFFFFFF       | 0xFFFFFFFFFFFFFFFF            |
|        | 255    | 65,535       | 4,294,967,295    | 18,446,744,073,709,551,615    |
| TMinw  | 0x80   | 0x8000       | 0x80000000       | 0x8000000000000000            |
|        | -128   | -32,768      | -2,147,483,648   | -9,223,372,036,854,775,808    |
| TMaxw  | 0x7F   | 0x7FFF       | 0x7FFFFFFF       | 0x7FFFFFFFFFFFFFFF            |
|        | 127    | 32,767       | 2,147,483,647    | 9,223,372,036,854,775,807     |
| -1     | 0xFF   | 0xFFFF       | 0xFFFFFFFF       | 0xFFFFFFFFFFFFFFFF            |
| 0      | 0x00   | 0x0000       | 0x00000000       | 0x0000000000000000            |

- `UMaxw` represents the maximum unsigned value for a given word size.
- `TMinw` represents the minimum two's-complement value (most negative) for a given word size.
- `TMaxw` represents the maximum two's-complement value for a given word size.
- `-1` is represented by all bits set to 1 across different word sizes.
- `0` is represented by all bits set to 0 across different word sizes.

# 2.2.4 Conversions Between Signed and Unsigned

## Casting Between Signed and Unsigned
- Casting from signed to unsigned (or vice versa) in C keeps the bit pattern but changes its interpretation.
- For example:
  - A 16-bit signed `short int` with a value of `-12345` has the same bit pattern as an unsigned `short int` of `53191`.
  - A 32-bit unsigned `int` of `4294967295` (`UMax_32`) has the same bit pattern as a signed `int` of `-1`.
- The conversion functions U2T and T2U describe the casting effects in C, based on the bit-level perspective.
- In C, casting does not change the bit pattern, only how the bit pattern is interpreted:
  - Negative signed values may become large unsigned values after casting.
  - Large unsigned values may become negative when cast to signed types.
- For values in the range 0 to 2^(w-1)-1, signed and unsigned representations are identical.
- For negative signed values or unsigned values greater than 2^(w-1), casting adds or subtracts 2^w:
  - For a negative signed value x, `T2Uw(x) = x + 2^w`.
  - For an unsigned value u that is >= 2^(w-1), `U2Tw(u) = u - 2^w`.
- Conversion examples:
  - Casting the most negative two's-complement number `TMinw` to unsigned gives `TMaxw + 1`.
  - Casting -1 to unsigned gives `UMaxw`.

### Understanding without Math Complexity
- Casting is like changing the label on a box without changing the contents.
- Imagine a box labeled "-1" and another labeled "4294967295". They actually contain the same thing, but the labels tell us how to interpret what's inside.
- When you're coding, remember that changing a variable's type doesn't change its underlying data, just how the computer uses it.
- This can cause unexpected behaviors, like making a negative number seem very large, or vice versa.
- To avoid issues, be mindful of these conversions, especially when dealing with operations that mix signed and unsigned types.

# 2.2.5 Signed vs. Unsigned in C

- C supports both signed and unsigned integers; by default, integers are signed.
- Unsigned constants are created with a 'U' or 'u' suffix, like `12345U`.
- Casting between signed and unsigned types does not change the bit pattern.
- On a two’s-complement machine, converting unsigned to signed applies `U2Tw`, and converting signed to unsigned applies `T2Uw`.
- Conversions can be explicit (with casting) or implicit (assignment without casting).
- `printf` uses `%d` for signed, `%u` for unsigned, and `%x` for hexadecimal output, regardless of the actual type.
- When combining signed and unsigned in operations, C casts the signed value to unsigned.
- This can lead to unintuitive results, especially with relational operators like `<` and `>`.
- Example: In the expression `-1 < 0U`, `-1` is cast to unsigned, resulting in a comparison of `4294967295U < 0U`.

### Practical Insights for Software Engineers

- When declaring literals in your code, be mindful of whether you want them to be treated as signed or unsigned to avoid implicit conversions that can cause bugs.
- Always use the correct format specifier when printing values with `printf` to avoid confusion about the value being represented.
- Be cautious with mixed signed and unsigned expressions, particularly with conditional statements, as the results may not be what you expect.
- Remember, casting changes the interpretation, not the actual data. Keep track of what type your variables should be and convert them explicitly if necessary.
- Understanding the bit-level behavior of your data types is crucial, as it affects operations and conversions in your code.

# 2.2.6 Expanding the Bit Representation of a Number

- Expanding a number to a larger word size involves either 'zero extension' (for unsigned) or 'sign extension' (for signed numbers).
- Zero extension adds leading zeros for unsigned numbers.
- Sign extension duplicates the most significant bit (sign bit) for signed numbers.
- The process ensures the same numeric value in the larger data type.
- Example from C code:
  - Converting 16-bit `-12345` (signed) to 32-bit preserves the value as `-12345`.
  - Converting 16-bit `53191` (unsigned) to 32-bit preserves the value as `53191`.
- For a signed number, the 16-bit representation `0xCFC7` becomes `0xFFFFCFC7` after sign extension.
- For an unsigned number, the 16-bit representation `0xCFC7` becomes `0x0000CFC7` after zero extension.

### Understanding the Concept

- Think of zero extension as adding zeroes in front of a shorter phone number to fit into a format that requires more digits, without changing the actual number you're dialing.
- Sign extension is like copying the area code for a local phone number to make it fit an international format, ensuring it's recognized correctly in a broader system.
- In programming, when you need to ensure a negative number stays negative in a larger type, you copy its sign bit (the leftmost bit) to the new bits.
- Remember, zero extension does not affect the sign because it only adds zeroes, which don't change the value.
- Always keep in mind that the purpose of these extensions is to retain the same value when you change to a larger data type.

# 2.2.7 Truncating Numbers

- Truncation reduces the number of bits representing a number, potentially altering its value due to overflow.
- In C, casting from a larger integer type to a smaller one truncates the number:
  - `(short) x` converts a 32-bit `int x` to a 16-bit `short`.
  - When casting back to `int`, sign extension occurs.
- Truncation drops the high-order bits, changing the bit vector from [xw-1, ..., x0] to [xk-1, ..., x0], where k is the new size.
- For unsigned numbers, truncating a w-bit number to k bits is like calculating x modulo 2^k:
  - `B2Uk([xk-1, ..., x0]) = B2Uw([xw-1, ..., x0]) mod 2^k`.
- For two's-complement numbers, truncation applies the modulus operation and then converts the result to the corresponding signed value:
  - `B2Tk([xk-1, ..., x0]) = U2Tk(B2Uw([xw-1, ..., x0]) mod 2^k)`.

### Practical Takeaways for Software Engineers

- Be cautious when truncating as it can lead to overflow and unexpected values.
- Casting to a smaller type does not simply cut off the higher bits; it applies a modulus operation which keeps the lower bits' pattern.
- When expanding or truncating integers, remember:
  - Zero extension is used for unsigned integers (add zeros at the high-order end).
  - Sign extension is used for signed integers (replicate the most significant bit at the high-order end).
- When converting back to a larger type, C will fill in the higher order bits appropriately based on whether the number is signed (sign extension) or unsigned (zero extension).
- Always ensure the destination type has enough bits to represent the value to avoid unintended truncation.

# 2.2.8 Advice on Signed vs. Unsigned

## Implicit Casting Issues
- Implicit casting can lead to nonintuitive and buggy behavior.
- Since casting occurs without explicit code markers, its effects are often overlooked.

## Practice Problems
### Problem 2.25
- Code attempts to sum elements of an array.
- With `length` equal to 0, `length-1` underflows since `length` is unsigned, leading to a large number and a memory error.
- Fix by changing loop condition to `i < length`.

### Problem 2.26
- Function checks if one string is longer than another using `strlen`, which returns `size_t` (unsigned).
- Incorrect results when the difference is negative since it will convert to a large unsigned value.
- Fix by comparing the `strlen` results directly, without subtracting.

## Security Vulnerability Example
- Mixing signed and unsigned can lead to security issues.
- Example of `copy_from_kernel` where a negative `maxlen` leads to a large unsigned value passed to `memcpy`, potentially exposing kernel memory.
- Fix by ensuring consistency in data types for lengths and sizes across related functions.

## Summary and Best Practices
- Be consistent in the use of signed or unsigned types, especially when dealing with lengths and sizes.
- Avoid unsigned types unless you need to treat integers as bit collections or for modular/multiprecision arithmetic.
- Java and other languages avoid unsigned types, possibly due to these complexities.
- Always check operations on unsigned types that can lead to underflow (result unexpectedly large) or overflow (result wraps around from a large number to zero).

## Takeaway for Software Engineers
- Understand the implications of signed and unsigned arithmetic in C to write robust and secure code.
- Use unsigned types judiciously and be aware of their behavior in computations and comparisons to prevent bugs.
- Consider the consequences of type conversions, especially in critical code paths, such as security checks or memory operations.

# 2.3 Integer Arithmetic

## Key Concept
In computers, integer arithmetic isn't exactly like regular math because computers have limited space to store numbers. This leads to some surprising behaviors, like:
- Adding two positive numbers can give a negative result
- `x < y` might give a different result than `x-y < 0`
- These oddities come from the finite (limited) nature of computer arithmetic

## 2.3.1 Unsigned Addition

### Basic Concepts
- For unsigned numbers with w bits:
  - Each number x or y must be between 0 and 2^w - 1
  - Their sum (x + y) could need w+1 bits to represent fully
  - When the sum is too big for w bits, we get overflow

### How It Works
- Computer performs "modular arithmetic": sum is calculated as (x + y) mod 2^w
- Overflow occurs when sum ≥ 2^w
- You can detect overflow by checking if the sum is less than one of the original numbers
  - If `s = x + y` and `s < x`, then overflow occurred

### Real-World Example
```c
unsigned char a = 250;  // 8 bits max (0 to 255)
unsigned char b = 10;
unsigned char sum = a + b;  // Expected: 260, Actual: 4 (260 mod 256)
// Overflow occurred because 260 is too big for 8 bits
```

## 2.3.2 Two's-Complement Addition

### Basic Concepts
- For signed numbers with w bits:
  - Numbers range from -2^(w-1) to 2^(w-1) - 1
  - Their sum needs w+1 bits for full representation
  - Two types of overflow can occur:
    1. Positive overflow: when sum is too positive (sum > 2^(w-1) -1)
    2. Negative overflow: when sum is too negative (sum < -2^(w-1))

#### Overflow Detection

1. Adding two positives:
   - If result is negative → Positive overflow occurred
   - Example (4-bit): 5 + 4 = [0101 + 0100 = 1001 = -7]
   - We know this overflowed because 5 + 4 should be 9, not -7

2. Adding two negatives:
   - If result is positive → Negative overflow occurred
   - Example (4-bit): -5 + (-4) = [1011 + 1100 = 0111 = 7]
   - We know this overflowed because -5 + (-4) should be -9, not 7

3. Adding a positive and a negative:
   - Cannot overflow! The result will always be representable
   - This is because the true mathematical sum will always be between the two operands
   - Example (4-bit): 5 + (-4) = [0101 + 1100 = 0001 = 1]
   - Example (4-bit): -5 + 6 = [1011 + 0110 = 0001 = 1]

We can detect overflow by looking at the operands (x, y) and result (s):
```c
int x, y;         // operands
int s = x + y;    // sum
int overflow =    // overflow detection
    ((x > 0 && y > 0 && s <= 0) ||     // positive + positive
     (x < 0 && y < 0 && s >= 0));      // negative + negative
```
## 2.3.3 Two's-Complement Negation

### Basic Rules
- For most numbers: negation is achieved by finding the number's additive inverse
  - How to Negate a Number
    1. Flip all bits (1s become 0s, 0s become 1s)
    2. Add 1 to the result

 - 4-bit Examples:

    ```
    Negating 3:
    0011  (3)
    1100  (flip bits)
    1101  (add 1)
    = -3

    Negating -3:
    1101  (-3)
    0010  (flip bits)
    0011  (add 1)
    = 3
    ```

- Special case: most negative number (TMin) is its own negative
  - Example: in 4 bits, -8 is its own negative because +8 can't be represented
    ```
    Trying to negate -8:
    1000  (-8)
    0111  (flip bits)
    1000  (add 1)
    = -8  (back to same number!)
    ```

  - This happens because:
    - In 4 bits, numbers range from -8 to 7
    - When we try to get +8, it doesn't fit
    - So -8 ends up being its own negative

  - 6-bit Example:
    ```
    Regular case (negating 12):
    001100  (12)
    110011  (flip bits)
    110100  (add 1)
    = -12

    Special case (negating -32):
    100000  (-32)
    011111  (flip bits)
    100000  (add 1)
    = -32   (same number!)
    ```

## 2.3.4 and 2.3.5 Unsigned and Two's-Complement Multiplication

### Key Points
- Result might need up to 2w bits (double the original size)
- Computer truncates result to w bits (keeps only lower half)
- Both unsigned and signed multiplication give same bit-level results
- Special cases to watch out for:
  - Multiplying large numbers can cause overflow
  - Result might be correct in one interpretation (signed/unsigned) but wrong in another

## 2.3.6 Multiplying by Constants

### Optimization Techniques
- Multiplication is slower than addition and shifting
- Compilers optimize multiplication by constants using shifts and adds
- Example: x * 14 can be written as:
  - `(x << 3) + (x << 2) + (x << 1)`  // x * 8 + x * 4 + x * 2
  - Or better: `(x << 4) - (x << 1)`   // x * 16 - x * 2

## 2.3.7 Dividing by Powers of Two

### Methods
- Division by 2^k can be done with right shifts
- Unsigned numbers: use logical right shift
- Signed numbers: use arithmetic right shift
- For negative numbers: need to add bias before shifting to round correctly

### Example
```c
// Divide by 4 (2^2):
unsigned x = 12;
unsigned result = x >> 2;  // 12/4 = 3

int y = -12;
int result2 = (y + (1<<2)-1) >> 2;  // -12/4 = -3 (rounded toward zero)
```

## 2.3.8 Final Thoughts

### Important Points
- Computer arithmetic is really modular arithmetic due to limited word size
- Operations can overflow
- Two's-complement is clever: same hardware works for both signed and unsigned
- Unsigned numbers can cause unexpected behavior
- Be careful with:
  - Integer constants (default type)
  - Library functions (might use unsigned unexpectedly)
  - Overflow conditions

# 2.4 Floating Point

## Historical Context & Introduction
- Before 1985: Each computer manufacturer had their own floating-point formats
- 1985: IEEE Standard 754 became the universal standard
  - Originated from Intel's 8087 chip design for the 8086 processor
  - Designed with help from Berkeley professor William Kahan
  - Now used by virtually all computers, improving program portability

## 2.4.1 Fractional Binary Numbers

### Decimal vs Binary Fractions
- Decimal fractions example: 12.34₁₀
  - 1 × 10¹ + 2 × 10⁰ + 3 × 10⁻¹ + 4 × 10⁻² = 12.34

- Binary fractions example: 101.11₂
  - 1 × 2² + 0 × 2¹ + 1 × 2⁰ + 1 × 2⁻¹ + 1 × 2⁻² = 5.75₁₀

### Key Properties
- Binary point (like decimal point) separates whole and fractional parts
- Moving binary point left divides by 2 (like decimal point and 10)
- Moving binary point right multiplies by 2
- Some numbers cannot be represented exactly in binary
  - Example: 1/5 (0.2 in decimal) requires an infinite binary representation
  - Similar to how 1/3 needs infinite decimal places (0.333...)

## 2.4.2 IEEE Floating-Point Format

### Basic Structure
Numbers are represented as: V = (-1)ˢ × M × 2ᴱ
- s = sign bit (0 for positive, 1 for negative)
- M = significand (a fractional value between 1 and 2, or between 0 and 1)
- E = exponent (weights the value by a power of 2)

### Format Types
1. Single Precision (32 bits total)
   - 1 bit: Sign
   - 8 bits: Exponent
   - 23 bits: Fraction

2. Double Precision (64 bits total)
   - 1 bit: Sign
   - 11 bits: Exponent
   - 52 bits: Fraction

### Three Categories of Values in IEEE Floating-Point

1. Normalized Values
   * Most common case: exponent field between all 0s and all 1s
   * Format: (-1)ˢ × (1 + fraction) × 2^(exp - bias)
   * 8-bit simplified format (1 sign, 4 exponent, 3 fraction bits, bias=7): 
      ```
      Example Value: 1.5 (decimal)
      Binary: 0 1000 100

      Detailed Calculation:
      Sign bit (s) = 0 → (-1)⁰ = +1
      Exponent = 1000₂ = 8 → 8 - 7(bias) = 1
      Fraction = 100₂ = 0.5 (because 1/2)

      Final math:
      1.5 = 1 × (1 + 0.5) × 2¹
      = 1 × 1.5 × 2
      = 1.5 × 2
      = 3
      ```

2. Denormalized Values
   * Used when exponent field is all zeros (very small numbers)
   * Format: (-1)ˢ × (0 + fraction) × 2^(1 - bias)
      ```
      Example 1 - Zero:
      Binary: 0 0000 000

      Calculation:
      Sign = 0 → positive
      Exponent = 0 → denormalized
      Fraction = 0
      Result: +0.0

      Example 2 - Tiny Number:
      Binary: 0 0000 001

      Detailed Calculation:
      Sign bit (s) = 0 → (-1)⁰ = +1 (positive)
      Exponent = 0000 → denormalized → use 2^(-6) (1 - bias)
      Fraction = 001₂ = 1/8

      Final math:
      = 1 × (0 + 1/8) × 2^(-6)
      = 1/8 × 1/64
      = 1/512 (smallest positive number in this format)
      ```

3. Special Values
   * Exponent field all ones (11...1)
   * Examples:
    ```
    Example 1 - Positive Infinity:
    Binary: 0 1111 000

    Breakdown:
    - Sign = 0 → positive
    - Exponent = 1111 → special value
    - Fraction = 000 → infinity

    Example 2 - NaN (Not a Number):
    Binary: 0 1111 100

    Breakdown:
    - Sign = 0 (ignored for NaN)
    - Exponent = 1111 → special value
    - Fraction = 100 → non-zero means NaN

    Example 3 - Negative Infinity:
    Binary: 1 1111 000

    Breakdown:
    - Sign = 1 → negative
    - Exponent = 1111 → special value
    - Fraction = 000 → infinity
    ```

Practical Uses:
- Normalized: Regular numbers like 1.0, 2.5, -3.75
- Denormalized: Very small numbers close to zero, and zero itself
- Special Values: 
  - ∞: Results of division by zero or very large computations
  - NaN: Results of invalid operations like 0/0 or √-1

You're right - let me combine both the original conceptual notes with the detailed examples for a complete understanding:

## 2.4.3 Rounding

### Overview
Floating-point arithmetic can only approximate real arithmetic due to limited precision. When exact representation isn't possible, we need systematic methods to find the "closest" matching value that can be represented.

### Four Rounding Modes

1. Round-to-even (Default Mode)
- Also called round-to-nearest
- Finds the closest matching value
- For halfway cases, rounds to the number with an even least significant digit
- Purpose: Avoids statistical bias in large computations
- Most commonly used mode in practice

```
Examples with decimals:
1.4 → 1.0
1.6 → 2.0
1.5 → 2.0 (halfway case, 2 is even)
2.5 → 2.0 (halfway case, 2 is even)
3.5 → 4.0 (halfway case, 4 is even)
-1.5 → -2.0 (halfway case, -2 is even)

Binary Examples (rounding to integer):
1.1000₂ (1.5₁₀) → 10₂ (2₁₀) 
10.1000₂ (2.5₁₀) → 10₂ (2₁₀)
11.1000₂ (3.5₁₀) → 100₂ (4₁₀)
```

2. Round-toward-zero
- Rounds positive numbers downward and negative numbers upward
- Creates value ˆx where |ˆx| ≤ |x|
- Useful for certain numerical bounds

```
Basic Rule: Always round toward 0 (truncate)

Decimal Examples:
1.8 → 1.0
1.2 → 1.0
-1.8 → -1.0
-1.2 → -1.0

Binary Examples:
1.1100₂ (1.75₁₀) → 1₂ (1₁₀)
10.1100₂ (2.75₁₀) → 10₂ (2₁₀)
-10.1100₂ (-2.75₁₀) → -10₂ (-2₁₀)
```

3. Round-down (toward negative infinity)
- Rounds both positive and negative numbers downward
- Creates value x⁻ where x⁻ ≤ x
- Useful for establishing lower bounds

```
Basic Rule: Always round down

Decimal Examples:
1.1 → 1.0
1.8 → 1.0
-1.1 → -2.0
-1.8 → -2.0

Binary Examples:
1.1100₂ (1.75₁₀) → 1₂ (1₁₀)
-1.1100₂ (-1.75₁₀) → -10₂ (-2₁₀)
```

4. Round-up (toward positive infinity)
- Rounds both positive and negative numbers upward
- Creates value x⁺ where x ≤ x⁺
- Useful for establishing upper bounds

```
Basic Rule: Always round up

Decimal Examples:
1.1 → 2.0
1.8 → 2.0
-1.1 → -1.0
-1.8 → -1.0

Binary Examples:
1.1100₂ (1.75₁₀) → 10₂ (2₁₀)
-1.1100₂ (-1.75₁₀) → -1₂ (-1₁₀)
```

### Importance of Round-to-Even
- Rounds up ~50% of the time and down ~50% of the time

The default round-to-even mode helps avoid systematic bias in computations:

```
Consider large dataset rounding:
Regular rounding up at .5:
1.5 → 2
2.5 → 3
3.5 → 4
4.5 → 5
Average shifts upward

Round-to-even:
1.5 → 2
2.5 → 2
3.5 → 4
4.5 → 4
Average stays balanced
```

### Practical Applications

```
Banking Example (rounding to cents):
$1.235 rounded four ways:
- Round-to-even: $1.24 
- Round-toward-zero: $1.23
- Round-down: $1.23
- Round-up: $1.24

Binary Precision Example:
1.0101₂ (1.3125₁₀) to 3 fractional bits:
- Round-to-even: 1.010₂ (1.25₁₀)
- Round-toward-zero: 1.010₂ (1.25₁₀)
- Round-down: 1.010₂ (1.25₁₀)
- Round-up: 1.011₂ (1.375₁₀)
```

## 2.4.4 Operations

### Key Properties
- Not exactly like real number arithmetic
- Addition (+f) and multiplication (*f) have special rules
- Operations may produce:
  - Normal results
  - Infinity (from overflow)
  - NaN (from undefined operations)

### Important Differences from Real Arithmetic
1. Not associative
   - (a +f b) +f c might ≠ a +f (b +f c)
   - Example: (3.14 + 1e10) - 1e10 = 0.0, but 3.14 + (1e10 - 1e10) = 3.14

2. Not distributive
   - a *f (b +f c) might ≠ (a *f b) +f (a *f c)

3. Can have rounding errors
   - Results are approximations of exact mathematical results

## 2.4.5 Floating Point in C

### Types
- float: Single precision (32 bits)
- double: Double precision (64 bits)
- long double: Extended precision (implementation dependent)

### Type Conversion Rules
- int to float: May round, cannot overflow
- int/float to double: Exact preservation
- double to float: May overflow or round
- float/double to int: Rounds toward zero, may overflow

Based on the summary of Chapter 2, here are the key points organized for future reference:

# Chapter 2 Summary - Computer Data Representation & Arithmetic

## Data Storage & Encoding
- Computers store all data as bits (binary), organized in byte sequences
- Different encodings exist for:
  - Integers (signed/unsigned)
  - Real numbers (floating-point)
  - Character strings
- Byte ordering varies by machine (endianness)

## C Language Implementation
- Designed for flexibility across different machines
- Word sizes: 
  - Typically 32-bit
  - Trend toward 64-bit
- Common implementations use:
  - Two's-complement for integers
  - IEEE format for floating-point

## Integer Arithmetic Characteristics
- Finite length causes overflow
- Properties that hold true:
  - Associativity
  - Commutativity 
  - Distributivity
- Example optimization: `7*x` can be computed as `(x<<3)-x`

## Floating-Point (IEEE 754)
- Represents numbers as: x × 2^y
- Common formats:
  - Single precision (32 bits)
  - Double precision (64 bits)
- Special values:
  - ±Infinity
  - Not-a-Number (NaN)
- Limited precision and range
- Does not follow standard mathematical properties (e.g., associativity)

## Critical Programming Considerations
1. Type Casting
   - Between signed/unsigned: Bit pattern typically unchanged
   - Can lead to unexpected behavior if not careful

2. Bit-Level Operations
   - Useful patterns: `~x+1` equals `-x` in two's-complement
   - Masking: Generate k ones using `(1<<k)-1`

3. Arithmetic Safety
   - Watch for overflow in integer operations
   - Example: `x*x` can be negative due to overflow
   - Floating-point requires careful handling due to precision limits

## Security and Portability
- Word sizes and numeric encodings vary by machine
- C standards intentionally avoid specifying these details
- Careful coding needed to ensure:
  - Cross-platform compatibility
  - Security against overflow vulnerabilities
  - Proper handling of implicit type conversions