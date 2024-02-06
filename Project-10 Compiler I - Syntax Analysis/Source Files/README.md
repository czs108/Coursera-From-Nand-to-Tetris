# The Analyzer

## Getting Started

### Prerequisites

- Install [*.NET Core SDK*](https://docs.microsoft.com/en-us/dotnet/core/install).

### Building

Switch to the project directory and execute the command:

```console
dotnet publish -c Release
```

## Usage

To analyze a `.jack` file, you must specify its name.

```console
Analyzer input-file
```

For example:

```console
Analyzer Main.jack
```

It will generate the `Main.xml` file.

You can also analyze a directory containing one or more `.jack` files.

```console
Analyzer input-dir
```

For example:

```console
Analyzer Square
```

It will generate a `.xml` file for every `.jack` file, stored in the same directory.