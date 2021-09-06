// Scan files looking for certain patterns
// this allows me to find all occurrences of ??? to merge files for inclusion to my library
// idea is to move copies to one place, merge them

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

//ReflectionHelper.SeekSimilarText();
//return;


var path = @"C:\Users\Chris\OneDrive\Code";
//var path = "H:\\Development\\Code";
//var path = "L:\\Programming\\Code2";

var outPath = @"FileCopies1234";
//var outPath = @"FileCopies1234";

var exts = new[]{"*.cs","*.fs","*.c","*.cpp","*.h","*.hpp"};
//var exts = new[] { "*.cs", "*.fs" };
//var exts = new[] { "*.cs" };

// var pat = "class.+[Vv]ec"; // vec3, vector, etc...
// var pat = " [Ss]huffle";
//var pat = "[^a-zA-Z][Dd][Dd][Aa][^a-zA-Z]";
//var pat = "[^a-zA-Z][Ff][Ff][Tt][^a-zA-Z]";
//var pat = "\\.[Pp][Pp][Yy]\\\"";
//var pat = "[Qq]uat[^ica]";
//var pat = "[Kk][Dd].+[Tt]ree";
var pat = "[Ff]ont";
//var ignore = "(MeshSplit|[Mm]esh[CT_])";
//var ignore = "(FontColor|FontChange|[ .;]font[ .;]|FontSize|Drawing\\.font|SoundFont|font-style|FontWeight)";
var ignore = "";
// var pathIgnore = "freetype|imgui|cairo|BORLANDC|stb|Antigrain|dosbox|openexr";
var pathIgnore = "";
var copy = false;

// ignore FontColor,FontSize,FontChange, _font_, Drawing.Font,SoundFont,FontIcon,font-style,FontWeight, _font., .font;font_freetype, _font_

var fileMatchFilter = "";
//var fileMatchFilter = "PropertyObserver";

// find:
// 

// find StatRecorder

// CIEColor, SOlid_Color, Colors, ColorPicker/Converter/etc, NColor,dir

Console.WriteLine("Searching files");

var pattern = new Regex(pat,RegexOptions.Compiled);
var ignoreRegex = String.IsNullOrEmpty(ignore)?null:new Regex(ignore, RegexOptions.Compiled);
var pathRegex = new Regex(pathIgnore, RegexOptions.Compiled);

var dst = Path.Combine(path, outPath);
if (!Directory.Exists(dst))
    Directory.CreateDirectory(dst);

int count = 0,extIndex = Directory.GetFiles(dst).Length+1;
int linecounter = 0, matchCount = 0, fileCounter = 0;
foreach (var ext in exts)
    //    foreach (var dir in Directory.GetDirectories("g:\\"))
    //        if (!dir.Contains("System Volume"))
    //foreach (var filename in Directory.EnumerateFiles(dir, ext, SearchOption.AllDirectories))
foreach (var filename in Directory.EnumerateFiles(path, ext, SearchOption.AllDirectories))
{
    try
    {
        if (!String.IsNullOrEmpty(pathIgnore) && pathRegex.IsMatch(filename))
            continue;
        if (Path.GetDirectoryName(filename) == dst)
            continue;
        if (!filename.Contains(fileMatchFilter))
            continue;
        //Console.WriteLine(filename);
        //Console.GetCursorPosition()
        var matchingLine = "";
        var matches = false;
        ++fileCounter;
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
                var ext1 = Path.GetExtension(filename);
                var d = $"{dst}\\{Path.GetFileNameWithoutExtension(filename)}_{extIndex}{ext1}";
                extIndex++;
                //Console.WriteLine(d);
                File.Copy(filename, d);
            }
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Exception: {ex}");
    }
}

Console.WriteLine($"{fileCounter} files searched, {linecounter} lines counted, {matchCount} matches");

