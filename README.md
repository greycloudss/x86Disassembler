# Disassembler

## Overview

The disassembler is a lightweight, C#-based disassembly application designed to facilitate the analysis and decoding of binary executable files. Built with modularity and extensibility in mind, this project demonstrates key concepts in x86 disassembly with a focus on clarity and performance.

## Features

- **Binary Parsing**: Analyze and parse binary executable files to extract meaningful instructions.
- **Instruction Decoding**: Supports decoding x86 16-bit instructions, including MOD-REG-R/M bytes.
- **Dynamic Operand Resolution**: Accurately identifies registers, memory addresses, and immediate values.
- **Error Handling**: Robust error handling for incomplete or malformed binary files.
- **User-Friendly Output**: Presents decoded instructions in an easily readable format.
- **Flexible Architecture**: Modular design for easy integration and future extension.
- **Debugging Information**: Outputs detailed information for each instruction, including MOD, REG, and R/M values.

## Getting Started

### Prerequisites

- **.NET 6.0 or newer**: Ensure your environment supports the latest C# features.
- **Visual Studio or VS Code**: Recommended for building and debugging the project.
- **Binary File**: A `.com` or `.exe` file to analyze.

## Usage

1. Compile and launch the application using your preferred IDE or command line.
2. Provide the path to the binary file as an argument when running the program.
3. The application will:
   - Parse the binary file.
   - Decode each instruction.
   - Output the disassembled instructions along with additional details like MOD, REG, and R/M values.

## Architecture

The disassembler employs a modular architecture to streamline the process of binary analysis and disassembly:

- **Binary Reader**:
  - Reads the binary file and stores its content for processing.
  - Ensures the integrity of the binary data before decoding.

- **Instruction Decoder**:
  - Parses x86 16-bit instructions, including MOD-REG-R/M bytes.
  - Decodes operands, including registers, memory addresses, and immediate values.
  - Handles errors gracefully for incomplete instructions or unsupported formats.

- **Control Unit**:
  - Manages the flow between binary reading, instruction decoding, and output generation.
  - Implements logic for addressing modes, operand resolution, and instruction categorization.

- **Output Formatter**:
  - Formats and outputs disassembled instructions in a clear, readable format.
  - Includes optional debugging details such as MOD, REG, and R/M values.

## Contribution

Contributions are welcome! If you’d like to report a bug or request a feature, feel free to open an issue or submit a pull request.
