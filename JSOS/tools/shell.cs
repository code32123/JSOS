using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security;
using Cosmos.System.Network.IPv4.TCP;
using g;
using static messages.errors;
using static messages.errors.arguments;
using static tools.shell;

namespace tools {
	public class shell {
		public static List<string> parseArgs(string input) {
			return tools.strings.Split(input, ' ');
		}
		public static command getCommand(string name) {
			foreach (command cmd in globals.loadedCommands) {
				if (cmd.name == name) {
					return cmd;
				}
			}
			return null;
		}
		public class localTracker {
			public Dictionary<string, string> Items = new Dictionary<string, string>();
			public localTracker() { }
			public void set(string key, string val) {
				this.Items[key] = val;
			}
			public string get(string key, string def) {
				return this.Items.GetValueOrDefault(key, def);
			}
			public bool getBool(string key, bool def = false) {
				return tools.strings.softBool(this.get(key, def ? "True" : "False"));
			}
			public ConsoleColor getColor(string key, string def = "BLACK") {
				return tools.console.GetColor(this.get(key, def));
			}
			public ConsoleColor getColor(string key, ConsoleColor def) {
				string val = this.get(key, "def");
				if (val == "def") {
					return def;
				}
				return tools.console.GetColor(val);
			}
		}
		public enum exitcode {
			ERROR=-1,
			HALT = 0,
			CONTINUE = 1,
			HANDLEDERROR = 2,
		}
		public class command {
			public string name;
			public string description;
			public List<tools.shell.argumentCondition> argsReqs;
			public virtual exitcode Start(List<string> args) { return exitcode.HALT; }
			public virtual exitcode Run() { return exitcode.HALT; }
			/// <summary>
			/// Called when the program is exited for any reason - suspending or quitting
			/// </summary>
			public virtual void Exit() { return; }
			public virtual void Resume() { return; }
			public virtual void KeyPress(ConsoleKeyInfo cki) { return; }
		}
		public class argumentCondition {
			public string name;
			public bool needed;
			public bool wasFound;
			public virtual metReturn met(ref List<simpleArg> arguments) {
				return new metReturn(true);
			}
			public virtual void reset() { }
		}
		public class argumentConditionFlag : argumentCondition {
			public string flag;
			public List<string> follow = new List<string>();
			public List<string> contents = new List<string>();
			public argumentConditionFlag(string name, string flag, List<string> follow = null, bool needed = true) {
				this.name = name;
				this.flag = flag;
				this.follow = follow ?? new List<string>();
				this.needed = needed;
			}
			public override metReturn met(ref List<simpleArg> arguments) {
				int flagPos = tools.lists.IndexOf(arguments, this.flag);
				if (flagPos == -1) {
					this.wasFound = false;
					return new metReturn(!this.needed, invalidArgsCode.flagNotFound, this.flag); // If it's not there and not required, then mark it as satisfied
				}
				// At this point, flag IS in the arguments at flagPos
				// This will return a success because it exists and it needs no followers
				if (this.follow.Count == 0) {
					arguments[flagPos].used = true;
					this.wasFound = true;
					return new metReturn(true);
				}
				// This will fail if there is not even enough room for all the followers the argument needs
				if (this.follow.Count > (arguments.Count - flagPos)) {
					this.wasFound = false;
					return new metReturn(invalidArgsCode.flagNotEnoughFollowers);
				}
				// Now to validate the followers
				List<simpleArg> possibleFollowers = arguments.GetRange(flagPos, this.follow.Count);
				// In lieu of checking each argument against a typecheck to assert that each follower is of a required type, 
				// we could have each of the items in followers be instead a function to validate it's correctness.
				// TODO ^
				// For now, we'll just assume that since since there's enough followers there, it's good enough
				foreach (simpleArg sArg in possibleFollowers) {
					sArg.used = true;
					this.contents.Add(sArg.contents);
				}
				arguments[flagPos].used = true;
				this.wasFound = true;
				return new metReturn(true);
			}
			public override void reset() {
				this.wasFound = false;
				this.contents = null;
			}
			public override string ToString() {
				return flag;
			}
		}
		public class argumentConditionPositional : argumentCondition {
			public int position;
			public string contents;
			public List<string> blacklist;
			public argumentConditionPositional(string name, int position, bool needed = true, List<string> blacklist = null) {
				this.name = name;
				this.blacklist = blacklist ?? new List<string>();
				this.position = position;
				this.needed = needed;
			}
			public override metReturn met(ref List<simpleArg> arguments) {
				int pos = this.position;
				// This allows for relative positioning from the end like python (Yay!), so -1 will be the final element.
				if (pos < 0) {
					pos = arguments.Count + pos;
				}
				// Just need to make sure that arguments contains elements at the location specified, and that there's enough room
				if (arguments.Count < tools.lists.IndiceMagnitude(pos)) {
					this.wasFound = false;
					return new metReturn(!this.needed, invalidArgsCode.posNotFound, this.position.ToString()); // If it's not there and not required, then mark it as satisfied
				}
				// As of here there is enough room for the arg, now to check it against the blacklist
				string thisArg = arguments[pos].contents;
				foreach (string bad in blacklist) {
					if (bad == thisArg) {
						this.wasFound = false;
						return new metReturn(!this.needed, invalidArgsCode.posNotFound); // If it's not there and not required, then mark it as satisfied
					}
				}

				this.contents = thisArg;
				this.wasFound = true;
				arguments[pos].used = true;
				return new metReturn(true);
			}
			public override void reset() {
				this.wasFound = false;
				this.contents = null;
			}
			public override string ToString() {
				return name;
			}
		}
		public static metReturn applyArguments(List<argumentCondition> argumentRequirements, List<String> arguments) {
			if (argumentRequirements.Count == 0) {
				return new metReturn(arguments.Count == 0, invalidArgsCode.argUnknown);
			}
			List<simpleArg> sArgs = new List<simpleArg>();
			foreach (string argument in arguments) {
				sArgs.Add(new simpleArg(argument));
			}
			foreach (argumentCondition argReq in argumentRequirements) {
				// Right now does not check for collisions so duplicates will pass, as will positionals including flags, and flags followers including other flags
				metReturn isMet = argReq.met(ref sArgs);
				if (!isMet.valid) {
					return isMet; // If one of the checks fails, just fail the whole thing.
				}
			}
			foreach (simpleArg sArg in  sArgs) {
				if (!sArg.used) {
					return new metReturn(invalidArgsCode.argUnknown, sArg.contents);
				}
			}
			return new metReturn(true);
		}
		public class metReturn {
			public bool valid;
			public invalidArgsCode errorCode;
			public string problemArg;
			public metReturn(bool valid, invalidArgsCode errorCode = 0, string problemArg = null) {
				this.valid = valid;
				this.errorCode = errorCode;
				this.problemArg = problemArg;
			}
			public metReturn(invalidArgsCode errorCode = 0, string problemArg = null) {
				this.errorCode = errorCode;
				this.valid = errorCode==0;
				this.problemArg = problemArg;
			}
		}
		public class simpleArg {
			public string contents;
			public bool used = false;
			public simpleArg(string contents) {
				this.contents = contents;
			}
		}
		public static exitcode step(command cmd) {
			if (Console.KeyAvailable) {
				ConsoleKeyInfo key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control) {
					cmd.Exit();
					messages.errors.interpreter.keyboardInterrupt();
					return exitcode.HANDLEDERROR;
				} else if (key.Key == globals.SuspendKey && key.Modifiers == ConsoleModifiers.Control) {
					cmd.Exit();
					globals.suspended = cmd;
					return exitcode.HALT;
				}
				cmd.KeyPress(key);
			}
			return cmd.Run();
		}
		public static exitcode interpret(string line) {
			if (line == "") {
				return exitcode.HALT;
			}
			List<string> args = tools.shell.parseArgs(line);
			if (args.Count == 0) {
				return exitcode.HALT;
			}
			string commandName = args[0];
			args.RemoveAt(0);
			command cmd = tools.shell.getCommand(commandName);
			if (cmd == null) {
				messages.errors.command.commandNotFound(commandName);
				return exitcode.HANDLEDERROR;
			}
			for (int i = 0; i < args.Count; i++) {
				args[i] = runThroughVariables(args[i]);
			}
			metReturn isMet = tools.shell.applyArguments(cmd.argsReqs, args);
			if (!isMet.valid) { messages.errors.arguments.invalidArgs(args, isMet); return exitcode.HANDLEDERROR; }

			exitcode exitCode = cmd.Start(args);
			while (exitCode == exitcode.CONTINUE) {
				exitCode = step(cmd);
			}
			if (exitCode != exitcode.HALT && exitCode != exitcode.CONTINUE && exitCode != exitcode.HANDLEDERROR) {
				Console.WriteLine((int)exitCode);
				return exitCode;
			}
			cmd.Exit();
			return 0;
		}
		public static exitcode interpretFile(string path) {
			string fileName = tools.path.fileName(path);
			if (tools.path.FileExists(path)) {
				List<string> fileData = File.ReadLines(path).ToList();
				string line;
				for (int i = 0; i < fileData.Count; i++) {
					line = fileData[i];
					if (line[0] == ';') {
						continue;
					}

					//List<string> args = line.Split().ToList();
					//string command = args[0];
					//args.RemoveAt(0);
					//if (command == "goto") {
					//	if (args.Count == 0) {
					//		messages.errors.arguments.invalidArgs(args, new metReturn(3, "line number"));
					//		return exitcode.HANDLEDERROR;
					//	}
					//	if (args.Count > 1) {
					//		messages.errors.arguments.invalidArgs(args, new metReturn(4, args[1]));
					//		return exitcode.HANDLEDERROR;
					//	}
					//	if (!Int32.TryParse(args[0], out i)) {
					//		Console.WriteLine("Failed to convert " + args[0] + " to int");
					//		return exitcode.HANDLEDERROR;
					//	}
					//	i--;
					//} else {
						exitcode result = tools.shell.interpret(line);
						if (result == exitcode.HANDLEDERROR) {
							messages.errors.interpreter.haltedOnLine(i);
							return exitcode.HANDLEDERROR;
						}
					//}
				}
				return exitcode.HALT;
			} else {
				messages.errors.file.fileNotFound(path);
				return exitcode.HANDLEDERROR;
			}
		}
		public static string runThroughVariables(string varName) {
			if (!varName.StartsWith('$')) {
				return varName;
			}
			varName = varName.Substring(1);

			if (varName == "getKey") {
				if (!Console.KeyAvailable) {
					return "null";
				}
				return Console.ReadKey(true).KeyChar.ToString();
			}

			if (!globals.Locals.Items.ContainsKey(varName)) {
				messages.errors.interpreter.variableNotFound(varName);
				return varName;
			}
			return globals.Locals.get(varName, "REALLY BIG PROBLEM");
		}
	}
}
