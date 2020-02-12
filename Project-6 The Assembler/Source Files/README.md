# The Assembler

## Getting Started

### Prerequisites

- Install [*.NET Core SDK*](https://docs.microsoft.com/en-us/dotnet/core/install/).

### Building

Switch to the project directory and execute the command:

```powershell
dotnet publish -c Release
```

## Usage

To translate a `.asm` file, you must specify its name.

```powershell
Assembler input-file
```

For example:

```powershell
Assembler Add.asm
```

It will generate the `Add.hack` file.