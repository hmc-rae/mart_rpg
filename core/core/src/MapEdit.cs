using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using prog;
using SharpDX.MediaFoundation;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static core.src.VertexModel;
using static core.src.VertexModelAdvanced;

namespace core.src
{
    public class MapEdit : GameScene
    {
        ContentManager _content;

        public PerspectiveViewer curViewer;

        public MapEditor mapEditor;
        public BrushEditor brushEditor;

        public BrushLibrary library;
        public override void OnCreate()
        {
            base.OnCreate();
        }
        public override void Load(ContentManager content)
        {
            base.OnDestroy();

            _content = content;
            library = new BrushLibrary(this);
            mapEditor = new MapEditor(this, library);
            brushEditor = new BrushEditor(this, library);

            Program.Game.Window.Title = "Map Editor";
            curViewer = mapEditor;
        }

        public override void OnFrame()
        {
            if (Input.IsKeyPressed(Keys.F1))
            {
                Program.Game.Window.Title = "Map Editor";
                curViewer = mapEditor;
            }
            if (Input.IsKeyPressed(Keys.F2))
            {
                Program.Game.Window.Title = "Brush Editor";
                curViewer = brushEditor;
            }
            curViewer.prepoll();
        }
        public override void PreRender()
        {
            curViewer.prerender();
        }
        public override void OnRender()
        {
            curViewer.render();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            curViewer.Destroy();
        }
    }
    
    public class VertexModel
    {
        public static int COMPILE_ROTATION = 1;
        public static int COMPILE_POLYGONS = 2;
        private GraphicsDevice _device;
        public Vector3 position;

        public Vector3 rotation_x, rotation_y, rotation_z;

        private Matrix rotmat;

        public TexturedPolygon[] polygons;   
        public class TexturedPolygon
        {
            public VertexPositionColorTexture[] verticies;
            public Texture2D texture;
            public bool textureBuilt;
            public VertexBuffer buffer;
            public int primitiveCount;
            public PrimitiveType primitiveType;

            public bool built;

            private GraphicsDevice _device;

            private static int id_master = 0;
            private int id;

            public TexturedPolygon(GraphicsDevice device, int vertexcount, int primitives, PrimitiveType type)
            {
                id = id_master++;
                built = false;
                _device = device;

                verticies = new VertexPositionColorTexture[vertexcount];
                primitiveCount = primitives;
                primitiveType = type;

                textureBuilt = false;

                buffer = new VertexBuffer(_device, typeof(VertexPositionColorTexture), verticies.Length, BufferUsage.WriteOnly);
            }

            public void Recompile()
            {
                buffer.SetData<VertexPositionColorTexture>(verticies);
                built = true;
            }

            public override string ToString()
            {
                return $"Polygon#{id}";
            }
        }

        public VertexModel(GraphicsDevice device, int pc)
        {
            _device = device;
            position = Vector3.Zero;
            polygons = new TexturedPolygon[pc];

            rotation_x = Vector3.UnitX;
            rotation_y = Vector3.UnitY;
            rotation_z = Vector3.UnitZ;
        }
        public void Recompile(int mode)
        {
            if ((mode & COMPILE_ROTATION) != 0)
            {
                //Recomp rotation vectors to a rotation matrix
            }

            if ((mode & COMPILE_POLYGONS) != 0)
            {
                //Recomp all polygons
                for (int i = 0; i < polygons.Length; i++)
                {
                    polygons[i].Recompile();
                }
            }
        }
        public void Draw(Camera camera)
        {
            camera.BindWorldPosition(position);
            for (int n = 0; n < polygons.Length; n++)
            {
                if (!polygons[n].built) continue;
                if (polygons[n].textureBuilt)
                    camera.BindTexture(polygons[n].texture);
                else
                    camera.BindTexture();
                _device.SetVertexBuffer(polygons[n].buffer);
                foreach (EffectPass pass in camera.effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _device.DrawPrimitives(polygons[n].primitiveType, 0, polygons[n].primitiveCount);
                }
            }
        }
        public void Draw(Camera camera, int n)
        {
            camera.BindWorldPosition(position);
            if (n < 0 || n >= polygons.Length) return;

            if (!polygons[n].built) return;
            if (polygons[n].textureBuilt)
                camera.BindTexture(polygons[n].texture);
            else
                camera.BindTexture();
            _device.SetVertexBuffer(polygons[n].buffer);
            foreach (EffectPass pass in camera.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawPrimitives(polygons[n].primitiveType, 0, polygons[n].primitiveCount);
            }
        }

