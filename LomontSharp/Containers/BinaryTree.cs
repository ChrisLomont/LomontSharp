using System;

// TODO:
//  1. use var
//  2. test
//  3. rename funcs as I like
//  4. Rename to RedBlackTree (?)
//  5. check color enum only adds bit or byte if possible...
//  6. other funcs to add: Split, Union, Intersect, SetDifference,Join
//  7. fix null and not-null stuff in code
//  8. remove usings

namespace Lomont.Containers
{
    /// <summary>
    /// Balanced binary tree, implemented as red/black tree
    /// </summary>
    public class BinaryTree<T> where T : IComparable
    {
        // notes http://staff.ustc.edu.cn/~csli/graduate/algorithms/book6/chap13.htm

        /// <summary>
        /// A node denoting a missing node, use instead of null
        /// </summary>
        public readonly static Node<T> Nil = new(default,Nil) { Color = Color.Black };

        /// <summary>
        /// Root node
        /// </summary>
        public Node<T> Root { get; private set; } = Nil;

        #region Modify
        /// <summary>
        /// Insert into tree, return existing node if exists, else new node
        /// </summary>
        /// <param name="value"></param>
        public Node<T> Insert(T value) 
        {
            var duplicate = Find(value);
            if (duplicate != Nil)
            {
                duplicate.Count += 1;
                return duplicate;
            }
            var node = new Node<T>(value,Nil);
            InsertNode(node);
            return node;
        }

        /// <summary>
        /// Delete value from tree, return node if existed, else BinaryTree.Nil
        /// </summary>
        /// <param name="value"></param>
        public Node<T> Delete(T value) 
        {
            var del = Find(value); // handles root nil case
            if (del == Nil)
                return Nil;
            else if (del.Count > 1)
                del.Count -= 1;
            else
                DeleteNode(del);
            return del;
        }
        #endregion

        #region Queries
        
        /// <summary>
        /// Find node with value, return BinaryTree.Nil if not present
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Node<T> Find(T value)
        {
            var node = Root;
            while (node != Nil && value.CompareTo(node.Value) != 0)
                node = value.CompareTo(node.Value) < 0 ? node.Left : node.Right;
            return node;
        }

        /// <summary>
        /// Does tree contain value?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value) => Find(value) != Nil;

        /// <summary>
        /// Get minimum value in (sub) tree
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Node<T> Minimum(Node<T>? n = null)
        {
            n ??= Root;
            while (n.Left != Nil)
                n = n.Left;
            return n;
        }
        /// <summary>
        /// Get maximum value in (sub) tree
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Node<T> Maximum(Node<T>? n = null)
        {
            n ??= Root;
            while (n.Right != Nil)
                n = n.Right;
            return n;
        }

        /// <summary>
        /// Get node with value right past the given value, or BinaryTree.Nil
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Node<T> Successor(T value)
        {
            var p = Root;
            if (p == Nil) return Nil;

            // find best node:
            var last = p;
            while (p != Nil) 
            {
                last = p;
                if (p.Value.CompareTo(value) <= 0)
                    p = p.Right;
                else if (p.Value.CompareTo(value) > 0)
                    p = p.Left;
            }
            if (p == Nil) p = last;
            // p now best node
            if (p.Value.CompareTo(value) > 0)
                return p;
            
            return Successor(p);
        }

        /// <summary>
        /// Get node with value right before the given value, or BinaryTree.Nil
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Node<T> Predecessor(T value)
        {
            var p = Root;
            if (p == Nil) return Nil;

            // find best node:
            var last = p;
            while (p != Nil)
            {
                last = p;
                if (p.Value.CompareTo(value) < 0)
                    p = p.Right;
                else if (p.Value.CompareTo(value) >= 0)
                    p = p.Left;
            }
            if (p == Nil) p = last;
            // p now best node
            if (p.Value.CompareTo(value) < 0)
                return p;

            return Predecessor(p);
        }

        /// <summary>
        /// Get node with value right past the given node, or BinaryTree.Nil
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Node<T> Successor(Node<T> n) 
        {
            if (n.Right != Nil)
                return Minimum(n.Right);
            var p = n.Parent;
            while (p != Nil && n == p.Right)
            {
                n = p;
                p = p.Parent;
            }
            return p;
        }

        /// <summary>
        /// Get node with value right before the given node, or BinaryTree.Nil
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Node<T> Predecessor(Node<T> n) 
        {
            if (n.Left != Nil)
                return Maximum(n.Left);
            var p = n.Parent;
            while (p != Nil && n == p.Left)
            {
                n = p;
                p = p.Parent;
            }
            return p;
        }
        #endregion


        public enum Color { Red, Black };
        public class Node<T1>
        {
            public Node(T1 value, Node<T1> nil) { Value = value; Left = Right = Parent = nil; }
            /// <summary>
            /// Value at this node. Left subtree is less than this, right subtree is more than this.
            /// </summary>
            public T1 Value { get; }
            public Node<T1> Left { get; set; }
            public Node<T1> Right { get; set; }
            public Node<T1> Parent { get; set; }
            /// <summary>
            /// Count of this value in the tree
            /// </summary>
            public int Count { get; set; } = 1;

            /// <summary>
            /// Used internally to balance the tree
            /// </summary>
            internal Color Color { get; set; } = Color.Red;
        }

        #region Implementation


