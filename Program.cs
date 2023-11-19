class Program {
	static string logFileName = "workedTime";
	
	enum action
	{
		ClockIn,
		ClockOut,
		CheckTime
	}
	static Dictionary<char, action> actionByShorthand = new() {
		{'I', action.ClockIn},
		{'O', action.ClockOut},
		{'T', action.CheckTime}
	};
	
	static string logFileDirectory = string.Empty;
	static void Main(string[] args) {
		List<string> directories = System.Reflection.Assembly.GetEntryAssembly().Location.Split("\\").ToList();
		directories.RemoveAt(directories.Count - 1);
		directories.ForEach(directory => logFileDirectory += directory + "\\");

		logFileDirectory += logFileName;

		action clockAction;

		if (args.Length != 0 && actionByShorthand.ContainsKey(args[0].ToUpper()[0])) {
			clockAction = actionByShorthand[args[0].ToUpper()[0]];
		}
		else {
			while (true) {
				Console.Write("Are you clocking-in, -out or checking time worked (i/o/t): ");
				string rawInput = Console.ReadLine();
				if (rawInput.Length != 0 && actionByShorthand.ContainsKey(rawInput.ToUpper()[0]))
					clockAction = actionByShorthand[rawInput.ToUpper()[0]];
			}
		}

		ConsoleColor originalConsoleColour;
		switch (clockAction) {
			case action.ClockIn:
				originalConsoleColour = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Green;
				byte[] data = BitConverter.GetBytes(DateTime.Now.ToBinary());
				using (FileStream F = new FileStream(logFileDirectory, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
					F.Write(data, 0, data.Length);
				}
				Console.WriteLine("SUCCESSFULLY CLOCKED IN");
				Console.ForegroundColor = originalConsoleColour;
				break;
			case action.ClockOut:
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
			case action.CheckTime:
				WriteTimeWorked();
				break;
		}

		static void WriteTimeWorked() {
			byte[] data;
			if (File.Exists(logFileDirectory))
				using (FileStream F = new FileStream(logFileDirectory, FileMode.OpenOrCreate, FileAccess.Read)) {
					if (F.Length == 0) {
						LogWarning("You haven't clocked-in yet.");
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
			string minutesAndSecondsWorked = decimalBaseMinutesToMinutesAndSecond(deltaMinutes);
			ConsoleColor originalConsoleColour = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine("Time Clocked:\t[mm:ss]");
			Console.WriteLine('\t' + minutesAndSecondsWorked);
			Console.ForegroundColor = originalConsoleColour;
		}
	}



	static void LogWarning(string warning) {
		ConsoleColor originalConsoleColour = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(warning);
		Console.ForegroundColor = originalConsoleColour;
	}

	
	/// <returns>Param <see cref="minutes"/> in the format "mm:ss"</returns>
	static string decimalBaseMinutesToMinutesAndSecond(double minutes) {
		int minutesPart = (int)Math.Floor(minutes);
		int secondsPart = (int)(60 * (minutes % 1));
		return
			(minutesPart == 0 ? "0" : minutesPart.ToString()) +
			":" +
			(secondsPart < 10 ? "0" + secondsPart.ToString() : secondsPart.ToString());
	}
}