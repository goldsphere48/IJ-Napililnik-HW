using System;
using System.IO;

namespace Lesson
{
	interface ILogger
	{
		void LogWrite(string message);
	}

	class ConsoleLogWriter : ILogger
	{
		public void LogWrite(string message)
		{
			Console.WriteLine($"[C] {message}");
		}
	}

	class FileLogWriter : ILogger
	{
		private readonly string _path;
		
		public FileLogWriter(string path)
		{
			_path = path;
		}

		public void LogWrite(string message)
		{
			File.AppendAllText(_path, $"[F] {message}");
		}
	}

	class FridayLogger : ILogger
	{
		private readonly ILogger _internalLogger;

		public FridayLogger(ILogger logger)
		{
			_internalLogger = logger;
		}

		public void LogWrite(string message)
		{
			if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday) {
				_internalLogger.LogWrite($"[Fr] {message}");
			}
		}
	}

	class CompositeLogger : ILogger
	{
		private readonly IEnumerable<ILogger> _loggers;

		public CompositeLogger(params ILogger[] loggers)
		{
			_loggers = loggers;
		}

		public void LogWrite(string message)
		{
			foreach (var logger in _loggers) {
				logger.LogWrite(message);
			}
		}
	}

	class Pathfinder
	{
		public readonly ILogger _logger;

		public Pathfinder(ILogger logger)
		{
			_logger = logger;
		}

		public void Find()
		{
			_logger.LogWrite("Some random message");
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			string logFile = "log.txt";
			var pt1 = new Pathfinder(new FileLogWriter(logFile));
			var pt2 = new Pathfinder(new ConsoleLogWriter());
			var pt3 = new Pathfinder(new FridayLogger(new FileLogWriter(logFile)));
			var pt4 = new Pathfinder(new FridayLogger(new ConsoleLogWriter()));
			var pt5 = new Pathfinder(new CompositeLogger(new ConsoleLogWriter(), new FridayLogger(new FileLogWriter(logFile))));

			pt1.Find();
			pt2.Find();
			pt3.Find();
			pt4.Find();
			pt5.Find();
		}
	}
}