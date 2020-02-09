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

To translate a `.vm` file, you must specify its name.

```powershell
VMTranslator input-file
```

For example:

```powershell
VMTranslator SimpleAdd.vm
```

It will generate the `SimpleAdd.asm` file.