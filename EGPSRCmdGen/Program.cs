using System;
using System.IO;
using System.Collections.Generic;
using RoboCup.AtHome.CommandGenerator;

namespace RoboCup.AtHome.EGPSRCmdGen
{
	/// <summary>
	/// Contains the program control logic
	/// </summary>
	public class Program : BaseProgram
	{
		/// <summary>
		/// Random Task generator
		/// </summary>
		protected EGPSRGenerator gen;

		protected override Generator Gen
		{
			get { return this.gen; }
		}

		public Program()
		{
			gen = new EGPSRGenerator();
		}

		/// <summary>
		/// Checks if at least one of the required files are present. If not, initializes the
		/// directory with example files
		/// </summary>
		public static void InitializePath()
		{
			int xmlFilesCnt = System.IO.Directory.GetFiles (Loader.ExePath, "*.xml", System.IO.SearchOption.TopDirectoryOnly).Length;
			if ((xmlFilesCnt < 4) || !System.IO.Directory.Exists (Loader.GetPath("egpsr_grammars")))
				ExampleFilesGenerator.GenerateExampleFiles ();
		}

		/// <summary>
		/// Request the user to choose an option for random task generation.
		/// </summary>
		/// <returns>The user's option.</returns>
		protected override char GetOption()
		{
			ConsoleKeyInfo k;
			Console.WriteLine("Press Esc to quit, q for QR Code, t for type in a QR, c to clear.");
			Console.Write("Hit Enter to generate: ");
			return (k = Console.ReadKey(true)).Key == ConsoleKey.Escape ? '\0' : k.KeyChar;
		}

		private Task GetTask()
		{
			return gen.GenerateTask(DifficultyDegree.High);
		}

		/// <summary>
		/// Starts the user input loop
		/// </summary>
		public override void Run()
		{
			Task task = null;
			char opc = '\0';
			Setup();
			RunOption('\r', ref task);
			do
			{
				opc = GetOption();
				RunOption(opc, ref task);
			}
			while (opc != '\0');
		}

		/// <summary>
		/// Executes the user's option
		/// </summary>
		/// <param name="opc">User option (category).</param>
		protected override void RunOption(char opc, ref Task task)
		{
			switch (opc)
			{
				case 'c':
					Console.Clear();
					return;

				case 't':
					DisplayQRText();
					return;

				case 'q':
					if (task == null)
					{
						Generator.Warn("Generate a task first");
						return;
					}

					ShowQRDialog(task.ToString());
					return;

				default:
					task = GetTask();
					break;
			}

			Console.WriteLine("Choosen category {0}", opc);
			PrintTask(task);
		}

		/// <summary>
		/// Initializes the random task Generator and loads data from lists and storage
		/// </summary>
		protected override void Setup()
		{
			this.gen = new EGPSRGenerator ();

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine ("EGPSR Generator 2019 Release Candidate");
			Console.WriteLine ();
			base.LoadData();
			Console.WriteLine ();
			Console.WriteLine ();
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args)
		{
			InitializePath ();
			if (args.Length == 0) {
				new Program ().Run ();
				return;
			}
			ParseArgs (args);
		}

		/// <summary>
		/// Parses the arguments.
		/// </summary>
		/// <param name="args">Arguments given to the application.</param>
		private static void ParseArgs (string[] args)
		{
			Program p = new Program ();

			p.Setup ();
			for (int i = 0; i < args.Length; ++i){
				if (args[i] == "--bulk"){
					DoBulk(p, args, ref i);
					Environment.Exit(0);
				}
			}
		}

		private static void DoBulk(Program p, string[] args, ref int i)
		{
			int dCount;
			if ((args.Length < (i + 2)) || !Int32.TryParse(args[++i], out dCount) || (dCount < 1))
				dCount = 100;

			Console.WriteLine("Generating {0} examples in bulk mode", dCount);
			try
			{
				BulkExamples(p, dCount);
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }
		}

		private static void BulkExamples(Program p, int count)
		{
			string oDir = String.Format("EGPSR Examples");
			if (!Directory.Exists(oDir))
				Directory.CreateDirectory(oDir);
			string oFile = Path.Combine(oDir, String.Format("{0}.txt", oDir));
			using (StreamWriter writer = new StreamWriter(oFile, false, System.Text.Encoding.UTF8))
			{
				for (int i = 1; i <= count; ++i)
				{
					Task task = p.GetTask();
					if (task == null) continue;
					string sTask = task.ToString().Trim();
					if (sTask.Length < 1) continue;
					sTask = sTask.Substring(0, 1).ToUpper() + sTask.Substring(1);

					WriteTaskToFile(writer, task, sTask, i);
					Console.WriteLine("Generating example {0}.", i);
					// GenerateTaskQR(sTask, i, oDir);
				}
			}
		}

		private static void WriteTaskToFile(StreamWriter writer, Task task, string sTask, int i)
		{
			writer.WriteLine(sTask);
		}

		private static void GenerateTaskQR(string task, int i, string oDir)
		{
			string oFile;
			System.Drawing.Image qr = CommandGenerator.GUI.QRDialog.GenerateQRBitmap(task, 500);
			oFile = Path.Combine(oDir, String.Format("Example{0} - {1}.png", i.ToString().PadLeft(3, '0'), task));
			if (File.Exists(oFile)) File.Delete(oFile);
			qr.Save(oFile, System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}