        public static VertexModel CreateGeneric(GraphicsDevice device)
        {
            VertexModel temp = new VertexModel(device, 6);

            //0: heading +x, yz
            temp.polygons[0] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[0].verticies = Generate4Point(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

            //1: heading -x, yz
            temp.polygons[1] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[1].verticies = Generate4Point(Vector3.UnitX * -1, Vector3.UnitY, Vector3.UnitZ);

            //2: heading +y, xz
            temp.polygons[2] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[2].verticies = Generate4Point(Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ);

            //3: heading -y, xz
            temp.polygons[3] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[3].verticies = Generate4Point(Vector3.UnitY * -1, Vector3.UnitX, Vector3.UnitZ);

            //4: heading +z, xy
            temp.polygons[4] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[4].verticies = Generate4Point(Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY);

            //5: heading -z, xy
            temp.polygons[5] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[5].verticies = Generate4Point(Vector3.UnitZ * -1, Vector3.UnitX, Vector3.UnitY);

            temp.rotation_x = Vector3.UnitX;
            temp.rotation_y = Vector3.UnitY;
            temp.rotation_z = Vector3.UnitZ;

            temp.Recompile(COMPILE_POLYGONS);

            return temp;
        }
        internal static VertexPositionColorTexture[] Generate4Point(Vector3 dir, Vector3 a, Vector3 b)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[4];

            verts[0] = new VertexPositionColorTexture(dir + ( a) + ( b), ColorByVectors(dir, a, b), new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + ( b), ColorByVectors(dir, -a, b), new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + ( a) + (-b), ColorByVectors(dir, a, -b), new Vector2(0, 1));
            verts[3] = new VertexPositionColorTexture(dir + (-a) + (-b), ColorByVectors(dir, -a, -b), new Vector2(1, 1));

            return verts;
        }
        internal static VertexPositionColorTexture[] Generate3Point(Vector3 dir, Vector3 a, Vector3 b)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[3];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), ColorByVectors(dir, a, b), new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), ColorByVectors(dir, -a, b), new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), ColorByVectors(dir, a, -b), new Vector2(0, 1));

            return verts;
        }
        internal static VertexPositionColorTexture[] Generate4Point(Vector3 dir, Vector3 a, Vector3 b, Color over)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[4];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), over, new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), over, new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), over, new Vector2(0, 1));
            verts[3] = new VertexPositionColorTexture(dir + (-a) + (-b), over, new Vector2(1, 1));

            return verts;
        }
        internal static VertexPositionColorTexture[] Generate3Point(Vector3 dir, Vector3 a, Vector3 b, Color over)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[3];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), over, new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), over, new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), over, new Vector2(0, 1));

            return verts;
        }
        internal static Color ColorByVectors(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 sum = a + b + c;
            //sum += Vector3.One;
            //sum /= 2;
            return new Color(sum);
        }
    }
    /// <summary>
    /// A replacement for typical VertexModels
    /// </summary>
    public class VertexModelAdvanced
    {
        public class Polygon
        {
            public VertexPositionColorTexture[] verticies;
            public int vertexCount;
            public int primitiveCount;
            public PrimitiveType primitiveType;

            public Texture2D texture;
            public bool textureBuilt;

            public VertexBuffer buffer;

            public bool built;

            protected GraphicsDevice _graphics;

            protected static int id_master = 0;
            protected int id;

            public Polygon(GraphicsDevice device, int points, int prims, PrimitiveType type)
            {
                vertexCount = points;
                _graphics = device;
                verticies = new VertexPositionColorTexture[points];
                primitiveCount = prims;
                primitiveType = type;

                textureBuilt = false;
                built = false;

                id = id_master++;

                buffer = new VertexBuffer(_graphics, typeof(VertexPositionColorTexture), points, BufferUsage.WriteOnly);
            }
            public Polygon(GraphicsDevice device, int points, int prims, PrimitiveType type, VertexPositionColorTexture[] verts) : this(device, points, prims, type)
            {
                SetVerts(verts);
            }
            public Polygon(GraphicsDevice device, int points, int prims, PrimitiveType type, Vector3[] vertPoints, bool autoColor) : this(device, points, prims, type)
            {
                SetVerts(vertPoints, autoColor);
            }

            public virtual void Compile()
            {
                buffer.SetData(verticies);
                built = true;
            }

            public virtual void SetVerts(VertexPositionColorTexture[] verts)
            {
                if (verts.Length != vertexCount) return;
                verticies = verts;
                Compile();
            }
            public virtual void SetVerts(Vector3[] vertPoints, bool autoColor)
            {
                if (vertPoints.Length != vertexCount) return;

                for (int i = 0; i < vertexCount; i++)
                {
                    verticies[i].Position = vertPoints[i];
                    if (autoColor)
                    {
                        verticies[i].Color = new Color(vertPoints[i]);
                    }
                }

                Compile();
            }

            public virtual Vector3 GetVertexPoint(int vertex)
            {
                return verticies[vertex].Position;
            }
            public virtual Color GetVertexColor(int vertex)
            {
                return verticies[vertex].Color;
            }
            public virtual Vector2 GetVertexTexturePos(int vertex)
            {
                return verticies[vertex].TextureCoordinate;
            }
            public virtual void SetVertexPoint(int vertex, Vector3 pos)
            {
                verticies[vertex].Position = pos;
            }
            public virtual void SetVertexColor(int vertex, Color col)
            {
                verticies[vertex].Color = col;
            }
            public virtual void SetVertexTexturePos(int vertex, Vector2 pos)
            {
                verticies[vertex].TextureCoordinate = pos;
            }
        }
        public class SharedPolygon : Polygon
        {
            public int[] vertexReferences;
            private List<SharedPoint> _points;
            public Vector3 core;

            public SharedPolygon(GraphicsDevice device, int points, int prims, PrimitiveType type, Vector3 origin, List<SharedPoint> ppoints) : base(device, points, prims, type)
            {
                vertexReferences = new int[vertexCount];
                core = origin;
                _points = ppoints;
            }
            public SharedPolygon(GraphicsDevice device, int points, int prims, PrimitiveType type, Vector3 origin, List<SharedPoint> ppoints, VertexPositionColorTexture[] verts, float rounding = 0.1f) : this(device, points, prims, type, origin, ppoints)
            {
                SetVerts(verts, rounding);
            }
            public SharedPolygon(GraphicsDevice device, int points, int prims, PrimitiveType type, Vector3 origin, List<SharedPoint> ppoints, Vector3[] vertPoints, bool autoColor, float rounding = 0.1f) : this(device, points, prims, type, origin, ppoints)
            {
                SetVerts(vertPoints, autoColor, rounding);
            }

            /// <summary>
            /// Sets the vertexposition data to the given array, and auto-references points by a default rounding value of 0.1.
            /// </summary>
            /// <param name="verts"></param>
            public override void SetVerts(VertexPositionColorTexture[] verts)
            {
                SetVerts(verts, 0.1f);
            }
            /// <summary>
            /// Sets the vertexposition data to the given array, and auto-references points by a given rounding value.
            /// </summary>
            /// <param name="verts"></param>
            /// <param name="rounding"></param>
            public void SetVerts(VertexPositionColorTexture[] verts, float rounding)
            {
                if (verts.Length != vertexCount) return;

                // For each vertice, look to see if there's a point in the ppoints list that matches the given position by the rounding value.
                // If there exists one, set to that pos and refer to it.
                // If there isn't, create a new ppoint entry for the given pos and refer to it.
                // Note that the pos must be factored to the origin point

                for (int i = 0; i < vertexCount; i++)
                {
                    verticies[i] = verts[i];

                    Vector3 temp = verticies[i].Position + core;

                    bool found = false;
                    for (int k = 0; k < _points.Count; k++)
                    {
                        float len = (_points[k].pos - temp).Length();
                        if (len <= rounding)
                        {
                            found = true;
                            vertexReferences[i] = k;
                            verticies[i].Position = _points[k].pos - core;
                            break;
                        }
                    }

                    if (!found)
                    {
                        vertexReferences[i] = _points.Count;
                        _points.Add(new SharedPoint(temp, _points.Count));
                    }
                }

                Compile();
            }

            /// <summary>
            /// Sets the vertexposition data to the given vectors with optional autocoloring, and auto-references points by a default rounding value of 0.1.
            /// </summary>
            /// <param name="vertPoints"></param>
            /// <param name="autoColor"></param>
            public override void SetVerts(Vector3[] vertPoints, bool autoColor)
            {
                SetVerts(vertPoints, autoColor, 0.1f);
            }
            /// <summary>
            /// Sets the vertexposition data to the given vectors with optional autocoloring, and auto-references points by a given rounding value.
            /// </summary>
            /// <param name="vertPoints"></param>
            /// <param name="autoColor"></param>
            /// <param name="rounding"></param>
            public void SetVerts(Vector3[] vertPoints, bool autoColor, float rounding)
            {
                if (vertPoints.Length != vertexCount) return;

                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3 temp = vertPoints[i] + core;

                    bool found = false;
                    for (int k = 0; k < _points.Count; k++)
                    {
                        float len = (_points[k].pos - temp).Length();
                        if (len <= rounding)
                        {
                            found = true;
                            vertexReferences[i] = k;
                            verticies[i].Position = _points[k].pos - core;
                            break;
                        }
                    }

                    if (!found)
                    {
                        vertexReferences[i] = _points.Count;
                        _points.Add(new SharedPoint(temp, _points.Count));
                    }

                    if (autoColor)
                        verticies[i].Color = new Color(verticies[i].Position);
                }

                Compile();
            }

            /// <summary>
            /// Corrects the polygon in event of any updates to referenced points. Returns true if any corrections necessary.
            /// </summary>
            /// <returns></returns>
            public bool Correct()
            {
                if (vertexReferences == null || verticies == null) return false;

                bool ret = false;

                for (int i = 0; i < vertexCount; i++)
                {
                    // In the event that this vertex reference is now outside of the point list, get an additive value which we'll subtract from the USE value, and set the reference to the top entry of the list.
                    // For example, if points to 10 but list now goes up to 5, set ref to 5, temp to (10 - 5) = 5. The list shrunk by 5 meaning the entry this points to (10) has been moved by 5 (5).
                    // When adding the USE value later, add the temp value (5) to get 0; it wont move now, and points to the correct position.

                    int temp = vertexReferences[i];
                    if (temp >= _points.Count)
                    {
                        vertexReferences[i] = _points.Count - 1;
                        temp = temp - vertexReferences[i];
                    }
                    else
                    {
                        temp = 0;
                    }

                    // If the USE value is nonzero, add the USE value (plus the temp value we specified earlier, since USE will be <0) to correct where this entry points to.
                    if (_points[vertexReferences[i]].USE < 0)
                    {
                        vertexReferences[i] += (_points[vertexReferences[i]].USE + temp);
                    }

                    // Finally, if an update is necessary, perform the update.
                    if (_points[vertexReferences[i]].update)
                    {
                        verticies[i].Position = _points[vertexReferences[i]].pos - core;
                        ret = true;
                    }
                }

                return ret;
            }

            /// <summary>
            /// Recompiles the vertex data
            /// </summary>
            public override void Compile()
            {
                if (Correct())
                    base.Compile();
            }

            /// <summary>
            /// Triggers a check to figure out which referenced points are no longer used.
            /// </summary>
            public void Heartbeat()
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    _points[vertexReferences[i]].USE++;
                }
            }

            public override Vector3 GetVertexPoint(int vertex)
            {
                Correct();
                return _points[vertexReferences[vertex]].pos - core;
            }
            public override void SetVertexPoint(int vertex, Vector3 pos)
            {
                _points[vertexReferences[vertex]].pos = pos + core;
                _points[vertexReferences[vertex]].update = true;
            }
        }
        public class SharedPoint
        {
            public Vector3 pos;
            public int ID;
            public int USE; // We'll use the USE value as a generic counter to use for miscellaneous checks. When removing an entry in a list,
                            // it can be used to self correct on all polygons that use it (set to a negative value for all that have been moved to the left.
                            // Self-correcting should be done in a specific procedure which I will denote later.
            public bool update;

            public SharedPoint(Vector3 p, int i)
            {
                ID = i;
                pos = p;

                USE = 0;
                update = false;
            }
        }

        public const int COMPILE_ROTATION = 1;
        public const int COMPILE_POLYGONS = 2;

        public List<SharedPoint> points; //This can be unused if you want.
        private GraphicsDevice _graphics;

        private List<Polygon> _polys;

        public Vector3 position;

        public Vector3 r_X, r_Y, r_Z;

        public VertexModelAdvanced(GraphicsDevice device)
        {
            _graphics = device;
            _polys = new List<Polygon>();

            points = new List<SharedPoint>();
        }

        /// <summary>
        /// Adds a basic polygon to the model
        /// </summary>
        /// <param name="poly"></param>
        public void AddBasicPoly(Polygon poly)
        {
            _polys.Add(poly);
        }
        /// <summary>
        /// Complicates the given polygon and adds it to the model
        /// </summary>
        /// <param name="poly"></param>
        public void AddComplexPoly(Polygon poly)
        {
            _polys.Add(new SharedPolygon(_graphics, poly.vertexCount, poly.primitiveCount, poly.primitiveType, position, points, poly.verticies));
        }
        /// <summary>
        /// Adds a complex polygon to the model.
        /// </summary>
        /// <param name="poly"></param>
        public void AddComplexPoly(SharedPolygon poly)
        {
            _polys.Add((Polygon)poly);
        }

        public Polygon GetPoly(int index)
        {
            if (index < 0 || index >= _polys.Count) return null;
            return _polys[index];
        }
        public void RemovePoly(int index)
        {
            if (index < 0 || index >= _polys.Count) return;
            _polys.RemoveAt(index);
        }

        /// <summary>
        /// Disposes of all SharedPoints that aren't currently being referenced.
        /// </summary>
        public void ClearUnusedReferences()
        {
            // Default all USE values to 0.
            for (int i = 0; i < points.Count; i++)
            {
                points[i].USE = 0;
            }

            // Heartbeat all shardpolygons
            for (int i = 0; i < _polys.Count; i++)
            {
                SharedPolygon temp = _polys[i] as SharedPolygon;
                if (temp == null) continue;

                temp.Heartbeat();
            }

            // For each USE value < 0, remove it, decrement REM by 1. Otherwise, set the USE value to the current REM value.
            int REM = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].USE == 0)
                {
                    points.RemoveAt(i);
                    REM--;
                    i--;
                    continue;
                }

                points[i].USE = REM;
            }

            // Run Correct on all sharedpolygons
            for (int i = 0; i < _polys.Count; i++)
            {
                SharedPolygon temp = _polys[i] as SharedPolygon;
                if (temp == null) continue;

                temp.Correct();
            }

            // Default USE back to 0.
            for (int i = 0; i < points.Count; i++)
            {
                points[i].USE = 0;
            }
        }

        public void Recompile(int mode)
        {
            if ((mode & COMPILE_ROTATION) != 0)
            {
                // Compile rotation vectors to a rotation matrix
            }

            if ((mode & COMPILE_POLYGONS) != 0)
            {
                // Compile all polygons
                for (int i = 0; i < _polys.Count; i++)
                {
                    _polys[i].Compile();
                }
            }
        }

        public void Draw(Camera camera)
        {
            camera.BindWorldPosition(position);
            for (int n = 0; n < _polys.Count; n++)
            {
                if (!_polys[n].built) continue;
                if (_polys[n].textureBuilt)
                    camera.BindTexture(_polys[n].texture);
                else
                    camera.BindTexture();

                _graphics.SetVertexBuffer(_polys[n].buffer);
                foreach (EffectPass pass in camera.effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphics.DrawPrimitives(_polys[n].primitiveType, 0, _polys[n].primitiveCount);
                }
            }
        }
        public void Draw(Camera camera, int n)
        {
            if (n < 0 || n >= _polys.Count) return;
            if (!_polys[n].built) return;

            camera.BindWorldPosition(position);

            if (_polys[n].textureBuilt)
                camera.BindTexture(_polys[n].texture);
            else
                camera.BindTexture();
            _graphics.SetVertexBuffer(_polys[n].buffer);
            foreach (EffectPass pass in camera.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphics.DrawPrimitives(_polys[n].primitiveType, 0, _polys[n].primitiveCount);
            }
        }

        public static VertexModelAdvanced CreateGenericCuboid(GraphicsDevice device, bool complex)
        {
            VertexModelAdvanced temp = new VertexModelAdvanced(device);

            //0: heading +x, yz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ)));

            //1: heading -x, yz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX * -1, Vector3.UnitY, Vector3.UnitZ)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX * -1, Vector3.UnitY, Vector3.UnitZ)));

            //2: heading +y, xz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ)));

            //3: heading -y, xz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY * -1, Vector3.UnitX, Vector3.UnitZ)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY * -1, Vector3.UnitX, Vector3.UnitZ)));

            //4: heading +z, xy
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY)));

            //5: heading -z, xy
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ * -1, Vector3.UnitX, Vector3.UnitY)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ * -1, Vector3.UnitX, Vector3.UnitY)));

            temp.r_X = Vector3.UnitX;
            temp.r_Y = Vector3.UnitY;
            temp.r_Z = Vector3.UnitZ;

            temp.Recompile(COMPILE_POLYGONS & COMPILE_ROTATION);

            return temp;
        }
        internal static VertexPositionColorTexture[] Generate4Point(Vector3 dir, Vector3 a, Vector3 b)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[4];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), new Color(dir + a + b), new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), new Color(dir - a + b), new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), new Color(dir + a - b), new Vector2(0, 1));
            verts[3] = new VertexPositionColorTexture(dir + (-a) + (-b), new Color(dir - a - b), new Vector2(1, 1));

            return verts;
        }
        internal static VertexPositionColorTexture[] Generate3Point(Vector3 dir, Vector3 a, Vector3 b)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[3];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), new Color(dir + a + b), new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), new Color(dir - a + b), new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), new Color(dir + a - b), new Vector2(0, 1));

            return verts;
        }
        internal static VertexPositionColorTexture[] Generate4Point(Vector3 dir, Vector3 a, Vector3 b, Color over)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[4];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), over, new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), over, new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), over, new Vector2(0, 1));
            verts[3] = new VertexPositionColorTexture(dir + (-a) + (-b), over, new Vector2(1, 1));

            return verts;
        }
        internal static VertexPositionColorTexture[] Generate3Point(Vector3 dir, Vector3 a, Vector3 b, Color over)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[3];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), over, new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), over, new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), over, new Vector2(0, 1));

            return verts;
        }

    }
    public class ButtonWidget2D
    {
        public enum ButtonMode
        {
            Hold,
            Toggle,
            Attend
        }
        public ButtonMode mode;
        public Vector2 screenPos;
        public Vector2 dimensions;

        private martgame _game;
        private GraphicsDevice _graphics;
        private Texture2D _rect;

        private bool bdown;
        public bool ButtonDown => bdown;

        public Vector2 multscale;
        public Vector2 offset;

        public ButtonWidget2D(ButtonMode mode, Vector2 screenPos, Vector2 dimensions)
        {
            _game = Program.Game;
            _graphics = _game.GraphicsDevice;

            this.mode = mode;
            this.screenPos = screenPos;
            this.dimensions = dimensions;
            bdown = false;

            _rect = new Texture2D(_graphics, (int)dimensions.X, (int)dimensions.Y);
            Color[] data = new Color[(int)(dimensions.X) * (int)(dimensions.Y)];
            for (int i = 0; i < data.Length; i++) data[i] = Color.Gray;

            _rect.SetData(data);

            multscale = Vector2.One;
            offset = Vector2.Zero;
        }

        public void Recompile()
        {
            _rect = new Texture2D(_graphics, (int)dimensions.X, (int)dimensions.Y);
            Color[] data = new Color[(int)(dimensions.X) * (int)(dimensions.Y)];
            for (int i = 0; i < data.Length; i++) data[i] = Color.Gray;

            _rect.SetData(data);
        }

        public void Poll()
        {
            Poll(Input.GetMousePosition());
        }

        public void Poll(Vector2 msps)
        {
            msps -= offset;
            msps *= multscale;

            msps = msps - screenPos;
            bool insideMouse = (msps.X <= dimensions.X && msps.Y <= dimensions.Y && msps.X >= 0 && msps.Y >= 0);

            switch (mode)
            {
                case ButtonMode.Toggle:
                    if (!insideMouse) break;

                    if (Input.IsMousePressed(Input.MouseButton.LeftMouse))
                    {
                        bdown = !bdown;
                    }
                    break;
                case ButtonMode.Hold:

                    if (!insideMouse)
                    {
                        bdown = false;
                        break;
                    }

                    if (!ButtonDown)
                    {
                        if (Input.IsMousePressed(Input.MouseButton.LeftMouse))
                        {
                            bdown = true;
                        }
                    }
                    else
                    {
                        if (!Input.IsMousePressed(Input.MouseButton.LeftMouse))
                        {
                            bdown = false;
                        }
                    }

                    break;

                case ButtonMode.Attend:
                    if (!insideMouse)
                    {
                        if (Input.IsMousePressed(Input.MouseButton.LeftMouse))
                            bdown = false;
                    }
                    else
                    {
                        if (Input.IsMousePressed(Input.MouseButton.LeftMouse))
                            bdown = true;
                    }
                    break;
            }
        }

        public void Release()
        {
            bdown = false;
        }

        public void Destroy()
        {
            _rect.Dispose();
        }
    }
    public class ListViewer<T> : ResourceInterface
    {
        static int spacing = 20;

        public List<T> viewedList;

        public int curIndex;
        public int scrolledOffset;
        public int pageSize;

        public Vector2 position, size;
        private Texture2D _rect, _rectSel;
        private ButtonWidget2D _button;
        private SpriteFont _font;
        private SpriteBatch _sprites;
        private ContentManager _content;

        private Func<T, int, string> gs;

        public bool ButtonActive => _button.ButtonDown;

        public ListViewer(List<T> list, int sizeOfPage, Vector2 p, Vector2 s, ContentManager content)
        {
            _content = content;
            viewedList = list;
            pageSize = sizeOfPage;

            _sprites = Program.Game._spriteBatch;
            curIndex = -1;
            scrolledOffset = 0;

            gs = GetString;

            position = p;
            size = s;

            _button = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, position, size);

            _rect = new Texture2D(Program.Game.GraphicsDevice, (int)s.X, (int)s.Y);

            Color[] data = new Color[(int)s.X * (int)s.Y];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Program.UI_COLOR;

                if (i % (int)s.X <= 1 || i / (int)s.X <= 1 || i % (int)s.X >= (int)s.X - 2 || i / (int)s.X >= (int)s.Y - 2 )
                {
                    data[i] = Color.Red;
                }
            }

            _rect.SetData(data);

            _rectSel = new Texture2D(Program.Game.GraphicsDevice, (int)s.X - 20, 20);
            data = new Color[(int)(s.X - 20) * 20];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.LightCoral;
            }
            _rectSel.SetData(data);

            _font = content.Load<SpriteFont>("Fonts\\Arial16");
        }

        public ListViewer(T[] list, int sizeOfPage, Vector2 p, Vector2 s, ContentManager content)
        {
            _content = content;

            viewedList = new List<T>();
            for (int i = 0; i < list.Length; i++)
            {
                viewedList.Add(list[i]);
            }

            gs = GetString;

            pageSize = sizeOfPage;

            _sprites = Program.Game._spriteBatch;
            curIndex = -1;
            scrolledOffset = 0;

            position = p;
            size = s;

            _button = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, position, size);

            _rect = new Texture2D(Program.Game.GraphicsDevice, (int)s.X, (int)s.Y);

            Color[] data = new Color[(int)s.X * (int)s.Y];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Program.UI_COLOR;

                if (i % (int)s.X <= 1 || i / (int)s.X <= 1 || i % (int)s.X >= (int)s.X - 2 || i / (int)s.X >= (int)s.Y - 2)
                {
                    data[i] = Color.Red;
                }
            }

            _rect.SetData(data);

            _rectSel = new Texture2D(Program.Game.GraphicsDevice, (int)s.X - 20, 20);
            data = new Color[(int)(s.X - 20) * 20];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.LightCoral;
            }
            _rectSel.SetData(data);

            _font = content.Load<SpriteFont>("Fonts\\Arial16");
        }


        public void SetStringFunc(Func<T, int, string> func)
        {
            gs = func;
        }
        public void ResetStringFunc()
        {
            gs = GetString;
        }

        public void SetData(T[] list)
        {
            viewedList.Clear();
            for (int i = 0; i < list.Length; i++)
            {
                viewedList.Add(list[i]);
            }
        }
        public T[] GetData()
        {
            T[] arr = new T[viewedList.Count];

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = viewedList[i];
            }

            return arr;
        }

        public void Poll()
        {
            if (!_button.ButtonDown)
            {
                _button.Poll();
                return;
            }
            _button.Poll();

            if (!_button.ButtonDown) return;

            int scroll = Input.GetMouseWheelDeltaNormal();

            scroll = -scroll;
            //if (scroll != 0) System.Console.WriteLine($"Scroll: {scroll}");
            scrolledOffset += scroll;
            if (scrolledOffset >= viewedList.Count - pageSize) scrolledOffset = (viewedList.Count - pageSize) - 1;
            if (scrolledOffset < 0) scrolledOffset = 0;

            bool mdown = Input.IsMousePressed(Input.MouseButton.LeftMouse);
            Vector2 pos = Input.GetMousePosition() - position;

            if (mdown && pos.X <= size.X)
            {
                int idx = (int)pos.Y / 20;

                idx += scrolledOffset;
                if (idx >= 0 && idx < viewedList.Count)
                    curIndex = idx;
                else 
                {
                    curIndex = -1;
                }
            }
        }

        public T? GetSelectedItem()
        {
            if (curIndex < 0 && curIndex >= viewedList.Count) return default;

            return viewedList[curIndex];
        }

        public void PreRender() { }
        public void Render(bool beginSprites = true)
        {
            if (beginSprites)
                _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            if (_button.ButtonDown)
                _sprites.Draw(_rect, position, Color.White);
            else
                _sprites.Draw(_rect, position, Color.LightGray);


            Vector2 pos = position;
            pos.X += 5;
            pos.Y += 5;

            for (int K = scrolledOffset, N = 0; N < pageSize && K >= 0 && K < viewedList.Count; N++, K++)
            {
                if (K == curIndex)
                {
                    _sprites.Draw(_rectSel, pos, Color.LightCoral);
                }
                _sprites.DrawString(_font, gs(viewedList[K], K), pos, Color.Black);

                pos.Y += spacing;
            }

            if (beginSprites)
                _sprites.End();
        }

        public void Destroy()
        {
            _rect.Dispose();
            _button.Destroy();
        }

        private string GetString(T entry, int i)
        {
            return entry.ToString();
        }
    }
    public class PropertyViewer : ResourceInterface
    {
        public Vector2 position, size;
        private Texture2D _rect;
        private ButtonWidget2D _button;
        private SpriteFont _font;
        private SpriteBatch _sprites;
        private GraphicsDevice _graphics;
        private ContentManager _content;

        private List<PropertyWidget> _widgets;

        private int scrolled;
        private int scrollMax;

        public class PropertyWidget
        {
            public Vector2 position, size;
            public ButtonWidget2D button;
            public Texture2D rect;

            public bool HasHeader;
            public string HeaderText;

            public string DisplayedText;
            public bool trysetTemp;
            public string TempText;

            private Func<PropertyWidget, int> getStr;
            private Func<PropertyWidget, int> attSetStr;

            private GraphicsDevice _graphics;

            private void setup(GraphicsDevice device, Vector2 pos, Vector2 siz, bool dostring, string disstring, Func<PropertyWidget, int> get, Func<PropertyWidget, int> set)
            {
                _graphics = device;
                position = pos;
                size = siz;
                trysetTemp = false;

                HasHeader = dostring;
                HeaderText = disstring;

                getStr = get;
                attSetStr = set;

                button = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, pos, size);
                rect = new Texture2D(_graphics, (int)size.X, (int)size.Y);

                Color[] data = new Color[(int)size.X * (int)size.Y];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Program.UI_ALT_COLOR;
                }
                rect.SetData(data);

                get(this);
            }

            public void Destroy()
            {
                rect.Dispose();
                button.Destroy();
            }

            public PropertyWidget(GraphicsDevice graphics, Vector2 pos, Vector2 siz)
            {
                setup(graphics, pos, siz, false, "", dGS, dASS);
            }
            public PropertyWidget(GraphicsDevice device, Vector2 pos, Vector2 siz, Func<PropertyWidget, int> get, Func<PropertyWidget, int> set)
            {
                setup(device, pos, siz, false, "", get, set);
            }
            public PropertyWidget(GraphicsDevice device, Vector2 pos, Vector2 siz, string header, Func<PropertyWidget, int> get, Func<PropertyWidget, int> set)
            {
                setup(device, pos, siz, true, header, get, set);
            }

            public void Poll(Vector2 mpos)
            {
                button.Poll(mpos);

                if (button.ButtonDown)
                {
                    trysetTemp = true;

                    bool shift = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);
                    for (int i = 0; i < 256; i++)
                    {
                        if (!Enum.IsDefined(typeof(Keys), i)) continue;

                        if (Input.IsKeyPressed((Keys)i))
                        {
                            char ret = Input.GetChar((Keys)i, shift);
                            if (ret != 0)
                                TempText += ret;
                        }
                    }

                    if (Input.IsKeyPressed(Keys.Back))
                    {
                        if (TempText.Length > 0)
                        {
                            TempText = TempText.Remove(TempText.Length - 1, 1);
                        }
                    }

                    if (Input.IsKeyPressed(Keys.Enter))
                    {
                        attSetStr(this);
                        getStr(this);
                        trysetTemp = false;
                        button.Release();
                        TempText = "";
                    }

                    if (Input.IsKeyPressed(Keys.Escape))
                    {
                        trysetTemp = false;
                        TempText = "";
                        button.Release();
                    }
                }
                else
                {
                    attSetStr(this);
                    getStr(this);
                    trysetTemp = false;
                    button.Release();
                    TempText = "";
                }
            }
            public void Render(SpriteBatch _sprites, SpriteFont font, Vector2 origin, int ySub, Vector2 within)
            {
                Vector2 renPos = position + origin;
                renPos.Y -= ySub;

                Vector2 temp = renPos - origin;
                if (temp.X < 0 || temp.Y < 0 || temp.X >= within.X - size.X || temp.Y >= within.Y - size.Y) return;

                _sprites.Draw(rect, renPos, Color.LightGray);
                renPos.X += 5;
                renPos.Y += 5;

                if (trysetTemp)
                    _sprites.DrawString(font, TempText, renPos, Color.White);
                else
                {
                    getStr(this);
                    _sprites.DrawString(font, DisplayedText, renPos, Color.White);
                }

                if (HasHeader)
                {
                    renPos.Y -= 25;
                    _sprites.DrawString(font, HeaderText, renPos, Color.Black);
                }

            }

            private static int dGS(PropertyWidget widget)
            {
                return 0;
            }
            private static int dASS(PropertyWidget widget)
            {
                widget.DisplayedText = widget.TempText;
                return 0;
            }
        }

        public PropertyViewer(GraphicsDevice graphics, Vector2 pos, Vector2 siz, int maxScroll)
        {
            _graphics = graphics;
            _sprites = Program.Game._spriteBatch;
            position = pos;
            size = siz;
            scrolled = 0;
            scrollMax = maxScroll;
            _content = Program.Game.Content;

            _font = _content.Load<SpriteFont>("Fonts\\Arial16");

            _button = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, position, size);

            _rect = new Texture2D(_graphics, (int)size.X, (int)size.Y);

            Color[] data = new Color[(int)size.X * (int)size.Y];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Program.UI_COLOR;

                if (i % (int)size.X <= 1 || i / (int)size.X <= 1 || i % (int)size.X >= (int)size.X - 2 || i / (int)size.X >= (int)size.Y - 2)
                {
                    data[i] = Color.Red;
                }
            }

            _rect.SetData(data);

            _widgets = new List<PropertyWidget>();
        }

        public void AddPropertyWidget(PropertyWidget widget)
        {
            _widgets.Add(widget);
        }

        public void Poll()
        {
            _button.Poll();

            if (!_button.ButtonDown)
            { //Reset any selections

                for (int i = 0; i < _widgets.Count; i++)
                {
                    _widgets[i].TempText = "";
                    _widgets[i].trysetTemp = false;
                }
            }
            else
            {
                Vector2 mPos = Input.GetMousePosition();
                int scroll = -Input.GetMouseWheelDeltaNormal() * 10;

                scrolled += scroll;
                if (scrolled < 0) scrolled = 0;
                else if (scrolled > scrollMax) scrolled = scrollMax;

                mPos.Y += scrolled;

                mPos -= position;

                for (int i = 0; i < _widgets.Count; i++)
                {
                    _widgets[i].Poll(mPos);
                }
            }
        }
        public void PreRender() { }
        public void Render(bool beginSprites = true)
        {
            if (beginSprites)
                Program.SpritesBeginDefault(_sprites);

            if (!_button.ButtonDown)
            {
                _sprites.Draw(_rect, position, Color.LightGray);
            } 
            else
            {
                _sprites.Draw(_rect, position, Color.White);
            }

            for (int i = 0; i < _widgets.Count; i++)
            {
                _widgets[i].Render(_sprites, _font, position, scrolled, size);
            }

            if (beginSprites)
                _sprites.End();
        }
        public void Destroy()
        {
            _rect.Dispose();

            for (int i = 0; i < _widgets.Count; i++)
            {
                _widgets[i].Destroy();
            }
        }
    }
    public class Brush
    {
        private int id;
        private static int id_const;
        public int ID => id;

        public VertexModel physical;
        public string[] textures;

        public List<VertexModel.TexturedPolygon> modelPolys;

        //Add all details here

        public Brush()
        {
            id = id_const++;
            modelPolys = new List<VertexModel.TexturedPolygon>();
        }

        public void Bind(VertexModel mod)
        {
            modelPolys.Clear();
            for (int i = 0; i < mod.polygons.Length; i++)
            {
                modelPolys.Add(mod.polygons[i]);
            }
            physical = mod;
        }

        /// <summary>
        /// Updates the physical model to be equivalent 
        /// </summary>
        public void Correct()
        {

        }

        public override string ToString()
        {
            return $"{ID}";
        }
    }
    public class BrushLibrary : ResourceInterface
    {
        public List<Brush> brushes;

        public ListViewer<Brush> brushViewer;

        public ButtonWidget2D button;

        private GameScene _parent;
        private GraphicsDevice _graphics;
        private ContentManager _content;
        private Texture2D _rect;
        private SpriteBatch _sprites;

        public BrushLibrary(GameScene parent)
        {
            _parent = parent;
            _graphics = parent._game.GraphicsDevice;
            _content = parent._game.Content;
            _sprites = parent._game._spriteBatch;

            brushes = new List<Brush>();
            Vector2 pos = new Vector2(Program.InternalScreen.Y, 32);
            pos.X += ((Program.InternalScreen.X - Program.InternalScreen.Y) / 3f) * 2;

            Vector2 size = new Vector2((Program.InternalScreen.X - Program.InternalScreen.Y) / 3f, Program.InternalScreen.Y);
            brushViewer = new ListViewer<Brush>(brushes, 32, pos, size, _content);

            pos.Y = 0;
            button = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, pos, size);

            size.Y = 32;

            _rect = new Texture2D(_graphics, (int)size.X, (int)size.Y);

            Color[] data = new Color[(int)size.X * (int)size.Y];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Gray;
            }

            _rect.SetData(data);
        }

        public void Poll()
        {
            button.Poll();
            brushViewer.Poll();

            if (button.ButtonDown)
            {
                if (Input.IsKeyPressed(Keys.N)) //generate fresh cuboid brush
                {
                    VertexModel temp = VertexModel.CreateGeneric(_graphics);
                    Brush brush = new Brush();
                    brush.Bind(temp);

                    brushes.Add(brush);
                }

                if (Input.IsKeyPressed(Keys.Delete) && (Input.IsKeyDown(Keys.LeftControl) || Input.IsKeyDown(Keys.RightControl)))
                {
                    if (brushViewer.curIndex != -1)
                    {
                        brushes.RemoveAt(brushViewer.curIndex);
                        brushViewer.curIndex = -1;
                    }
                }
            }
        }

        public void PreRender() { }
        public void Render(bool beginSprites = true)
        {
            if (beginSprites)
                _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            _sprites.Draw(_rect, button.screenPos, Color.Gray);

            brushViewer.Render(false);

            if (beginSprites)
                _sprites.End();
        }
        public void Destroy()
        {
            _rect.Dispose();
            brushViewer.Destroy();
        }
    }
    public class MapFile : ResourceInterface
    {
        public List<Brush> mapBrushes;
        public ListViewer<Brush> mapBrushViewer;

        private GameScene parent;
        private SpriteBatch _sprites;

        public MapFile(GameScene scene)
        {
            parent = scene;
            _sprites = parent._game._spriteBatch;

            Vector2 pos = new Vector2(Program.InternalScreen.Y, 0);
            Vector2 siz = new Vector2((Program.InternalScreen.X - Program.InternalScreen.Y) / 3f, Program.InternalScreen.Y / 2);
            pos.X += siz.X;
            mapBrushes = new List<Brush>();
            mapBrushViewer = new ListViewer<Brush>(mapBrushes, 10, pos, siz, Program.Game.Content);
        }

        public void Poll()
        {
            mapBrushViewer.Poll();
        }
        public void PreRender()
        {

        }
        public void Render(bool beginSprites = true)
        {
            if (beginSprites)
                Program.SpritesBeginDefault(_sprites);

            mapBrushViewer.Render(false);

            if (beginSprites)
                _sprites.End();
        }
        public void Destroy()
        {
            mapBrushViewer.Destroy();
        }
    }

    public class PerspectiveViewer
    {
        public static int PERSPECTIVE_ID = 0;
        public static int BIRDSEYE_ID = 1;
        public static int FORWARD_ID = 2;
        public static int SIDE_ID = 3;

        protected GraphicsDevice _graphics;
        protected GameScene _parent;
        protected SpriteBatch _sprites;
        protected SpriteFont font;
        internal static DepthStencilState _dss = new DepthStencilState() { DepthBufferEnable = true, DepthBufferFunction = CompareFunction.Less };

        //RENDER TARGET DEETS
        public class PerspectiveViewerWindow
        {
            public int ID;
            public RenderTarget2D Target;
            public Rectangle Culler;
            public Vector2 TargetOrigin;
            public ButtonWidget2D Focuser;
            public Camera Camera;
            public float Range, Aim;

            public Vector3 Local_X, Local_Y, Local_Z, Position;
            public Vector3 Flatten;

            private GraphicsDevice _graphics;
            private GameScene _parent;
            private SpriteBatch _sprites;

            public PerspectiveViewerWindow(GameScene parent, int id, Vector2 origin, Vector2 dimensions)
            {
                _parent = parent;
                _graphics = parent._game.GraphicsDevice;
                _sprites = parent._game._spriteBatch;

                ID = id;

                TargetOrigin = origin;
                Target = new RenderTarget2D(_graphics, (int)dimensions.X, (int)dimensions.Y, true, SurfaceFormat.Alpha8, DepthFormat.Depth24Stencil8);

                Culler = new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y);

                Focuser = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, origin, dimensions);
                
                Camera = new Camera(_graphics);

                Position = Vector3.Zero;

                Aim = 0;
                Range = 1;
            }

            bool ortho = false;
            public void BuildPerspective(float n, float f)
            {
                Local_X = Vector3.UnitX;
                Local_Y = Vector3.UnitY;
                Local_Z = Vector3.UnitZ;

                Flatten = Vector3.One;

                Camera.Recompile(n, f);

                ortho = false;
            }

            public void BuildOrthographic(Vector3 f, Vector3 u, Vector3 r)
            {
                Local_X = r;
                Local_Z = f;
                Local_Y = u;

                Flatten = Vector3.One - f;

                Camera.TargetUp = Local_Y;
                Camera.TargetAngle = Local_Z;
                Camera.Position = Position + (Local_Z * Aim);

                Camera.Recompile(10, 10, -Range, Range);

                ortho = true;
            }
            public Vector2 GetVector2(Vector3 pos)
            {
                pos *= Flatten; 
                Vector3 comp;
                float x = 0;
                float y = 0;

                comp = pos * Local_X;
                if (Local_X.X != 0) x = comp.X;
                if (Local_X.Y != 0) x = comp.Y;
                if (Local_X.Z != 0) x = comp.Z;

                comp = -pos * Local_Y;
                if (Local_Y.X != 0) y = comp.X;
                if (Local_Y.Y != 0) y = comp.Y;
                if (Local_Y.Z != 0) y = comp.Z;

                return new Vector2(x, y);
            }

            public Vector3 GetVector3(Vector2 pos, Vector3 core)
            {
                Vector3 fresh = (Local_X * pos.X) + (Local_Y * -pos.Y);

                fresh += (core * -Local_Z);

                return fresh;
            }

            public void Poll()
            {
                Focuser.Poll();
                if (ortho)
                {
                    Camera.Position = Position + (Local_Z * Aim);
                    Camera.Recompile(10, 10, -Range, Range);
                }
            }

            public void Destroy()
            {
                Target.Dispose();
                Focuser.Destroy();
            }
        }
        protected int _targetID;

        protected PerspectiveViewerWindow[] windows;
        protected string[] names = { "Perspective", "Top", "Front", "Side" };
        protected Color[] colors =
        {
            Color.White,
            Color.Red,
            Color.Green,
            Color.Blue
        };

        public PerspectiveViewer(GameScene parent)
        {
            _parent = parent;
            _graphics = parent._game.GraphicsDevice;
            _sprites = _parent._game._spriteBatch;

            _targetID = -1;

            windows = new PerspectiveViewerWindow[4];
            Vector2 size = new Vector2(Program.InternalScreen.Y / 2f, Program.InternalScreen.Y / 2f);
            for (int i = 0; i < windows.Length; i++)
            {
                Vector2 temp = new Vector2(i % 2, i / 2);
                temp *= Program.InternalScreen.Y / 2;

                windows[i] = new PerspectiveViewerWindow(_parent, i, temp, size);
            }
            font = Program.Game.Content.Load<SpriteFont>("Fonts\\Arial16");

            //Setup cameras to be appropriate

            //Cam 0 is just normal 3d space camera
            windows[0].BuildPerspective(0.1f, 100);

            //Cam 1 will be birds eye with the up angle being forward, point to the right
            windows[1].BuildOrthographic(Vector3.Down, Vector3.Forward, Vector3.Right);

            //Cam 2 will be forwards
            windows[2].BuildOrthographic(Vector3.Forward, Vector3.Up, Vector3.Right);

            //Cam 3 will be sideways
            windows[3].BuildOrthographic(Vector3.Left, Vector3.Up, Vector3.Forward);
        }
        public void prepoll()
        {
            for (int i = 0; i < 4; i++)
            {
                windows[i].Poll();
                if (windows[i].Focuser.ButtonDown)
                {
                    _targetID = i;
                }
            }

            Poll();

            if (windows[PERSPECTIVE_ID].Focuser.ButtonDown)
                DefaultProcess(windows[PERSPECTIVE_ID]);

            if (windows[BIRDSEYE_ID].Focuser.ButtonDown)
                OrthoProcess(windows[BIRDSEYE_ID], WindowContext.Birdseye);

            if (windows[FORWARD_ID].Focuser.ButtonDown)
                OrthoProcess(windows[FORWARD_ID], WindowContext.Forward);

            if (windows[SIDE_ID].Focuser.ButtonDown)
                OrthoProcess(windows[SIDE_ID], WindowContext.Side);
        }
        public virtual void Poll() { }
        public void prerender()
        {
            _graphics.SetRenderTarget(windows[PERSPECTIVE_ID].Target);
            DefaultRender(windows[PERSPECTIVE_ID]);

            for (int i = BIRDSEYE_ID; i <= SIDE_ID; i++)
            {
                _graphics.SetRenderTarget(windows[i].Target);
                OrthoRender(windows[i], (WindowContext)i);
            }
        }
        public void render()
        {
            _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            for (int i = 0; i < 4; i++)
            {
                _sprites.Draw(windows[i].Target, windows[i].TargetOrigin, windows[i].Culler, Color.White);
            }

            _sprites.End();

            AddlRender();
        }
        public virtual void AddlRender() { }

        //Default perspective mode
        public virtual void DefaultProcess(PerspectiveViewerWindow window) { }
        public virtual void DefaultRender(PerspectiveViewerWindow window) { }

        //Orthographic process (will apply to all 3 windows)
        public enum WindowContext
        {
            Perspective = 0,
            Birdseye = 1,
            Forward = 2,
            Side = 3
        }
        public virtual void OrthoProcess(PerspectiveViewerWindow window, WindowContext context) 
        {
            Vector3 Y = Input.KeyDelta(Keys.W, Keys.S) * window.Local_Y;
            Vector3 X = Input.KeyDelta(Keys.A, Keys.D) * window.Local_X;

            Y *= (float)(Program.Game.FrameTime * 5);
            X *= (float)(Program.Game.FrameTime * 5);

            window.Position += Y + X;

            int scroll = Input.GetMouseWheelDeltaNormal();

            window.Aim += (float)(scroll / Program.Game.FrameRate) * 10;

            int rangeDelta = Input.KeyDelta(Keys.E, Keys.Q);

            window.Range += (float)(rangeDelta / Program.Game.FrameRate);
            if (window.Range < 0) window.Range = 0;
        }
        public virtual void OrthoRender(PerspectiveViewerWindow window, WindowContext context) 
        {
            Program.SpritesBeginDefault(_sprites);

            _sprites.DrawString(font, names[window.ID], Vector2.Zero, colors[window.ID]);
            _sprites.DrawString(font, $"({Program.Vec3ToString(window.Camera.Position)})", new Vector2(0, 20), Color.White);
            _sprites.DrawString(font, $"{String.Format("{0:0.00}", window.Range)}", new Vector2(0, 40), Color.White);

            _sprites.End();
        }

        public virtual void Destroy()
        {
            for (int i = 0; i < 4; i++)
            {
                windows[i].Destroy();
            }
        }
    }
    public class MapEditor : PerspectiveViewer
    {
        public BrushLibrary library;
        public MapFile map;

        public MapEditor(GameScene parent, BrushLibrary lib) :  base(parent)
        {
            library = lib;
            map = new MapFile(parent);
        }

        public override void Destroy()
        {
            base.Destroy();
            map.Destroy();
        }
        public override void Poll()
        {
            base.Poll();
            map.Poll();
            library.Poll();
        }
        public override void AddlRender()
        {
            Program.SpritesBeginDefault(_sprites);
            map.Render(false);
            library.Render(false);

            _sprites.End();
        }
    }
    public class BrushEditor : PerspectiveViewer
    {
        public Brush CurrentBrush;
        public BrushLibrary library;

        protected ListViewer<VertexPositionColorTexture> polyList;
        protected ListViewer<VertexModel.TexturedPolygon> brushContentsList;
        protected PropertyViewer vertexPropertyViewer;

        protected PropertyViewer brushPropertyViewer;

        protected PropertyViewer brushPolygonPropertyViewer;

        protected VertexModel[] xyzPlanes;
        protected Texture2D circleTex;
        protected Vector2 circleOffset;
        private bool renderPlanes;
        public BrushEditor(GameScene parent, BrushLibrary lib) : base(parent)
        {
            library = lib;
            Vector2 p = new Vector2(Program.InternalScreen.Y, 0);

            //The size of a single "small" scroller
            Vector2 s = new Vector2((Program.InternalScreen.X - Program.InternalScreen.Y) / 3f, Program.InternalScreen.Y / 3f);
            
            brushContentsList = new ListViewer<VertexModel.TexturedPolygon>(new List<VertexModel.TexturedPolygon>(), 10, p, s, Program.Game.Content);

            p.Y += Program.InternalScreen.Y / 3f;
            s.Y /= 2;
            polyList = new ListViewer<VertexPositionColorTexture>(new List<VertexPositionColorTexture>(), 10, p, s, Program.Game.Content);
            polyList.SetStringFunc((VertexPositionColorTexture ent, int i) =>
            {
                return $"{i}: ({Program.Vec3ToString(ent.Position)})";
            });

            p.Y += (Program.InternalScreen.Y / 3f) / 2;
            brushPolygonPropertyViewer = new PropertyViewer(_graphics, p, s, 0);

            s.Y *= 2; 
            p.Y += (Program.InternalScreen.Y / 3f) / 2;

            vertexPropertyViewer = new PropertyViewer(_graphics, p, s, 100);

            SetupVertexProperty();

            p = new Vector2(Program.InternalScreen.Y, 0);
            p.X += s.X;
            s.Y = Program.InternalScreen.Y;

            brushPropertyViewer = new PropertyViewer(_graphics, p, s, 0);

            SetupBrushProperty();


            CurrentBrush = null;

            windows[PERSPECTIVE_ID].Camera.TargetAngle = new Vector3(-1, -1, -1);
            windows[PERSPECTIVE_ID].Camera.Position = windows[PERSPECTIVE_ID].Camera.TargetAngle * -d_Distance;

            xyzPlanes = new VertexModel[4];

            int opac = 64;
            
            //X facing (SIDE)
            xyzPlanes[SIDE_ID] = new VertexModel(_graphics, 1);
            xyzPlanes[SIDE_ID].polygons[0] = new VertexModel.TexturedPolygon(_graphics, 4, 2, PrimitiveType.TriangleStrip);
            xyzPlanes[SIDE_ID].polygons[0].verticies = VertexModel.Generate4Point(Vector3.Zero, Vector3.UnitY * 5, Vector3.UnitZ * 5, new Color(colors[SIDE_ID], opac));

            //Z facing (FORWARD)
            xyzPlanes[FORWARD_ID] = new VertexModel(_graphics, 1);
            xyzPlanes[FORWARD_ID].polygons[0] = new VertexModel.TexturedPolygon(_graphics, 4, 2, PrimitiveType.TriangleStrip);
            xyzPlanes[FORWARD_ID].polygons[0].verticies = VertexModel.Generate4Point(Vector3.Zero, Vector3.UnitY * 5, Vector3.UnitX * 5, new Color(colors[FORWARD_ID], opac));

            //Y facing (TOP)
            xyzPlanes[BIRDSEYE_ID] = new VertexModel(_graphics, 1);
            xyzPlanes[BIRDSEYE_ID].polygons[0] = new VertexModel.TexturedPolygon(_graphics, 4, 2, PrimitiveType.TriangleStrip);
            xyzPlanes[BIRDSEYE_ID].polygons[0].verticies = VertexModel.Generate4Point(Vector3.Zero, Vector3.UnitZ * 5, Vector3.UnitX * 5, new Color(colors[BIRDSEYE_ID], opac));

            for (int i = 1; i < 4; i++)
            {
                xyzPlanes[i].Recompile(VertexModel.COMPILE_POLYGONS);
                xyzPlanes[i].position = windows[i].Camera.Position;
            }

            circleTex = new Texture2D(_graphics, 51, 51);
            Color[] data = new Color[51 * 51];
            circleOffset = new Vector2(25, 25);

            for (float x = -25, k = 0; x <= 25; x++)
            {
                for (float y = -25; y <= 25; y++, k++)
                {
                    float del = (x * x) + (y * y);
                    del = (float)Math.Sqrt(del);
                    if (del <= 25)
                    {
                        data[(int)k] = Color.White;
                    }
                    else
                    {
                        data[(int)k] = new Color(0, 0, 0, 0);
                    }
                }
            }

            circleTex.SetData(data);
        }

        public void SetupVertexProperty()
        {
            Vector2 pos = new Vector2(15, 50);
            Vector2 siz = new Vector2(80, 30);
            //POSITION X
            PropertyViewer.PropertyWidget widget = new PropertyViewer.PropertyWidget(_graphics, pos, siz, "Position (XYZ)", (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = String.Format("{0:0.00}", CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Position.X);
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Position.X = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            });
            vertexPropertyViewer.AddPropertyWidget(widget);

            //POSITION Y (NO HEADER)
            pos.X += 85;
            widget = new PropertyViewer.PropertyWidget(_graphics, pos, siz, (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = String.Format("{0:0.00}", CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Position.Y);
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Position.Y = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            });
            vertexPropertyViewer.AddPropertyWidget(widget);

            //POSITION Z (NO HEADER)
            pos.X += 85;
            widget = new PropertyViewer.PropertyWidget(_graphics, pos, siz, (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = String.Format("{0:0.00}", CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Position.Z);
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Position.Z = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            });
            vertexPropertyViewer.AddPropertyWidget(widget);

            pos.X = 15;
            pos.Y += 75;

            siz.X = 50;

            //COLOR R
            vertexPropertyViewer.AddPropertyWidget(new PropertyViewer.PropertyWidget(_graphics, pos, siz, "Color (RGB)", (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = $"{CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.R}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.R = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            }));

            //COLOR G
            pos.X += 85;
            vertexPropertyViewer.AddPropertyWidget(new PropertyViewer.PropertyWidget(_graphics, pos, siz, (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = $"{CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.G}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.G = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            }));

            //COLOR B
            pos.X += 85;
            vertexPropertyViewer.AddPropertyWidget(new PropertyViewer.PropertyWidget(_graphics, pos, siz, (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = $"{CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.B}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.B = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            }));

            pos.X = 15;
            pos.Y += 75;

            //COLOR R
            vertexPropertyViewer.AddPropertyWidget(new PropertyViewer.PropertyWidget(_graphics, pos, siz, "Alpha", (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = $"{CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.A}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].Color.A = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            }));

            pos.Y += 75;
            siz.X = 100;

            //TEXTURE X
            vertexPropertyViewer.AddPropertyWidget(new PropertyViewer.PropertyWidget(_graphics, pos, siz, "Texture Coordinates (X/Y)", (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = $"{CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].TextureCoordinate.X}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].TextureCoordinate.X = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            }));

            //TEXTURE Y

            pos.X += 105;
            vertexPropertyViewer.AddPropertyWidget(new PropertyViewer.PropertyWidget(_graphics, pos, siz, (PropertyViewer.PropertyWidget parent) => //Get
            {
                if (brushContentsList.curIndex == -1 || selVert == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return 0;
                }
                parent.DisplayedText = $"{CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].TextureCoordinate.Y}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies[selVert].TextureCoordinate.Y = temp;
                    CurrentBrush.physical.polygons[brushContentsList.curIndex].Recompile();
                    polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);
                }
                return 0;
            }));
        }

        public void SetupBrushProperty()
        {
            Vector2 pos = new Vector2(15, 30);
            Vector2 siz = new Vector2(250, 30);

            brushPolygonPropertyViewer.AddPropertyWidget(new PropertyViewer.PropertyWidget(_graphics, pos, siz, "Texture", (PropertyViewer.PropertyWidget parent) => //get
            {
                if (brushContentsList.curIndex == -1 || library.brushViewer.curIndex == -1)
                {
                    parent.DisplayedText = "UNDF";
                    return -1;
                }
                if (!CurrentBrush.modelPolys[brushContentsList.curIndex].textureBuilt)
                {
                    parent.DisplayedText = "Default";
                }
                else
                {
                    parent.DisplayedText = CurrentBrush.modelPolys[brushContentsList.curIndex].texture.Name;
                }
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //set
            {
                if (brushContentsList.curIndex == -1 || library.brushViewer.curIndex == -1) return -1;
                if (parent.TempText.ToLower() == "default")
                {
                    CurrentBrush.modelPolys[brushContentsList.curIndex].textureBuilt = false;
                    return 0;
                }
                try
                {
                    CurrentBrush.modelPolys[brushContentsList.curIndex].texture = Program.Game.Content.Load<Texture2D>(parent.TempText);
                    CurrentBrush.modelPolys[brushContentsList.curIndex].textureBuilt = true;
                    parent.TempText = "";
                }
                catch (Exception e) { }
                return 0;
            }));

        }

        public override void Destroy()
        {
            base.Destroy();
            circleTex.Dispose();
            brushContentsList.Destroy();
            vertexPropertyViewer.Destroy();
            brushPropertyViewer.Destroy();
        }

        int lastIndexLib = -1;
        int lastIndexBrush = -1;
        public override void Poll()
        {
            brushContentsList.Poll();
            brushPropertyViewer.Poll();
            library.Poll();
            polyList.Poll();

            //Select brush from the library
            if (lastIndexLib != library.brushViewer.curIndex)
            {
                lastIndexLib = library.brushViewer.curIndex;
                selVert = -1;
                polyList.curIndex = -1;
                mouseState = 0;
                if (lastIndexLib != -1)
                {
                    CurrentBrush = library.brushViewer.viewedList[lastIndexLib]; 
                    brushContentsList.viewedList = CurrentBrush.modelPolys;
                    rendersides = true;
                    brushContentsList.curIndex = -1;
                }
                else
                {
                    CurrentBrush = null;
                    brushContentsList.viewedList = new List<VertexModel.TexturedPolygon>();
                    rendersides = true;
                    brushContentsList.curIndex = -1;
                }
            }

            //Select polygon in the brush
            if (lastIndexBrush != brushContentsList.curIndex)
            {
                lastIndexBrush = brushContentsList.curIndex;
                selVert = -1;
                polyList.curIndex = -1;
                mouseState = 0;
                if (lastIndexBrush != -1)
                {
                    polyList.SetData(CurrentBrush.physical.polygons[lastIndexBrush].verticies);
                }
                else
                {
                    polyList.viewedList = new List<VertexPositionColorTexture>();
                }
            }
            
            //Select vertice from the current polygon
            if (selVert != polyList.curIndex)
            {
                selVert = polyList.curIndex;
                if (selVert != -1)
                {
                    mouseState = 1;
                }
                else
                {
                    mouseState = 0;
                }
            }

            //Poll if only active
            if (selVert != -1)
            {
                vertexPropertyViewer.Poll();
            }
            if (brushContentsList.curIndex != -1)
            {
                brushPolygonPropertyViewer.Poll();
            }

            //brush contents delete/create polys
            if (brushContentsList.ButtonActive)
            {
                if (brushContentsList.curIndex != -1 && (Input.IsKeyDown(Keys.LeftControl) || Input.IsKeyDown(Keys.RightControl)) && Input.IsKeyPressed(Keys.Delete)) //deletes
                {
                    VertexModel temp = CurrentBrush.physical;
                    VertexModel.TexturedPolygon[] polyNew = new VertexModel.TexturedPolygon[temp.polygons.Length - 1];
                    for (int i = 0, ext = 0; i < polyNew.Length; i++, ext++)
                    {
                        if (ext == brushContentsList.curIndex) ext++;
                        polyNew[i] = temp.polygons[ext];
                    }
                    temp.polygons = polyNew;

                    CurrentBrush.Bind(temp);
                    CurrentBrush.physical.Recompile(VertexModel.COMPILE_POLYGONS);
                    brushContentsList.curIndex = -1;
                    selVert = -1;
                }

                if (Input.IsKeyDown(Keys.LeftControl) || Input.IsKeyDown(Keys.RightControl)) //creates
                {
                    VertexModel.TexturedPolygon insert = null;
                    bool y = false;
                    if (Input.IsKeyPressed(Keys.D3))
                    {
                        y = true;
                        insert = new VertexModel.TexturedPolygon(_graphics, 3, 1, PrimitiveType.TriangleList);
                        insert.verticies = VertexModel.Generate3Point(Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY);
                    }
                    else if (Input.IsKeyPressed(Keys.D4))
                    {
                        y = true;
                        insert = new VertexModel.TexturedPolygon(_graphics, 4, 2, PrimitiveType.TriangleStrip);
                        insert.verticies = VertexModel.Generate4Point(Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY);
                    }

                    if (y)
                    {
                        VertexModel temp = CurrentBrush.physical;
                        VertexModel.TexturedPolygon[] polyNew = new VertexModel.TexturedPolygon[temp.polygons.Length + 1];
                        for (int i = 0; i < temp.polygons.Length; i++)
                        {
                            polyNew[i] = temp.polygons[i];
                        }
                        polyNew[temp.polygons.Length] = insert;
                        temp.polygons = polyNew;
                        CurrentBrush.Bind(temp);
                        CurrentBrush.physical.Recompile(VertexModel.COMPILE_POLYGONS);
                    }
                }
            }
        }
        public override void AddlRender()
        {
            _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);
            
            brushContentsList.Render(false);
            brushPropertyViewer.Render(false);

            if (selVert != -1)
                vertexPropertyViewer.Render(false);

            if (brushContentsList.curIndex != -1)
                brushPolygonPropertyViewer.Render(false);

            if (lastIndexBrush != -1)
                polyList.Render(false);

            library.Render(false);

            _sprites.End();
        }

        //DEFAULT PERSPECTIVE
        float d_Distance = 5;
        bool rendersides = true;
        public override void DefaultProcess(PerspectiveViewerWindow window)
        {
            int scroll = -Input.GetMouseWheelDeltaNormal();

            d_Distance += (float)(scroll / Program.Game.FrameRate) * 10;
            window.Camera.Position = window.Camera.TargetAngle * -d_Distance;

            if (Input.IsKeyPressed(Keys.P))
            {
                renderPlanes = !renderPlanes;
            }
            if (Input.IsKeyPressed(Keys.R))
            {
                rendersides = !rendersides;
            }
        }
        public override void DefaultRender(PerspectiveViewerWindow window)
        {
            _graphics.Clear(Color.CornflowerBlue);
            _graphics.DepthStencilState = _dss;

            window.Camera.SetupDraw();

            if (CurrentBrush != null)
            {
                Vector3 temp = CurrentBrush.physical.position;
                CurrentBrush.physical.position = Vector3.Zero;

                //TODO: Optional mode to render only selected polygon
                if (brushContentsList.curIndex != -1 && !rendersides)
                {
                    CurrentBrush.physical.Draw(window.Camera, brushContentsList.curIndex);
                }
                else
                {
                    CurrentBrush.physical.Draw(window.Camera);
                }

                CurrentBrush.physical.position = temp;
            }

            if (renderPlanes)
            {
                for (int i = 1; i < 4; i++)
                {
                    xyzPlanes[i].position = windows[i].Camera.Position;
                    xyzPlanes[i].Draw(window.Camera);
                }
            }

            Program.SpritesBeginDefault(_sprites);
            _sprites.DrawString(font, names[0], Vector2.Zero, colors[0]);

            _sprites.End();
        }

        //Orthos
        int selVert = -1;
        int mouseState = 0;
        public override void OrthoProcess(PerspectiveViewerWindow window, WindowContext context)
        {
            base.OrthoProcess(window, context);

            if (brushContentsList.curIndex != -1)
            {
                Vector2 mp = Input.GetMousePosition() - window.TargetOrigin;
                Vector2 pos;

                VertexModel.TexturedPolygon poly = CurrentBrush.physical.polygons[brushContentsList.curIndex];
                switch (mouseState)
                {
                    case 0:
                        //Acquire an initial vertex ID to refer by
                        if (Input.IsMousePressed(Input.MouseButton.LeftMouse))
                        { //Check all to see if it is within 25 of the point
                            bool clickWithin = false;

                            for (int i = (selVert+1)%poly.verticies.Length, k = 0; k < poly.verticies.Length; i = (i + 1) % poly.verticies.Length, k++)
                            {
                                VertexPositionColorTexture det = poly.verticies[i];

                                pos = window.GetVector2(det.Position);

                                pos.X *= (window.Target.Width / 10);
                                pos.Y *= (window.Target.Height / 10);

                                pos += new Vector2(window.Target.Width, window.Target.Height) / 2;

                                pos -= mp;

                                if (pos.Length() <= 25)
                                {
                                    selVert = i;
                                    polyList.curIndex = selVert;
                                    clickWithin = true;
                                    break;
                                }
                            }

                            if (!clickWithin)
                            {
                                selVert = -1;
                                break;
                            }

                            mouseState = 1;
                        }
                        break;
                    case 1:
                        //Allow for freemove: if click outside, go to 0

                        pos = window.GetVector2(poly.verticies[selVert].Position);

                        pos.X *= (window.Target.Width / 10);
                        pos.Y *= (window.Target.Height / 10);

                        pos += new Vector2(window.Target.Width, window.Target.Height) / 2;

                        pos -= mp;

                        if (Input.IsMousePressed(Input.MouseButton.LeftMouse))
                        {
                            if (pos.Length() <= 25)
                            {
                                mouseState = 2;
                            }
                            else
                            {
                                mouseState = 0;
                                selVert = -1;
                                polyList.curIndex = -1;
                            }
                        }

                        break;
                    case 2:
                        //Manip a vertex

                        Vector3 MouseProjection;

                        Vector2 MouseOrigin = mp;
                        if (MouseOrigin.X < 0 || MouseOrigin.Y < 0 || MouseOrigin.X > window.Target.Width || MouseOrigin.Y > window.Target.Height) mouseState = 1;

                        MouseOrigin -= new Vector2(window.Target.Width, window.Target.Height) / 2;

                        MouseOrigin.Y /= (window.Target.Height / 10);
                        MouseOrigin.X /= (window.Target.Width / 10);

                        MouseProjection = window.GetVector3(MouseOrigin, poly.verticies[selVert].Position);

                        poly.verticies[selVert].Position = MouseProjection;
                        poly.Recompile();
                        polyList.SetData(CurrentBrush.physical.polygons[brushContentsList.curIndex].verticies);

                        if (!Input.IsMouseDown(Input.MouseButton.LeftMouse))
                        {
                            mouseState = 1;
                        }
                        break;
                }
            }
        }
        public override void OrthoRender(PerspectiveViewerWindow window, WindowContext context)
        {
            _graphics.Clear(Color.Black);

            if (CurrentBrush != null)
            {
                window.Camera.SetupDraw();

                Vector3 temp = CurrentBrush.physical.position;
                CurrentBrush.physical.position = Vector3.Zero;

                if (brushContentsList.curIndex == -1) //whole shape
                {
                    CurrentBrush.physical.Draw(window.Camera);

                    CurrentBrush.physical.position = temp;
                }

                else //draw single poly + dots to represent its verts
                {
                    CurrentBrush.physical.Draw(window.Camera, brushContentsList.curIndex);

                    VertexModel.TexturedPolygon poly = CurrentBrush.physical.polygons[brushContentsList.curIndex];
                    Program.SpritesBeginDefault(_sprites);
                    for (int i = 0; i < poly.verticies.Length; i++)
                    {
                        VertexPositionColorTexture det = poly.verticies[i];

                        Vector2 pos = window.GetVector2(det.Position);

                        pos.X *= (window.Target.Width / 10);
                        pos.Y *= (window.Target.Height / 10);

                        pos += new Vector2(window.Target.Width, window.Target.Height) / 2;

                        if (selVert == i)
                        {
                            _sprites.Draw(circleTex, (pos - circleOffset), Color.Gold);
                        }
                        else
                        {
                            _sprites.Draw(circleTex, (pos - circleOffset), Color.White);
                        }
                    }
                    _sprites.End();
                }
            }

            base.OrthoRender(window, context);
        }
    }
}
