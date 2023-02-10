using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Sys = Cosmos.System;

namespace JSOS
{
	public class Kernel : Sys.Kernel
	{
		// Comments like this mean you can leave notes in you code, so you can remind yourself of things later
		// I'll add comments to explain what each line does
		// Write your own comments if you want to, but you don't have to
		protected override void BeforeRun()
		{
			var textscr = Cosmos.HAL.Global.TextScreen;
			Cosmos.System.Global.Console = new Cosmos.System.Console(textscr);
			Cosmos.HAL.Global.TextScreen = textscr;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.White;

			Console.Clear();														// Clears the screen so we can do our own startup sequence!
			Console.WriteLine("Welcome to Jimmy S. Operating System");				// 'WriteLine' prints text and moves to the next line
			Console.Write("Beginning sys-check ");									// Write out the syscheck
			for (int i = 0; i <= 100; ++i)	//	Sets i to 0, then adds one to it (++i) while the condition is met (i <= 100)
			{
				//Thread.Sleep(4);													// Delays for 8 milliseconds
				Console.Write("\rBeginning sys-check {0}%   ", i);					// '\r' returns to the beginning of the line, so all previous
																					// messages are overwritten with the new percentage
			}
			Console.WriteLine(" - All Checks Passed");								// prints the text and then moves to the next line
		}

		protected override void Run()
		{

			// In linux, the prompt is usually the current directory, but we havn't set that up yet. For now, just do this:
			Console.Write("JSOS> "); // This is the 'prompt', or what comes right before the text

			string input = Console.ReadLine();                                         // Gets user input and saves it to the input variable
			List<string> args = input.Split().ToList();
			string command = args[0];
			args.RemoveAt(0);
			int argPos;

			if (command == "sysinfo")
			{
				Console.WriteLine("This computer's information:");
				Console.WriteLine("   Hardware:");
				Console.WriteLine("      RAM:            " + Cosmos.Core.CPU.GetAmountOfRAM().ToString() + "MB");
				Console.WriteLine("      CPU Brand:      " + Cosmos.Core.CPU.GetCPUBrandString());
				Console.WriteLine("      CPU Vendor:     " + Cosmos.Core.CPU.GetCPUVendorName());
				Console.WriteLine("      CPU Speed:      " + (Cosmos.Core.CPU.GetCPUCycleSpeed() / 100000000).ToString() + "Ghz");
				Console.WriteLine("      CPU Uptime:     " + (Cosmos.Core.CPU.GetCPUUptime() / (ulong)Cosmos.Core.CPU.GetCPUCycleSpeed()).ToString() + "s");
				Console.WriteLine("   Operating System:");
				Console.WriteLine("      Name:           JSOS, Jimmy S. Operating System");
				Console.WriteLine("      Version:        v0.1.0");
				Console.WriteLine("      Build Date:     8/2/2023");
			} else if (command == "clear") {
				Console.Clear();
			} else if (command == "color") {
				argPos = myIndexOf(args, "-help");
				if (argPos != -1) {
					Console.WriteLine("-help");
					Console.WriteLine("-fg [color]");
					Console.WriteLine("-bg [color]");
				} else {
					argPos = myIndexOf(args, "-fg");
					if (argPos != -1) {
						Console.ForegroundColor = getColor(args[argPos + 1].ToString());
					}
					argPos = myIndexOf(args, "-bg");
					if (argPos != -1) {
						Console.BackgroundColor = getColor(args[argPos + 1].ToString());
					}
					Console.Clear();
				}
			} else if (command == "shutdown") {
				argPos = myIndexOf(args, "-r");
				if (argPos != -1) {
					Sys.Power.Reboot();
				}
				Sys.Power.Shutdown();
			} else if (command == "credits")
			{
				Console.WriteLine("This project was made as a group broject between:");
				Console.WriteLine("Dion:");
				Console.WriteLine("   Github: evildion07");
				Console.WriteLine("   Discord: Musical Eevee#6926");
				Console.WriteLine("Jimmy: ");
				Console.WriteLine("   Github: code32123");
				Console.WriteLine("   Discord: Jimmy32#2036");
			}
			else if (command == "help")
			{
				Console.WriteLine("Currently loaded commands:");
				Console.WriteLine("   help                Prints out this system help message");
				Console.WriteLine("   sysinfo             Lists hardware and software commands");
				Console.WriteLine("   clear               Clears the screen");
				Console.WriteLine("   credits             lists contact info for creators");
			}
			else
			{ // Here is the else, what we do if nothing matches
				Console.WriteLine("SYSERR: Command '" + command + "' not found on the system.");
			}
		}
		string debugList(List<string> stringList) {
			return String.Join(", ", stringList.ToArray()); ;
		}
		ConsoleColor getColor(string colorName){
			colorName = colorName.ToLower();
			if (colorName == "black") {
				return ConsoleColor.Black;
			} else if (colorName == "white") {
				return ConsoleColor.White;
			} else if (colorName == "blue") {
				return ConsoleColor.Blue;
			} else if (colorName == "darkblue") {
				return ConsoleColor.DarkBlue;
			} else if (colorName == "cyan") {
				return ConsoleColor.Cyan;
			} else if (colorName == "darkcyan") {
				return ConsoleColor.DarkCyan;
			} else if (colorName == "gray") {
				return ConsoleColor.Gray;
			} else if (colorName == "darkgray") {
				return ConsoleColor.DarkGray;
			} else if (colorName == "green") {
				return ConsoleColor.Green;
			} else if (colorName == "darkgreen") {
				return ConsoleColor.DarkGreen;
			} else if (colorName == "magenta") {
				return ConsoleColor.Magenta;
			} else if (colorName == "darkmagenta") {
				return ConsoleColor.DarkMagenta;
			} else if (colorName == "red") {
				return ConsoleColor.Red;
			} else if (colorName == "darkred") {
				return ConsoleColor.DarkRed;
			} else if (colorName == "yellow") {
				return ConsoleColor.Yellow;
			} else if (colorName == "darkyellow") {
				return ConsoleColor.DarkYellow;
			} else {
				return ConsoleColor.Black;
			}
		}
		int myIndexOf(List<string> stringList, string Find) {
			int at = -1;
			for (int i = 0; i < stringList.Count(); i++) {
				if (stringList[i] == Find) {
					at = i;
					break;
				}
			}
			return at;
		}
	}
}
