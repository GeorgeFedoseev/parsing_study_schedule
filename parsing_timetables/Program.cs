using System;
using System.Net;
using System.Xml;


using HtmlAgilityPack;

namespace parsing_timetables
{
	class MainClass
	{
		public static void Main (string[] args)
		{

			Console.WriteLine ("\nУровни обучения:");
			var studyLevels = TimetableParser.getStudyLevels ();
			foreach (var sl in studyLevels) {
				Console.WriteLine (sl);
			}

			var selectLevel = 1;
			Console.WriteLine ("\nНаправления "+studyLevels[selectLevel]+":");
			var levelPrograms = TimetableParser.getLevelPrograms (studyLevels[selectLevel]);
			foreach (var lp in levelPrograms) {
				Console.WriteLine (lp);
			}

			var selectProgram = 0;
			Console.WriteLine ("\nГоды поступления "+levelPrograms[selectProgram]+":");
			var years = TimetableParser.getProgramYears (studyLevels[selectLevel], levelPrograms[selectProgram]);
			foreach (var link in years) {
				Console.WriteLine (link);
			}

			var selectYear = 2;
			Console.WriteLine ("\nГруппы года поступления "+years[selectYear].name);
			var groups = TimetableParser.getProgramYears (studyLevels[selectLevel], levelPrograms[selectProgram]);
			foreach (var link in years) {
				Console.WriteLine (link);
			}
		}
	}
}
