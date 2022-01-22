using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lomont.Parser
{
    /// <summary>
    /// Represent a tokenizer that splits a stream into tokens for a parser
    /// </summary>
    /// <typeparam name="TTokenType"></typeparam>
    public class Tokenizer<TTokenType> : ItemStream<Token<TTokenType>, int> where TTokenType : IComparable
    {
        /// <summary>
        /// Match a token type to a regular expression
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="RegularExpression"></param>
        /// <param name="Exact"></param>
        public record TokenMatch(TTokenType Type,string RegularExpression, IList<string> Exact = null);

        /// <summary>
        /// Make match from a regular expression definition
        /// </summary>
        /// <param name="type"></param>
        /// <param name="regularExpression"></param>
        /// <param name="Exact"></param>
        /// <returns></returns>
        public static TokenMatch Regex(TTokenType type, string regularExpression)
            => new TokenMatch(type, regularExpression, null);
        
        /// <summary>
        /// Make a match to a list of strings
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Exact"></param>
        /// <returns></returns>
        public static TokenMatch List(TTokenType Type, IList<string> Exact)
            => new TokenMatch(Type, null, Exact);

        //public List<string> Messages { get;  } = new List<string>();
        public Action<string> Logger { get;  }

        bool HasFatal = false;
        void Fatal(string msg)
        {
            HasFatal = true;
            Logger(msg);
        }

        /// <summary>
        /// Create a tokenizer
        /// </summary>
        /// <param name="tokenMatchers"></param>
        /// <param name="stream"></param>
        public Tokenizer(
            IList<TokenMatch> tokenMatchers,
            string textToTokenize,
            Action<string> logger = null            
            )
        {
            Logger = logger;
            if (Logger == null)
                Logger = s => { }; // do nothing, avoids null check

            MakeRegexes(tokenMatchers);

            chStream = new CharStream(textToTokenize);

            curPos = 0;
        }

        /// <summary>
        /// Define indentation and dedentation tokens
        /// todo - describe
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="dedent"></param>
        public void DefineIndentationTokens(
            TTokenType indent, 
            TTokenType dedent,
            TTokenType whitespace,
            TTokenType endline,
            TTokenType comment
            )
        {
            indentationToken = indent;
            dedentationToken = dedent;
            whitespaceTokenType = whitespace;
            endlineTokenType = endline;
            commentTokenType = comment;
            useIndentation = true;
        }


        public void DefineBlockComment(
            TTokenType blockComment,
            string openRegexDef = Utility.OpenCStyleBlockComment,
            string closeRegexDef = Utility.CloseCStyleBlockComment,
            bool allowNesting = false
            )
        {
            this.blockComment = blockComment;
            startBlockCommentRegex = new Regex(openRegexDef, RegexOptions.Compiled);
            endBlockCommentRegex   = new Regex(closeRegexDef, RegexOptions.Compiled);
            allowBlockCommentNesting = allowNesting;
            useBlockComents = true;
        }

        /// <summary>
        /// Are there more tokens?
        /// </summary>
        /// <returns></returns>
        public override bool More()
        {
            if (HasFatal) return false;
            if (curPos < tokens.Count)
                return true; // some already processed
            if (chStream.More())
                return true; // more in backing
            if (useIndentation && indentLevels.Count > 1)
                return true; // must unindent the item
            return false;
        }

        /// <summary>
        /// Get the next token, or null if no more.
        /// </summary>
        /// <returns></returns>
        public override Token<TTokenType> Next()
        {
            if (HasFatal)
                return null; //  emptyToken;

            // if exists, return it 
            if (curPos < tokens.Count)
                return tokens[curPos++];

            var text = chStream.Rest();

            Token<TTokenType> token = null;

            // check for block comments
            if (useBlockComents && token == null)
                token = MatchBlockComment(text);

            if (token == null)
                token = MatchMatchers(text);

            // insert process indentation tokens on token list
            if (useIndentation)
                ProcessIndentation(token);

            // add token in all cases to list. Note that indentation didn't modify curPos
            if (token != null)
            {
                tokens.Add(token);
                curPos++;
            }

            if (useIndentation && curPos-1 < tokens.Count)            
                token = tokens[curPos-1]; // this needed since ProcessIndentation could have inserted tokens into the stream

            // more to parse, no token found
            if (token == null)
            {
                var cc = chStream.More();
                if (cc)
                {
                    var nxt = chStream.Rest().Take(10).ToArray();
                    var str = new string(nxt);
                    Fatal($"Unknown token at {chStream}. Characters left cannot be tokenized. Maybe a tab character? Next: {nxt}");
                    return null; // emptyToken;
                }
            }
            return token;

            //if (curPos - 1 >= tokens.Count)
            //{
            //    // todo = why is this here?
            //    throw new NotImplementedException("");
            //    // add an empty token, return it
            //    //var pos = chStream.Pos();
            //    //var emptyToken = new Token<TTokenType>("", emptyTokenType, pos, pos);
            //    //tokens.Add(emptyToken);
            //}
            //
            //return tokens[curPos - 1];
        }

        #region Implementation

        #region Block comments
        bool useBlockComents = false;
        bool allowBlockCommentNesting = false;
        TTokenType blockComment;
        Regex startBlockCommentRegex, endBlockCommentRegex;

        Token<TTokenType> MatchBlockComment(string text)
        {
            var m = startBlockCommentRegex.Match(text);
            if (!m.Success || m.Index != 0)
                return null; // not a block comment

            // determine these token text for this block comment
            string tokenText;

            if (!allowBlockCommentNesting)
            { // first end is actual end
              // get first end if it exists
                var e = endBlockCommentRegex.Match(text, m.Value.Length);
                if (!e.Success)
                    throw new Exception($"Block comment unterminated {Pos()}");
                tokenText = text[0..(e.Index + e.Value.Length)];
            }
            else
            {

                var depth = 1; // nesting depth
                var index = m.Value.Length; // pos to start at
                while (depth > 0)
                {
                    // there has to be another end, else fatal error
                    var nextE = endBlockCommentRegex.Match(text, index);
                    if (!nextE.Success)
                        throw new Exception($"Block comment unterminated {Pos()}");

                    var nextS = startBlockCommentRegex.Match(text, index);
                    if (nextS.Success && nextS.Index < nextE.Index)
                    { // next item was a start
                        depth++;
                        index = nextS.Index + nextS.Value.Length;
                    }
                    else
                    { // nest item was an end
                        depth--;
                        index = nextE.Index + nextE.Value.Length;
                    }
                }

                Debug.Assert(depth == 0); // should be the case
                tokenText = text[0..index];
            }

            var startPos = chStream.Pos();
            chStream.Consume(tokenText.Length);
            var endPos = chStream.Pos();
            return new Token<TTokenType>(tokenText, blockComment, startPos, endPos);
        }
        #endregion

        #region Indentation

        bool useIndentation = false;
        TTokenType indentationToken, dedentationToken;
        TTokenType commentTokenType;
        TTokenType whitespaceTokenType;
        TTokenType endlineTokenType;

        // call before this new token is issued, handles the Indent and Dedent tokens
        List<int> indentLevels = new List<int> { 0 }; // start at zero depth
        // passing null adds dedents to the list until empty
        void ProcessIndentation(Token<TTokenType> token)
        {
            // first, ignore comments for this process

            // if this token is not whitespace, and (prev was whitespace and prev before was endline) or (prev was endline), then
            // do indent/dedent calculations
            if (token == null)
            {
                curPos++; // last already fed
                while (indentLevels.Count > 1)
                {
                    var pos1 = new CharPosition(-1, -1, -1);
                    // add dedents
                    var dedent = new Token<TTokenType>("", dedentationToken, pos1, pos1);
                    tokens.Add(dedent);
                    indentLevels.RemoveAt(indentLevels.Count - 1);
                }
                return;
            }

            if (token.Type.CompareTo(whitespaceTokenType)==0|| token.Type.CompareTo(commentTokenType)==0 || token.Type.CompareTo(endlineTokenType)==0)
                return; // no change

            // get prev non-comment token, which must be whitespace or endline
            var pos = curPos - 1; // last index
            while (pos > 0 && tokens[pos].Type.CompareTo(commentTokenType)==0)
                pos--;
            if (pos < 0) return;
            var prevToken = tokens[pos];
            pos--;
            if (prevToken.Type.CompareTo(whitespaceTokenType)!=0 && prevToken.Type.CompareTo(endlineTokenType)!=0)
                return; // no change

            var len = 0; // assume was endline
            if (prevToken.Type.CompareTo(endlineTokenType)!=0)
            {
                len = prevToken.Text.Length;
                // get prev non-comment token before that, which must be endline
                while (pos > 0 && tokens[pos].Type.CompareTo(commentTokenType)==0)
                    pos--;
                if (pos < 0) return;
                var endlineToken = tokens[pos];
                if (endlineToken.Type.CompareTo(endlineTokenType)!=0)
                    return; // no change
            }

            var curIndent = indentLevels.Last();
            if (len > curIndent)
            { // indent
                indentLevels.Add(len);
                var indent = new Token<TTokenType>("", indentationToken, token.Start, token.Start);
                tokens.Add(indent);
            }
            else if (len < curIndent)
            { // undent
                while (len < curIndent)
                {
                    indentLevels.RemoveAt(indentLevels.Count - 1);
                    var dedent = new Token<TTokenType>("", dedentationToken, token.Start, token.Start);
                    tokens.Add(dedent);
                    curIndent = indentLevels.Last();
                }
            }
        }

        #endregion

        #region Misc
        /// <summary>
        /// The source of characters
        /// </summary>
        readonly CharStream chStream;

        // check prefix orders are ok
        bool CheckList(IList<string> items)
        {
            // sanity check: throw if any token a prefix of a later one, must reverse them
            if (items != null)
            {
                var size = items.Count;
                for (var i = 0; i < size; ++i)
                {
                    var early = items[i];
                    for (var j = i + 1; j < size; ++j)
                    {
                        var late = items[j];
                        if (late.StartsWith(early))
                        {
                            Fatal($"operators {early} and {late} in wrong munch order in tokenizer. Reverse their order.");
                            return false;
                        }
                    }
                }
            }

            // 
            // todo - if (chStream.Rest().Contains("\t"))
            // todo - {
            // todo -     Fatal("Source contains tab characters. Replace withspaces."); //todo - nicer error code?
            // todo -     return false;
            // todo - }
            return true;
        }

        
        // tokens store here
        // todo - make more mem efficient later
        readonly List<Token<TTokenType>> tokens = new();
        #endregion

        #region Token matching
        /// <summary>
        /// The ordered list of token matching rules
        /// </summary>
        readonly List<(TTokenType type, Regex regularEpxression, IList<string> exact)> tokenMatchers = new();

        /// <summary>
        /// Convert token match rules into actionable items
        /// </summary>
        /// <param name="tokenMatchers"></param>
        void MakeRegexes(IList<TokenMatch> tokenMatchers)
        {
            foreach (var (t, s, e) in tokenMatchers)
            {
                if (!String.IsNullOrEmpty(s))
                {
                    // match start of next text with regular expression
                    var s2 = s.StartsWith("^")
                        ? s
                        : "^" + s;
                    var r = new Regex(s2, RegexOptions.Compiled);
                    this.tokenMatchers.Add(new(t, r, e));
                }
                else // match against list
                {
                    // check list for prefix errors
                    if (!CheckList(e))
                        throw new ArgumentException("Invalid munch order");
                    this.tokenMatchers.Add(new(t, null, e));
                }
            }
        }

        Token<TTokenType> MatchMatchers(string text)
        {
            foreach (var (t, r, e) in tokenMatchers)
            {
                if (r != null)
                {
                    if (r.IsMatch(text))
                    { // matched a token, extract and return it
                        var tokenText = r.Match(text).Value;
                        var start = chStream.Pos();
                        var end = chStream.Consume(tokenText.Length);
                        return new Token<TTokenType>(tokenText, t, start, end);
                    }
                }
                else
                {
                    foreach (var m in e)
                    {
                        if (text.StartsWith(m))
                        {
                            // matched a token, extract and return it
                            var tokenText = m;
                            var start = chStream.Pos();
                            var end = chStream.Consume(tokenText.Length);
                            return new Token<TTokenType>(tokenText, t, start, end);
                        }
                    }
                }
            }
            return null; // no match
        }


        #endregion

        #endregion

    }
}

