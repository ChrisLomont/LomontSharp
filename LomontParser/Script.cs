using Lomont.Parser.Example;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if false // todo

namespace Lomont.Parser
{
    using OurAST = AST<OurAstType>;

    /// <summary>
    /// Manage a script - parsing, tokenization, execution, etc.
    /// </summary>
    class Script <TTokenType>
    {
        private OurAST syntaxTree;
        private Logger logger;
        private OurTokenizer tokenizer;
        private OurParser parser;
        private Optimizer<OurAstType> optimizer;
        private Evaluator eval;

        // return our ast, our tokenizer, and an error string
        public static (OurAST, List<Token<TTokenType>>, string) Test()
        {
            return (null, null, ""); // todo - reinstate
            var path = @"..\..\Model\FinScript\Tests\";
            //var filename = path + "test.txt";
            var filename = path + "Expressions.txt";
            var text = File.ReadAllText(filename);
            var sc = new Script<TTokenType>();
            return sc.Load(text);
        }

        List<Token<TTokenType>> CheckRoundTrip(string script)
        {
            var tokens = new List<Token<TTokenType>>();
            var stream = new CharStream(script);
            tokenizer = new OurTokenizer(stream, logger);
            var sb = new StringBuilder(); // for sanity check
            var pos = new CharPosition(-1, -1, -1);
            while (tokenizer.More())
            {
                var t = tokenizer.Next();
                sb.Append(t.Text);
                tokens.Add(t);
                Debug.WriteLine(t);
            }

            Debug.Assert(sb.ToString() == script); // check roundtrippable
            return tokens;
        }

        // return our ast, our tokenizer, and an error string
        public (OurAST, List<Token<TTokenType>>, string) Load(string script)
        {
            syntaxTree = new OurAST(OurAstType.Assign, new CharPosition(-1, -1, -1));
            logger = new Logger();
            optimizer = new Optimizer<OurAstType>();

            var tokens = CheckRoundTrip(script);

            // try
            {

                //Debug.WriteLine(sb.ToString());


                // now parse it 
                logger = new Logger();
                var stream = new CharStream(script);
                tokenizer = new OurTokenizer(stream, logger);
                parser = new OurParser();
                syntaxTree = parser.Parse(tokenizer, logger);
                syntaxTree = optimizer.Optimize(syntaxTree, logger);
                eval = new Evaluator();
                syntaxTree = eval.Evaluate(syntaxTree, logger);

                Debug.WriteLine("ERRORS: " + logger.ToString());

            }
            //catch (Exception ex)
            {
                if (syntaxTree == null)
                {
                    var pos = new CharPosition(-1, -1, -1);
                    syntaxTree = new OurAST(OurAstType.None, pos);
                }

                //errorWriter.WriteLine($"EXCEPTION: {ex}");
            }
            return (syntaxTree, tokens, logger.ToString());
        }
    }
}
#endif