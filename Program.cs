using System.IO;

class Program {
	static string logFileName = "workedTime";
	
	
	static string logFileDirectory = string.Empty;
	static void Main(string[] args) {
		List<string> directories = System.Reflection.Assembly.GetEntryAssembly().Location.Split("\\").ToList();
		directories.RemoveAt(directories.Count - 1);
		directories.ForEach(directory => logFileDirectory += directory + "\\");

		logFileDirectory += logFileName;
		
		char action = ' ';

		if (args.Length != 0 && args[0].ToUpper()[0] is ('I' or 'O' or 'T')) {
			action = args[0].ToUpper()[0];
		}
		else {
			while (action is not ('I' or 'O' or 'T')) {
				Console.Write("Are you clocking in, out or checking time worked (i/o/t): ");
				string rawInput = Console.ReadLine();
				if (rawInput.Length != 0)
					action = rawInput.ToUpper()[0];
			}
		}

		ConsoleColor originalConsoleColour;
		switch (action) {
			case 'I': //clock in
				originalConsoleColour = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Green;
				byte[] data = BitConverter.GetBytes(DateTime.Now.ToBinary());
				using (FileStream F = new FileStream(logFileDirectory, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
					F.Write(data, 0, data.Length);
				}
				Console.WriteLine("SUCCESSFULLY CLOCKED IN");
				Console.ForegroundColor = originalConsoleColour;
				break;
			case 'O': //clock out
				if (File.Exists(logFileDirectory)) {
					WriteTimeWorked();
					originalConsoleColour = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write("Confirm Clocking-Out (y): ");
					string rawInput = Console.ReadLine();
					if (rawInput.Length > 0 && rawInput.ToUpper()[0] == 'Y') {
						File.Delete(logFileDirectory);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("SUCCESSFULLY CLOCKED-OUT");
					}
					else {
						Console.Write("Clock-Out Cancelled");
					}
					Console.ForegroundColor = originalConsoleColour;
				}
				break;
			case 'T': //check time worked
				WriteTimeWorked();
				break;
		}

		static void WriteTimeWorked() {
			byte[] data;
			if (File.Exists(logFileDirectory))
				using (FileStream F = new FileStream(logFileDirectory, FileMode.OpenOrCreate, FileAccess.Read)) {
					if (F.Length == 0) {
						LogWarning("You haven't clocked in yet.");
						return;
							
					}
					data = new byte[F.Length];
					F.Read(data, 0, data.Length);
				}
			else {
				LogWarning($"Can't find log-file: \"{logFileDirectory}\"");
				return;
			}
			//if (data.Length == )
			DateTime clockInTime = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
		
			double deltaMinutes = DateTime.Now.Subtract(clockInTime).TotalMinutes;
			int workedMinutes = (int)Math.Floor(deltaMinutes);
			int workedSeconds = (int)(60 * (deltaMinutes % 1));
			ConsoleColor originalConsoleColour = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine("Time Clocked:\t[MM:SS]");
			Console.WriteLine(
				$"\t{(workedMinutes == 0 ? "0" : workedMinutes.ToString())}:{(workedSeconds < 10 ? "0" + workedSeconds.ToString() : workedSeconds.ToString())}");
			Console.ForegroundColor = originalConsoleColour;
		}
	}



	static void LogWarning(string warning) {
		ConsoleColor originalConsoleColour = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(warning);
		Console.ForegroundColor = originalConsoleColour;
	}
}