using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using g;

namespace tools {
	static public class path {
		public class LSResults {
			public List<string> files;
			public List<string> dirs;
			public List<string> all;
			public LSResults(List<string> files, List<string> dirs) {
				this.files = files;
				this.dirs = dirs;
				this.all = tools.lists.Combine(files, dirs);
			}
		}
		static public LSResults List(string directory) {
			List<string> directory_list = Directory.GetDirectories(directory).ToList();
			List<string> file_list = Directory.GetFiles(directory).ToList();
			return new LSResults(file_list, directory_list);
		}
		static public bool DirectoryExists(string directory) {
			try {
				return (globals.fs.GetDirectory(directory) != null);
			} catch (Exception) {
				return false;
			}
		}
		internal static bool FileExists(string directory) {
			try {
				return (globals.fs.GetFile(directory) != null);
			} catch (Exception) {
				return false;
			}
		}
		static public string fileName(string directory) {
			return directory.Split(@"\").Last();
		}
		static public string Validate(string directory, bool finalSlash = true, string root = @"0:\") {

			if (directory == null) {
				return globals.cwd;
			}

			directory = directory.Replace("/", @"\");
			// Fix beginning
			if (directory[..1] == @"\") {
				directory = path.Join(root, directory);
			} else {
				if (directory[..1] == "~") {
					directory = path.Join(root, directory[1..]);
				} else if (directory.Substring(1, 2) != @":\") {
					directory = path.Join(globals.cwd, directory);
				}
			}
			// Make sure it doesn't end with a slash for the split process
			if (directory.EndsWith(@"\")) {
				directory = directory[..^1];
			}
			// Iterate through
			List<string> directorySplit = directory.Split(@"\").ToList();
			directory = path.Join(directorySplit);
			//for (int i = 0; i < directorySplit.Count; i++) {
				//if (directorySplit[i] == "..") {
				//	List<string> firstPart = directory.Split(@"\").ToList();
				//	directory = path.Join(firstPart.GetRange(0, firstPart.Count-1));
				//} else if (directorySplit[i] != ".") {
				//directory = path.Join(directory, directorySplit[i]);
				//}
			//}
			// Fix Ending
			if (finalSlash && !directory.EndsWith(@"\")) {
				directory += @"\";
			} else if (!finalSlash && directory.EndsWith(@"\")) {
				directory = directory[..^1];
			}
			// Return
			return directory;
		}
		static public string Join(string directory1, string directory2) {
			if (directory1.EndsWith(@"\")) {
				directory1 = directory1[..^1];
			}
			if (directory2.StartsWith(@"\")) {
				directory2 = directory2[1..];
			}
			if (directory1 == "") {
				return directory2;
			}
			if (directory2 == "") {
				return directory1;
			}
			return directory1 + @"\" + directory2;
		}
		static public string Join(List<string> directories) {
			string dirToReturn = "";
			for (int i = 0; i < directories.Count; i++) {
				dirToReturn = tools.path.Join(dirToReturn, directories[i]);
			}
			return dirToReturn;
		}
	}
}