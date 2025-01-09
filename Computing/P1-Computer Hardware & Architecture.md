- [Foundation Path 1: Computer Hardware \& Architecture](#foundation-path-1-computer-hardware--architecture)
  - [Digital Logic Fundamentals](#digital-logic-fundamentals)
    - [Evolution of Computing Components](#evolution-of-computing-components)
      - [Resources](#resources)
    - [Modern Semiconductor Evolution](#modern-semiconductor-evolution)
      - [Current Manufacturing Process](#current-manufacturing-process)
      - [Current Challenges \& Future](#current-challenges--future)
    - [Binary \& Boolean Algebra](#binary--boolean-algebra)
      - [Resources](#resources-1)
      - [Why Computers Use Binary](#why-computers-use-binary)
      - [Boolean Algebra \& Operations](#boolean-algebra--operations)


# Foundation Path 1: Computer Hardware & Architecture

## Digital Logic Fundamentals

### Evolution of Computing Components

#### Resources
- [Early Computing: Crash Course Computer Science #1](https://youtu.be/O5nskjZ_GoI?si=hiBF9Q2kLwn-iArr)
- [Relay vs Vacuum Tube vs Transistor](https://images.app.goo.gl/9sBsLCKZoj3zP5H86)

- **Relays (1930s-40s)**
  - Electromechanical switches that used electromagnets to physically move metal contacts, making or breaking electrical connections
  - Think of them like electronic versions of light switches, but automated
  - While revolutionary for their time, they were slow (operating at milliseconds), bulky, and prone to mechanical failure
  - Historical example: Harvard Mark I computer showed both the potential and limitations of relay-based computing

- **Vacuum Tubes (1940s-50s)**
  - A major leap forward: controlled electron flow through a vacuum-sealed glass tube
  - Much faster than relays (operating at microseconds) and had no moving parts
  - However, they generated significant heat, consumed lots of power, and were fragile
  - The ENIAC computer demonstrated their potential and challenges: using about 18,000 vacuum tubes, it consumed 150 kilowatts of power - enough to power a small neighborhood!

- **Transistors (1950s-present)**
  - The revolution that enabled modern computing
  - Made from semiconductor materials like silicon
  - Perform the same switching functions as vacuum tubes but with massive improvements:
    - Incredibly small (modern transistors are nanometers in size - thousands could fit across a human hair)
    - Highly reliable (no vacuum seal to break)
    - Energy efficient (generating far less heat)
    - Lightning fast (operating at nanoseconds or faster)
    - Cheap to mass produce
  - Historical importance: First developed at Bell Labs in 1947
  - Modern impact: Today's processors contain billions of transistors on a single chip, enabling everything from smartphones to supercomputers

- **Integrated Circuits**
  - The next big leap: combining many transistors onto a single chip
  - Enabled complex processing in tiny spaces
  - Modern manufacturing pushes the boundaries of physics:
    - Feature sizes measured in nanometers
    - Complex layering and etching processes
    - Constant innovation in materials and design
  - Moore's Law (Intel co-founder Gordon Moore): is the observation that the number of transistors in an integrated circuit doubles every two years.

### Modern Semiconductor Evolution

#### Current Manufacturing Process
1. **Wafer Production** (Think of it like baking a high-tech cake)
   - Start with pure silicon crystal
   - Slice into ultra-thin wafers
   - Polish to atomic smoothness

2. **Circuit Creation** (Like painting with atomic precision)
   - Photolithography patterns circuits
   - Add/remove material layers
   - Create 3D structure of transistors

3. **Testing & Integration** (Quality control at nanoscale)
   - Test each circuit
   - Cut into individual chips
   - Package for protection and connection

#### Current Challenges & Future
1. **Physical Limits**
   - Approaching atomic scale barriers
   - Quantum effects becoming significant
   - Heat density increasing

2. **Economic Challenges**
   - Each new generation costs billions
   - Requires incredible precision
   - Few companies can afford development

3. **Future Directions**
   - 3D chip stacking
   - New materials (beyond silicon)
   - Quantum computing integration

### Binary & Boolean Algebra

#### Resources
- [Boolean Logic & Logic Gates: Crash Course Computer Science #3](https://youtu.be/gI-qXk7XojA?si=7oMEJqcCfaMBXf8f)
- [Representing Numbers and Letters with Binary: Crash Course Computer Science #4](https://youtu.be/1GSjbWt0c9M?si=76DMIlhWAblNlcnh)

#### Why Computers Use Binary
- **Binary vs Multi-State Systems**
 - Binary (2-state) won out over alternatives:
   - Ternary (3-state) computers: -1, 0, +1
     - More information per digit
     - Used in some Soviet computers (Setun)
     - Failed because: harder to maintain stable voltage levels, more complex circuits
   
   - Pentavalent (5-state) systems
     - Theoretical advantage: more data per digit
     - Never gained traction: voltage level distinction too unreliable
     - Hardware complexity increased exponentially
   
   - Modern quantum computers do use multiple states (qubits)
     - But they're specialized machines, not general-purpose computers
     - Require extreme conditions (near absolute zero temperature)

- **Why Binary Dominated**
 - Perfect match for physical switches:
   - ON/OFF
   - HIGH/LOW voltage
   - CHARGED/UNCHARGED
 - Much more reliable than multi-state systems:
   - Clear distinction between states
   - Better noise immunity
   - Simpler error detection
 - Simpler hardware implementation:
   - Transistors work naturally as binary switches
   - Circuits can be smaller and more efficient
   - Lower power consumption
   - Easier to maintain stable states

- **Binary Number System**
  - Uses only two digits: 0 and 1 (bits)
  - Each position in a binary number corresponds to a power of 2, starting from \(2^0\) at the rightmost position and increasing to the left:
    - Example: For the binary number 101:
      - Rightmost bit (1): \(2^0 = 1\)
      - Middle bit (0): \(2^1 = 2\) (value is 0 because the bit is 0)
      - Leftmost bit (1): \(2^2 = 4\)
      - Decimal equivalent: \(1 \times 2^2 + 0 \times 2^1 + 1 \times 2^0 = 4 + 0 + 1 = 5\).
  - Common groupings of binary digits:
    - **Nibble**: 4 bits (range: 0 to 15).
    - **Byte**: 8 bits (range: 0 to 255).
    - **Word**: Typically 16, 32, or 64 bits, depending on the system architecture.
  - Binary arithmetic is simpler than decimal:
    - Addition:
      - \(1 + 0 = 1\)
      - \(1 + 1 = 10\) (carry the 1).
    - Carries work like in decimal arithmetic, but instead of carrying over when the sum reaches 10, you carry over when the sum reaches 2.

#### Boolean Algebra & Operations
- **Fundamental Concepts**
 - Created by George Boole for mathematical logic
 - Perfect for computer science: deals with true/false values
 - Three basic operations form the foundation:
   - AND: true only if both inputs are true
   - OR: true if at least one input is true
   - NOT: inverts the input

- **Basic Operations in Detail**
 - **AND Operation (symbolized by · or ∧)**
   - Like multiplication for binary:
     - 0 AND 0 = 0
     - 0 AND 1 = 0
     - 1 AND 0 = 0
     - 1 AND 1 = 1
   - Real-world analogy: Both switches must be ON for light to work

 - **OR Operation (symbolized by + or ∨)**
   - Like addition but 1+1=1:
     - 0 OR 0 = 0
     - 0 OR 1 = 1
     - 1 OR 0 = 1
     - 1 OR 1 = 1
   - Real-world analogy: Either switch can be ON for light to work

 - **NOT Operation (symbolized by ¬ or ')**
   - Simple inversion:
     - NOT 0 = 1
     - NOT 1 = 0
   - Real-world analogy: Inverter that makes output opposite of input
- **Key Boolean Laws**
  - Help simplify complex expressions
  - Essential for circuit optimization
  - Common Laws:
    - **Commutative**: The order of operands doesn't matter
      - Boolean Logic: A·B = B·A and A+B = B+A
      - C++ Comparison:
        - `A && B == B && A` (AND)
        - `A || B == B || A` (OR)
    - **Associative**: Grouping doesn't affect the result
      - Boolean Logic: (A·B)·C = A·(B·C)
      - C++ Comparison:
        - `(A && B) && C == A && (B && C)` (AND)
        - `(A || B) || C == A || (B || C)` (OR)
    - **Distributive**: Multiplication distributes over addition
      - Boolean Logic: A·(B+C) = (A·B) + (A·C)
      - C++ Comparison:
        - `A && (B || C) == (A && B) || (A && C)`
    - **Identity**: Combining with 1 or 0 leaves the value unchanged, depending on the operation (AND or OR)
      - Explanation:
        - For AND (·):
          - Combining with 1 keeps the value unchanged because anything AND true is itself
          - Boolean Logic: A·1 = A
          - C++ Comparison: `A && true == A`
        - For OR (+):
          - Combining with 0 keeps the value unchanged because anything OR false is itself
          - Boolean Logic: A+0 = A
          - C++ Comparison: `A || false == A`
    - **Complement**: A value combined with its complement produces extreme outcomes, depending on the operation (AND or OR)
      - Explanation:
        - For AND (·):
          - Combining with a value's complement results in 0 because anything AND its opposite is always false
          - Boolean Logic: A·¬A = 0
          - C++ Comparison: `A && !A == false`
        - For OR (+):
          - Combining with a value's complement results in 1 because anything OR its opposite is always true
          - Boolean Logic: A+¬A = 1
          - C++ Comparison: `A || !A == true`
    - **Double Negation**: Negating a negation restores the original value
      - Boolean Logic: ¬(¬A) = A
      - C++ Comparison: `!!A == A`