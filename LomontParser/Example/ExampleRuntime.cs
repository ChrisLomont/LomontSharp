using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lomont.Parser.Example
{
#if false // todo
    using Ast = AST<OurAstType>;
    /// <summary>
    /// Evaluate an expression given the ast for the expression,
    /// and the tree of existing values
    /// </summary>
    public class Evaluator
    {
        private Logger logger;
        /// <summary>
        /// Evaluate tree
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="logger"></param>
        public Ast Evaluate(Ast tree, Logger logger)
        {
            this.logger = logger;
            try
            {
                rootScope = new Scope();
                curScope = rootScope;
                AddDefaultFuncs();
                Recurse(tree);
            }
            catch (Exception ex)
            {
                logger.Fatal($"Exception: {ex}");
            }

            return tree;
        }

        void AddDefaultFuncs()
        {
            funcs.Clear();
            AddFunc("print");
        }

        void AddFunc(string name, Ast tree = null)
        {
            funcs.Add(name, new FuncDef(name, tree));
        }

        void RecurseChildren(Ast node)
        {
            foreach (var ch in node.Children)
                Recurse(ch);
        }

        class Variable
        {
            public Variable(string name)
            {
                Name = name;
            }
            public string Name { get; }
            public object Value { get; private set; }

            public void SetValue(object value)
            {
                Value = value;
            }

        }

        Dictionary<string, FuncDef> funcs = new Dictionary<string, FuncDef>();

        class FuncDef
        {
            public FuncDef(string name, Ast tree = null)
            {

            }
            // name of func
            public string Name { get; }
            // assoc node, or null if builtin
            public Ast TreeNode { get; }
        }


        class Scope
        {
            public Scope Parent = null;
            public List<Variable> Variables = new List<Variable>();
            Variable Find(string name)
            {
                return Variables.FirstOrDefault(v => v.Name == name);
            }

            // add variable, throw if exists
            public Variable AddVariable(string name)
            {
                if (Find(name) != null)
                    throw new Exception($"Cannot add existing variable {name}");
                var v = new Variable(name);
                Variables.Add(v);
                return v;
            }
            // get variable, throw if not exists
            public Variable GetVariable(string name)
            {
                var v = Find(name);
                if (v == null)
                    throw new Exception($"Cannot find variable {name}");
                return v;
            }
        }

        private Scope rootScope, curScope;

        void ProcessVarDef(Ast node)
        {
            ProcessAssignment(node, true);
        }

        void ProcessAssignment(Ast node, bool addVars = false)
        {
            Debug.Assert(node.Type == OurAstType.Assignment || node.Type == OurAstType.Vardef);
            // count ids, get list of variables to assign to
            var varCount = 0;
            var vars = new List<Variable>();
            while (node.Children[varCount].Type == OurAstType.Id)
            {
                var name = node.Children[varCount].Value;
                var variable = addVars ? curScope.AddVariable(name) : curScope.GetVariable(name);
                vars.Add(variable);
                varCount++;
            }

            Debug.Assert(node.Children.Count == 2 * varCount);

            // now evaluate expressions, get value for assignment
            for (var j = 0; j < varCount; ++j)
            {
                var expr = node.Children[j + varCount];
                var value = ProcessExpression(expr);
                vars[j].SetValue(value);
            }
        }

        void ProcessFunccall(Ast node)
        {
            Debug.Assert(node.Type == OurAstType.Funccall);
            var name = node.Value;
            if (!funcs.ContainsKey(name))
                logger.Fatal($"Cannot call missing function {name}");
            else
            {
                var func = funcs[name];
                if (func.TreeNode == null)
                { // built in
                    switch (name)
                    {
                        case "print":
                            foreach (var ch in node.Children)
                            {
                                var obj = ProcessExpression(ch);
                                if (obj is string)
                                { // formatting
                                    var s = obj as string;
                                    while (true)
                                    {
                                        var m = formatRegex.Match(s);
                                        if (!m.Success)
                                            break;
                                        var i = m.Index;
                                        var name1 = m.Value;
                                        name1 = name1.Substring(1, name1.Length - 2);
                                        var v = curScope.GetVariable(name1);
                                        s = s.Replace(m.Value, v.Value.ToString());
                                    }
                                    obj = s;
                                }
                                logger.Info($"Print: {obj}");
                            }
                            break;
                        default:
                            logger.Fatal("Missing builtin func {}");
                            break;
                    }
                }
                else
                {
                    logger.Fatal($"Cannot process user func {name}");
                }
            }
        }

        Regex formatRegex = new Regex(@"\{[a-zA-Z][a-zA-Z_]*\}", RegexOptions.Compiled);


        void Recurse(Ast node)
        {
            switch (node.Type)
            {
                case OurAstType.Program:
                    RecurseChildren(node);
                    break;
                case OurAstType.Vardef:
                    ProcessVarDef(node);
                    break;
                case OurAstType.Assignment:
                    ProcessAssignment(node);
                    break;
                case OurAstType.Funccall:
                    ProcessFunccall(node);
                    break;
                default:
                    throw new InvalidExpressionException($"Unknown AST type {node.Type}");
            }
        }

        object ProcessExpression(Ast node)
        {
            Debug.Assert(node.Type == OurAstType.Expr);
            Debug.Assert(node.Children.Count == 1);
            return ExpressionRecurse(node.Children[0], logger);
        }

        // todo - use this to reduce constants from optimizer
        // call on expr
        // 
        public static object ExpressionRecurse(Ast node, Logger logger, Evaluator eval = null)
        {
            object left = null, right = null, ans = null;
            if (node.Children.Count >= 1)
                left = ExpressionRecurse(node.Children[0], logger);
            if (node.Children.Count == 2)
                right = ExpressionRecurse(node.Children[1], logger);

            // return from switch, else fail
            switch (node.Type)
            {
                // literals
                case OurAstType.Real:
                    return double.Parse(node.Value);
                case OurAstType.String:
                    return node.Value;
                // operators
                case OurAstType.OpAdd:
                    ans = Compute(typeof(double), typeof(double), (a, b) => (double)a + (double)b) ?? null;
                    if (ans != null)
                        return ans;
                    break;
                case OurAstType.OpSub:
                    if (left is double && right is double)
                        return (int)left - (int)right;
                    break;
                case OurAstType.OpMul:
                    if (left is double && right is double)
                        return (int)left * (int)right;
                    break;
                case OurAstType.OpDiv:
                    if (left is double && right is double)
                        return (int)left / (int)right;
                    break;
                default:
                    logger.Fatal($"Unknown expression node {node.Type}");
                    break;
            }
            logger.Fatal($"Unknown expression node {node.Type}");
            return null;

            //helper to reduce clutter in code
            object Compute(Type leftType, Type rightType, Func<object, object, object> func)
            {
                if (left.GetType() == leftType && right.GetType() == rightType)
                    return func(left, right);
                return null;
            }

        }


    }
#endif
}
