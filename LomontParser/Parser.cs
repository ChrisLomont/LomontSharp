using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lomont.Parser
{

    public abstract class Parser<TTokenType,AType> where TTokenType : IComparable
    {
        #region Abstracted out


        // the place to start parsing
        ParserDelegate startParser;
        IList<TTokenType> tokensToIgnore;

        #region expression parser needs
        struct ExpressionNeeds
        {
            public OpDef[] ops; // todo - set from external

            // how to parse function call parameters
            public ParserDelegate ParseParams;
            // how to parse array call parameters
            public ParserDelegate ParseArrayParams;

            // how to parse constants in expression
            public ParserDelegate ParseConstant;

            // is token terminal for expression? 
            // id, string, etc... basically constant?
            public IsTerminalDelegate IsTerminal;
            public StartExprFuncArrayDelegate StartFuncCall, StartArray;
            public string endFuncCall, endArray;

        }

        public delegate bool StartExprFuncArrayDelegate(Token<TTokenType> token, string opText);
        public delegate bool IsTerminalDelegate(Token<TTokenType> token);

        ExpressionNeeds expressionNeeds;

        protected void UseExpressionParser(
            OpDef[] ops,
            IsTerminalDelegate isTerminalDelegate
            // todo - more
            )
        {
            expressionNeeds = new ExpressionNeeds();
            expressionNeeds.ops = ops;
            
            // default to C style
            expressionNeeds.StartFuncCall = (token, opText) => opText == "(";
            expressionNeeds.endFuncCall = ")";
            expressionNeeds.StartArray = (token, opText) => opText == "[";
            expressionNeeds.endArray = "]";
            expressionNeeds.IsTerminal = isTerminalDelegate;
        }
        #endregion


        #endregion

        // todo - rename to start, or something, call in overriden version


        // set these before parsing
        //protected AType programType;
        //protected AType exprType;
        //protected AType defaultValueType; // todo - remove, add proper parsing 

        //public AST<AType> ParseFile(string filename, Logger logger) 
        //{
        //    if (!File.Exists(filename))
        //    {
        //        logger.Error($"File {filename} foes not exist");
        //        return null;
        //    }
        //    return ParseText(File.ReadAllText(filename), logger);
        //}
        //
        //public AST<AType> ParseText(string text, Logger logger)
        //{
        //    var ch = new CharStream(text);
        //    var t = new Tokenizer<TTokenType>();
        //    return Parse(t,logger);
        //}

        // todo - try to move stuff into constructor or Parse command
        //
        // logger taken from : here, else tokenizer
        protected void Init(
            ParserDelegate startProgramParser,
            AType startNodeType,
            IList<TTokenType> tokensToIgnore, // comments, whitespace,
            Tokenizer<TTokenType> tokenizer,
            Action<string> logger = null
            )
        {
            this.startNodeType = startNodeType;
            this.startParser = startProgramParser;
            this.tokensToIgnore = tokensToIgnore;
            this.tokenizer = tokenizer;
            Logger = logger;
            if (Logger == null)
                Logger = tokenizer.Logger;
        }
        Tokenizer<TTokenType> tokenizer;

        public Action<string> Logger { get; private set; }

        AType startNodeType;


        // start parsing on this tokenizer
        // throws on fatal errors
        // return abstract syntax tree, or null if errors
        // other errors on some output? (TODO) - make elsewhere
        public AST<AType> Parse()
        {
            try
            {
                // set up initial state on stack
                parseStack.Push(new ParseState(startNodeType, tokenizer));

                //// dump how parser sees tokens
                //while (PeekToken().Type != TokenType.Empty)
                //{
                //    var tt = NextToken();
                //    errorWriter.WriteLine(tt);
                //}
                //return null;

                var tree = startParser();

                // todo  - check post conditions: tokenizers out, stack out.. more?
                

                var stackDepth = parseStack.Count();
                var t = PeekToken();

                if (stackDepth != 1 || t != null)
                {
                    Error($"ERROR: at {t} - could not parse");
                    //cout << "Parse stack is depth " << stackDepth << endl;
                    //cout << "Tokenizer type is " << (int)(t.Type) << endl;
                    //cout << "Last error: " << lastErrorMessage << endl;
                }

                // todo - error if tokenizer type is not empty
                // todo - error if any more on stack

                // output error messages
                foreach (var msg in state().errorMessages)
                    Logger(msg);

                return tree;
            }
            catch (Exception ex)
            {
                Logger($"Fatal exception: {ex}");
            }
            return null;
        }

        #region Parser state
        /************************ parser state ************************/

        // set as bottom most error on first unparseable branch
        string lastErrorMessage;

        class ParseState
        { // todo - make struct

            public ParseState(AType type, Tokenizer<TTokenType> tokenizer)
            {
                this.tokenizer = tokenizer;
                tokenizerPos = tokenizer.Pos();

                CharPosition pos = new CharPosition(-1, -1, -1);
                // todo - get pos?
                node = new AST<AType>(type, pos);
            }
            public Tokenizer<TTokenType> tokenizer;
            public int tokenizerPos;
            public AST<AType> node;
            public List<string> errorMessages = new List<string>();
            public Token<TTokenType> lastToken = null;
        };

        Stack<ParseState> parseStack = new Stack<ParseState>();

        // any local errors?
        protected bool HasError()
        {
            return state().errorMessages.Any();
        }

        // always get a token, token type is Empty when no more left
        // skips white space and comments
        // handles multiple file parsing
        protected Token<TTokenType> NextToken()
        {
            SkipIgnoredTokens(state().tokenizer);
            state().lastToken = state().tokenizer.Next();
            Debug.Write($" [{state().lastToken?.Text}] ");
            return state().lastToken;
        }


        // peek at a next token        
        // skips white space and comments
        // always get a token, token type is Empty when no more left
        // handles multiple file parsing
        protected Token<TTokenType> PeekToken()
        {
            SkipIgnoredTokens(state().tokenizer);
            return state().tokenizer.Peek();
        }

        // last token from Next formatted for error messages
        string LastTokenErrorText()
        {
            return $"{state().lastToken}";
        }

        // set value of current node
        protected void SetNodeValue(string value)
        {
            state().node.Value = value;
        }

        // add child node to current node
        void AddChild(AST<AType> node)
        {
            var parent = state().node;
            parent.AddChild(node);
        }

        // detach this from bottom of current tree
        // must exist, else error
        // return true if detached
        bool Detach(AST<AType> node)
        {
            if (node == null)
                return false;
            var parent = state().node;
            return parent.RemoveChild(node);
        }


        // get active state
        ParseState state() { return parseStack.Peek(); }
#endregion


        /************************ utility *****************************/

        // consume any tokens that should be ignored
        void SkipIgnoredTokens(Tokenizer<TTokenType> tokenizer)
        {

            if (!tokenizer.More())
                return;

            var t1 = tokenizer.Peek().Type;
            while (tokensToIgnore.Contains(t1))
            {
                tokenizer.Consume();
                t1 = tokenizer.Peek().Type;
            }
            return;

            // todo - remove below

            // void SkipWhitespace()
            // {
            //     var t1 = tokenizer.Peek().Type;
            //     while (t1 == TokenType.Comment || t1 == TokenType.Whitespace)
            //     {
            //         tokenizer.Consume();
            //         t1 = tokenizer.Peek().Type;
            //     }
            // }
            // 
            // 
            // 
            // SkipWhitespace();
            // 
            // var t = tokenizer.Peek().Type;
            // while (t == TokenType.Endline)
            // {
            //     tokenizer.SavePosition();
            //     tokenizer.Next(); // skip endline
            //     SkipWhitespace(); // remove more comments and spaces
            //     t = tokenizer.Peek().Type; // now where are we?
            //     if (t == TokenType.Endline)
            //         tokenizer.DiscardPosition(); // we have a blank line
            //     else
            //     {
            //         tokenizer.RestorePosition();
            //         break;
            //     }
            // }


        }

        #region Combinators

        /************************ combinators *************************/
        protected delegate AST<AType> ParserDelegate();


        // large int for max times to apply something in practice
        protected static int infinity = int.MaxValue;

        // start a new state of type
        protected void Start(AType type)
        {
            // Start a new parse state:
            // 1. save current node being worked on
            // 2. save tokenizer position
            // 3. Create new node to work on of requested type
            Debug.Assert(state() != null);
            parseStack.Push(new ParseState(type, state().tokenizer));
            state().tokenizer.SavePosition();

            Debug.Write($"Try parse {type}...");

        }

        // finish this parse state, return node if good
        protected AST<AType> Finish(string errorMsg)
        {
            // 1. get current state, pop from stack
            // 2. if no errors, return proper tree node in retval
            // 3. if errors
            //    a. consume a token to advance parse
            //    b. return retval with errors

            var s = state();
            Debug.Assert(s != null);
            parseStack.Pop(); // remove state

            var node = s.node;

            if (!s.errorMessages.Any())
            { // no errors, link node to parent
                Debug.WriteLine($"... {node.Type} success");
                s.tokenizer.DiscardPosition();
                var parent = state().node;
                parent.AddChild(node);
                lastErrorMessage = "";
            }
            else
            { // errors, restore tokenizer, move errors up
                Debug.WriteLine($"... {node.Type} fail");
                s.tokenizer.RestorePosition();
                //for (var & e : s.errorMessages)
                //	state().errorMessages.push_back(e);
                lastErrorMessage = s.errorMessages.First();
                node = null;
            }

            return node;
        }

        // set state error message
        protected void Error(string errorMessage)
        {
            state().errorMessages.Add(LastTokenErrorText() + errorMessage);
        }


        // apply func between [min,max] inclusive times to state
        protected void Save(ParserDelegate func, int minCount = 1, int maxCount = 1)
        {
            if (state().errorMessages.Any()) return;

            var count = 0;
            while (count < maxCount)
            {
                var node = func();
                if (node != null)
                { // func above already added child
                  // todo - remove this branch
                  //AddChild(node);
                }
                else if (minCount <= count)
                    return;
                else
                {
                    Error("Expected more of TODO");
                    return;
                }
                ++count;
            }
            return;
        }

        // save a node of this type and token
        protected void Save(AType type, string token)
        {
            // todo - get file pos, or move construction elsewhere
            CharPosition pos = new CharPosition(-1, -1, -1);
            var node = new AST<AType>(type, pos);
            node.Value = token;

            // parse state
            var parent = state().node;
            parent.AddChild(node);
        }

        protected AST<AType> ProcessLiteral(AType aType, TTokenType tType, Regex regex = null)
        {
            Start(aType);
            var t = NextToken();
            if (t != null && t.Type.CompareTo(tType)==0 && (regex == null || regex.IsMatch(t.Text)))
                SetNodeValue(t.Text);
            else
                Error($"Expected literal {aType}");
            return Finish($"Expected literal type {tType}");
        }

        // read and discard token. Update state
        protected void Discard(string text)
        {
            if (state().errorMessages.Any()) return;

            // next is given text, else error
            var t = NextToken();
            if (t == null || t.Text != text)
            {
                Error($"Expected token {text}");
                return;
            }
            return;
        }

        // read and discard token type
        protected void Discard(TTokenType type, int minCount = 1, int maxCount = 1)
        {
            if (state().errorMessages.Any()) return;

            var count = 0;
            while (count < maxCount)
            {
                var t = PeekToken();
                if (t?.Type.CompareTo(type) != 0)
                    break;
                NextToken();
                ++count;

                if (minCount <= count)
                    return;
            }
            if (count < minCount)
            {
                Error($"Expected token type {type}");
                return;
            }

            return;
        }

        // get exactly one of
        protected void OneOf(params ParserDelegate[] funcs)
        {
            if (state().errorMessages.Any()) return;

            foreach (var func in funcs)
            {
                var ret = func();
                if (ret != null)
                    return;
            }
            Error("Could not parse choice");
            return;
        }

        // while next token is next, discard it and then apply func to state
        protected void WhileNext(string text, ParserDelegate func)
        {
            if (state().errorMessages.Any()) return;

            while (PeekToken().Text == text)
            {
                NextToken();
                var ret = func();
                if (ret == null) // better have one of these
                {
                    Error($"Missing item in parsing after {text}");
                    return;
                }
            }
            return;
        }
#endregion

#region expression handler
        /************************ expression handler ******************/

        // expressions are handled by a precedence climbing parser
        // much cleaner and faster than trying to fit a grammar
        // see https://www.engr.mun.ca/~theo/Misc/exp_parsing.htm
        // see also operator precedence in C++ https://en.wikipedia.org/wiki/Operators_in_C_and_C%2B%2B

        protected enum Assoc
        {
            L2R,
            R2L
        };
        protected enum Arity
        {
            Binary,
            Unary
        };
        protected struct OpDef
        {
            public OpDef(int precedence, string text, Assoc assoc, Arity arity, AType aType)
            {
                prec = precedence;
                this.text = text;
                this.assoc = assoc;
                this.arity = arity;
                Type = aType;
            }
            public int prec;    // precedence
            public string text; // token text
            public Assoc assoc; // associativity
            public Arity arity; // arity 1 or 2
            public AType Type;  // type to put on node
        };



        bool IsBinary(Token<TTokenType> t)
        {
            foreach (var op in expressionNeeds.ops)
            {
                if (op.text == t.Text && op.arity == Arity.Binary)
                    return true;
            }
            return false;
        }

        // "ToBinary" converts a token matched by B to an operator.
        OpDef ToBinary(Token<TTokenType> t)
        {
            foreach (var op in expressionNeeds.ops)
            {
                if (op.text == t.Text && op.arity == Arity.Binary)
                    return op;
            }
            throw new Exception($"Missing op {t.Text}");
        }

        bool IsUnary(Token<TTokenType> t)
        {
            foreach (var op in expressionNeeds.ops)
            {
                if (op.text == t.Text && op.arity == Arity.Unary)
                    return true;
            }
            return false;
        }
        // "ToUnary" converts a token matched by U to an operator.
        OpDef ToUnary(Token<TTokenType> t)
        {
            // todo - merge IsUnary/IsBinary and ToUnary/ToBinary
            foreach (var op in expressionNeeds.ops)
            {
                if (op.text == t.Text && op.arity == Arity.Unary)
                    return op;
            }
            throw new Exception($"Missing op {t.Text}");
        }


        // "MakeNode" takes an operator and one or two trees and returns a tree.
        AST<AType> MakeNode(OpDef op, AST<AType> left, AST<AType> right = null)
        {
            CharPosition pos = new CharPosition(-1, -1, -1); // todo - find all these -1,-1,-1, get better values
            var n = new AST<AType>(
                op.Type,
                //op.arity == Arity.Binary ? binaryOpType : unaryOpType, 
                pos);
            n.Value = op.text;
            n.AddChild(left);
            if (right != null)
                n.AddChild(right);
            return n;
        }

        // todo ?
        // "MakeLeaf" takes a token text and makes a node
        // AST<AType> MakeLeaf(string text)
        // {
        //     CharPosition pos = new CharPosition(-1, -1, -1);
        //     // todo - get correct item, add ParseConstant
        //     var n = new AST<AType>(defaultValueType, pos);
        //     n.Value = text;
        //     return n;
        // }
        // is token terminal for expression? 
        // id, string, etc... basically constant?
        //bool IsTerminal(Token<TTokenType> token)
        //{
        //    return TokenHelper.IsTerminal(token.Type);
        //}


        /// <summary>
        /// Call from derived to handle expression parsing using TODO
        /// </summary>
        /// <returns></returns>
        protected AST<AType> ExecuteExpressionHelper(AType exprType)
        {
            Start(exprType);

            var n = Expression(0);

            if (!state().errorMessages.Any())
            {
                var parent = state().node;
                parent.AddChild(n);
            }

            return Finish("Expected expression");
        }

        AST<AType> Expression(int p)
        {
            var t = P();
            var peek = PeekToken();
            while (IsBinary(peek) && ToBinary(peek).prec >= p)
            {
                var op = ToBinary(peek);
                NextToken();

                // todo - abstract better
                if (expressionNeeds.StartFuncCall(peek,op.text))
                { // func call
                  // todo - move up, call list of 0 or more expressions
                    var t1 = expressionNeeds.ParseParams();
                    if (!String.IsNullOrEmpty(expressionNeeds.endFuncCall))
                        Discard(expressionNeeds.endFuncCall);
                    if (t1 != null)
                    {
                        Detach(t1);
                    }
                    t = MakeNode(op, t, t1);
                }
                else
                {
                    var q = op.assoc == Assoc.R2L ? op.prec : 1 + op.prec;
                    var t1 = Expression(q);
                    if (expressionNeeds.StartArray(peek,op.text))                        
                    { // deref map/array
                        if (!string.IsNullOrEmpty(expressionNeeds.endArray))
                            Discard(expressionNeeds.endArray);
                        //todo - pack node
                    }
                    t = MakeNode(op, t, t1);
                }
            }
            return t;
        }

        AST<AType> P()
        {
            //todo - ToUnary () needs func and grouping...
            //todo unary []
            var peek = PeekToken();
            if (IsUnary(peek))
            {
                var op = ToUnary(peek);
                NextToken();
                var t = Expression(op.prec);
                return MakeNode(op, t);
            }
            else if (peek.Text == "(")
            {
                NextToken();
                var t = Expression(0);
                Discard(")");
                return t;
            }
            //else if (peek.token_ == "[")
            //{ // array deref
            //    NextToken();
            //    var t = Expression(0);
            //    Discard("]");
            //    return t;
            //}
            else if (expressionNeeds.IsTerminal(peek))
            {
                var t = expressionNeeds.ParseConstant();
                // t got attached to wrong place in tree by parser
                if (t != null && Detach(t))
                    return t;
            }
            Error("Expression error");
            return null;
        }
#endregion
    }
}

