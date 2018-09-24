using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit;
using NUnit.Common;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnitLite;
using SQLite;

namespace SQLiteTests
{
	public class Program
	{
		static void Main(string[] args)
		{
			try 
			{
				// InternalTrace.Initialize (Console.Out, InternalTraceLevel.Debug);

				var runner = new NUnitTestAssemblyRunner (new DefaultTestAssemblyBuilder ());
				runner.Load (
					typeof (Program).Assembly,
					new Dictionary<string, object> {
						[FrameworkPackageSettings.NumberOfTestWorkers] = 0,
						[FrameworkPackageSettings.SynchronousEvents] = true,
						[FrameworkPackageSettings.RunOnMainThread] = true,
					}
				);

				var listener = new TestListener ();
				var r = runner.Run (listener, TestFilter.Empty);
				listener.ReportResults (r);
			}
			catch(Exception e) {
				Console.WriteLine (e);
			}
		}

		class TestListener : ITestListener
		{
			private TextUI _textUI;

			public TestListener ()
			{
				_textUI = new TextUI (new ColorConsoleWriter (false), Console.In, new NUnitLiteOptions ());
				_textUI.DisplayRuntimeEnvironment ();
			}

			public void TestFinished (ITestResult result) {
				 _textUI.TestFinished (result);
			}

			public void TestOutput (TestOutput output) {
				_textUI.TestOutput (output);
			}

			public void TestStarted (ITest test)
			{
				_textUI.TestStarted (test);
			}

			public void ReportResults(ITestResult result)
			{
				var summary = new ResultSummary (result);

				if (summary.ExplicitCount + summary.SkipCount + summary.IgnoreCount > 0) {
					_textUI.DisplayNotRunReport (result);
				}

				if (result.ResultState.Status == TestStatus.Failed || result.ResultState.Status == TestStatus.Warning) {
					_textUI.DisplayErrorsFailuresAndWarningsReport (result);
				}

				_textUI.DisplayRunSettings ();

				_textUI.DisplaySummaryReport (summary);
			}
		}

		private static void BasicTest ()
		{
			try {
				Console.WriteLine ("Starting...");

				// Get an absolute path to the database file
				var databasePath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "MyData.db");

				{
					var db = new SQLiteConnection (databasePath);

					db.CreateTable<Stock> ();
					db.CreateTable<Valuation> ();

					AddStock (db, "MSFT");
					AddStock (db, "AAPL");

					var query = db.Table<Stock> ().Where (v => v.Symbol.StartsWith ("M"));

					foreach (var stock in query) {
						Console.WriteLine ("Stock: " + stock.Symbol);
					}

					db.Delete (query.First ());

					db.Close ();
				}

				Console.WriteLine ($"File {databasePath}: {new FileInfo (databasePath).Length}");

				{
					var db = new SQLiteConnection (databasePath);
					var query = db.Table<Stock> ().Where (v => v.Symbol.StartsWith ("A"));

					foreach (var stock in query) {
						Console.WriteLine ("Stock: " + stock.Symbol);
					}

					db.Close ();
				}
			}
			catch (Exception e) {
				Console.WriteLine (e);
			}
		}

		public static void AddStock(SQLiteConnection db, string symbol)
		{
			db.Insert(new Stock()
			{
				Symbol = symbol
			});
		}
	}
	

	public class Stock
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Symbol { get; set; }
	}

	public class Valuation
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		[Indexed]
		public int StockId { get; set; }
		public DateTime Time { get; set; }
		public decimal Price { get; set; }
	}
}
