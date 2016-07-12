using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;

namespace ArticlesBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GetPDF();

            Console.WriteLine("done");
            Console.ReadLine();
        }

        public static async Task<byte[]> UrlByte(string url)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetByteArrayAsync(url);

            return response;
        }

        public static string UrlToString(string URL)
        {
           var result = UrlByte(URL).Result;
           return Encoding.ASCII.GetString(result);
        }

        public static void GetPDF() //Object obj
        {

            const string URL = "http://nz.ukma.edu.ua/index.php?option=com_content&task=category&sectionid=10&id=60&Itemid=47";
            const string path = @"E:\PDF";

            string html = UrlToString(URL);

            Regex regexForm = new Regex(@"contentpane.+</table>.+<input", RegexOptions.Singleline);
            Match match = regexForm.Match(html);

            Match[] matches = Regex.Matches(match.Value, @"<a href=.(.+).>")
                               .Cast<Match>()
                               .ToArray();

            foreach (var i in matches)
            {
                Regex rgx = new Regex("amp;");
                string link = rgx.Replace(i.Groups[1].Value, "");
          
                string file = UrlToString(link);

                Match[] pdf = Regex.Matches(file, "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?.pdf")
                                .Cast<Match>()
                                .ToArray();

                downloadPDF(path, pdf);
            }
        }

        public static void downloadPDF(string path, Match[] files)
        {
            foreach (var j in files)
            {
                Thread.Sleep(5000);
                var bytefile = UrlByte(j.Value).Result;
                string name = filename(j.Value);
                Directory.CreateDirectory(path);
                File.WriteAllBytes($"{path}\\{name}", bytefile);
                Console.WriteLine($"The file {name} has been downloaded");
            }
        }

        public static string filename(string url)
        {
            Uri uri = new Uri(url);
            string filename = Path.GetFileName(uri.AbsolutePath);
            return filename;
        }

    }

}


