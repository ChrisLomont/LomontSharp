using System;
using System.Collections.Generic;
using System.Diagnostics;
#if false // todo

namespace Lomont.Parser.Example
{
    using Ast = AST<OurAstType>;

    /// <summary>
    /// Given an AST tree, optimize it for the evaluator
    /// </summary>
    public class Optimizer<AType>
    {

        // remove any of this type
        void RemoveChildren(Ast node, OurAstType type)
        {
            node.RemoveChildren(c => c.Type == type);
        }

        // walk node children, any of given type are deleted, moving all children up
        void MoveChildrenUp(Ast node, OurAstType type, bool recurse = false)
        {
            // first gather all children
            var temp = new List<Ast>();
            foreach (var child in node.Children)
            {
                if (child.Type == type)
                {
                    foreach (var grandchild in child.Children)
                        temp.Add(grandchild);
                }
            }

            // now remove all children
            node.RemoveChildren(c => c.Type == type);

            node.AddChildren(temp);

            if (recurse)
            {
                foreach (var ch in node.Children)
                {
                    MoveChildrenUp(ch, type, recurse);
                }
            }
        }

        void ApplyRecurse(Ast node, Action<Ast> action)
        {
            action(node);
            foreach (var ch in node.Children)
                ApplyRecurse(ch, action);
        }

        private Logger logger;
        public Ast Optimize(Ast tree, Logger logger)
        {
            this.logger = logger;

            // remove some intermediate nodes
            MoveChildrenUp(tree, OurAstType.Declaration);
            MoveChildrenUp(tree, OurAstType.Statement, true);

            // remove dead nodes
            RemoveChildren(tree, OurAstType.None);

            // for each vardef, remove the intermediate assignment node
            MoveChildrenUp(tree, OurAstType.Assignment, true);

            // move fun names up to funcdef and funccall
            ApplyRecurse(tree, n =>
            {
                if (n.Type == OurAstType.Funccall)
                {

                    var id = n.Children[0];
                    Debug.Assert(id.Type == OurAstType.Id && n.Value == null);
                    n.Value = id.Value;
                    n.RemoveChild(id);
                }
            });

            // move func call parameters up
            MoveChildrenUp(tree, OurAstType.Params, true);

            // remove Constant node in epxressions
            MoveChildrenUp(tree, OurAstType.Constant, true);

            // for each expression, if all children constant, reduce to that constant
            ApplyRecurse(tree, n =>
                {
                    if (n.Type == OurAstType.Expr)
                    {
                        var constant = CheckConstant(n);

                        if (constant)
                        {
                            var obj = Evaluator.ExpressionRecurse(n.Children[0], logger);
                            if (obj is int)
                            {
                                n.RemoveChildren(n1 => true); // remove all
                                var ii = new Ast(OurAstType.Real, n.Position);
                                ii.Value = obj.ToString();
                                n.AddChild(ii);

                            }
                            // todo - add more types
                            //else
                            //    logger.Fatal($"Cannot constant fold type {obj.GetType().Name}");
                        }
                    }
                }
                );


            return tree;
        }

        bool CheckConstant(Ast n)
        {
            if (n.Children.Count == 0)
                return n.Type == OurAstType.Real;
            var allConstant = true;
            foreach (var c in n.Children)
                allConstant &= CheckConstant(c);
            return allConstant;
        }
    }
}
#endif