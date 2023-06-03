using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Cosmos.System.FileSystem.Listing;
using g;
using Microsoft.VisualBasic;
using tools;
using static commands.main;
using static tools.path;
using static tools.shell;
using Sys = Cosmos.System;

namespace commands {
	public partial class tool {
		public class scr : command {
			public scr() {
				name = "scr";
				description = "Script runner";
				fileArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { fileArg };
			}
			tools.shell.argumentConditionPositional fileArg;
			string filePath;
			//string fileName;
			List<string> fileLines;
			int currentLine;
			string line;
			public override exitcode Start(List<string> args) {
				filePath = tools.path.Validate(fileArg.contents, finalSlash: false);
				bool fileExists = tools.path.FileExists(filePath);
				if (!fileExists) { messages.errors.file.fileNotFound(filePath); return exitcode.HANDLEDERROR; }
				//fileName = tools.path.fileName(filePath);
				fileLines = File.ReadAllLines(filePath).ToList();
				currentLine = 0;
				return exitcode.CONTINUE;
			}
			List<string> tokenize(string line) {
				List<string> tokens = new List<string>();
				string token = "";
				while (line.Length > 0) {
					char thisChar = line[0];
					if (isWordBoundary(thisChar)) {
						if (token.Length != 0) {
							tokens.Add(token);
							//Console.WriteLine(token);
							token = "";
						}
						tokens.Add(thisChar.ToString());
						//Console.WriteLine(thisChar);
					} else {
						token += thisChar;
					}
					line = line.Remove(0, 1);
				}
				if (token.Length != 0) {
					tokens.Add(token);
					//Console.WriteLine(token);
					token = "";
				}
				Console.WriteLine(tools.lists.ToString(tokens));
				return tokens;
			}
			List<string> abstractize(List<string> tokens) {
				List<string> abstracts = new List<string>();

				foreach (string token in tokens) {

				}

				Console.WriteLine(tools.lists.ToString(abstracts));
				return abstracts;
			}
			public override exitcode Run() {
				Console.WriteLine("Running line: " + currentLine.ToString());
				line = fileLines[currentLine];
				List<string> tokens = tokenize(line);
				List<string> abstracts = abstractize(tokens);
				currentLine++;
				if (currentLine == fileLines.Count) {
					return exitcode.HALT;
				}
				return exitcode.CONTINUE;
			}
			bool isWordBoundary(char toTest) {
				return !(char.IsLetterOrDigit(toTest) || toTest == ' ');
			}
		}
	}
}
