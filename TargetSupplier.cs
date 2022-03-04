using System.Text.RegularExpressions;

namespace DDoS_Manager;

public class TargetSupplier
{

    private readonly (string, string)[] _targets;
    private int _counter;

    public TargetSupplier(string filename)
    {
        var lines = File.ReadAllLines(filename);
        _targets = new (string, string)[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            _targets[i].Item1 = TargetParse(lines[i]);
            _targets[i].Item2 = GetMethod(lines[i]);
        }
    }

    private static string GetMethod(string line)
    {
        var splits = line.Trim().Split(' ');
        if (splits[^1] != "UDP" && splits[^1] != "HTTP" && splits[^1] != "Slowloris")
        {
            splits[^1] = "Slowloris"; // The default method
        }

        return splits[^1];
    }
    
    private static string TargetParse(string target)
    {
        var splits = target.Split('/');
        foreach (var split in splits)
        {
            if (Regex.IsMatch(split, ".+\\..+"))
            {
                return split;
            }
        }

        throw new ArgumentException("Error: " + target + " is not a URL");
    }
    
    public (string, string) GetNextTarget()
    {
        var target = _targets[_counter].Item1;
        var method = _targets[_counter].Item2;
        Console.WriteLine("A target given: " + target);
        _counter++;
        if (_counter == _targets.Length)
        {
            _counter = 0;
        }

        return (target, method);
    }
}