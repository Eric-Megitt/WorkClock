class Program {
	static string logFileName = "workedTime";

	enum TimeUnit {
		Fortnight,
		Day,
		Hour,
		Minute,
		Second
	}
	static Dictionary<TimeUnit, TimeSpan> timeSpanByTimeUnit = new() {
		{ TimeUnit.Fortnight, TimeSpan.FromDays(14) },
		{ TimeUnit.Day, TimeSpan.FromDays(1) },
		{ TimeUnit.Hour, TimeSpan.FromHours(1) },
		{ TimeUnit.Minute, TimeSpan.FromMinutes(1) },
		{ TimeUnit.Second, TimeSpan.FromSeconds(1) }
	};
	static Dictionary<TimeUnit, string> timeUnitRepresentationByTimeUnit = new() {
		{ TimeUnit.Fortnight, "fortnights" },
		{ TimeUnit.Day, "d" },
		{ TimeUnit.Hour, "h" },
		{ TimeUnit.Minute, "m" },
		{ TimeUnit.Second, "s" }
	};
	
	enum action
	{
		ClockIn,
		ClockOut,
		CheckTime,
		ChangeTimeFormat
	}
	static List<action> actionsPriority = new() {
		action.ChangeTimeFormat,
		action.CheckTime,
		action.ClockOut,
		action.ClockIn
	};
	static Dictionary<action, int> parameterAmountByAction = new() {
		{ action.ClockIn, 0 },
		{ action.ClockOut, 0 },
		{ action.CheckTime, 0 },
		{ action.ChangeTimeFormat, 1 }
	};
	static Dictionary<string, action> actionByHandle = new() {
		{ "-I", action.ClockIn },
		{ "--ClockIn", action.ClockIn },
		
		{ "-O", action.ClockOut },
		{ "--ClockOut", action.ClockOut },
		
		{ "-T", action.CheckTime },
		{ "--CheckTime", action.CheckTime },
		
		{ "-F", action.ChangeTimeFormat },
		{ "--TimeFormat", action.ChangeTimeFormat },
		//TODO { "/?", action.Help }
	};
	
	static List<TimeUnit> timeFormat = new() { TimeUnit.Minute, TimeUnit.Second };
	
	static string logFileDirectory = string.Empty;
	
	static void Main(string[] args) {
		List<string> directories = System.Reflection.Assembly.GetEntryAssembly().Location.Split("\\").ToList();
		directories.RemoveAt(directories.Count - 1);
		directories.ForEach(directory => logFileDirectory += directory + "\\");

		logFileDirectory += logFileName;

		ConsoleColor originalConsoleColour;

		var clockActions = GetProgramActions(args);
		
		foreach (action _action in actionsPriority) {
			if (!clockActions.ContainsKey(_action)) continue;
			var clockAction = _action;
			var actionParameters = clockActions[clockAction];
			
			switch (clockAction) {
				case action.ChangeTimeFormat:
					ChangeTimeFormat(actionParameters[0]);
					break;
				case action.ClockIn:
					ClockIn();
					break;
				case action.ClockOut:
					ClockOut();
					break;
				case action.CheckTime:
					CheckTime();
					break;
			}
			Console.WriteLine();
		}
	}

	static void ChangeTimeFormat(string parameter) { //format of parameter is e.g. "FortnigHts:HH:ss"
		List<string> timeUnits = parameter.ToLower().Split(':').ToList();
		foreach (string timeUnit in timeUnits) {
			if (!timeUnitRepresentationByTimeUnit.ContainsValue(timeUnit)) {
				LogWarning($"Syntax error in {actionByHandle.FindKeyByValue(action.ChangeTimeFormat)} argument parameters.\n" +
					"\tFormat it like this \"[time unit]:[time unit]\", example: \"FortnigHts:D:HH:s\"");
				return;
			}
		}
		
		timeFormat.Clear();
		timeUnits.ForEach(timeUnit => timeFormat.Add(timeUnitRepresentationByTimeUnit.FindKeyByValue(timeUnit)));
	}
	
	static void ClockIn() {
		ConsoleColor originalConsoleColour = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Green;
		
		byte[] data = BitConverter.GetBytes(DateTime.Now.ToBinary());
		using (FileStream file = new FileStream(logFileDirectory, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
			file.Write(data, 0, data.Length);
		}
		
		Console.WriteLine("SUCCESSFULLY CLOCKED IN");
		Console.ForegroundColor = originalConsoleColour;
	}

	static void ClockOut() {
		if (!File.Exists(logFileDirectory)) return;
		
		ConsoleColor originalConsoleColour = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Write("Confirm Clocking-Out (y): ");
		string rawInput = Console.ReadLine();
		if (rawInput.Length > 0 && rawInput.ToUpper()[0] == 'Y') {
			File.Delete(logFileDirectory);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("SUCCESSFULLY CLOCKED-OUT");
		}
		else {
			Console.WriteLine("Clock-Out Cancelled");
		}
		Console.ForegroundColor = originalConsoleColour;
	}

	static void CheckTime() {
		byte[] data;
		if (File.Exists(logFileDirectory)) {
			using (FileStream file = new FileStream(logFileDirectory, FileMode.OpenOrCreate, FileAccess.Read)) {
				if (file.Length == 0) {
					LogWarning("You haven't clocked-in yet.");
					return;
				}
				data = new byte[file.Length];
				file.Read(data, 0, data.Length);
			}
		}
		else {
			LogWarning($"Can't find log-file: \"{logFileDirectory}\"");
			return;
		}
		
		DateTime clockInTime = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
		
		WriteTimeWorked(DateTime.Now.Subtract(clockInTime));
	}
		
	static void WriteTimeWorked(TimeSpan workedTime) {
		string minutesAndSecondsWorked = TimeSpanIntoDesiredTimeUnits(workedTime);
		ConsoleColor originalConsoleColour = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Magenta;
		Console.WriteLine(minutesAndSecondsWorked);
		Console.ForegroundColor = originalConsoleColour;
	}
	
	static Dictionary<action, List<string>> GetProgramActions(string[] args) {
		Dictionary<action, List<string>> clockActions = new Dictionary<action, List<string>>();

		while (true) {
			if (args.Length != 0) {
				bool argsContainCorrectParameters = TryProcessArguments(args, ref clockActions);

				if (argsContainCorrectParameters)
					return clockActions;
			}

			Console.Write("Provide Arguments: ");
			args = Console.ReadLine().Split(' ');
		}
	}

	static bool TryProcessArguments(string[] args, ref Dictionary<action, List<string>> clockActions) {
		action lastActionWithPriority = default;
		int numberOfParametersLeft = 0;

		foreach (string arg in args) {
			if (numberOfParametersLeft != 0) {
				clockActions[lastActionWithPriority].Add(arg);
				continue;
			}

			if (!actionByHandle.ContainsKey(arg.ToUpper())) {
				clockActions.Clear();
				return false;
			}

			action currentActionWithPriority = actionByHandle[arg.ToUpper()];
			clockActions.Add(currentActionWithPriority, new());
			numberOfParametersLeft = parameterAmountByAction[currentActionWithPriority];
			lastActionWithPriority = currentActionWithPriority;
		}

		return true;
	}

	static void LogWarning(string warning) {
		ConsoleColor originalConsoleColour = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(warning);
		Console.ForegroundColor = originalConsoleColour;
	}
	
	static string TimeSpanIntoDesiredTimeUnits(TimeSpan timeSpan) {
		long ticks = timeSpan.Ticks;
		string returnValue = "Time Clocked:\t[";
		string numberPart = "";
		foreach (KeyValuePair<TimeUnit, TimeSpan> timeSpanToTimeUnit in timeSpanByTimeUnit) {
			if (!timeFormat.Contains(timeSpanToTimeUnit.Key)) continue;

			if (numberPart == string.Empty) {
				long numberOfUnits = ticks / timeSpanToTimeUnit.Value.Ticks;
				ticks = ticks % timeSpanToTimeUnit.Value.Ticks;
				numberPart += numberOfUnits.ToString();
				returnValue += timeUnitRepresentationByTimeUnit[timeSpanToTimeUnit.Key];
			}
			else {
				long numberOfUnits = ticks / timeSpanToTimeUnit.Value.Ticks;
				ticks = ticks % timeSpanToTimeUnit.Value.Ticks;
				numberPart += ':' + numberOfUnits.ToString();
				returnValue += ':' + timeUnitRepresentationByTimeUnit[timeSpanToTimeUnit.Key];
			}
		}
		returnValue += "]\n\t" + numberPart;
		return returnValue;
	}

	static int Floor(double number) => (int)Math.Floor(number);
}


public static class IDictionaryExtensions
{
	public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
	{
		if (dictionary == null)
			throw new ArgumentNullException(nameof(dictionary));

		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			if (value.Equals(pair.Value)) return pair.Key;

		throw new Exception("the value is not found in the dictionary");
	}
}