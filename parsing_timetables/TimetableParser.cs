using System;
using System.Net;
using System.Xml;

using System.Collections.Generic;

using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace parsing_timetables
{
	public enum WeekType {
		Even, // четная
		Odd
	}


	public class WeekTimetable {
		public DateTime weekStartDay;
		public List<DayTimetable> days;
		public WeekType weekType;

		public WeekTimetable(DateTime _weekStartDay){
			weekStartDay = _weekStartDay;
			weekType = GetIso8601WeekNumber(weekStartDay)%2==0?WeekType.Even:WeekType.Odd;
			days = new List<DayTimetable> ();
		}

		private static int GetIso8601WeekNumber(DateTime date)
		{    var thursday = date.AddDays(3 - ((int)date.DayOfWeek + 6) % 7);
			return 1 + (thursday.DayOfYear - 1) / 7;
		}

		public override string ToString () {
			var t = "";
			t += "\n";
			t += (weekType==WeekType.Even?"Четная неделя":"Нечетная неделя")+"\n";
			t += weekStartDay.ToString ("yyyy-MM-dd")+"\n";
			foreach (var d in days) {
				t += "\n|"+d.day.DayOfWeek+"| ("+d.day.ToString ("yyyy-MM-dd")+")\n";
				foreach (var p in d.pairs) {
					t += p.startTime.ToString("yyyy-MM-dd HH:mm")+" - "+p.endTime.ToString("yyyy-MM-dd HH:mm")+"\n";
					t += p.name+"\n";
					t += p.location+"\n";
					t += "room: " + p.room+"\n";
					t += p.lecturer+"\n";
					t += "------"+"\n";
				}
			}

			return t;			
		}
	}

	public class DayTimetable {
		public DateTime day;
		public List<Pair> pairs;

		public DayTimetable(DateTime _day){
			day = _day;
			pairs = new List<Pair> ();
		}
	}

	public class Pair {
		private DateTime day;

		public string name;
		public string time;
		public string location;
		public string lecturer;

		public string room;

		public DateTime startTime;
		public DateTime endTime;

		public Pair(DateTime _day, string _name, string _time, string _location, string _lecturer){
			day = _day;
			name = _name;
			time = _time;
			location = _location;
			lecturer = _lecturer;

			parseRoom ();
			parseTime ();
		}

		private void parseRoom(){
			var parser = new Regex(@"([0-9]*[А-Я]?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			//Console.WriteLine ("Location: "+location);
			room = parser.Match (location).Groups [1].Value;
			//Console.WriteLine ("Room: "+room);
		}

		private void parseTime(){
			//Console.WriteLine ("Time: "+time);
			var time_parts = time.Split ('–');
			var time1 = time_parts [0];
			var time2 = time_parts [1];

			var t1_parts = time1.Split (':');
			var t1_h = int.Parse(t1_parts [0]);
			var t1_m = int.Parse(t1_parts [1]);
			startTime = day.AddHours (t1_h).AddMinutes (t1_m);

			var t2_parts = time2.Split (':');
			var t2_h = int.Parse(t2_parts [0]);
			var t2_m = int.Parse(t2_parts [1]);
			endTime = day.AddHours (t2_h).AddMinutes (t2_m);		

			//Console.WriteLine (startTime.ToString("yyyy-MM-dd HH:mm"));
			//Console.WriteLine (endTime.ToString("yyyy-MM-dd HH:mm"));
		}
	}


	public class Link {
		public string name, url;
		public Link(string _name, string _url){
			name = _name;
			url = _url;
		}

		public override string ToString ()
		{
			return name + " : " + url;
		}
	}

	public class TimetableParser {



		public static WeekTimetable getTimetable(string timetable_url, DateTime week_start){
			var timetable = new WeekTimetable (week_start);

			var formatted_date = week_start.ToString ("yyyy-MM-dd");
			var html = getHtmlFromUrl("http://timetable.spbu.ru"+timetable_url+"/"+formatted_date);

			var dayNodes = html.DocumentNode.SelectNodes ("*//div[contains(@class, 'panel-default')]");
			if (dayNodes != null) {
				foreach (var d in dayNodes) {
					var day_of_week_str = d.SelectSingleNode ("*//*[@class='panel-title']/text()").InnerText.Trim ();
					var day_of_week = parseDayOfWeek (day_of_week_str);

					//Console.WriteLine (day_of_week+" - "+week_start.AddDays((int)day_of_week-1).Date.DayOfWeek);

					var dayTimetable = new DayTimetable (week_start.AddDays((int)day_of_week-1).Date);

					// parse day pairs

					var pairNodes = d.SelectNodes ("ul/li");
					if (pairNodes != null) {
						foreach (var pairNode in pairNodes) {
							var time = pairNode.SelectSingleNode ("div[contains(@class, 'studyevent-datetime')]").InnerText.Trim();
							var name = pairNode.SelectSingleNode ("div[contains(@class, 'studyevent-subject')]").InnerText.Trim();
							var location = getAllText(pairNode.SelectSingleNode ("div[contains(@class, 'locations')]")).Trim();
							var lecturer = getAllText(pairNode.SelectSingleNode ("div[contains(@class, 'educators')]")).Trim();
							//Console.WriteLine (time);
							//Console.WriteLine (name);
							//Console.WriteLine (location);
							//Console.WriteLine (lecturer);
							var pair = new Pair (dayTimetable.day, name, time, location, lecturer);
							dayTimetable.pairs.Add (pair);

						}
					} else {
						Console.WriteLine ("Pairs Not found");
					}

					timetable.days.Add (dayTimetable);
				}
			} else {
				Console.WriteLine ("Days Not found");
			}



			return timetable;
		}





		private static DayOfWeek parseDayOfWeek(string str){
			var parser = new Regex(@"(\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var d = parser.Match (str).Groups [1];

			switch(d.Value){
			case "понедельник":
				return DayOfWeek.Monday;
				break;
			case "вторник":
				return DayOfWeek.Tuesday;
				break;
			case "среда":
				return DayOfWeek.Wednesday;
				break;
			case "четверг":
				return DayOfWeek.Thursday;
				break;
			case "пятница":
				return DayOfWeek.Friday;
				break;
			case "суббота":
				return DayOfWeek.Saturday;
				break;
			case "воскресенье":
				return DayOfWeek.Sunday;
				break;
			default:
				return DayOfWeek.Sunday;				
			}
		}





		private static HtmlDocument getHtmlFromUrl(string url){
			using (WebClient client = new WebClient ()){
				client.Headers.Add(HttpRequestHeader.Cookie, "_culture=ru"); 
				HtmlDocument html = new HtmlDocument();
				html.LoadHtml(client.DownloadString(url));
				return html;
			}		
		}

		private static HtmlNodeCollection findByClassContains(string className, HtmlNode node){
			return node.SelectNodes(string.Format("//*[contains(@class,'{0}')]", className));
		}

		private static HtmlNodeCollection findByIdContains(string id, HtmlNode node){

			return node.SelectNodes(string.Format("//*[contains(@id, '{0}')]", id))
				;
		}

		private static string getPlainText(HtmlNode node){
			var t = "";
			var textNodes = node.SelectNodes ("text()");

			if (textNodes != null) {
				foreach(HtmlNode n in textNodes){
					t += n.InnerText;
				}
			} 

			return t;
		}

		private static string getAllText(HtmlNode node){
			var t = getPlainText(node);
			var textNodes = node.SelectNodes ("*//text()");

			if (textNodes != null) {
				foreach(HtmlNode n in textNodes){
					t += n.InnerText;
				}
			} 

			return t;
		}

		public static List<string> getStudyLevels(){
			var res = new List<string> ();
			var html = getHtmlFromUrl("http://timetable.spbu.ru/AMCP");
			var studyLevelNodes = html.DocumentNode.SelectNodes ("//div[@id='accordion']/div/div[@class='panel-heading']/h4[contains(@class, 'panel-title')]/a[contains(@href, '#studyProgramLevel')]");
			if (studyLevelNodes != null) {
				foreach (var n in studyLevelNodes) {
					res.Add (getPlainText(n).Trim());
				}	
			}

			return res;
		}

		public static List<string> getLevelPrograms(string level){
			var res = new List<string> ();
			var html = getHtmlFromUrl("http://timetable.spbu.ru/AMCP");

			var studyProgramNodes = html.DocumentNode.SelectNodes ("//div[@id='accordion']/div/div[@class='panel-heading']//a[contains(text(), '"+level+"')]/../../../ul/li/div[count(*)=0]");
			if (studyProgramNodes != null) {
				foreach (var n in studyProgramNodes) {
					res.Add (getPlainText(n).Trim());
				}	
			}

			return res;
		}

		public static List<Link> getProgramYears(string level, string program){
			var res = new List<Link> ();
			var html = getHtmlFromUrl("http://timetable.spbu.ru/AMCP");

			var programYearsNodes = html.DocumentNode.SelectNodes ("//div[@id='accordion']/div/div[@class='panel-heading']//a[contains(text(), '"+level+"')]/../../../ul/li/div[contains(text(), '"+program+"')]/../div/a");
			if (programYearsNodes != null) {
				foreach (var n in programYearsNodes) {
					res.Add (new Link(getPlainText(n).Trim(), n.Attributes["href"].Value));
				}	
			}

			return res;
		}

		public static List<Link> getGroups(string relative_url){
			var res = new List<Link> ();
			var html = getHtmlFromUrl("http://timetable.spbu.ru"+relative_url);

			var groupNodes = html.DocumentNode.SelectNodes ("//ul[@id='studentGroupsForCurrentYear']/li/div[@class='tile']");
			if (groupNodes != null) {
				foreach (var n in groupNodes) {
					var linkParser = new Regex(@"'(/\S+)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					res.Add (
						new Link(n.SelectSingleNode("div/text()").InnerText.Trim(),
							linkParser.Match(n.Attributes["onclick"].Value).Groups[1].Value
						)
					);
				}	

			}

			return res;
		}

		public static string getPrimaryTimetableLink(string group_link){
			var html = getHtmlFromUrl("http://timetable.spbu.ru"+group_link);
			var linkNodes = html.DocumentNode.SelectNodes ("//a[contains(@href, 'Primary')]");

			var maxTableId = 0;
			var linkNode = linkNodes [0];
			var idParser = new Regex(@"([0-9]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			foreach (var n in linkNodes) {
				var t_id = int.Parse (idParser.Match (n.Attributes ["href"].Value).Groups [1].Value);
				if (t_id > maxTableId) {
					linkNode = n;
					maxTableId = t_id;
				}
			}

			if (linkNode != null) {
				return linkNode.Attributes ["href"].Value;
			}

			return null;
		}




	}
}

