using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace Task3
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // Renewed Task 2.1, but without removing duplicates
            const String text = "Last year winner FC Dynamo (Kyiv, Ukraine, et. 1927) lost the game with 0:2 to FC Kryvbas (Kryvyi Rih, Ukraine, est. 1959)." +
            "Share price of Comp. Microsoft (Information Technology, US) dropped to $100" +
            "John Smith (1983/11/05, male) has been sentenced to a 3 year imprisonment for corruption. Just saw the film JAWS (1975, horror), it's really impressive! I bought this phone Iphone 5s (32Gb) on ebay. The Queen Square Library has a collection of over 10000 books and 900 EJournals" +
            "The highest peaks – Hoverla (2061 m, Ukraine), Brebenescul (2032 m, Ukraine), Everest (8848 m, Nepal). The bus 119 leaves from near the station exit and runs from around 5 a.m. until 0:30 p.m.!";

            var parsers = new List<Func<string, IEnumerable<IParsee>>>();
            parsers.Add(Person.ParserFunc);
            parsers.Add(FootballClub.ParserFunc);
            parsers.Add(Film.ParserFunc); 
            parsers.Add(Phone.ParserFunc);
            parsers.Add(Library.ParserFunc);
            parsers.Add(Mountain.ParserFunc);
            parsers.Add(Bus.ParserFunc);

            var parsedEntities = parsers.SelectMany(p => p(text));

            Console.WriteLine("All objects:");
            Console.WriteLine(parsedEntities.Stringify());

            // And Task 3.1
            // Create XML-files
            List<IParsee> all = new List<IParsee>(parsedEntities.ToArray());

            String path = Path.Combine(Directory.GetCurrentDirectory(), @"Data\"); 
            XML.encode(all, path);

            // Create objects from XML-files
            path = $"{path}\\FootballClub\\Dynamo1927Kyiv.xml";
            var newClub = XML.decode<FootballClub>(path);
            Console.WriteLine(newClub.ToString());

            Console.WriteLine("\nProperties:");
            Entities.properties(all);

            Console.WriteLine("\nTypes count:");
            Entities.propTypes(all);

            Console.ReadKey();
        }
    }

    class Entities
    {
        public static Dictionary<string, string> properties(List<IParsee> all)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();

            foreach (object item in all)
            {
                foreach (var prop in item.GetType().GetProperties())
                {
                    if (!props.ContainsKey(prop.Name))
                        props.Add(prop.Name, prop.PropertyType.Name);
                }
            }

            foreach (KeyValuePair<string, string> pair in props)
            {
                Console.WriteLine("<{0} - {1}>", pair.Key, pair.Value);
            }

            return props;
        }

        public static void propTypes(List<IParsee> all)
        {
            Dictionary<string, int> props = new Dictionary<string, int>();

            foreach (object item in all)
            {
                foreach (var prop in item.GetType().GetProperties())
                {
                    if (props.ContainsKey(prop.PropertyType.Name))
                    {
                        props[prop.PropertyType.Name]++;
                    }
                    else
                    {
                        props.Add(prop.PropertyType.Name, 1);
                    }
                }
            }

            foreach (KeyValuePair<string, int> item in props.OrderByDescending(key => key.Value))
            {
                Console.WriteLine("{0} - {1}", item.Key, item.Value);
            }
        }
    }

    class XML
    {
        public XML() { }

        public static void encode(List<IParsee> list, string path)
        {
            if (list.Count == 0)
                throw new System.ArgumentException("The array is empty!");

            foreach (var item in list)
            {
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(item.GetType());

                String folder = item.GetType().Name;
                String fileName = createFileName(item);

                String fullpath = Path.Combine(path, folder, fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

                System.IO.FileStream file = System.IO.File.Create(fullpath);

                writer.Serialize(file, item);
                file.Dispose(); // I cant use Close()
            }

            Console.WriteLine($"XML files were created. Their location: {path}");
        }

        public static T decode<T>(string path)
        {
            if (!File.Exists(path))
                throw new System.ArgumentException($"The path ({path}) is incorrect! There is no such file!");
            
            XmlSerializer serializer = new
            XmlSerializer(typeof(T));
            
            FileStream fs = new FileStream(path, FileMode.Open);
            XmlReader reader = XmlReader.Create(fs);
            
            T temp = (T)serializer.Deserialize(reader);
            fs.Dispose();

            return temp;
        }

        public static string createFileName(Object item)
        {
            string filename = "";
            foreach (var prop in item.GetType().GetProperties())
            {
                filename += prop.GetValue(item, null);
            }

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            filename = rgx.Replace(filename, "");

            return $"{filename}.xml";
        }
    }


    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class RegexAttribute : Attribute
    {
        public string expression { get; set; }

        public RegexAttribute(string Expression)
        {
            expression = Expression;
        }
    }

    public interface IParsee { }

    [RegexAttribute(@"(\p{Lu}\w+)?\s(\p{Lu}\w+)?\s\((\d{4}/\d{2}/\d{2}),\s(\w+)?\)")]
    public class Person : IParsee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        [XmlAttribute("gender")]
        public string Gender { get; set; }

        public Person() { }
        public Person(string firstName, string lastName, DateTime birthDate, string gender)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            Gender = gender;
        }

        public int Age => (DateTime.Now - BirthDate).Days / 365;

        public static IEnumerable<Person> ParserFunc(string text) 
        {

            string regex = typeof(Person).GetTypeInfo().GetCustomAttribute<RegexAttribute>().expression; //@"(\p{Lu}\w+)?\s(\p{Lu}\w+)?\s\((\d{4}/\d{2}/\d{2}),\s(\w+)?\)";
            var Matches = Regex.Matches(text, regex);
            var persons = new Person[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];

                var firstName = match.Groups[1].Value;
                var lastName = match.Groups[2].Value;
                var birthDate = DateTime.Parse(match.Groups[3].Value);
                var gender = match.Groups[4].Value;

                persons[i] = new Person(firstName, lastName, birthDate, gender);
            }

            return persons;
        }

        public override string ToString()
        {
            return $"{FirstName}, {LastName} ({Gender}, {Age})";
        }
    }


    [RegexAttribute(@"FC\s(\p{Lu}\w+)?\s\((\p{Lu}.+?),\s(\p{Lu}\w+).+?(\d{4})")]
    public class FootballClub : IParsee
    {
        public string Name { get; set; }
        public int Creation { get; set; }
        public string Adress { get; set; }

        public FootballClub() { }
        public FootballClub(string name, string adress, int year)
        {
            Name = name;
            Adress = adress;
            Creation = year;
        }

        public static IEnumerable<FootballClub> ParserFunc(string text)
        {

            string regex = typeof(FootballClub).GetTypeInfo().GetCustomAttribute<RegexAttribute>().expression;
            var Matches = Regex.Matches(text, regex); 
            var clubs = new FootballClub[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];
                int year = Int32.Parse(match.Groups[4].Value);

                clubs[i] = new FootballClub(match.Groups[1].Value, match.Groups[2].Value, year);
            }

            return clubs;
        }

        public override string ToString()
        {
            return $"FC {Name}, ({Adress}, founded in {Creation})";
        }
    }


    [RegexAttribute(@"film\s(\p{Lu}.+)\s\((\d{4})\,\s(\w+)\)")]
    public class Film : IParsee
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }

        public Film() { }
        public Film(string name, int year, string genre)
        {
            Name = name;
            Genre = genre;
            Year = year;
        }

        public static IEnumerable<Film> ParserFunc(string text)
        {

            string regex = typeof(Film).GetTypeInfo().GetCustomAttribute<RegexAttribute>().expression;
            var Matches = Regex.Matches(text, regex);
            var films = new Film[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];
                int year = Int32.Parse(match.Groups[2].Value);

                films[i] = new Film(match.Groups[1].Value, year, match.Groups[3].Value);
            }

            return films;
        }

        public override string ToString()
        {
            return $"Film {Name}, ({Year}, {Genre})";
        }
    }


    [RegexAttribute(@"phone\s(\w+)\s(\w+)\s\((\w+)Gb\)")]
    public class Phone : IParsee
    {
        public string Name { get; set; }
        public int Memory { get; set; }
        public string Model { get; set; }

        public Phone() { }
        public Phone(string name, int memory, string model)
        {
            Name = name;
            Memory = memory;
            Model = model;
        }

        public static IEnumerable<Phone> ParserFunc(string text)
        {
            string regex = typeof(Phone).GetTypeInfo().GetCustomAttribute<RegexAttribute>().expression;
            var Matches = Regex.Matches(text, regex);
            var phones = new Phone[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];
                int memory = Int32.Parse(match.Groups[3].Value);

                phones[i] = new Phone(match.Groups[1].Value, memory, match.Groups[2].Value);
            }

            return phones;
        }

        public override string ToString()
        {
            return $"Phone {Name}, ({Model}, {Memory})";
        }
    }


    [RegexAttribute(@"The\s(.+)\shas.+\s(\d{1,10})\sbooks\sand\s(\d{1,100})\sEJournals")]
    public class Library : IParsee
    {
        public string Name { get; set; }
        public int NumebrOfBooks { get; set; }
        public int NumberOfEJournals { get; set; }

        public Library() { }
        public Library(string name, int books, int ejournals)
        {
            Name = name;
            NumebrOfBooks = books;
            NumberOfEJournals = ejournals;
        }

        public static IEnumerable<Library> ParserFunc(string text)
        {
            string regex = typeof(Library).GetTypeInfo().GetCustomAttribute<RegexAttribute>().expression;
            var Matches = Regex.Matches(text, regex);
            var libraries = new Library[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];
                int books = Int32.Parse(match.Groups[2].Value);
                int ejournals = Int32.Parse(match.Groups[3].Value);

                libraries[i] = new Library(match.Groups[1].Value, books, ejournals);
            }

            return libraries;
        }

        public override string ToString()
        {
            return $"The {Name} has {NumebrOfBooks} books and {NumberOfEJournals} E-Journals";
        }
    }


    [RegexAttribute(@"(\p{Lu}\w+)\s\((\d{1,10})\sm\,\s(\p{Lu}\w+)\)")]
    public class Mountain : IParsee
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public string Country { get; set; }

        public Mountain() { }
        public Mountain(string name, int height, string country)
        {
            Name = name;
            Height = height;
            Country = country;
        }

        public static IEnumerable<Mountain> ParserFunc(string text)
        {
            string regex = typeof(Mountain).GetTypeInfo().GetCustomAttribute<RegexAttribute>().expression;
            var Matches = Regex.Matches(text, regex);
            var mountains = new Mountain[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];
                int height = Int32.Parse(match.Groups[2].Value);

                mountains[i] = new Mountain(match.Groups[1].Value, height, match.Groups[3].Value);
            }

            return mountains;
        }

        public override string ToString()
        {
            return $"The height of Mount {Name} is {Height} meters, it is located in {Country}";
        }
    }

    [RegexAttribute(@"The\sbus\s(\d{1,5}).+\saround\s(.+)\suntil\s(.+)!")]
    public class Bus : IParsee
    {
        public string Name { get; set; }
        public string From { get; set; }
        public string Until { get; set; }

        public Bus() { }
        public Bus(string name, string from, string until)
        {
            Name = name;
            From = from;
            Until = until;
        }

        public static IEnumerable<Bus> ParserFunc(string text)
        {
            string regex = typeof(Bus).GetTypeInfo().GetCustomAttribute<RegexAttribute>().expression;
            var Matches = Regex.Matches(text, regex);
            var timetable = new Bus[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];

                timetable[i] = new Bus(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
            }

            return timetable;
        }

        public override string ToString()
        {
            return $"The bus {Name} runs from {From} until {Until}";
        }
    }

    public static class ParseeExtensions
    {
        public static string Stringify(this IEnumerable<IParsee> persons)
        {
            var strBuilder = new StringBuilder();
            foreach (var p in persons)
                strBuilder.AppendLine(p.ToString());

            return strBuilder.ToString();
        }
    }
}
