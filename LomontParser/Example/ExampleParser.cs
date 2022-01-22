using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Lomont.Parser.Example
{
    using T = Example.TokenType;

    /// <summary>
    /// Ast node types
    /// </summary>
    public enum OurAstType
    { // comments and whitespace removed by here (todo - add filtering)
        Program,     // Declaration*
        Declaration, // (Statement | FuncDef)
        Statement,   // Assignment | For | If | FuncCall | Return | Endline
        FunctionDef, // 'func' Identifier '(' IdList? ')' Endline Block
        Endline,     // T.Endline
        
        Assignment,  // Identifier '=' Expr Endline
        For,         // 'for' Identifier '=' Expr 'to' Expression Endline Block
        If,          // 'if' Expr EndLine Block
        FunctionCall,// Identifier '(' ExprList? ')'
        Return,      // 'return' Expr Endline

        IdList,      // Identifier (',' Identifier)* 
        ExprList,    // Expr (',' Expr)* 
        Block,       // T.Indent Statement+ T.Dedent Endline

        Expr,        // T.Number

        // terminal items are leaves in the AST - these are identifiers, strings, numbers
        Identifier,  // [_a-zA-Z][_a-zA-Z0-9]*
        Number,      // [0-9]+
        String,      // '[^']*'

        // simple expression things
        // a = "bob"; func(10); 34+(12/2); -123; 21+-3; f(1,2,3)*(g("a",1)-10), etc.
        //Literal,     // Number | Identifier | String | 
        //ExprMul,     // todo...
        //ExprAdd,

        /*
                None = 0,

                Id, Idlist,

                Program, Declaration, Funcdef,
                Block, Statement,
                Vardef, Assignment,

                Jump, Break, Continue, Return, Fail,

                Funccall,

                If, Else,
                While,
                For, ForContainer, ForRange,

                // TODO - add these as needed

                // literals
                String,
                Real,

                // operators and such
                OpAddEq,
                OpSubEq,
                OpMulEq,
                OpDivEq,

                OpGeq,
                OpLeq,
                OpEq,
                OpNeq,
                OpGt,
                OpLt,

                Assign,

                LParen,
                RParen,

                LBrace,
                RBrace,

                LBracket,
                RBracket,

                OpLogAnd,
                OpLogOr,
                OpLogNot,

                OpDeref,

                OpBitAnd,
                OpBitOr,
                OpBitXor,

                OpAdd,
                OpSub,
                OpMul,
                OpDiv,
                OpMod,

                OpSuffixIncr,
                OpSuffixDecr,
                OpPrefixIncr,
                OpPrefixDecr,

                OpUnaryPlus,
                OpUnaryNeg,

                OpBitNot,
                OpBitMod,

                OpShiftLeft,
                OpShiftRight,

                // for demo
                Params, Dict, DictEntry, KeyValue,
                Constant,
                Expr

                //BinaryOp, UnaryOp, Expr,
        */
    }

    /*
     
    keywords: func, for, to, if, return
    library: print
      
     */

    public static class OurLang
    {
        //public static string Func = "func";
        //public static string Var = "var";
        //public static string If = "if";
        //public static string Else = "else";
        //public static string While = "while";
        //public static string For = "for";
        //public static string In = "in";
        //public static string Break = "break";
        //public static string Continue = "continue";
        //public static string Return = "return";
        //public static string Fail = "fail";
        //public static string Assign = "=";
        //public static string AssignSeparator = ",";
        //public static string Range = "..";
        //public static string FuncCallSeparator = ",";
    }
    

    
    public class OurParser : Parser<T, OurAstType>
    {

        public OurParser(Tokenizer<T> tokenizer)
        {
            // ParseProgram not usable in parameter to base...
            Init(
                ParseProgram,OurAstType.Program, 
                new List<T> { T.Comment, T.Whitespace }, 
                tokenizer,
                null // take logger from tokenizer
                );
#if false
            // todo - make into map, or static and assign to parent
            ops = new OpDef[]
                {
                // precedence is opposite dir of wiki table, starts at 0
                // C/C++ style operators to demonstrate parsing
                new OpDef(11, "++", Assoc.L2R, Arity.Unary , OurAstType.OpSuffixIncr), // postfix
                new OpDef(11, "--", Assoc.L2R, Arity.Unary , OurAstType.OpSuffixDecr), // postfix
                new OpDef(11, "(",  Assoc.L2R, Arity.Binary, OurAstType.LParen), // func call start - have to special case the close, made Binary to get into parsing
                new OpDef(11, "[",  Assoc.L2R, Arity.Binary, OurAstType.LBrace), // array deref start - have to special case the close, made Binary to get into parsing
                new OpDef(11, ".",  Assoc.L2R, Arity.Binary, OurAstType.OpDeref), // element selection by ref
                new OpDef(10, "++", Assoc.R2L, Arity.Unary , OurAstType.OpPrefixIncr), // prefix
                new OpDef(10, "--", Assoc.R2L, Arity.Unary , OurAstType.OpPrefixDecr), // prefix
                new OpDef(10, "+",  Assoc.R2L, Arity.Unary , OurAstType.OpUnaryPlus), // ToUnary
                new OpDef(10, "-",  Assoc.R2L, Arity.Unary , OurAstType.OpUnaryNeg), // ToUnary
                new OpDef(10, "!",  Assoc.R2L, Arity.Unary , OurAstType.OpLogNot), // logical NOT
                new OpDef(10, "~",  Assoc.R2L, Arity.Unary , OurAstType.OpBitNot), // bitwise NOT
                new OpDef(9, "*",   Assoc.L2R, Arity.Binary, OurAstType.OpMul), // mult
                new OpDef(9, "/",   Assoc.L2R, Arity.Binary, OurAstType.OpDiv), // div
                new OpDef(9, "%",   Assoc.L2R, Arity.Binary, OurAstType.OpMod), // mod
                new OpDef(8, "+",   Assoc.L2R, Arity.Binary, OurAstType.OpAdd), // add
                new OpDef(8, "-",   Assoc.L2R, Arity.Binary, OurAstType.OpSub), // sub
                new OpDef(7, "<<",  Assoc.L2R, Arity.Binary, OurAstType.OpShiftRight), // bitwise shift left
                new OpDef(7, ">>",  Assoc.L2R, Arity.Binary, OurAstType.OpShiftLeft), // bitwise shift right
                new OpDef(6, "<",   Assoc.L2R, Arity.Binary, OurAstType.OpLt), // less than
                new OpDef(6, "<=",  Assoc.L2R, Arity.Binary, OurAstType.OpLeq), // less than or equal
                new OpDef(6, ">",   Assoc.L2R, Arity.Binary, OurAstType.OpGt), // greater than
                new OpDef(6, ">=",  Assoc.L2R, Arity.Binary, OurAstType.OpGeq), // greater than or equal
                new OpDef(5, "==",  Assoc.L2R, Arity.Binary, OurAstType.OpEq), // equal
                new OpDef(5, "!=",  Assoc.L2R, Arity.Binary, OurAstType.OpNeq), // not equal
                new OpDef(4, "&",   Assoc.L2R, Arity.Binary, OurAstType.OpBitAnd), // bitwise AND
                new OpDef(3, "^",   Assoc.L2R, Arity.Binary, OurAstType.OpBitXor), // bitwise XOR
                new OpDef(2, "|",   Assoc.L2R, Arity.Binary, OurAstType.OpBitOr), // bitwise OR
                new OpDef(1, "&&",  Assoc.L2R, Arity.Binary, OurAstType.OpLogAnd), // logical AND
                new OpDef(0, "||",  Assoc.L2R, Arity.Binary, OurAstType.OpLogOr) // logical OR
                };
#endif

            //programType = OurAstType.Program;
            //exprType = OurAstType.Expr;
            //defaultValueType = OurAstType.Real;
        }

        /************************ utility *****************************/

        // consume any tokens that should be ignored
        // whitespace, comments, blank lines
        //protected override void SkipIgnoredTokens(Tokenizer<T> tokenizer)
        //{
        //    if (!tokenizer.More())
        //        return;
        //    void SkipWhitespace()
        //    {
        //        var t1 = tokenizer.Peek().Type;
        //        while (t1 == TokenType.Comment || t1 == TokenType.Whitespace)
        //        {
        //            tokenizer.Consume();
        //            t1 = tokenizer.Peek().Type;
        //        }
        //    }
        //
        //
        //
        //    SkipWhitespace();
        //
        //    var t = tokenizer.Peek().Type;
        //    while (t == TokenType.Endline)
        //    {
        //        tokenizer.SavePosition();
        //        tokenizer.Next(); // skip endline
        //        SkipWhitespace(); // remove more comments and spaces
        //        t = tokenizer.Peek().Type; // now where are we?
        //        if (t == TokenType.Endline)
        //            tokenizer.DiscardPosition(); // we have a blank line
        //        else
        //        {
        //            tokenizer.RestorePosition();
        //            break;
        //        }
        //    }
        //}


        /************************ grammar *****************************/

        // todo - remove, rename?
        AST<OurAstType> ParseProgram()
        {
            // Endline* (Decl ( Endline+ Decl)* ) ?

            Start(OurAstType.Program);
#if false
            //Discard(T.Endline, 0, infinity);
            //Save(0,1,
            //    ParseDeclaration,
            //    Seq(0,infinity, Discard(T.Endline,1,infinity),ParseDeclaration)))
            //    )
#else
            Save(ParseNumber /* ParseDeclaration */, 0, infinity);
#endif
            return Finish("invalid program");
        }

#if false
        AST<OurAstType> ParseDeclaration()
        {  // Statement | FunctionDef
            Start(OurAstType.Declaration);
            Discard(T.Endline, 0, infinity);
            OneOf(ParseStatement, ParseFunctionDef);
            Discard(T.Endline, 1, infinity);
            return Finish("invalid declaration");
        }
        AST<OurAstType> ParseStatement()
        { // Assignment | For | If | FunctionCall | Return
            Start(OurAstType.Statement);
            OneOf(
                ParseAssignment,                
                //ParseFor,
                ParseIf,
                ParseFunctionCall, 
                ParseReturn
            );

            return Finish("Expected statement");
        }

        AST<OurAstType> ParseFunctionDef()
        { // 'func' identifier '(' identifierList? ')' Endline Block
            Start(OurAstType.FunctionDef);
            Discard("func");
            Save(ParseIdentifier);
            Discard("(");
            Save(ParseIdList,0,1);
            Discard(")");
            Discard(T.Endline);
            Save(ParseBlock);
            return Finish("Function parse error");
        }

        AST<OurAstType> ParseAssignment()
        {
            Start(OurAstType.Assignment);
            Save(ParseIdentifier);
            Discard("="); // todo - name these
            Save(ParseExpr);
            return Finish("Assignment error");
        }
        AST<OurAstType> ParseFor()
        {
            // for a = 1 to n
            Start(OurAstType.For);
            Discard("For");
            Save(ParseIdentifier);
            Discard("=");
            Save(ParseExpr);
            Discard("to");
            Save(ParseExpr);
            Discard(T.Endline);
            ParseBlock();
            return Finish("Expected for");
        }
        AST<OurAstType> ParseIf()
        {
            Start(OurAstType.If);
            Discard("if");
            Save(ParseExpr);
            //Discard(T.Endline);
            //Save(ParseBlock);
            return Finish("Expected if");
        }

        AST<OurAstType> ParseFunctionCall()
        {
            Start(OurAstType.FunctionCall);
            Save(ParseIdentifier);
            Discard("(");
            Save(ParseExprList,0,1);
            Discard(")");
            return Finish("Expected func call");
        }
        AST<OurAstType> ParseReturn()
        {
            Start(OurAstType.Return);
            Discard("return");
            Save(ParseExpr);
            return Finish("Expected func call");
        }

        AST<OurAstType> ParseIdList()
        {
            Start(OurAstType.IdList);

            // (Identifier (',' Identifier)* )
            Save(ParseIdentifier);
            WhileNext(",", ParseIdentifier);
            return Finish("Expected identifier list");
        }
        AST<OurAstType> ParseExprList()
        {
            Start(OurAstType.ExprList);

            // Expr (',' Expr)* )
            Save(ParseExpr);
            WhileNext(",", ParseExpr);
            return Finish("Expected expression list");
        }

        AST<OurAstType> ParseBlock()
        {
            Start(OurAstType.Block);
            Discard(T.Indent);
            Save(ParseStatement, 0, infinity);
            Discard(T.Dedent);
            return Finish("Expected block");
        }

        AST<OurAstType> ParseExpr()
        {
            // todo proper expressions later
            Start(OurAstType.Expr);
            OneOf(ParseIdentifier, ParseNumber, ParseString);
            return Finish("Expected expression");
        }


        // terminal items in the tree are identifiers, numbers, strings, etc...
        AST<OurAstType> ParseIdentifier()
        {
            return ProcessLiteral(OurAstType.Identifier, T.Identifier);
        }
        AST<OurAstType> ParseString()
        {
            return ProcessLiteral(OurAstType.String, T.String);
        }
#endif
        AST<OurAstType> ParseNumber()
        {
            return ProcessLiteral(OurAstType.Number, T.Integer);
        }

        /************************ lexical pieces **********************/

    }
}
