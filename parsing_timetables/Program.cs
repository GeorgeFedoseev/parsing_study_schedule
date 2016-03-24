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
			var studyLervels = TimetableParser.getStudyLevels ();
			foreach (var sl in studyLervels) {
				Console.WriteLine (sl);
			}
		}
	}
}
