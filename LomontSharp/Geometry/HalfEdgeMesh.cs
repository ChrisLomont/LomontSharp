using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Formats;
using Lomont.Numerical;

namespace Lomont.Geometry
{
    // Good and simple way to do HE data structure
    // This is slightly different to shrink memory usage but still give O(1)
    // behavior for common activities
    // http://kaba.hilvi.org/homepage/blog/halfedge/halfedge.htm
    //
    // Also good
    // https://fgiesen.wordpress.com/2012/02/21/half-edge-based-mesh-representations-theory/
    // http://www.enseignement.polytechnique.fr/informatique/INF562/Slides/MeshDataStructures.pdf
    // https://www.flipcode.com/archives/The_Half-Edge_Data_Structure.shtml
    // https://ubm-twvideo01.s3.amazonaws.com/o1/vault/gdc2012/slides/Programming%20Track/Rhodes_Graham_Math_for_Games_Tutorial_Computational_Geometry.pdf

    /*
     todo - add derived types


     */

    /// <summary>
    /// Half edge mesh representation. Allows rapid connectivity traversals.
    /// Also known as a Doubly-Connected Edge List (DCEL).
    ///
    /// Cannot represent non-orientable surfaces.
    /// 
    /// 
    /// </summary>
    public class HalfEdgeMeshNew
    {
        /// <summary>
        /// Index for a missing or empty item
        /// </summary>
        public const int INVALID = 0x7FFF_FFFF;

        /// <summary>
        /// Initialize Half edge mesh with vertices and faces.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>

        public HalfEdgeMeshNew(IList<Vec3> vertices, IEnumerable<(int,int,int)> triangles)
        {
            foreach (var v in vertices)
                this.vertices.Add(new VertexData(v, INVALID));
            foreach (var (i1, i2, i3) in triangles)
                faces.Add(new PolygonData(INVALID, i1, i2, i3));
            Construct();
        }

        #region Accessors
        public int NextHalfEdge(int currentHalfEdgeIndex)
        {
            if (currentHalfEdgeIndex == INVALID)
                return INVALID;
            return halfEdges[currentHalfEdgeIndex].nextHalfIndex;
        }
        public int PrevHalfEdge(int currentHalfEdgeIndex)
        {
            if (currentHalfEdgeIndex == INVALID)
                return INVALID;
            
            var cur = currentHalfEdgeIndex;
            
            // if this doesn't terminate, there is an internal error
            // todo - detect and throw?
            while (true)
            {
                var nxt = halfEdges[cur].nextHalfIndex;
                if (nxt == currentHalfEdgeIndex)
                    return cur;
                cur = nxt;
            }
        }

        public int DestVertex(int currentHalfEdgeIndex)
        {
            if (currentHalfEdgeIndex == INVALID)
                return INVALID;

            return SourceVertex(NextHalfEdge(currentHalfEdgeIndex));
        }
        public int SourceVertex(int currentHalfEdgeIndex)
        {
            if (currentHalfEdgeIndex == INVALID)
                return INVALID;

            return halfEdges[currentHalfEdgeIndex].originVertexIndex;
        }

        public int PairedHalfEdge(int currentHalfEdgeIndex)
        {
            if (currentHalfEdgeIndex == INVALID)
                return INVALID;
            return currentHalfEdgeIndex ^ 1; // they're in pairs
        }

        public int LeftFaceIndex(int currentHalfEdgeIndex)
        {
            if (currentHalfEdgeIndex == INVALID)
                return INVALID;
            return halfEdges[currentHalfEdgeIndex].leftPolygonIndex;
        }
        public int VertexCount => vertices.Count;
        public int FaceCount => faces.Count;

        public IEnumerable<Vec3> GetVertices()
        {
            foreach (var v in vertices)
                yield return v.vertex;
        }
        public IEnumerable<List<int>> GetFaces()
        {
            foreach (var f in faces)
                yield return new List<int>{f.i1,f.i2,f.i3};
        }

        #endregion

        /// <summary>
        /// Dump system to the given output
        /// </summary>
        /// <param name="output"></param>
        public void Dump(TextWriter output, string preMessage = "")
        {
            if (!String.IsNullOrEmpty(preMessage))
                output.WriteLine(preMessage);

            output.WriteLine("Vertices: index, halfEdgeIndex, x,y,z");
            for (var vIndex = 0; vIndex < vertices.Count; ++vIndex)
            {
                var v = vertices[vIndex];
                var vv = v.vertex;
                output.WriteLine($"{vIndex}: {v.halfIndex} ({vv.X:F3},{vv.Y:F3},{vv.Z:F3})");
            }
            
            output.WriteLine("");
            output.WriteLine("Faces: index, halfEdgeIndex, vIndex1,2,3");
            for (var fIndex = 0; fIndex < faces.Count; ++fIndex)
            {
                var f = faces[fIndex];
                output.WriteLine($"{fIndex}: {f.halfIndex} {f.i1} {f.i2} {f.i3}");
            }

            output.WriteLine("");
            output.WriteLine("Half edges: index, next, left poly, origin vertex");
            for (var hIndex = 0; hIndex < halfEdges.Count; ++hIndex)
            {
                var h = halfEdges[hIndex];
                output.WriteLine($"{hIndex}: {h.nextHalfIndex} {h.leftPolygonIndex} {h.originVertexIndex}");
            }

            output.Flush();

        }

