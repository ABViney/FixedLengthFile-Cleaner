using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

// Turns wildcard patterns into valid regular expressions
public class PatternMatcher
{
    private readonly Configuration _configuration;
    private readonly Regex[] _patterns;

    public bool Matches(string input)
    {
        return _patterns.Any(p => p.IsMatch(input));
    }
    
    public PatternMatcher(Configuration configuration)
    {
        _configuration = configuration;
        _patterns = CreateFromPatterns(configuration.ExcludePatterns);
    }
    
    private Regex[] CreateFromPatterns(params string[] patterns)
    {
        return patterns.Select(pattern => CreateFromPattern(pattern)).ToArray();
    }
    
    private Regex CreateFromPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            throw new ArgumentException("Pattern must be a non-empty string", nameof(pattern));
        }
        
        StringBuilder regexPattern = new StringBuilder();
        
        regexPattern.Append('^');
        foreach (char character in pattern)   
        {
            if (character == '*')
            {
                regexPattern.Append(".*");
            }
            else if (character == '?')
            {
                regexPattern.Append(".");
            }
            else
            {
                regexPattern.Append(character);
            }
        }
        regexPattern.Append('$');

        return new Regex(regexPattern.ToString());
    }

}