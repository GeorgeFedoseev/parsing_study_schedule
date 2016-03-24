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
			var groups = TimetableParser.getGroups (years[selectYear].url);
			foreach (var link in groups) {
				Console.WriteLine (link);
			}


			var primary_timetable_url = TimetableParser.getPrimaryTimetableLink (groups [groups.Count - 1].url);
			Console.WriteLine ("Ссылка на страницу основного расписания: "
				+ primary_timetable_url
			);

			Console.WriteLine ("Парсим расписание..");



			var currentWeekStart = DateTime.Today.AddDays(-(int)(DateTime.Today.DayOfWeek-1)).Date;
			var tt1 = TimetableParser.getTimetable (primary_timetable_url, currentWeekStart);
			Console.WriteLine ("Сейчас идет "+(tt1.weekType==WeekType.Even?"четная":"нечетная")+" неделя");
			Console.WriteLine(tt1.ToString ());

			var nextWeek = DateTime.Now.AddDays (7).Date;
			var nextWeekStart = nextWeek.AddDays(-(int)(nextWeek.DayOfWeek-1)).Date;

			var tt2 = TimetableParser.getTimetable (primary_timetable_url, nextWeekStart);
			Console.WriteLine ("\n\nА через неделю ("+nextWeekStart.ToString ("yyyy-MM-dd")+") будет "
				+(tt2.weekType==WeekType.Even?"четная":"нечетная")+" неделя");
			//Console.WriteLine(tt2.ToString ());
		}



	}


}
