// Scan files looking for certain patterns
// this allows me to find all occurrences of ??? to merge files for inclusion to my library
// idea is to move copies to one place, merge them

using System;
using System.IO;
using System.Text.RegularExpressions;

//ReflectionHelper.SeekSimilarText();
//return;


var path = @"C:\Users\Chris\OneDrive\Code";
//var path = "H:\\Development\\Code";

var outPath = @"FileCopies1234";
//var outPath = @"FileCopies1234";

var exts = new[]{"*.cs","*.fs","*.c","*.cpp"};
//var exts = new[] { "*.cs", "*.fs" };

// var pat = "class.+[Vv]ec"; // vec3, vector, etc...
// var pat = " [Ss]huffle";
//var pat = "[^a-zA-Z][Dd][Dd][Aa][^a-zA-Z]";
//var pat = "[^a-zA-Z][Ff][Ff][Tt][^a-zA-Z]";
var pat = "[a]lpha blend";
//var ignore = "[Aa]pply|[Mm]ultiply|reply|simply|MULTIPLY|supply";
var ignore = "";
var copy = false;

// CIEColor, SOlid_Color, Colors, ColorPicker/Converter/etc, NColor,dir

Console.WriteLine("Searching files");

var pattern = new Regex(pat,RegexOptions.Compiled);
var ignoreRegex = String.IsNullOrEmpty(ignore)?null:new Regex(ignore, RegexOptions.Compiled);

var dst = Path.Combine(path, outPath);
if (!Directory.Exists(dst))
    Directory.CreateDirectory(dst);

int count = 0;
int linecounter = 0, matchCount = 0;
foreach (var ext in exts)
foreach (var filename in Directory.EnumerateFiles(path, ext, SearchOption.AllDirectories))
{
    if (Path.GetDirectoryName(filename) == dst)
        continue;
    //Console.WriteLine(filename);
    //Console.GetCursorPosition()
    var matchingLine = "";
    var matches = false;
    foreach (var line in File.ReadAllLines(filename))
    {
        linecounter++;
        if (pattern.IsMatch(line) && (ignoreRegex == null || !ignoreRegex.IsMatch(line)))
        {
            ++matchCount;
            matchingLine = line;
            matches = true;
            break;
        }
    }

    if (matches)
    {
        ++count;
        Console.WriteLine($"{filename} => {matchingLine}");
        if (copy)
        {
            var d = $"{dst}\\{Path.GetFileNameWithoutExtension(filename)}_{count}.cs";
            //Console.WriteLine(d);
            File.Copy(filename, d);
        }
    }
}

Console.WriteLine($"{linecounter} lines counted, {matchCount} matches");

