using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using tools;
using g;
using static tools.shell;

namespace messages {
	static public class errors {
		static string prefix {
			get { return globals.messagePrefix ? "SYSERR: " : ""; }
		}
		static public class arguments {
			static public void invalidArgs(List<String> args, metReturn failure) {
				if (failure.errorCode == 1) {
					Console.WriteLine($"A flag '{failure.problemArg}' was needed and not found");
				} else if (failure.errorCode == 2) {
					Console.WriteLine("A flag had insufficient followers");
				} else if (failure.errorCode == 3) {
					Console.WriteLine($"A positional '{failure.problemArg}' was needed and not found");
				} else if (failure.errorCode == 4) {
					Console.WriteLine($"Unknown argument {failure.problemArg}");
				} else {
					Console.WriteLine(prefix + "Invalid arguments '" + tools.lists.ToString(args) + "', no additional information given");
				}
			}
			static public void invalidSymbol(string arg, string symbol) {
				Console.WriteLine(prefix + "Invalid symbol '" + symbol + "' in '" + arg + "'");
			}
		}
		static public class file {
			static public void fileNotFound(string file) {
				Console.WriteLine(prefix + "The file '" + file + "' could not be found");
			}
			static public void directoryNotFound(string path) {
				Console.WriteLine(prefix + "The path '" + path + "' could not be found");
			}
			static public void fileAlreadyExists(string file) {
				Console.WriteLine(prefix + "The file '" + file + "' already exists");
			}
		}
		static public class command {
			static public void commandNotFound(string command) {
				Console.WriteLine(prefix + "The command '" + command + "' could not be found");
			}
		}
		static public class interpreter {
			static public void haltedOnLine(int line) {
				Console.WriteLine(prefix + "Interpreter halted on line " + line.ToString());
			}
			static public void variableNotFound(string variable) {
				Console.WriteLine(prefix + "Interpreter couldn't resolve name '" + variable + "'");
			}
			static public void keyboardInterrupt() {
				Console.WriteLine(prefix + "Ctrl+C pressed, exiting");
			}
		}
	}
	static public class confirmations {
		static string prefix {
			get { return globals.messagePrefix ? "SYSMSG: " : ""; }
			//set { ; }
		}
		static public class file {
			static public void fileCreated(string file) {
				Console.WriteLine(prefix + "The file '" + file + "' was created successfully");
			}
			public static void directoryCreated(string path) {
				Console.WriteLine(prefix + "The directory '" + path + "' was created successfully");
			}
			static public void fileDeleted(string file) {
				Console.WriteLine(prefix + "The file '" + file + "' was deleted successfully");
			}
			public static void directoryDeleted(string path) {
				Console.WriteLine(prefix + "The directory '" + path + "' was deleted successfully");
			}
		}
	}
}