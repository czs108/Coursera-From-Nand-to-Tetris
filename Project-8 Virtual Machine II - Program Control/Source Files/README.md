# The VM Translator

## Getting Started

### Prerequisites

- Install [*.NET Core SDK*](https://docs.microsoft.com/en-us/dotnet/core/install).

### Building

Switch to the project directory and execute the command:

```console
dotnet publish -c Release
```

## Usage

To translate a `.vm` file, you must specify its name.

```console
VMTranslator input-file
```

For example:

```console
VMTranslator SimpleAdd.vm
```

It will generate the `SimpleAdd.asm` file.

You can also translate a directory containing one or more `.vm` files.

```console
VMTranslator input-dir
```

For example:

```console
VMTranslator FibonacciSeries
```

It will generate the `FibonacciSeries.asm` file. This single file contains the assembly code resulting from translating all the `.vm` files in the `FibonacciSeries` directory.