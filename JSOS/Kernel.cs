using System;
//using System.Collections;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading;
using Sys = Cosmos.System;
using tools;
//using messages;
using g;

//using System.Text.Json;
//using System.IO;
//using System.Net.NetworkInformation;
using static tools.shell;
using System.Security.Authentication.ExtendedProtection;
using System.IO;
using System.Linq;
using static commands.main;
//using commands;

// File format ideas:
//		.prg/.csl		custom scripting language
//		.scr		script for shell

namespace g {
	public static class globals {
		public static Sys.FileSystem.CosmosVFS fs;
		public static string cwd = @"0:\";
		public static bool messagePrefix = true;
		public static List<string> commandHistory = new();
		public static int screenWidth = 80;
		public static int screenHeight = 25;
		public static List<shell.command> loadedCommands;
		public static localTracker Locals = new localTracker();
		public const ConsoleKey SuspendKey = ConsoleKey.Oem3; // Tick/Grave/Tilde
		public static shell.command suspended = null;
	}
}
namespace JSOS {
	public class Kernel : Sys.Kernel {
		Sys.FileSystem.CosmosVFS fs;
		protected override void BeforeRun() {
			Console.OutputEncoding = Cosmos.System.ExtendedASCII.CosmosEncodingProvider.Instance.GetEncoding(437);
			//Console.InputEncoding = Cosmos.System.ExtendedASCII.CosmosEncodingProvider.Instance.GetEncoding(437);
			Console.WriteLine("[....] Loading text screen");
			var textscr = Cosmos.HAL.Global.TextScreen;
			Cosmos.System.Global.Console = new Cosmos.System.Console(textscr);
			Cosmos.HAL.Global.TextScreen = textscr;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.White;
			Console.Clear();
			Console.WriteLine("[DONE] Loading text screen");

			Console.WriteLine("[....] Loading filesystem");
			globals.fs = new Sys.FileSystem.CosmosVFS();
			Sys.FileSystem.VFS.VFSManager.RegisterVFS(globals.fs);
			var available_space = globals.fs.GetAvailableFreeSpace(@"0:\");
			var fs_type = globals.fs.GetFileSystemType(@"0:\");
			Console.CursorLeft = 0;
			Console.CursorTop--;
			Console.WriteLine("[DONE] Loading filesystem");

			Console.WriteLine("[....] Loading commands");
			globals.loadedCommands = new List<tools.shell.command>() {
				new commands.main.sysinfo(),
				new commands.main.help(),
				new commands.main.set(),
				new commands.main.clear(),
				new commands.main.credits(),
				new commands.main.stat(),
				new commands.main.echo(),
				new commands.main.cat(),
				new commands.main.ls(),
				new commands.main.mkFile(),
				new commands.main.rmFile(),
				new commands.main.mkDir(),
				new commands.main.rmDir(),
				new commands.main.cd(),
				new commands.main.shutdown(),
				new commands.main.jsh(),
				new commands.main.move(),
				new commands.main.copy(),
				new commands.main.keyTest(),
				new commands.main.resume(),

				new commands.tool.edit(),
				//new commands.tool.hexedit(),

			};
			Console.CursorLeft = 0;
			Console.CursorTop--;
			Console.WriteLine("[DONE] Loading commands");

			const string startUpFileName = "start.jsh";
			if (tools.path.FileExists(@"0:\" + startUpFileName)) {
				Console.WriteLine("[....] Running " + startUpFileName);
				bool loadStart = true;
				for (int i = 0; i < 20; i++) {
					if (Console.KeyAvailable) {
						ConsoleKeyInfo key = Console.ReadKey(true);
						if (key.Key == ConsoleKey.Escape) {
							Console.CursorLeft = 0;
							Console.CursorTop--;
							Console.WriteLine("[SKIP] Running " + startUpFileName);
							loadStart = false;
							break;
						}
					}
				}
				if (loadStart) {
					tools.shell.interpretFile(@"0:\" + startUpFileName);
				}
			}

			//Console.WriteLine("Loading settings.json");
			//string fileName = "settings.json";
			//string jsonString = File.ReadAllText(fileName);
			//WeatherForecast weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(jsonString)!;

			Console.WriteLine("Welcome to Jimmy's Operating System");               // 'WriteLine' prints text and moves to the next line
																					//while (true) {
																					//	ConsoleKeyInfo cki = Console.ReadKey(true);
																					//	bool ctrl = (cki.Modifiers & ConsoleModifiers.Control) != 0;
																					//	if (ctrl) {
																					//		Console.WriteLine("A'" + cki.KeyChar.ToString() + "',B'" + ((int)cki.KeyChar).ToString() + "'");
																					//	}
																					//}

			//Console.WriteLine("This builds tests");

			//Console.Write("Beginning sys-check ");                                // Write out the syscheck
			//for (int i = 0; i <= 100; ++i)                                        //	Sets i to 0, then adds one to it (++i) while the condition is met (i <= 100)
			//{
			//	Thread.Sleep(1);                                                    // Delays for 1 millisecond
			//	Console.Write("\rBeginning sys-check {0}%   ", i);                  // '\r' returns to the beginning of the line, so all previous
			//																		// messages are overwritten with the new percentage
			//}
			//Console.WriteLine(" - All Checks Passed");                              // prints the text and then moves to the next line
		}

		protected override void Run() {
			try {
				_Run();
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
		void _Run() {
			Console.BackgroundColor = tools.console.GetColor(globals.Locals.get("SHELL_BCOLOR", "WHITE"));

			Console.ForegroundColor = globals.Locals.getColor("SHELL_PROMPT_PATH_COLOR", globals.Locals.getColor("SHELL_COLOR"));
			bool doFinalSlash = globals.Locals.getBool("SHELL_PROMPT_PATH_SLASH");
			Console.Write(tools.path.Validate(globals.cwd, finalSlash: doFinalSlash));

			Console.ForegroundColor = globals.Locals.getColor("SHELL_PROMPT_END_COLOR", globals.Locals.getColor("SHELL_COLOR"));
			Console.Write(">");
			Console.ForegroundColor = globals.Locals.getColor("SHELL_COLOR");

			string input = tools.console.ReadLine();
			if (input == "") return;

			//List<string> args = tools.shell.parseArgs(input);
			//string commandName = args[0];
			//args.RemoveAt(0);
			//command cmd = tools.shell.getCommand(commandName);
			//if (cmd == null) {
			//	messages.errors.command.commandNotFound(commandName);
			//	return;
			//}
			//metReturn isMet = tools.shell.applyArguments(cmd.argsReqs, args);
			//if (!isMet.valid) { messages.errors.arguments.invalidArgs(args, isMet); return; }

			//exitcode exitCode = cmd.BeforeRun(args);
			//while (exitCode == exitcode.CONTINUE) {
			//	exitCode = cmd.Run(args);
			//}
			//if (exitCode != exitcode.HALT && exitCode != exitcode.CONTINUE && exitCode != exitcode.HANDLEDERROR) {
			//	Console.WriteLine((int)exitCode);
			//}
			exitcode exitCode = tools.shell.interpret(input);
			if (exitCode != exitcode.HALT && exitCode != exitcode.CONTINUE && exitCode != exitcode.HANDLEDERROR) {
				Console.WriteLine((int)exitCode);
			}
		}
	}
}
