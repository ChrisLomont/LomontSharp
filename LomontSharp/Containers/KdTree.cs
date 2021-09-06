using System;
using System.Collections.Generic;

namespace Lomont.Containers
{
    public class KdTree<T> 
    {
        /// <summary>
        /// Create a kd-tree with the given dimension
        /// </summary>
        /// <param name="dimension"></param>
        public KdTree(int dimension)
        {
            dim = dimension;
            root = null;
        }

        /// <summary>
        /// Remove all from tree
        /// </summary>
        public void Clear()
        {
            root = null; // clears tree
        }

        /// <summary>
        /// Insert point x into tree with associated item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="x"></param>
        public void Insert(T item, params double [] x)
        {   
            root = Insert(new Point(x), item, root, 0);
        }

        /// <summary>
        /// Delete point x from tree.
        /// Throw on point missing.
        /// </summary>
        /// <param name="x"></param>
        public void Delete(params double [] x)
        {   
            root = Delete(new Point(x), root, 0);
        }

        // find nearest neighbor
        public Tuple<double [],T> NearestNeighbor(params double [] Q)
        {
            var s = new State
            {
                Q = new Point(Q),
                best_dist = Double.MaxValue,
                best = null
            };
            var r = new Rect(s.Q.Count);

            NearestNeighbor(root, 0, r, s);
            return new Tuple<double[],T>(s.best.ToArray(),s.best_node.item);
        }


        /// <summary>
        /// Points closer than this are considered the same
        /// </summary>
        public double Tolerance { get; set; } = 0.000001; 


    #region Implementation
        // based on http://www.cs.cmu.edu/~ckingsf/bioinfo-lectures/kdtrees.pdf
        // and https://www.cs.umd.edu/class/spring2002/cmsc420-0401/
        // TODO - allow multiple with same point
        // TODO - allow general distance
        // TODO - allow adding items to each point
        Node root; // tree root
        int dim;   // dimension of data

        class Node
        {
            public Node left, right;
            public Point data;
            public T item;

            public Node(Point d, T item)
            {
                data = d;
                this.item = item;
            }

        }

        class Point : List<double>
        {
            public Point(params double [] pts)
            {
                AddRange(pts);
            }
        }

        class Rect
        {
            public Point min, max;

            public Rect(int dim)
            {
                min = new Point(new double[dim]);
                max = new Point(new double[dim]);
                for (var i = 0; i < dim; ++i)
                {
                    min[i] = Double.MinValue;
                    max[i] = Double.MaxValue;
                }
            }

            public Rect Clone()
            {
                var r = new Rect(this.min.Count);
                for (var i = 0; i < min.Count; ++i)
                {
                    r.min[i] = min[i];
                    r.max[i] = max[i];
                }
                return r;
            }

            public Rect TrimLeft(int cd, Point x)
            {
                if (x[cd] < max[cd])
                {
                    //var r = Clone();
                    max[cd] = x[cd];
                    //return r;
                }
                return this;
            }

            public Rect TrimRight(int cd, Point x)
            {
                if (min[cd] < x[cd])
                {
                    var r = Clone();
                    r.min[cd] = x[cd];
                    return r;
                }
                return this;
            }
        }

        // insert node x in tree t, starting on cutting dimension cd
        // return new node t
        // if x data < node data, go left, else right
        Node Insert(Point x, T item, Node t, int cd)
        {
            if (t == null)
                t = new Node(x, item);
            else if (distance(x,t.data) < Tolerance)
                throw new Exception("Duplicate item in KD Tree");
            else if (x[cd] < t.data[cd])
                t.left = Insert(x,item,t.left,(cd+1)%dim);
            else
                t.right = Insert(x, item, t.right, (cd + 1) % dim);
            return t;
        }

        // find point with the smallest value in the requested dimension
        Point FindMin(Node t, int dim1, int cd)
        {
            if (t == null)
                return null;
            if (cd == dim1)
            { // cannot be right subtree, so recurse left
                if (t.left == null)
                    return t.data; // if no left node, this node is min
                return FindMin(t.left, dim1, (cd + 1) % dim);
            }

            // node can be either side, check all 
            var left = FindMin(t.left, dim1, (cd + 1)%dim);
            var right = FindMin(t.right, dim1, (cd + 1)%dim);
            var minNode = t.data; // assume this
            if (left != null && left[dim1] < minNode[dim1])
                minNode = left; // better still
            if (right != null && right[dim1] < minNode[dim1])
                minNode = right; // better stil
            return minNode;
        }

