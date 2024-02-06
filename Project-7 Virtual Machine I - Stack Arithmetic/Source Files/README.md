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