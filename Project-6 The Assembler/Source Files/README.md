# The Assembler

## Getting Started

### Prerequisites

- Install [*.NET Core SDK*](https://docs.microsoft.com/en-us/dotnet/core/install).

### Building

Switch to the project directory and execute the command:

```console
dotnet publish -c Release
```

## Usage

To translate a `.asm` file, you must specify its name.

```console
Assembler input-file
```

For example:

```console
Assembler Add.asm
```

It will generate the `Add.hack` file.