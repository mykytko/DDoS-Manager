using System.Text.RegularExpressions;

namespace DDoS_Manager;

public class TargetSupplier
{
    
    private readonly string[] _targets;
    private int _counter;

    public TargetSupplier(string filename)
    {
        _targets = File.ReadAllLines(filename);
        for (var i = 0; i < _targets.Length; i++)
        {
            _targets[i] = TargetParse(_targets[i]);
        }
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
    
    public string GetNextTarget()
    {
        var target = _targets[_counter];
        Console.WriteLine("A target given: " + target);
        _counter++;
        if (_counter == _targets.Length)
        {
            _counter = 0;
        }

        return target;
    }
}