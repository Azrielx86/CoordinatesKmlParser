using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mono.Options;
using Newtonsoft.Json;

namespace CoordinatesKmlParser;

public static partial class Program
{
    public static void Main(string[] args)
    {
        var dirPath = string.Empty;
        var invert = false;

        var options = new OptionSet
        {
            {"f|filepath=", "Path to the file or directory to parse.", f => dirPath = f},
            {"i|invert", "Invert coordinates.", i => invert = i != null}
        };

        try
        {
            options.Parse(args);
        }
        catch (OptionException e)
        {
            Console.WriteLine($"Error parsing arguments: {e.Message}");
            options.WriteOptionDescriptions(Console.Out);
        }

        if (File.Exists(dirPath))
        {
            GetCoordinates(dirPath, invert);
        }
        else if (Directory.Exists(dirPath))
        {
            var files = Directory.EnumerateFiles(dirPath).Where(f => f.EndsWith(".kml"));

            foreach (var file in files)
            {
                Console.WriteLine($"From file {file}:");
                GetCoordinates(file, invert);
            }
        }
        else
        {
            Console.WriteLine("Path does not exists!");
        }
    }

    private static void GetCoordinates(string filepath, bool invert = true)
    {
        var doc = XDocument.Load(filepath);
        const StringSplitOptions splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        
        var coordinates = (from c in doc.Descendants() where c.Name.LocalName == "coordinates" select new XElement(c)).ToList();
        
        var points = coordinates
            .Select(v => SpecialCharacters().Replace(v.Value, "")
                .Split(",0", splitOptions)
                .Select(c =>
                    c.Split(",", splitOptions)
                        .Select(float.Parse).ToArray()));

        if (invert)
            points = points.Select(v => v.Select(p => new[] { p[1], p[0] }));

        var json = JsonConvert.SerializeObject(points);

        Console.WriteLine(json);
    }

    [GeneratedRegex(@"[\t|\s]")]
    private static partial Regex SpecialCharacters();
}