        void InsertNode(Node<T> z)
        {
            var y = Nil;
            var x = Root;
            while (x != Nil)
            {
                y = x;
                x = z.Value.CompareTo(x.Value) < 0 ? x.Left : x.Right;
            }
            z.Parent = y;
            if (y == Nil)
                Root = z;
            else if (z.Value.CompareTo(y.Value) < 0)
                y.Left = z;
            else
                y.Right = z;
            z.Left = z.Right = Nil;
            z.Color = Color.Red;
            InsertFixup(z);
        }

        void InsertFixup(Node<T> z)
        {
            while (z.Parent.Color == Color.Red)
            {
                if (z.Parent == z.Parent.Parent.Left)
                {
                    var y = z.Parent.Parent.Right;
                    if (y.Color == Color.Red)
                    {
                        z.Parent.Color = Color.Black;
                        y.Color = Color.Black;
                        z.Parent.Parent.Color = Color.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Right)
                        {
                            z = z.Parent;
                            LeftRotate(z);
                        }
                        z.Parent.Color = Color.Black;
                        z.Parent.Parent.Color = Color.Red;
                        RightRotate(z.Parent.Parent);
                    }
                }
                else
                {
                    var y = z.Parent.Parent.Left;
                    if (y.Color == Color.Red)
                    {
                        z.Parent.Color = Color.Black;
                        y.Color = Color.Black;
                        z.Parent.Parent.Color = Color.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Left)
                        {
                            z = z.Parent;
                            RightRotate(z);
                        }
                        z.Parent.Color = Color.Black;
                        z.Parent.Parent.Color = Color.Red;
                        LeftRotate(z.Parent.Parent);
                    }
                }
            }
            Root.Color = Color.Black;
        }

        void LeftRotate(Node<T> node)
        {
            var t = node.Right;
            node.Right = t.Left;
            if (t.Left != Nil)
                t.Left.Parent = node;
            t.Parent = node.Parent;
            if (node.Parent == Nil)
                Root = t;
            else if (node == node.Parent.Left)
                node.Parent.Left = t;
            else
                node.Parent.Right = t;
            t.Left = node;
            node.Parent = t;
        }

        void RightRotate(Node<T> node)
        {
            var t = node.Left;
            node.Left = t.Right;
            if (t.Right != Nil)
                t.Right.Parent = node;
            t.Parent = node.Parent;
            if (node.Parent == Nil)
                Root = t;
            else if (node == node.Parent.Right)
                node.Parent.Right = t;
            else
                node.Parent.Left = t;
            t.Right = node;
            node.Parent = t;
        }

        void DeleteNode(Node<T> z)
        {
            var y = z;
            var startColor = y.Color;
            Node<T> x;
            if (z.Left == Nil)
            {
                x = z.Right;
                Transplant(z, z.Right);
            }
            else if (z.Right == Nil)
            {
                x = z.Left;
                Transplant(z, z.Left);
            }
            else
            {
                y = Minimum(z.Right);
                startColor = y.Color;
                x = y.Right;
                if (y.Parent == z)
                    x.Parent = y;
                else
                {
                    Transplant(y, y.Right);
                    y.Right = z.Right;
                    y.Right.Parent = y;
                }
                Transplant(z, y);
                y.Left = z.Left;
                y.Left.Parent = y;
                y.Color = z.Color;
            }
            if (startColor == Color.Black)
                DeleteFixup(x);
        }
        void Transplant(Node<T> u, Node<T> v)
        {
            if (u.Parent == Nil)
                Root = v;
            else if (u == u.Parent.Left)
                u.Parent.Left = v;
            else
                u.Parent.Right = v;
            v.Parent = u.Parent;
        }
        private void DeleteFixup(Node<T> x)
        {
            while (x != Root && x.Color == Color.Black)
            {
                if (x == x.Parent.Left)
                {
                    var w = x.Parent.Right;
                    if (w.Color == Color.Red)
                    {
                        w.Color = Color.Black;
                        x.Parent.Color = Color.Red;
                        LeftRotate(x.Parent);
                        w = x.Parent.Right;
                    }
                    if (w.Left.Color == Color.Black && w.Right.Color == Color.Black)
                    {
                        w.Color = Color.Red;
                        x = x.Parent;
                    }
                    else
                    {
                        if (w.Right.Color == Color.Black)
                        {
                            w.Left.Color = Color.Black;
                            w.Color = Color.Red;
                            RightRotate(w);
                            w = x.Parent.Right;
                        }
                        w.Color = x.Parent.Color;
                        x.Parent.Color = Color.Black;
                        w.Right.Color = Color.Black;
                        LeftRotate(x.Parent);
                        x = Root;
                    }
                }
                else
                {
                    var w = x.Parent.Left;
                    if (w.Color == Color.Red)
                    {
                        w.Color = Color.Black;
                        x.Parent.Color = Color.Red;
                        RightRotate(x.Parent);
                        w = x.Parent.Left;
                    }
                    if (w.Right.Color == Color.Black && w.Left.Color == Color.Black)
                    {
                        w.Color = Color.Red;
                        x = x.Parent;
                    }
                    else
                    {
                        if (w.Left.Color == Color.Black)
                        {
                            w.Right.Color = Color.Black;
                            w.Color = Color.Red;
                            LeftRotate(w);
                            w = x.Parent.Left;
                        }
                        w.Color = x.Parent.Color;
                        x.Parent.Color = Color.Black;
                        w.Left.Color = Color.Black;
                        RightRotate(x.Parent);
                        x = Root;
                    }
                }
            }
            x.Color = Color.Black;
        }
        #endregion


    }
}