        #region Implementation


        /// <summary>
        /// Build internal invariants. Assumes all vertices and face vertex indices are correct.
        /// This may reorder faces to align them consistently
        /// </summary>
        void Construct()
        {
            // should be first pass
            Trace.Assert(halfEdges.Count == 0);

            // map of vertex pair to half edge index
            var map = new Dictionary<ulong, int>();

            // stack for flood fill when making regions orientation consistent
            var faceStack = new Stack<int>();

            // add consistent triangle for each face
            for (var fIndex = 0; fIndex < faces.Count; ++fIndex)
            {
                var face = faces[fIndex];
                var h12Index = GetEdge(face.i1, face.i2, fIndex);
                var h23Index = GetEdge(face.i2, face.i3, fIndex);
                var h31Index = GetEdge(face.i3, face.i1, fIndex);

                // make loop, point to face
                halfEdges[h12Index] = halfEdges[h12Index] with { nextHalfIndex = h23Index, leftPolygonIndex = fIndex };
                halfEdges[h23Index] = halfEdges[h23Index] with { nextHalfIndex = h31Index, leftPolygonIndex = fIndex };
                halfEdges[h31Index] = halfEdges[h31Index] with { nextHalfIndex = h12Index, leftPolygonIndex = fIndex };
            }

            //Dump(Console.Out,"Triangles added");

            // todo - can check here all half edges with a valid face are part of a 3 loop

            // all half edges added, faces all empty. Make faces consistent
            for (var fIndex = 0; fIndex < faces.Count; ++fIndex)
            {
                if (faces[fIndex].halfIndex == INVALID)
                    OrientDomain(fIndex);
            }

            // now orient any boundary loops, which can be scattered around
            for (var hIndex = 0; hIndex < halfEdges.Count; ++hIndex)
            {
                if (LeftFaceIndex(hIndex) == INVALID)
                {
                    var nbrIndex = PairedHalfEdge(hIndex);
                    Trace.Assert(LeftFaceIndex(nbrIndex)!=INVALID);

                    // want to compute next index
                    var nextIndex = nbrIndex;
                    while (LeftFaceIndex(nextIndex) != INVALID)
                        nextIndex = PairedHalfEdge(PrevHalfEdge(nextIndex));

                    // ensure correct
                    halfEdges[hIndex] = halfEdges[hIndex] 
                        with { 
                            originVertexIndex = DestVertex(nbrIndex), 
                            nextHalfIndex = nextIndex
                        };
                }
            }

            // todo- check all loops close nicely - todo

            //Dump(Console.Out, "Final");

            return;

            // see if aligned same by checking dest and src vertices different
            bool Consistent(int halfEdgeIndex1, int halfEdgeIndex2)
            {
                Trace.Assert(halfEdgeIndex1 == PairedHalfEdge(halfEdgeIndex2));
                if (SourceVertex(halfEdgeIndex1) != DestVertex(halfEdgeIndex2))
                    return false;
                if (SourceVertex(halfEdgeIndex2) != DestVertex(halfEdgeIndex1))
                    return false;
                return true;
            }

            // Given face and edge v1->v2, get the corresponding half edge
            // if exists, return it, else allocate and return it
            // half edge has origin and face set. Does not modify face
            int GetEdge(int v1Index, int v2Index, int fIndex)
            {
                var key = Pair(v1Index, v2Index);
                if (!map.ContainsKey(key))
                {
                    // new blank pair
                    var (hIndex,t) = MakeHalfEdgePair();
                    halfEdges[hIndex] = halfEdges[hIndex] with
                    {
                        leftPolygonIndex = fIndex, originVertexIndex = v1Index
                    };
                    halfEdges[t] = halfEdges[t] with { originVertexIndex = v2Index};
                    map.Add(key,hIndex);
                    return hIndex;
                }
                else
                {
                    var hIndex = map[key]; // was allocated elsewhere, so should be face allocated
                    Trace.Assert(halfEdges[hIndex].leftPolygonIndex != INVALID);
                    if (LeftFaceIndex(hIndex) != fIndex)
                        hIndex = PairedHalfEdge(hIndex);
                    Trace.Assert(LeftFaceIndex(hIndex) == INVALID || LeftFaceIndex(hIndex) == fIndex);
                    if (LeftFaceIndex(hIndex) == INVALID) // was never set
                        halfEdges[hIndex] = halfEdges[hIndex] with {leftPolygonIndex = fIndex};

                    return hIndex;
                }
            }

            // given a face index, assume it's orientation is correct, then extend this to all that can be deduced from this one
            void OrientDomain(int faceIndexIn)
            {
                faceStack.Push(faceIndexIn);
                while (faceStack.Count > 0)
                {
                    var faceIndex = faceStack.Pop();
                    var face = faces[faceIndex];

                    var h1Index = GetEdge(face.i1, face.i2, faceIndex); // assume this correct order
                    var h2Index = NextHalfEdge(h1Index);
                    var h3Index = NextHalfEdge(h2Index);
                    Trace.Assert(NextHalfEdge(h3Index) == h1Index);

                    // mark this face as done
                    faces[faceIndex] = faces[faceIndex] with { halfIndex = h1Index }; 

                    // Dump(Console.Out,$"Orient face {faceIndex}");

                    // deal with neighbors
                    ProcessNeighbor(h1Index);
                    ProcessNeighbor(h2Index);
                    ProcessNeighbor(h3Index);
                }
            }

            // add neighbors if not already aligned. Also check aligned neighbors are correct
            void ProcessNeighbor(int halfEdgeIndex)
            {
                var nbrHeIndex = PairedHalfEdge(halfEdgeIndex);
                var nbrFaceIndex = halfEdges[nbrHeIndex].leftPolygonIndex;
                if (nbrFaceIndex == INVALID)
                {
                    // boundary of shape, no neighbor, must be oriented and attached after all else done
                    return; 
                }

                Trace.Assert(NextHalfEdge(nbrFaceIndex) != INVALID); // should be in triangle
                Trace.Assert(LeftFaceIndex(nbrHeIndex) != INVALID);

                if (faces[nbrFaceIndex].halfIndex == INVALID)
                { // this face has not been processed yet, so add to work queue
                    faceStack.Push(nbrFaceIndex); 

                    // if neighbor misoriented, reorient it
                    if (!Consistent(halfEdgeIndex, nbrHeIndex))
                        ReverseTriangleLoop(nbrHeIndex); 
                }
                else
                { // face was already done, check for sanity
                    Trace.Assert(Consistent(halfEdgeIndex, nbrHeIndex));
                }
            }

            // reverse this half edge loop
            void ReverseTriangleLoop(int currentHalfEdgeIndex)
            {
                var fIndex = LeftFaceIndex(currentHalfEdgeIndex);
                var f = faces[fIndex];
                faces[fIndex] = f with { i1 = f.i1, i2 = f.i3, i3 = f.i2 };

                var h1 = currentHalfEdgeIndex;
                var h2 = NextHalfEdge(h1);
                var h3 = NextHalfEdge(h2);
                var v1 = SourceVertex(h1);
                var v2 = SourceVertex(h2);
                var v3 = SourceVertex(h3);

                halfEdges[h1] = halfEdges[h1] with { nextHalfIndex = h3, originVertexIndex = v2 };
                halfEdges[h2] = halfEdges[h2] with { nextHalfIndex = h1, originVertexIndex = v3 };
                halfEdges[h3] = halfEdges[h3] with { nextHalfIndex = h2, originVertexIndex = v1 };
            }


        }

