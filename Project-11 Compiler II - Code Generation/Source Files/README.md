# The Compiler

## Getting Started

### Prerequisites

- Install [*.NET Core SDK*](https://docs.microsoft.com/en-us/dotnet/core/install/).

### Building

Switch to the project directory and execute the command:

```powershell
dotnet publish -c Release
```

## Usage

To compile a `.jack` file, you must specify its name.

```powershell
Compiler input-file
```

For example:

```powershell
Compiler Main.jack
```

It will generate the `Main.vm` file.

You can also analyze a directory containing one or more `.jack` files.

```powershell
Compiler input-dir
```

For example:

```powershell
Compiler Square
```

It will generate a `.vm` file for every `.jack` file, stored in the same directory.