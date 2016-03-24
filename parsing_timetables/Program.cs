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
			var studyLevels = TimetableParser.getStudyLevels ();
			foreach (var sl in studyLevels) {
				Console.WriteLine (sl);
			}

			var levelPrograms = TimetableParser.getLevelPrograms ("Бакалавриат");
			foreach (var lp in levelPrograms) {
				Console.WriteLine (lp);
			}
		}
	}
}