        // delete node with point x in tree t, cut on cutting dimension cd
        Node Delete(Point x, Node t, int cd)
        {
            if (t == null)
                throw new Exception("Error: KD point not found");
            var next_cd = (cd + 1) % dim;

            if (distance(x, t.data) < Tolerance)
            {
                if (t.right != null)
                {
                    t.data = FindMin(t.right, cd, next_cd);
                    t.right = Delete(t.data, t.right, next_cd);
                }
                else if (t.left != null)
                {
                    t.data = FindMin(t.left, cd, next_cd);
                    t.right = Delete(t.data, t.left, next_cd);
                }
                else
                {
                    t = null;
                }
            }
            else if (x[cd] < t.data[cd])
                t.left = Delete(x, t.left, next_cd);
            else
                t.left = Delete(x, t.right, next_cd);
            return t;
        }

        // distance between points a and b
        double distance(Point a, Point b)
        {
            double dist = 0;
            for (var i =0; i <a.Count; ++i)
            {
                var delta = a[i] - b[i];
                dist += delta*delta;
            }
            return Math.Sqrt(dist);
        }

        // distance between point p and rectangle BB
        double LowerBound(Point p, Rect BB)
        {
            var dist = 0.0; // assume inside rectangle
            for (var cd = 0; cd < dim; ++cd)
            {
                if (p[cd] < BB.min[cd])
                    dist = Math.Max(dist, BB.min[cd] - p[cd]);
                if (BB.min[cd] < p[cd])
                    dist = Math.Max(dist, p[cd] - BB.max[cd]);
            }
            return dist;
        }


        class State
        {
            public Node best_node;
            public Point best; // searching - best point so far
            public double best_dist = 0; // searching - best distance so far
            public Point Q; // query point
        }



        void NearestNeighbor(Node t, int cd, Rect BB, State state)
        {
            if (t == null || LowerBound(state.Q, BB) > state.best_dist)
                return;
            var next_cd = (cd+1)%dim;
            var dist = distance(state.Q, t.data);
            if (dist < state.best_dist)
            {
                state.best_dist = dist;
                state.best = t.data;
                state.best_node = t;
            }
            var min = BB.min[cd]; // save these
            var max = BB.max[cd];
            if (state.Q[cd] < t.data[cd])
            {
                NearestNeighbor(t.left, next_cd, BB.TrimLeft(cd,t.data),state);
                BB.min[cd] = min;
                BB.max[cd] = max;
                NearestNeighbor(t.right, next_cd, BB.TrimRight(cd, t.data),state);
            }
            else
            {
                NearestNeighbor(t.right, next_cd, BB.TrimRight(cd, t.data),state);
                BB.min[cd] = min;
                BB.max[cd] = max;
                NearestNeighbor(t.left, next_cd, BB.TrimLeft(cd, t.data),state);
            }
            BB.min[cd] = min;
            BB.max[cd] = max;
        }

    #endregion

        public void DumpTree(Action<string> messageAction)
        {
            DumpTree(messageAction,root,0);

        }
        void DumpTree(Action<string> messageAction, Node t, int depth)
        {
            if (t == null)
                return;
            var prefix = new string('-',depth*2);
            messageAction($"{prefix}: {t.data[0]},{t.data[1]}");
            DumpTree(messageAction, t.left, depth + 1);
            DumpTree(messageAction, t.right, depth + 1);
        }

        public static void TestKD(Action<string> messageAction)
        {
            var t = new KdTree<string>(2);
            // test points from https://www.cs.umd.edu/class/spring2002/cmsc420-0401/pbasic.pdf
            var pts = new double[]
            {
                35,90,  70,80,  10,75,
                80,40,  50,90,  70,30,
                90,60,  50,25,  25,10, 
                25,50,  60,10
            };
            for (var i = 0; i < pts.Length; i += 2)
                t.Insert("test "+ i, pts[i],pts[i+1]);

            Action<int, int> Test = (i, j) =>
            {
                var c = t.NearestNeighbor(i,j);
                messageAction($"Closest to ({i},{j}) is ({c.Item1[0]},{c.Item1[1]})");
            };

            Test(80, 40);
            Test(51, 90);
            Test(50, 50);
            Test(0, 0);


            t.DumpTree(messageAction);
            messageAction("Deleting (35,90)");
            t.Delete(35, 90);
            t.DumpTree(messageAction);


        }
    }
}
