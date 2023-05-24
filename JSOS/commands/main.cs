using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Cosmos.System.FileSystem.Listing;
using g;
using tools;
using static commands.main;
using static Cosmos.HAL.BlockDevice.ATA_PIO;
using static tools.path;
using static tools.shell;
using Sys = Cosmos.System;

namespace commands {
	public class main {
		public class sysinfo : command {
			public sysinfo() {
				name = "sysinfo";
				description = "Lists hardware and software stats";
				argsReqs = new() { };
			}
			public override exitcode BeforeRun(List<string> args) {
				Console.WriteLine("                         Hardware:");
				Console.WriteLine("                            RAM:            " + Cosmos.Core.CPU.GetAmountOfRAM().ToString() + "MB");
				Console.WriteLine("                            CPU Brand:      " + Cosmos.Core.CPU.GetCPUBrandString());
				Console.WriteLine("                            CPU Vendor:     " + Cosmos.Core.CPU.GetCPUVendorName());
				Console.WriteLine("                            CPU Speed:      " + (Cosmos.Core.CPU.GetCPUCycleSpeed() / 100000000).ToString() + "Ghz");
				Console.WriteLine("                            CPU Uptime:     " + (Cosmos.Core.CPU.GetCPUUptime() / (ulong)Cosmos.Core.CPU.GetCPUCycleSpeed()).ToString() + "s");
				Console.WriteLine("                         Operating System:");
				Console.WriteLine("                            Name:           JSOS, Jimmy S. Operating System");
				Console.WriteLine("                            Version:        v0.1.1");
				Console.WriteLine("                            Build Date:     5/20/2023");
				return exitcode.HALT;
			}
		}
		public class help : command {
			tools.shell.argumentConditionPositional cmdName;
			public help() {
				name = "help";
				description = "Lists currently loaded commands";
				cmdName = new tools.shell.argumentConditionPositional("Command", 0, needed: false);
				argsReqs = new() { cmdName };
			}
			public override exitcode BeforeRun(List<string> args) {
				if (cmdName.wasFound) {
					command cmd = tools.shell.getCommand(cmdName.contents);
					if (cmd == null) {
						messages.errors.command.commandNotFound(cmdName.contents);
						return exitcode.HANDLEDERROR;
					}
					Console.WriteLine(cmd.description);
					if (cmd.argsReqs.Count == 0) {
						return exitcode.HALT;
					}
					List<string> argumentNames = new List<string>();
					foreach (argumentCondition arg in cmd.argsReqs) {
						string argumentName = arg.ToString();
						if (!arg.needed) {
							argumentNames.Add("[" + argumentName + "]");
						} else {
							argumentNames.Add(argumentName);
						}
					}
					Console.WriteLine(cmdName.contents + " " + tools.strings.join(" ", argumentNames));
					foreach (argumentCondition arg in cmd.argsReqs) {
						if (arg is argumentConditionFlag) {
							Console.WriteLine(arg.ToString() + ": " + arg.name);
						}
					}
					return exitcode.HALT;
				}
				Console.WriteLine("Currently loaded commands:");
				foreach (shell.command cmd in globals.loadedCommands) {
					Console.WriteLine("   " + (cmd.name + ":").PadRight(20) + cmd.description);
				}
				return exitcode.HALT;
			}
		}
		public class set : command {
			tools.shell.argumentConditionPositional varName;
			tools.shell.argumentConditionPositional newValue;
			public set() {
				name = "set";
				description = "Sets a local variable";
				varName = new tools.shell.argumentConditionPositional("Variable", 0, needed: true);
				newValue = new tools.shell.argumentConditionPositional("Value", 1, needed: true);
				argsReqs = new() { varName, newValue };
			}
			public override exitcode BeforeRun(List<string> args) {
				if (varName.contents.Contains('$')) { messages.errors.arguments.invalidSymbol(varName.contents, "$"); return exitcode.ERROR; }
				globals.Locals.set(varName.contents, newValue.contents);
				return exitcode.HALT;
			}
		}
		public class clear : command {
			public clear() {
				name = "clear";
				description = "Clears the screen";
				argsReqs = new() { };
			}
			public override exitcode BeforeRun(List<string> args) {
				Console.BackgroundColor = tools.console.GetColor(globals.Locals.get("SHELL_BCOLOR", "WHITE"));
				Console.Clear();
				return exitcode.HALT;
			}
		}
		public class credits : command {
			public credits() {
				name = "credits";
				description = "Gives credit to the amazing authors!";
				argsReqs = new() { };
			}
			public override exitcode BeforeRun(List<string> args) {
				Console.WriteLine("This project was made as a group broject between:");
				Console.WriteLine("Dion:");
				Console.WriteLine("   Github: evildion07");
				Console.WriteLine("   Discord: Musical Eevee#6926");
				Console.WriteLine("Jimmy: ");
				Console.WriteLine("   Github: code32123");
				Console.WriteLine("   Discord: Jimmy32#2036");
				return exitcode.HALT;
			}
		}
		public class stat : command {
			tools.shell.argumentConditionPositional fileArg;
			public stat() {
				name = "stat";
				description = "File/directory information";
				fileArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { fileArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = fileArg.contents;
				path = tools.path.Validate(path, finalSlash: false);
				bool fileExists = tools.path.FileExists(path);
				if (!fileExists) { messages.errors.file.fileNotFound(path); return exitcode.ERROR; }

				string FileContent = File.ReadAllText(path);

				Console.WriteLine("Name: " + path.Split(@"\").LastOrDefault());
				Console.WriteLine("Path: " + path);
				Console.WriteLine("Size: " + FileContent.Length.ToString());
				return exitcode.HALT;
			}
		}
		public class echo : command {
			tools.shell.argumentConditionPositional text;
			public echo() {
				name = "echo";
				description = "Duh";
				text = new tools.shell.argumentConditionPositional("Text", -1, needed: true);
				argsReqs = new() { text };
			}
			public override exitcode BeforeRun(List<string> args) {
				Console.WriteLine(text.contents);
				return exitcode.HALT;
			}
		}
		public class cat : command {
			tools.shell.argumentConditionPositional fileArg;
			public cat() {
				name = "cat";
				description = "Echo out the contents of the file";
				fileArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { fileArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = fileArg.contents;
				path = tools.path.Validate(path, finalSlash: false);
				bool fileExists = tools.path.FileExists(path);
				if (!fileExists) { messages.errors.file.fileNotFound(path); return exitcode.ERROR; }

				string FileContent = File.ReadAllText(path);

				Console.WriteLine(FileContent);
				return exitcode.HALT;
			}
		}
		public class ls : command {
			tools.shell.argumentConditionPositional pathArg;
			tools.shell.argumentConditionFlag prettyPrintArg;
			tools.shell.argumentConditionFlag plainArg;
			public ls() {
				name = "ls";
				description = "List files in the current directory";
				pathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: false, blacklist: new() { "-p", "-nc" });
				prettyPrintArg = new tools.shell.argumentConditionFlag("Pretty print", "-p", needed: false);
				plainArg = new tools.shell.argumentConditionFlag("No Color", "-nc", needed: false);
				argsReqs = new() { pathArg, prettyPrintArg, plainArg };
			}
			private void WriteItem(string name, bool isDir) {
				if (!plainArg.wasFound) {
					if (isDir) {
						Console.ForegroundColor = globals.Locals.getColor("LS_DIR_COLOR", "BLUE");
					} else {
						Console.ForegroundColor = globals.Locals.getColor("LS_FILE_COLOR", "BLACK");
					}
				}
				Console.Write(name);
				Console.BackgroundColor = tools.console.GetColor(globals.Locals.get("SHELL_BCOLOR", "WHITE"));
				Console.ForegroundColor = globals.Locals.getColor("SHELL_COLOR");
			}
			public override exitcode BeforeRun(List<string> args) {
				LSResults files;
				if (!pathArg.wasFound) {
					files = tools.path.List(globals.cwd);
				} else {
					string path = pathArg.contents;
					path = tools.path.Validate(path);
					bool pathExists = tools.path.DirectoryExists(path);
					if (!pathExists) { messages.errors.file.directoryNotFound(path); return exitcode.ERROR; }
					files = tools.path.List(path);
				}
				if (prettyPrintArg.wasFound) {
					int maxLength = 0;
					for (int i = 0; i < files.all.Count; i++) {
						if (files.all[i].Length > maxLength) {
							maxLength = files.all[i].Length;
						}
					}
					int wordAmt = globals.screenWidth / (maxLength + 2);
					int wordsOut = 0;
					for (int i = 0; i < files.dirs.Count; i++) {
						WriteItem(files.dirs[i], true);
						Console.Write(tools.strings.mult(' ', maxLength + 2 - files.dirs[i].Length));
						wordsOut++;
						if ((wordsOut % wordAmt) == (wordAmt - 1)) {
							Console.WriteLine();
						}
					}
					for (int i = 0; i < files.files.Count; i++) {
						WriteItem(files.files[i], false);
						Console.Write(tools.strings.mult(' ', maxLength + 2 - files.files[i].Length));
						wordsOut++;
						if ((wordsOut % wordAmt) == (wordAmt - 1)) {
							Console.WriteLine();
						}
					}
					Console.WriteLine();
				} else {
					for (int i = 0; i < files.dirs.Count; i++) {
						WriteItem(files.dirs[i], true);
						Console.WriteLine();
					}
					for (int i = 0; i < files.files.Count; i++) {
						WriteItem(files.files[i], false);
						Console.WriteLine();
					}
				}
				return exitcode.HALT;
			}
		}
		public class mkFile : command {
			tools.shell.argumentConditionPositional pathArg;
			public mkFile() {
				name = "mkFile";
				description = "Make a file";
				pathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { pathArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = pathArg.contents;
				path = tools.path.Validate(path, finalSlash: false);
				if (tools.path.FileExists(path)) {
					messages.errors.file.fileAlreadyExists(path);
					return exitcode.HANDLEDERROR;
				}
				try {
					FileStream file_stream = File.Create(path);
					file_stream.Close();
					//messages.confirmations.file.fileCreated(path);
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
				return exitcode.HALT;
			}
		}
		public class rmFile : command {
			tools.shell.argumentConditionPositional pathArg;
			public rmFile() {
				name = "rmFile";
				description = "Remove a file";
				pathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { pathArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = pathArg.contents;
				path = tools.path.Validate(path, finalSlash: false);
				bool fileExists = tools.path.FileExists(path);
				if (!fileExists) { messages.errors.file.fileNotFound(path); return exitcode.ERROR; }

				try {
					globals.fs.DeleteFile(globals.fs.GetFile(path));
					messages.confirmations.file.fileDeleted(path);
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
				return exitcode.HALT;
			}
		}
		public class mkDir : command {
			tools.shell.argumentConditionPositional pathArg;
			public mkDir() {
				name = "mkDir";
				description = "Make a directory";
				pathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { pathArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = pathArg.contents;
				path = tools.path.Validate(path, finalSlash: true);

				try {
					DirectoryEntry file_stream = globals.fs.CreateDirectory(path);
					//messages.confirmations.file.directoryCreated(path);
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
				return exitcode.HALT;
			}
		}
		public class rmDir : command {
			tools.shell.argumentConditionPositional pathArg;
			public rmDir() {
				name = "rmDir";
				description = "Remove a directory";
				pathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { pathArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = pathArg.contents;
				path = tools.path.Validate(path, finalSlash: true);
				bool fileExists = tools.path.DirectoryExists(path);
				if (!fileExists) { messages.errors.file.directoryNotFound(path); return exitcode.ERROR; }

				try {
					globals.fs.DeleteDirectory(globals.fs.GetDirectory(path));
					messages.confirmations.file.directoryDeleted(path);
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
				return exitcode.HALT;
			}
		}
		public class cd : command {
			tools.shell.argumentConditionPositional pathArg;
			public cd() {
				name = "cd";
				description = "Change the current working directory";
				pathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { pathArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = pathArg.contents;
				path = tools.path.Validate(path);
				bool pathExists = tools.path.DirectoryExists(path);
				if (!pathExists) { messages.errors.file.directoryNotFound(path); return exitcode.ERROR; }
				globals.cwd = path;
				return exitcode.HALT;
			}
		}
		public class shutdown : command {
			tools.shell.argumentConditionFlag restartArg;
			public shutdown() {
				name = "shutdown";
				description = "Shutdown the computer";
				restartArg = new tools.shell.argumentConditionFlag("Restart", "-r", needed: false);
				argsReqs = new() { restartArg };
			}
			public override exitcode BeforeRun(List<string> args) {

				if (restartArg.wasFound) {
					Sys.Power.Reboot();
				}
				Sys.Power.Shutdown();
				return exitcode.HALT;
			}
		}
		public class jsh : command {
			tools.shell.argumentConditionPositional fileArg;
			public jsh() {
				name = "jsh";
				description = "Run a jsh file";
				fileArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { fileArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string path = fileArg.contents;
				path = tools.path.Validate(path, finalSlash: false);
				bool fileExists = tools.path.FileExists(path);
				if (!fileExists) { messages.errors.file.fileNotFound(path); return exitcode.ERROR; }

				tools.shell.interpretFile(path);
				return exitcode.HALT;
			}
		}
		public class move : command {
			tools.shell.argumentConditionPositional fromPathArg;
			tools.shell.argumentConditionPositional toPathArg;
			public move() {
				name = "move";
				description = "Move a file";
				fromPathArg = new tools.shell.argumentConditionPositional("Path", -2, needed: true);
				toPathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { fromPathArg, toPathArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string pathFrom = fromPathArg.contents;
				string pathTo = toPathArg.contents;
				pathFrom = tools.path.Validate(pathFrom, finalSlash: false);
				pathTo = tools.path.Validate(pathTo, finalSlash: false);
				if (!tools.path.FileExists(pathFrom)) {
					messages.errors.file.fileNotFound(pathFrom);
					return exitcode.HANDLEDERROR;
				}
				if (tools.path.FileExists(pathTo)) {
					messages.errors.file.fileAlreadyExists(pathTo);
					return exitcode.HANDLEDERROR;
				}
				try {
					byte[] fileContents = File.ReadAllBytes(pathFrom);
					File.WriteAllBytes(pathTo, fileContents);
					File.Delete(pathFrom);
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
				return exitcode.HALT;
			}
		}
		public class copy : command {
			tools.shell.argumentConditionPositional fromPathArg;
			tools.shell.argumentConditionPositional toPathArg;
			public copy() {
				name = "copy";
				description = "Copy a file";
				fromPathArg = new tools.shell.argumentConditionPositional("Path", -2, needed: true);
				toPathArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { fromPathArg, toPathArg };
			}
			public override exitcode BeforeRun(List<string> args) {
				string pathFrom = fromPathArg.contents;
				string pathTo = toPathArg.contents;
				pathFrom = tools.path.Validate(pathFrom, finalSlash: false);
				pathTo = tools.path.Validate(pathFrom, finalSlash: false);
				if (!tools.path.FileExists(pathFrom)) {
					messages.errors.file.fileNotFound(pathFrom);
					return exitcode.HANDLEDERROR;
				}
				if (tools.path.FileExists(pathTo)) {
					messages.errors.file.fileAlreadyExists(pathTo);
					return exitcode.HANDLEDERROR;
				}
				try {
					byte[] fileContents = File.ReadAllBytes(pathFrom);
					File.WriteAllBytes(pathTo, fileContents);
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
				return exitcode.HALT;
			}
		}
		public class keyTest : command {
			public keyTest() {
				name = "keyTest";
				description = "Keyboard tester";
				argsReqs = new() { };
			}
			public override exitcode BeforeRun(List<string> args) {
				ConsoleKeyInfo cki = Console.ReadKey(true);
				Console.Write(((int)cki.Key).ToString());
				Console.Write(":");
				Console.Write(cki.KeyChar);
				Console.Write(":");
				Console.WriteLine((int)cki.KeyChar);
				return exitcode.HALT;
			}
		}
		public class resume : command {
			public resume() {
				name = "resume";
				description = "Resume suspended command";
				argsReqs = new() { };
			}
			public override exitcode BeforeRun(List<string> args) {
				if (globals.suspended == null) {
					Console.WriteLine("No program is suspended");
					return exitcode.HANDLEDERROR;
				}
				command cmd = globals.suspended;
				globals.suspended = null;
				exitcode exitCode;
				Console.Clear();
				do {
					exitCode = step(cmd);
				} while (exitCode == exitcode.CONTINUE);

				if (exitCode != exitcode.HALT && exitCode != exitcode.CONTINUE && exitCode != exitcode.HANDLEDERROR) {
					Console.WriteLine((int)exitCode);
					return exitCode;
				}
				return exitcode.HALT;
			}
		}
	}
}
