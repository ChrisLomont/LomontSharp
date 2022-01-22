using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Parser.Example
{

    // aliases to simplify code
    using TT = Example.TokenType;
    using TM = Tokenizer<Example.TokenType>.TokenMatch;
    using T  = Tokenizer<Example.TokenType>;

    /// <summary>
    /// Example parser
    /// </summary>
    public class Example
    {

        public void Test(bool testTokenizer)
        {
            // where to put output
            var output = Console.Out;

            // create an action sending messages to output
            Action<string> logger = s => output.WriteLine(s);

            // the program to parse
            var textToParse = ExamplePrograms.Program4;

            // make a tokenizer
            var tokenizer = new Tokenizer<TT>(tokenDefs, textToParse, logger);

            // enable block comments using an open and close tag
            // also set if allowed to nest them
            tokenizer.DefineBlockComment(
                TT.BlockComment,
                "/\\*", "\\*/", // regexes to match open and close block comments
                true // allow nested
                );

            // language is indentation based
            tokenizer.DefineIndentationTokens(TT.Indent, TT.Dedent, TT.Whitespace, TT.Endline, TT.Comment);

            if (testTokenizer)
            {
                output.WriteLine($"Tokens: (line:pos) positions, token names, [token text]");
                while (tokenizer.More())
                {
                    var token = tokenizer.Next();
                    output.WriteLine(token);
                }
            }
            else
            {
                var parser = new OurParser(tokenizer);
                var ast = parser.Parse();
                output.WriteLine("----------------------------");

                // dump ast tree - todo move one to Utility
                Lomont.Formats.TreeTextFormatter.Format
                    (
                    output,
                    ast,
                    a => a.Children.ToList()
                    );
            }
        }

        /// <summary>
        /// Define token types, makes parser below easier to write
        /// </summary>
        public enum TokenType
        {

            Whitespace, Endline, Comment,
            Identifier, String, Integer,
            Operator,
            BlockComment,
            Indent, Dedent,
            //Punctuator,Keyword,Literal
        }


        // operators sorted for maximal munch
        // that is, put == before =
        static string[] operators = new[] {
            "&&","||",     // boolean
            "<<",">>",     // shifting
            "++","--",     // pre/post inc/dec
            "==","!=","<=",">=","<",">", // comparison
            "=","+=","-=","*=","/=","&=","|=", // update
            "!", // boolean
            "+","-","*","/","%", // arithmetic
            "&","|","~",   // bitwise
            "(",")",   // grouping, func call
            "{","}",   // scoping
            "[","]",   // arrays, maps
            ":",       // key value separator
            ";",       // end terminator
            ",",       // parameter separator
            "."        // dereference
        };


        // Map of token type to regular expression for matching
        // match in this order
        static List<TM> tokenDefs = new()
        {
            T.Regex(TT.Whitespace, @"[ ]+"),    // whitespace, ignored by program (except possible indent and dedent)
            T.Regex(TT.Endline, @"(\r\n|\r|\n)"), // needed for some language types, needed for indent/dedent stuff
            T.Regex(TT.Comment, @"#[^\r\n]*"),    // user comment ignored by program
            T.Regex(TT.Identifier, @"[a-zA-Z_][a-zA-Z0-9_]*"), // an identifier
            T.Regex(TT.String, "'[^']*'"), // simple string, using ' as quotes
            T.Regex(TT.Integer, "[0-9]+"),          // simple integer
            T.List (TT.Operator, operators),  // all operators
        };

    }

}


