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
		public class edit : command {
			public edit() {
				name = "edit";
				description = "Mini text editor";
				fileArg = new tools.shell.argumentConditionPositional("Path", -1, needed: true);
				argsReqs = new() { fileArg };
			}
			enum menus {
				none,
				view,
				saveas,
			}
			tools.shell.argumentConditionPositional fileArg;
			int cursorPosX = 0, cursorPosY = 0;
			int scroll = 0;
			int horizontalPane = 0;
			string fileName;
			string filePath;
			List<string> fileBuffer;
			bool edited = false;
			notif displayNotif = null;
			menus menu = menus.none;

			int availableHeight;
			int availableWidth;
			int gutterWidth;
			const int screenMarginTop = 1;
			exitcode toReturn = exitcode.CONTINUE;

			public override exitcode Start(List<string> args) {
				Console.Clear();
				cursorPosX = 0;
				cursorPosY = 0;
				scroll = 0;
				horizontalPane = 0;
				edited = false;
				menu = menus.none;

				filePath = tools.path.Validate(fileArg.contents, finalSlash: false);
				bool fileExists = tools.path.FileExists(filePath);
				fileName = tools.path.fileName(filePath);
				if (!fileExists) {
					//messages.errors.file.fileNotFound(filePath); return exitcode.HANDLEDERROR;
					edited = true;
					fileBuffer = new List<string> { "" };
					displayNotif.set("New File");
				} else {
					fileBuffer = File.ReadAllLines(filePath).ToList();
					if (fileBuffer.Count == 0) {
						fileBuffer = new List<string> { "" };
					}
					displayNotif.set("Loaded " + fileBuffer.Count.ToString() + " lines");
				}


				availableHeight = globals.screenHeight - 3;
				gutterWidth = fileBuffer.Count.ToString().Length;
				availableWidth = globals.screenWidth - gutterWidth;
				renderDocument();

				return exitcode.CONTINUE;
			}
			private void printTopBar() {
				//Console.CursorVisible = false;
				Console.SetCursorPosition(0, 0);
				int needToFill = globals.screenWidth - fileName.Length - 3;
				tools.console.InvertColors();
				Console.Write(edited ? "* " : "  ");
				tools.console.InvertColors();
				Console.Write(" " + tools.path.fileName(filePath) + " ");
				if (!displayNotif.expired) {
					tools.console.InvertColors();
					Console.Write(tools.strings.mult(' ', needToFill - displayNotif.message.Length - 2));
					Console.Write(displayNotif.message);
					Console.Write(' ');
					tools.console.InvertColors();
				} else {
					tools.console.InvertColors();
					Console.Write(tools.strings.mult(' ', needToFill));
					tools.console.InvertColors();
				}
				//Console.CursorVisible = true;
			}
			private string handleHorizontal(string line) {
				return line.Substring(0, Math.Min(availableWidth, line.Length));
			}
			private void printDocument() {
				//Console.CursorVisible = false;
				int most = Math.Min(scroll+availableHeight, fileBuffer.Count);
				for (int i = scroll; i < most; i++) {
					Console.SetCursorPosition(0, i+screenMarginTop);
					tools.console.InvertColors();
					Console.Write(i.ToString().PadLeft(gutterWidth));
					tools.console.InvertColors();
					Console.Write(handleHorizontal(fileBuffer[i]));
					for (int pad = 0; pad < availableWidth - handleHorizontal(fileBuffer[i]).Length; pad++) {
						Console.Write(" ");
					}
				}
				if (most == fileBuffer.Count) { // If maxed out by the file size (and not the window size), it is safe to throw a blank line underneath to clean stuff up
					Console.SetCursorPosition(0, screenMarginTop + scroll + fileBuffer.Count);
					for (int pad = 0; pad < availableWidth; pad++) {
						Console.Write(" ");
					}
				}
				//Console.CursorVisible = true;
			}
			private void updateCursor() {
				Console.SetCursorPosition(cursorPosX+gutterWidth, cursorPosY + 1 - scroll);
			}
			private void renderDocument() {
				gutterWidth = (fileBuffer.Count-1).ToString().Length;
				displayNotif.expire();
				printTopBar();
				printDocument();
				updateCursor();
			}
			private void addStringToBuffer(int x, int y, string toAdd) {
				edited = true;
				fileBuffer[y] = fileBuffer[y].Insert(x, toAdd);
				cursorPosX += toAdd.Length;
			}
			private enum cursorDirection {
				up,
				down,
				left,
				right,
			}
			private bool tryMoveCursor(cursorDirection dir) {
				if (dir == cursorDirection.up) {
					if (cursorPosY > 0) {
						cursorPosY--;
					} else {
						return false;
					}
				} else if (dir == cursorDirection.down) {
					if (cursorPosY < fileBuffer.Count-1) {
						cursorPosY++;
					} else {
						cursorPosX = fileBuffer[cursorPosY].Length;
					}
				} else if (dir == cursorDirection.left) {
					if (cursorPosX > 0) {
						cursorPosX--;
					} else {
						if (tryMoveCursor(cursorDirection.up)) {
							cursorPosX = fileBuffer[cursorPosY].Length;
						} else {
							return false;
						}
					}
				} else if (dir == cursorDirection.right) {
					if (cursorPosX < fileBuffer[cursorPosY].Length) {
						cursorPosX++;
					} else {
						if (tryMoveCursor(cursorDirection.down)) {
							cursorPosX=0;
						} else {
							return false;
						}
					}
				} else { return false; }
				if (cursorPosX > fileBuffer[cursorPosY].Length) {
					cursorPosX = fileBuffer[cursorPosY].Length;
				}
				return true;
			}
			private class notif {
				const int lastsForSeconds = 5;
				public string message = "";
				public ulong startTime = 0;
				public bool expired = false;
				public void set(string message) {
					this.message = message;
					startTime = (Cosmos.Core.CPU.GetCPUUptime() / (ulong)Cosmos.Core.CPU.GetCPUCycleSpeed());
				}
				public void expire() {
					expired = ((Cosmos.Core.CPU.GetCPUUptime() / (ulong)Cosmos.Core.CPU.GetCPUCycleSpeed())) > (startTime + lastsForSeconds);
				}
			}
			public override void Exit() {
				Console.Clear();
				Console.SetCursorPosition(0, 0);
			}
			public override void KeyPress(ConsoleKeyInfo cki) {
				if (cki.Key == ConsoleKey.NoName) {
					toReturn = exitcode.CONTINUE;
					return;
				}
				bool ctrl = (cki.Modifiers & ConsoleModifiers.Control) != 0;
				bool alt = (cki.Modifiers & ConsoleModifiers.Alt) != 0;
				bool shift = (cki.Modifiers & ConsoleModifiers.Shift) != 0;
				if (ctrl) {
					if (cki.Key == ConsoleKey.Q) {
						Console.Clear();
						toReturn = exitcode.HALT;
						return;
					} else if (cki.Key == ConsoleKey.S) {
						if (shift) {

						} else {
							File.WriteAllText(filePath, String.Join('\n', fileBuffer.ToArray()));
							edited = false;
							displayNotif.set("Saved");
							renderDocument();
						}
					//} else if (cki.Key == ConsoleKey.) {
						//Menu
					}
				} else if (alt) {

				} else {
					if (cki.Key == ConsoleKey.Home) {
						cursorPosX = 0;
					} else if (cki.Key == ConsoleKey.End) {
						cursorPosX = fileBuffer[cursorPosY].Length;
					} else if (cki.Key == ConsoleKey.UpArrow) {
						tryMoveCursor(cursorDirection.up);
					} else if (cki.Key == ConsoleKey.DownArrow) {
						tryMoveCursor(cursorDirection.down);
					} else if (cki.Key == ConsoleKey.LeftArrow) {
						tryMoveCursor(cursorDirection.left);
					} else if (cki.Key == ConsoleKey.RightArrow) {
						tryMoveCursor(cursorDirection.right);
						//} else if (cki.Key == ConsoleKey.PageUp) {
						//	Console.CursorVisible = true;
						//	Console.Write("T");
						//} else if (cki.Key == ConsoleKey.PageDown) {
						//	Console.CursorVisible = false;
						//	Console.Write("F");
					} else if (cki.Key == ConsoleKey.Backspace) {
						edited = true;
						if (fileBuffer[cursorPosY].Length > 0 && cursorPosX > 0) {
							cursorPosX--;
							fileBuffer[cursorPosY] = fileBuffer[cursorPosY].Remove(cursorPosX, 1);
						} else if (cursorPosY > 0 && cursorPosX == 0) {
							cursorPosY--;
							cursorPosX = fileBuffer[cursorPosY].Length;
							fileBuffer[cursorPosY] = fileBuffer[cursorPosY] + fileBuffer[cursorPosY + 1];
							fileBuffer.RemoveAt(cursorPosY + 1);
						}
						renderDocument();
					} else if (cki.Key == ConsoleKey.Delete) {
						edited = true;
						if (fileBuffer[cursorPosY].Length > cursorPosX) {
							fileBuffer[cursorPosY] = fileBuffer[cursorPosY].Remove(cursorPosX, 1);
						} else if (cursorPosY < fileBuffer.Count && cursorPosX == fileBuffer[cursorPosY].Length) {
							fileBuffer[cursorPosY] = fileBuffer[cursorPosY] + fileBuffer[cursorPosY + 1];
							fileBuffer.RemoveAt(cursorPosY + 1);
						}
						renderDocument();
					} else if (cki.Key == ConsoleKey.Enter) {
						edited = true;

						string line = fileBuffer[cursorPosY];
						if (line.Length > 0) {
							fileBuffer[cursorPosY] = line.Substring(0, cursorPosX);
							fileBuffer.Insert(cursorPosY + 1, line.Substring(cursorPosX, line.Length - cursorPosX));
						} else {
							fileBuffer.Insert(cursorPosY + 1, "");
						}
						cursorPosY++;
						cursorPosX = 0;
						renderDocument();
					} else {
						addStringToBuffer(cursorPosX, cursorPosY, cki.KeyChar.ToString());
						renderDocument();
					}
					updateCursor();
				}
				toReturn = exitcode.CONTINUE;
				return;


			}
			public override void Resume() {
				renderDocument();
			}
			public override exitcode Run() {
				return toReturn;
			}
		}
	}
}
