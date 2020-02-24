# The VM Translator

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

You can also translate a directory containing one or more `.vm` files.

```powershell
VMTranslator input-dir
```

For example:

```powershell
VMTranslator FibonacciSeries
```

It will generate the `FibonacciSeries.asm` file. This single file contains the assembly code resulting from translating all the `.vm` files in the `FibonacciSeries` directory.