        // add pair of half edges, return their indices
        (int he1, int he2) MakeHalfEdgePair()
        {
            // vertex order is temp here, corrected later
            var he1 = new HalfData(INVALID, INVALID, INVALID);
            var he2 = new HalfData(INVALID, INVALID, INVALID);

            halfEdges.Add(he1);
            halfEdges.Add(he2);
            var i = halfEdges.Count;

            return (i - 2, i - 1);
        }

        /// <summary>
        /// unpack two vertices from a key
        /// </summary>
        /// <param name="packed"></param>
        /// <returns></returns>
        static (int, int) Unpair(ulong packed)
        {
            var v2 = (int)packed;
            var v1 = (int)(packed >> 32);
            return (v1, v2);
        }


        /// <summary>
        /// Map to vertices to one key
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        static ulong Pair(int v1, int v2)
        {
            if (v1 > v2)
                (v1, v2) = (v2, v1);
            ulong p = (ulong)v1;
            p <<= 32;
            p += (ulong)v2;
            return p;
        }



        // class used during matching
        class P
        {
            public int f1 = INVALID;
            public int f2 = INVALID;
            public int he = INVALID;
        }


        // internal storage
        readonly List<HalfData> halfEdges = new();
        readonly List<PolygonData> faces = new ();
        readonly List<VertexData> vertices = new();

        // pair half edge is this index ^ 1 (created in pairs)
        // prev half edge is found via loop
        // dest vertex is found from paired 
        record HalfData(int nextHalfIndex, int originVertexIndex, int leftPolygonIndex);
        record VertexData(Vec3 vertex, int halfIndex);
        record PolygonData(int halfIndex, int i1, int i2, int i3);
#endregion

    }
}
