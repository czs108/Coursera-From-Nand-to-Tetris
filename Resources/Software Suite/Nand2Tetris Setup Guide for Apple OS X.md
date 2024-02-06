# *Nand2Tetris* Setup Guide for Apple OS X

This guide describes how to install and run the *Nand2Tetris* software suite on Apple OS X.

## Install Java

Older versions of Apple OS X come with Java pre-installed, so there may be no need to install Java on your Mac. Furthermore, there is no need to modify your Java `CLASSPATH`.

To find out if Java is already installed on your Mac, start the Terminal application ("*Applications*" - "*Utilities*" - "*Terminal*"). Once the terminal window appears, type the following:

```console
java -version
```

Depending on what happens next, you may or may not need to install Java.

- If you see an output on the console such as `java version "1.8.0_31"`, then Java is already installed. You can proceed to install the *Nand2Tetris* software suite.
- If a window appears, stating "*To use the 'java' command-line tool you need to install a JDK*", then click the "*More Info...*" button. A web page will open, and you should follow the instructions to [download and install Java](http://www.oracle.com/technetwork/java/javase/downloads/index.html).

After installing Java, verify the installation by starting a new Terminal window and typing `java -version`. You should see something like "*java version '1.8.0_31'*".

## Install the Nand2Tetris Software Suite

Double-click the `.zip` file you've downloaded from the [Nand2Tetris Software page](http://nand2tetris.org/software.php). OS X will automatically extract the contents of the `.zip` file to a folder. Move this folder to your desktop.

To run any one of the *Nand2Tetris* tools on OS X, you must use the command line. Start the Terminal application. Once the terminal window appears, type the following:

```console
~/Desktop/nand2tetris/tools/HardwareSimulator.sh
```

(The first character, called a "*tilde*" is located to the left of the <kbd>1</kbd> on most keyboards.) At this point, the supplied *Hardware Simulator* should started running in a new window.

From now on, when you wish to run the supplied *Hardware Simulator*, simply execute the command shown above.

All the supplied *Nand2Tetris* software tools are started in a similar way: just replace `HardwareSimulator` with the name of the software tool you wish to run.

| Tool                 | Command                |
| -------------------- | ---------------------- |
| *Hardware Simulator* | `HardwareSimulator.sh` |
| *CPU Emulator*       | `CPUEmulator.sh`       |
| *Assembler*          | `Assembler.sh`         |
| *VM Emulator*        | `VMEmulator.sh`        |
| *Jack Compiler*      | `JackCompiler.sh`      |

To run any of these tools, open a Terminal window and type the following, replacing `COMMAND` with one of the commands listed above.

```console
~/Desktop/nand2tetris/tools/COMMAND
```

> ### That's a lot to type. Can I shorten this?

Indeed you can. Open a Terminal window and type (once and for all):

```bash
echo "export PATH=$PATH:~/Desktop/nand2tetris/tools" >> ~/.bash_profile ; source ~/.bash_profile
```

You can now run any of the supplied *Nand2Tetris* software tools by typing just the command. For example:

```console
HardwareSimulator.sh
```

> ### I'm Working on *Project 9*. How Do I Compile My `.jack` Files?

Unlike the simulators, which feature an interactive user interface, the *Jack Compiler* is a terminal-oriented application. In order to run it, you must supply the name of the file or folder that you wish to compile. For example, suppose you wish to compile all `.jack` files stored in the folder `projects/09/Square` (that's a folder called `Square`, located in the `09` folder, which is located in the `projects` folder). To do so, open a Terminal window and type:

```console
JackCompiler.sh ~/Desktop/nand2tetris/projects/09/Square
```

This results in either a successful compilation of the `.jack` files in `Square`, or some compilation errors.

> ### Why Am I Seeing a "*Command Not Found*" Message?

The likely reason is that your folder location is not on the Mac OS X desktop, which is assumed by the instructions above. Replace `Desktop` above with the correct path to your `nand2tetris/tools` folder, or move your `nand2tetris` folder to your desktop.