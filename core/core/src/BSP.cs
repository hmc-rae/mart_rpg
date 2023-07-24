using Microsoft.Xna.Framework;
using SharpDX.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public static class SpacePartition
    {
        public const int CONTENTS_SOLID = -1;
        public const int CONTENTS_EMPTY = -2;

        /// <summary>
        /// Checks to see where a given point exists in real space. <br></br>
        /// The point is a center of a cuboid with the radius being 0 on all axis.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="nodenum"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int CheckContents(bsp_node[] nodes, int nodenum, Vector3 pos)
        {
            while (nodenum >= 0 && nodenum < nodes.Length)
            {
                Vector3 rel = pos - nodes[nodenum].origin;
                float dot = Vector3.Dot(nodes[nodenum].normal, rel);

                //If > 0, check forward (left) node
                if (dot > 0)
                    nodenum = nodes[nodenum].left;
                else
                    nodenum = nodes[nodenum].right;
            }
            return nodenum;
        }

        /// <summary>
        /// Checks to see where a given cuboid exists in real space. <br></br>
        /// The three given vectors for x, y, z represent the angle and magnitudes of the three cardinal radii of the cuboid.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="nodenum"></param>
        /// <param name="pos"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static int CheckContents(bsp_node[] nodes, int nodenum, Vector3 pos, Vector3 x, Vector3 y, Vector3 z)
        {
            Matrix rotmat = new Matrix(new Vector4(x, 0), new Vector4(y, 0), new Vector4(z, 0), new Vector4(0, 0, 0, 1));

            while (nodenum >= 0 && nodenum < nodes.Length)
            {
                float dot = 0;
                if (!nodes[nodenum].isShiftable)
                {
                    Vector3 rel = pos - nodes[nodenum].origin;
                    dot = Vector3.Dot(nodes[nodenum].normal, rel);
                }
                else
                {
                    Vector3 rel = pos - GetMutatedPosition(nodes[nodenum].normal, nodes[nodenum].origin, rotmat);
                    dot = Vector3.Dot(nodes[nodenum].normal, rel);
                }

                //If > 0, check forward (left) node
                if (dot > 0)
                    nodenum = nodes[nodenum].left;
                else
                    nodenum = nodes[nodenum].right;
            }
            return nodenum;
        }
        
        /// <summary>
        /// Traces a line through real space to find a point at which the line intersects a plane, if any.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="nodenum"></param>
        /// <param name="pos_start"></param>
        /// <param name="pos_end"></param>
        /// <param name="intersect"></param>
        /// <returns></returns>
        public static bool TraceLine(bsp_node[] nodes, int nodenum, Vector3 pos_start, Vector3 pos_end, ref Vector3 intersect)
        {
            if (nodenum == CONTENTS_SOLID)
            {
                intersect = pos_start;
                return true;
            }
            else if (nodenum == CONTENTS_EMPTY)
            {
                return false;
            }

            float dot1 = Vector3.Dot(nodes[nodenum].normal, pos_start - nodes[nodenum].origin);
            float dot2 = Vector3.Dot(nodes[nodenum].normal, pos_end - nodes[nodenum].origin);

            //Delegate to forward if both forward
            if (dot1 > 0 && dot2 > 0)
            {
                return TraceLine(nodes, nodes[nodenum].left, pos_start, pos_end, ref intersect);
            }
            else if (dot1 <= 0 && dot2 <= 0) //Else, delegate to backward if both backward
            {
                return TraceLine(nodes, nodes[nodenum].right, pos_start, pos_end, ref intersect);
            }

            Vector3 midpoint = Vector3.Zero;
            if (!GetMidpoint(pos_start, pos_end, nodes[nodenum].origin, nodes[nodenum].normal, ref midpoint, 0))
                return false;

            bool side = dot1 > 0 ? true : false; //if true, the first dot is forward (left) and the other backward (right).

            if (side)
            {
                if (TraceLine(nodes, nodes[nodenum].left, pos_start, midpoint, ref intersect))
                    return true;
                return TraceLine(nodes, nodes[nodenum].right, midpoint, pos_end, ref intersect);
            }
            else
            {
                if (TraceLine(nodes, nodes[nodenum].right, pos_start, midpoint, ref intersect))
                    return true;
                return TraceLine(nodes, nodes[nodenum].left, midpoint, pos_end, ref intersect);
            }
        }
        /// <summary>
        /// Sweeps a cuboid through real space to find a point at which that cuboid will intersect a plane, if any exists. <br></br>
        /// The cuboid is represented by the three given vectors representing the angle and direction at which the faces of the cuboid exist.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="nodenum"></param>
        /// <param name="pos_start"></param>
        /// <param name="pos_end"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="intersect"></param>
        /// <returns></returns>
        public static bool TraceLine(bsp_node[] nodes, int nodenum, Vector3 pos_start, Vector3 pos_end, Vector3 x, Vector3 y, Vector3 z, ref Vector3 intersect)
        {
            Matrix rotmat = new Matrix(new Vector4(x, 0), new Vector4(y, 0), new Vector4(z, 0), new Vector4(0, 0, 0, 1));
            return TraceLine(nodes, nodenum, pos_start, pos_end, rotmat, ref intersect);
        }
        /// <summary>
        /// Sweeps a cuboid through real space to find a point at which that cuboid will intersect a plane, if any exists. <br></br>
        /// The given 4x4 matrix represents the three unit vectors in R3 corresponding to the angle and direction at which the faces of the cuboid exist.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="nodenum"></param>
        /// <param name="pos_start"></param>
        /// <param name="pos_end"></param>
        /// <param name="rotmat"></param>
        /// <param name="intersect"></param>
        /// <returns></returns>
        public static bool TraceLine(bsp_node[] nodes, int nodenum, Vector3 pos_start, Vector3 pos_end, Matrix rotmat, ref Vector3 intersect)
        {
            if (nodenum == CONTENTS_SOLID)
            {
                intersect = pos_start;
                return true;
            }
            else if (nodenum == CONTENTS_EMPTY)
            {
                return false;
            }

            Vector3 nodepos = nodes[nodenum].origin;
            if (nodes[nodenum].isShiftable)
            {
                nodepos = GetMutatedPosition(nodes[nodenum].normal, nodes[nodenum].origin, rotmat);
            }

            float dot1 = Vector3.Dot(nodes[nodenum].normal, pos_start - nodepos);
            float dot2 = Vector3.Dot(nodes[nodenum].normal, pos_end - nodepos);

            //Delegate to forward if both forward
            if (dot1 > 0 && dot2 > 0)
            {
                return TraceLine(nodes, nodes[nodenum].left, pos_start, pos_end, rotmat, ref intersect);
            }
            else if (dot1 <= 0 && dot2 <= 0) //Else, delegate to backward if both backward
            {
                return TraceLine(nodes, nodes[nodenum].right, pos_start, pos_end, rotmat, ref intersect);
            }

            Vector3 midpoint = Vector3.Zero;
            if (!GetMidpoint(pos_start, pos_end, nodepos, nodes[nodenum].normal, ref midpoint, 0))
                return false;

            bool side = dot1 > 0 ? true : false; //if true, the first dot is forward (left) and the other backward (right).

            if (side)
            {
                if (TraceLine(nodes, nodes[nodenum].left, pos_start, midpoint, rotmat, ref intersect))
                    return true;
                return TraceLine(nodes, nodes[nodenum].right, midpoint, pos_end, rotmat, ref intersect);
            }
            else
            {
                if (TraceLine(nodes, nodes[nodenum].right, pos_start, midpoint, rotmat, ref intersect))
                    return true;
                return TraceLine(nodes, nodes[nodenum].left, midpoint, pos_end, rotmat, ref intersect);
            }
        }

        /// <summary>
        /// Returns the point in a line defined by a, b, which intersects the plane p, n, in the given vector 'intersect'.
        /// Returns false if it did not intersect.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <param name="n"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool GetMidpoint(Vector3 a, Vector3 b, Vector3 p, Vector3 n, ref Vector3 intersect, double epsilon = 1e-6)
        {
            Vector3 u = b - a;
            float dot = Vector3.Dot(n, u);

            if (Math.Abs(dot) > epsilon)
            {
                Vector3 w = a + p;
                float frac = -Vector3.Dot(n, w) / dot;
                u = frac * u;

                intersect = a + u;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Mutates a plane by the rotational matrix.
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="pos"></param>
        /// <param name="rotmat"></param>
        /// <returns></returns>
        public static Vector3 GetMutatedPosition(Vector3 normal, Vector3 pos, Matrix rotmat)
        {
            Matrix mutate = new Matrix(
                            new Vector4(normal.X, 0, 0, 0),
                            new Vector4(0, normal.Y, 0, 0),
                            new Vector4(0, 0, normal.Z, 0),
                            new Vector4(0, 0, 0, 1)
                        );

            mutate = Matrix.Multiply(rotmat, mutate);

            return pos + new Vector3(mutate.M11, mutate.M22, mutate.M33);
        }
    }

    public class bsp_node
    {
        //Left is forward, right is backward
        public int id, left, right;

        public Vector3 origin, normal;

        public bool isShiftable;
    }
}
