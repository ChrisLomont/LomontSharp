using System;
using System.Collections.Generic;

namespace Lomont.Parser
{
    /// <summary>
    /// Abstract Syntax Tree Node
    /// todo - merge tokens in here too?
    /// </summary>
    public class AST<AType>
    {
        public AST<AType> Parent { get; set; }
        public IReadOnlyList<AST<AType>> Children { get; }
        public AType Type { get; }
        public int Id { get; }
        public CharPosition Position { get; }

        static int idIndex = 0; // unique index
        List<AST<AType>> localChildren { get; } = new List<AST<AType>>();

        public AST(AType type, CharPosition position)
        {
            Type = type;
            Position = position;
            Id = ++idIndex;
            Children = localChildren;
        }

        // return true if existed
        public bool RemoveChild(AST<AType> child)
        {
            var index = localChildren.IndexOf(child);
            if (index != -1)
            {
                localChildren.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveChildren(Predicate<AST<AType>> match)
        {
            localChildren.RemoveAll(match);
        }

        public void AddChildren(IEnumerable<AST<AType>> children)
        {
            foreach (var c in children)
                AddChild(c);
        }

        public void AddChild(AST<AType> child)
        {
            localChildren.Add(child);
            child.Parent = this;
        }

        // a node can have a value todo - store double here?
        public string Value { get; set; }

        public override string ToString()
        {
            return $"Type: {Type} id:{Id} {Value}";
        }

    }

}
