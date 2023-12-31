﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using prog;
using System;
using System.Collections.Generic;
using static core.src.PropertyViewer;

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
            curViewer.BindCommands();
        }

        public override void OnFrame()
        {
            if (Input.IsKeyPressed(Keys.F1))
            {
                Program.Game.Window.Title = "Map Editor";
                curViewer.UnbindCommands();
                curViewer = mapEditor;
                curViewer.BindCommands();
            }
            if (Input.IsKeyPressed(Keys.F2))
            {
                Program.Game.Window.Title = "Brush Editor";
                curViewer.UnbindCommands();
                curViewer = brushEditor;
                curViewer.BindCommands();
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

            public void SetVertsColor(Color c)
            {
                for (int i = 0; i < verticies.Length; i++)
                {
                    verticies[i].Color = c;
                }
                Compile();
            }

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

            public Polygon(Polygon origin, bool cmp = true)
            {
                verticies = new VertexPositionColorTexture[origin.verticies.Length];
                for (int i = 0; i < verticies.Length; i++)
                {
                    verticies[i] = origin.verticies[i];
                }

                texture = origin.texture;
                _graphics = origin._graphics;
                id = id_master++;
                built = origin.built;
                textureBuilt = origin.textureBuilt;
                primitiveType = origin.primitiveType;
                primitiveCount = origin.primitiveCount;
                vertexCount = origin.vertexCount;

                buffer = new VertexBuffer(_graphics, typeof(VertexPositionColorTexture), vertexCount, BufferUsage.WriteOnly);

                if (cmp)
                    Compile();
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
            public virtual void SetVerts(Vector3[] p, bool autoColor)
            {
                if (p.Length != vertexCount) return;

                for (int i = 0; i < vertexCount; i++)
                {
                    verticies[i].Position = p[i];
                    if (autoColor)
                    {
                        verticies[i].Color = new Color(p[i]);
                    }
                    else
                    {
                        verticies[i].Color = Color.DarkOliveGreen;
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

            public override string ToString()
            {
                return $"{id}";
            }

            public void Destroy()
            {
                buffer.Dispose();
                if (!texture.IsDisposed)
                    texture.Dispose();
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

            public SharedPolygon(SharedPolygon origin, List<SharedPoint> ppoints) : base(origin as Polygon, false)
            {
                _points = ppoints;
                core = origin.core;
                vertexReferences = new int[origin.vertexReferences.Length];
                for (int i = 0; i < vertexReferences.Length; i++)
                {
                    vertexReferences[i] = origin.vertexReferences[i];
                }

                Compile();
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
                Correct();
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

            public override string ToString()
            {
                return $"C:{id}";
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

            public SharedPoint(SharedPoint origin)
            {
                ID = origin.ID;
                pos = origin.pos;
                USE = origin.USE;
                update = origin.update;
            }
        }

        public const int COMPILE_ROTATION = 1;
        public const int COMPILE_POLYGONS = 2;

        public List<SharedPoint> points; //This can be unused if you want.
        private GraphicsDevice _graphics;

        public List<Polygon> Polygons => _polys;
        private List<Polygon> _polys;

        public Vector3 position;
        public Vector3 scale;

        public RotationProfile profile;

        public VertexModelAdvanced(GraphicsDevice device)
        {
            _graphics = device;
            _polys = new List<Polygon>();

            points = new List<SharedPoint>();
            profile = new RotationProfile();
            scale = Vector3.One;
            position = Vector3.Zero;
        }

        public VertexModelAdvanced(VertexModelAdvanced copy)
        {
            _graphics = copy._graphics;

            _polys = new List<Polygon>();
            points = new List<SharedPoint>();
            for (int i = 0; i < copy.points.Count; i++)
            {
                points.Add(new SharedPoint(copy.points[i]));
            }
            for (int i = 0; i < copy.Polygons.Count; i++)
            {
                if (copy.Polygons[i] as SharedPolygon != null)
                {
                    //make a sharedpoly copy
                    SharedPolygon tmp = new SharedPolygon(copy.Polygons[i] as SharedPolygon, points);
                    _polys.Add(tmp);
                }
                else
                {
                    Polygon tmp = new Polygon(copy.Polygons[i]);
                    _polys.Add(tmp);
                }
            }

            position = copy.position;
            profile = copy.profile;
            scale = copy.scale;
        }

        public void Destroy()
        {
            for (int i = 0; i < _polys.Count; i++)
            {
                _polys[i].Destroy();
            }
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
        public void AddComplexPoly(Polygon poly, float rounding = 0.1f)
        {
            _polys.Add(new SharedPolygon(_graphics, poly.vertexCount, poly.primitiveCount, poly.primitiveType, position, points, poly.verticies, rounding));
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

                for (int i = 0; i < points.Count; i++)
                {
                    points[i].update = false;
                }
            }
        }

        public void Draw(Camera camera)
        {
            camera.BindWorldPosition(position, profile, scale);
            for (int n = 0; n < _polys.Count; n++)
            {
                if (!_polys[n].built) continue;
                if (_polys[n].textureBuilt)
                    camera.BindTexture(_polys[n].texture);
                else
                    camera.BindTexture();

                //_polys[n].Compile();
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

            camera.BindWorldPosition(position, profile, scale);

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

            temp.profile = new RotationProfile();

            temp.Recompile(COMPILE_POLYGONS & COMPILE_ROTATION);

            return temp;
        }
        public static VertexModelAdvanced CreateGenericCuboid(GraphicsDevice device, bool complex, Vector3 dims)
        {
            VertexModelAdvanced temp = new VertexModelAdvanced(device);

            //0: heading +x, yz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX * dims, Vector3.UnitY * dims, Vector3.UnitZ * dims)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX * dims, Vector3.UnitY * dims, Vector3.UnitZ * dims)));

            //1: heading -x, yz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX * -1 * dims, Vector3.UnitY * dims, Vector3.UnitZ * dims)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitX * -1 * dims, Vector3.UnitY * dims, Vector3.UnitZ * dims)));

            //2: heading +y, xz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY * dims, Vector3.UnitX * dims, Vector3.UnitZ * dims)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY * dims, Vector3.UnitX * dims, Vector3.UnitZ * dims)));

            //3: heading -y, xz
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY * -1 * dims, Vector3.UnitX * dims, Vector3.UnitZ * dims)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitY * -1 * dims, Vector3.UnitX * dims, Vector3.UnitZ * dims)));

            //4: heading +z, xy
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ * dims, Vector3.UnitX * dims, Vector3.UnitY * dims)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ * dims, Vector3.UnitX * dims, Vector3.UnitY * dims)));

            //5: heading -z, xy
            if (!complex)
                temp.AddBasicPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ * -1 * dims, Vector3.UnitX * dims, Vector3.UnitY * dims)));
            else
                temp.AddComplexPoly(new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, Generate4Point(Vector3.UnitZ * -1 * dims, Vector3.UnitX * dims, Vector3.UnitY * dims)));

            temp.profile = new RotationProfile();

            temp.Recompile(COMPILE_POLYGONS & COMPILE_ROTATION);

            return temp;
        }

        public static VertexModelAdvanced CreateGenericMesh(GraphicsDevice device, bool complex, float xRange, float zRange, float step)
        {
            VertexModelAdvanced temp = new VertexModelAdvanced(device);

            bool flipsw = true;
            for (float x = -xRange; x < xRange; x += step)
            {
                for (float z = -zRange; z < zRange; z += step)
                {
                    Vector3[] autoPoints =
                    {
                        new Vector3(x, 0, z),
                        new Vector3(x+step, 0, z),
                        new Vector3(x, 0, z+step),
                        new Vector3(x+step, 0, z+step)
                    };

                    Polygon tPoly = new Polygon(device, 4, 2, PrimitiveType.TriangleStrip, autoPoints, false);

                    if (flipsw)
                        tPoly.SetVertsColor(Color.Beige);
                    else
                        tPoly.SetVertsColor(Color.SlateGray);

                    if (complex)
                        temp.AddComplexPoly(tPoly);
                    else
                        temp.AddBasicPoly(tPoly);

                    flipsw = !flipsw;
                }
                flipsw = !flipsw;
            }

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

        public bool EscapeKey;

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
                        if (EscapeKey && Input.IsMousePressed(Input.MouseButton.RightMouse))
                            bdown = false;
                    }
                    break;
            }
        }

        public void Release()
        {
            bdown = false;
        }
        public void SetState(bool state)
        {
            bdown = state;
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
            if (Input.IsMousePressed(Input.MouseButton.RightMouse))
            {
                curIndex = -1;
                return;
            }
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

            public int UsableValue;

            private Func<PropertyWidget, int> getStr;
            private Func<PropertyWidget, int> attSetStr;

            private GraphicsDevice _graphics;

            private void setup(GraphicsDevice device, Vector2 pos, Vector2 siz, bool dostring, string disstring, Func<PropertyWidget, int> get, Func<PropertyWidget, int> set)
            {
                UsableValue = 0;
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

            public void ForceSet()
            {
                if (trysetTemp)
                {
                    attSetStr(this);
                    getStr(this);
                    trysetTemp = false;
                    button.Release();
                    TempText = "";
                }
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

            public virtual void Poll(Vector2 mpos)
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
                    if (trysetTemp)
                    {
                        attSetStr(this);
                        getStr(this);
                        trysetTemp = false;
                        button.Release();
                        TempText = "";
                    }   
                }
            }
            public virtual void Render(SpriteBatch _sprites, SpriteFont font, Vector2 origin, int ySub, Vector2 within)
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
                    renPos.Y -= 30;
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

            public class ButtonWidget : PropertyWidget
            {
                public bool State;
                public bool DoText;
                public string HighText;
                public string LowText;

                public ButtonWidget(GraphicsDevice graphics, Vector2 pos, Vector2 siz) : base(graphics, pos, siz)
                {
                    State = false;

                    rect.Dispose();

                    rect = new Texture2D(_graphics, (int)siz.X, (int)siz.Y);

                    Color[] dat = new Color[(int)siz.X * (int)siz.Y];
                    for (int i = 0; i < dat.Length; i++)
                    {
                        dat[i] = Color.White;
                    }
                    rect.SetData(dat);

                    button.mode = ButtonWidget2D.ButtonMode.Toggle;

                    getStr = getbool;
                    attSetStr = setbool;
                }
                public ButtonWidget(GraphicsDevice graphics, Vector2 pos, Vector2 siz, Func<PropertyWidget, int> get, Func<PropertyWidget, int> set) : this(graphics, pos, siz)
                {
                    getStr = get;
                    attSetStr = set;
                }
                public ButtonWidget(GraphicsDevice device, Vector2 pos, Vector2 siz, string header, Func<PropertyWidget, int> get, Func<PropertyWidget, int> set) : this(device, pos, siz, get, set)
                {
                    HeaderText = header;
                    HasHeader = true;
                }

                bool prev;
                public override void Poll(Vector2 mpos)
                {
                    prev = button.ButtonDown;
                    button.Poll(mpos);
                    getStr(this);

                    if (button.ButtonDown != prev)
                    {
                        attSetStr(this);
                        button.SetState(State);
                    }
                }

                public override void Render(SpriteBatch _sprites, SpriteFont font, Vector2 origin, int ySub, Vector2 within)
                {
                    Vector2 renPos = position + origin;
                    renPos.Y -= ySub;

                    Vector2 temp = renPos - origin;
                    if (temp.X < 0 || temp.Y < 0 || temp.X >= within.X - size.X || temp.Y >= within.Y - size.Y) return;

                    if (State)
                        _sprites.Draw(rect, renPos, Color.DarkSlateGray);
                    else
                        _sprites.Draw(rect, renPos, Color.Gray);

                    renPos.X += 5;
                    renPos.Y += 5;

                    if (DoText)
                    {
                        if (State)
                            _sprites.DrawString(font, HighText, renPos, Color.White);
                        else
                            _sprites.DrawString(font, LowText, renPos, Color.White);
                    }

                    if (HasHeader)
                    {
                        renPos.Y -= 30;
                        _sprites.DrawString(font, HeaderText, renPos, Color.Black);
                    }
                }

                private int setbool(PropertyWidget widg)
                {
                    ButtonWidget temp = widg as ButtonWidget;
                    if (temp == null) return 0;
                    temp.State = !temp.State;
                    return 0;
                }
                private int getbool(PropertyWidget widg)
                {
                    ButtonWidget temp = widg as ButtonWidget;
                    if (temp == null) return 0;
                    return 0;
                }
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
            float y = widget.position.Y + widget.size.Y;
            if (y >= size.Y + scrollMax)
            {
                scrollMax = (int)(y - size.Y) * 2;
            }
        }

        public void ForceAllWidgets()
        {
            for (int i = 0; i < _widgets.Count; i++)
            {
                _widgets[i].ForceSet();
            }
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

        public VertexModelAdvanced physical;
        public string name;

        //Add all details here

        public bool IsSolid;        //Whether or not the brush will be generated into the BSP
        public bool IsVisible;      //Whether or not the brush will be saved to the physical map data

        public bool IsDynamic;      //Whether or not the brush will move through a predetermined path.
        public bool IsTrigger;      //Whether or not the brush acts as a trigger when interacted with;
        public bool IsLighting;     //If the brush acts as a region for lighting
        public bool IsAnchor;       //If the brush will be registered as an origin point in a list of origins (spawnpoints usually)

        //Dynamic details
        public Brush DynamicInclusionZone;  //If dynamic AND solid, define the zone that it will move through here.
                                            //Might be able to generate this based on the motion pack
        //Insert a motion detail pack

        //Trigger
        public ulong TriggerFlag;   //Dedicated flag to act upon, can be either an ID or a quickflag 
        public bool TriggerQuickFlag;   //If true, act as quick flag (bitflag) - else, access a whole table of flags.
        public int TriggerFlagAction;   //What to do on the flag (raise, lower, etc). An enum.

        //Anchor
        public ulong AnchorID;


        public Brush()
        {
            id = id_const++;
            name = "Brush";
        }

        public Brush(Brush origin)
        {
            id = id_const++;
            name = origin.name;

            physical = new VertexModelAdvanced(origin.physical);
        }

        public void Bind(VertexModelAdvanced mod)
        {
            physical = mod;
        }

        public override string ToString()
        {
            return $"({ID}) {name}";
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

        public MapFile(GameScene scene, Vector2 pos, Vector2 siz)
        {
            parent = scene;
            _sprites = parent._game._spriteBatch;

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

        public const int ATTENTION_UNFOCUSED = 0;
        public int Attention_Level = 0;
        public bool OutlineShapes = false;

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
            public Vector2 WindowScale;
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

                WindowScale = Vector2.One * 10;

                Aim = 0;
                Range = 10;
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

                WindowScale = Vector2.One * 10;

                Flatten = Vector3.One - f;

                Camera.TargetUp = Local_Y;
                Camera.TargetAngle = Local_Z;
                Camera.Position = Position + (Local_Z * Aim);

                Camera.Recompile(10, 10, -Range, Range);

                ortho = true;
            }
            public void BuildOrthographic(Vector3 f, Vector3 u, Vector3 r, Vector2 scal)
            {
                Local_X = r;
                Local_Z = f;
                Local_Y = u;

                WindowScale = scal;

                Flatten = Vector3.One - f;

                Camera.TargetUp = Local_Y;
                Camera.TargetAngle = Local_Z;
                Camera.Position = Position + (Local_Z * Aim);

                Camera.Recompile(scal.X, scal.Y, -Range, Range);

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
            public Vector2 MapToReal(Vector2 pos)
            {
                pos.X *= (Target.Width / WindowScale.X);
                pos.Y *= (Target.Height / WindowScale.Y);
                pos += new Vector2(Target.Width, Target.Height) / 2;
                return pos;
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
                    Camera.Recompile(WindowScale.X, WindowScale.Y, -Range, Range);
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

            Attention_Level = ATTENTION_UNFOCUSED;

            rotator = new Rotor();
            rotator.Multiplier = ROTATOR_SENS;

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
            if (Attention_Level == ATTENTION_UNFOCUSED)
            {
                for (int i = 0; i < 4; i++)
                {
                    windows[i].Poll();
                    if (windows[i].Focuser.ButtonDown)
                    {
                        _targetID = i;
                    }
                }
            }

            Poll();

            if (Attention_Level == ATTENTION_UNFOCUSED)
            {
                if (windows[PERSPECTIVE_ID].Focuser.ButtonDown)
                    DefaultProcess(windows[PERSPECTIVE_ID]);

                if (windows[BIRDSEYE_ID].Focuser.ButtonDown)
                    OrthoProcess(windows[BIRDSEYE_ID], WindowContext.Birdseye);

                if (windows[FORWARD_ID].Focuser.ButtonDown)
                    OrthoProcess(windows[FORWARD_ID], WindowContext.Forward);

                if (windows[SIDE_ID].Focuser.ButtonDown)
                    OrthoProcess(windows[SIDE_ID], WindowContext.Side);
            }
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
        protected Rotor rotator;
        protected const float ROTATOR_SENS = 1;
        protected const float FREELOOK_SPEED = 5;
        public virtual void DefaultProcess(PerspectiveViewerWindow window) 
        {
            float az = Input.KeyDelta(Keys.Right, Keys.Left) * (float)Program.Game.FrameTime;
            float el = Input.KeyDelta(Keys.Up, Keys.Down) * (float)Program.Game.FrameTime;
            rotator.Update(az, el);

            float lr = Input.KeyDelta(Keys.D, Keys.A) * (float)Program.Game.FrameTime * FREELOOK_SPEED;
            Vector2 tmp = rotator.Azimuth_V;
            Vector3 movTmp = new Vector3(-tmp.Y, 0, tmp.X) * lr;

            float fd = Input.KeyDelta(Keys.W, Keys.S) * (float)Program.Game.FrameTime * FREELOOK_SPEED;
            movTmp += fd* new Vector3(tmp.X, 0, tmp.Y);

            float ud = Input.KeyDelta(Keys.Space, Keys.LeftControl) * (float)Program.Game.FrameTime * FREELOOK_SPEED;
            movTmp += ud * Vector3.Up;

            window.Position += movTmp;
            window.Camera.Position = window.Position;

            window.Camera.TargetAngle = rotator.Angle;
        }
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
            Vector3 X = Input.KeyDelta(Keys.D, Keys.A) * window.Local_X;

            Y *= (float)(Program.Game.FrameTime * 5);
            X *= (float)(Program.Game.FrameTime * 5);

            window.Position += Y + X;

            int scroll = Input.GetMouseWheelDeltaNormal();

            window.Aim += (float)(scroll / Program.Game.FrameRate) * 10;

            int rangeDelta = Input.KeyDelta(Keys.E, Keys.Q);

            window.Range += (float)(rangeDelta / Program.Game.FrameRate);
            if (window.Range < 0) window.Range = 0;

            float winScalDelta = (float)(Input.KeyDelta(Keys.V, Keys.F) / Program.Game.FrameRate) * 10;

            float scal = window.WindowScale.X + winScalDelta;
            window.WindowScale.X = scal;
            window.WindowScale.Y = scal;

            if (winScalDelta != 0)
            {
                window.Camera.Recompile(scal, scal, -window.Range, window.Range);
            }
        }
        public virtual void OrthoRender(PerspectiveViewerWindow window, WindowContext context) 
        {
            Program.SpritesBeginDefault(_sprites);

            _sprites.DrawString(font, names[window.ID], Vector2.Zero, colors[window.ID]);
            _sprites.DrawString(font, $"({Program.Vec3ToString(window.Camera.Position)})", new Vector2(0, 20), Color.White);
            _sprites.DrawString(font, $"{String.Format("{0:0.00}", window.Range)}", new Vector2(0, 40), Color.White);

            _sprites.End();
        }

        public void BindCommands()
        {
            //Commands
            Program.Game.CommandConsole.RegisterCommand("range", setrange, "Defines the render range of a given window.");
            Program.Game.CommandConsole.RegisterCommand("pos", pos, "Gets or sets the position of the given window.");
        }
        public void UnbindCommands()
        {
            Program.Game.CommandConsole.DeregisterCommand("range");
            Program.Game.CommandConsole.DeregisterCommand("pos");
        }
        public int setrange(string[] args)
        {
            if (args.Length < 2) return -1;

            int tmp = 0;
            float val = 0;

            if (!int.TryParse(args[0], out tmp))
                return -1;

            if (!float.TryParse(args[1], out val))
                return -1;

            if (tmp < 0 || tmp >= 4) return 0;

            windows[tmp].Range = val;

            return 0;
        }
        public int pos(string[] args)
        {
            if (args.Length < 1) return -1;

            int id = -1;
            if (!int.TryParse(args[0], out id)) return -1;
            
            if (id < 0 || id >= windows.Length) return -1;

            if (args.Length < 4)
            {
                Program.Game.CommandConsole.PushString(Program.Vec3ToString(windows[id].Position));
                return 0;
            }

            float[] poss =
            {
                windows[id].Position.X, windows[id].Position.Y, windows[id].Position.Z
            };

            for (int i = 0; i < 3; i++)
            {
                if (args[i + 1].ToLower() == "*") continue;
                if (!float.TryParse(args[i + 1], out poss[i])) return -1;
            }

            windows[id].Position = new Vector3(poss[0], poss[1], poss[2]);
            return 0;
        }
        public virtual void Destroy()
        {
            for (int i = 0; i < 4; i++)
            {
                windows[i].Destroy();
            }

            Program.Game.CommandConsole.DeregisterCommand("pos");
            Program.Game.CommandConsole.DeregisterCommand("range");
        }
    }
    public class MapEditor : PerspectiveViewer
    {
        public BrushLibrary library;
        public MapFile map;

        public PropertyViewer mapBrushProperties;

        protected bool viewAll;

        public MapEditor(GameScene parent, BrushLibrary lib) :  base(parent)
        {
            library = lib;
            viewAll = true;

            Vector2 pos = new Vector2(Program.InternalScreen.Y, 0);
            Vector2 siz = new Vector2((Program.InternalScreen.X - Program.InternalScreen.Y) / 3f, Program.InternalScreen.Y / 2);

            pos.X += siz.X;
            map = new MapFile(parent, pos, siz);

            pos.Y += siz.Y;
            mapBrushProperties = new PropertyViewer(_graphics, pos, siz, 0);

            SetupPropertyViewers();
        }

        public void SetupPropertyViewers()
        {
            Vector2 pos = new Vector2(15, 30);
            Vector2 siz = new Vector2(60, 30);

            //mapBrushProperties
            {
                siz.X = 250;
                //name
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Name", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = map.mapBrushes[map.mapBrushViewer.curIndex].name;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        return -1;
                    }
                    map.mapBrushes[map.mapBrushViewer.curIndex].name = widg.TempText;
                    return 0;
                }));

                siz.X = 80;
                pos.Y += 60;

                //position X
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Position", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.position.X);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        return -1;
                    }
                    float temp = 0;
                    if (float.TryParse(widg.TempText, out temp))
                    {
                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.position.X = temp;
                    }
                    return 0;
                }));

                pos.X += 90;
                //position Y
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.position.Y);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        return -1;
                    }
                    float temp = 0;
                    if (float.TryParse(widg.TempText, out temp))
                    {
                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.position.Y = temp;
                    }
                    return 0;
                }));

                pos.X += 90;
                //position Z
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.position.Z);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        return -1;
                    }
                    float temp = 0;
                    if (float.TryParse(widg.TempText, out temp))
                    {
                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.position.Z = temp;
                    }
                    return 0;
                }));

                //Rotation
                pos.X -= 180;
                pos.Y += 60;

                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Azimuth", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile.ang2d.X * Program.RAD_CONST);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    if (float.TryParse(widg.TempText, out float tmp))
                    {
                        RotationProfile ol = map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile;

                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile = new RotationProfile(tmp * Program.DEG_CONST, ol.ang2d.Y, ol.roll);
                    }
                    return 0;
                }));

                pos.X += 90;

                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Elevation", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile.ang2d.Y * Program.RAD_CONST);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    if (float.TryParse(widg.TempText, out float tmp))
                    {
                        RotationProfile ol = map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile;

                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile = new RotationProfile(ol.ang2d.X, tmp * Program.DEG_CONST, ol.roll);
                    }
                    return 0;
                }));

                pos.X += 90;
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Roll", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile.roll * Program.RAD_CONST);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    if (float.TryParse(widg.TempText, out float tmp))
                    {
                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.profile.roll = tmp * Program.DEG_CONST;
                    }
                    return 0;
                }));

                //Scale
                pos.Y += 60;
                pos.X -= 180;
                //position X
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Scale", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.scale.X);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        return -1;
                    }
                    float temp = 0;
                    if (float.TryParse(widg.TempText, out temp))
                    {
                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.scale.X = temp;
                    }
                    return 0;
                }));

                pos.X += 90;
                //position Y
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.scale.Y);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        return -1;
                    }
                    float temp = 0;
                    if (float.TryParse(widg.TempText, out temp))
                    {
                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.scale.Y = temp;
                    }
                    return 0;
                }));

                pos.X += 90;
                //position Z
                mapBrushProperties.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = String.Format("{0:0.00}", map.mapBrushes[map.mapBrushViewer.curIndex].physical.scale.Z);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1)
                    {
                        return -1;
                    }
                    float temp = 0;
                    if (float.TryParse(widg.TempText, out temp))
                    {
                        map.mapBrushes[map.mapBrushViewer.curIndex].physical.scale.Z = temp;
                    }
                    return 0;
                }));

                pos.X -= 180;
                //Checks (basic, not detailed like the map)
                siz.X = 30;
                //IsSolid
                pos.Y += 60;
                mapBrushProperties.AddPropertyWidget(new PropertyWidget.ButtonWidget(_graphics, pos, siz, "IsSolid", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    (widg as PropertyWidget.ButtonWidget).State = map.mapBrushes[map.mapBrushViewer.curIndex].IsSolid;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    map.mapBrushes[map.mapBrushViewer.curIndex].IsSolid = !map.mapBrushes[map.mapBrushViewer.curIndex].IsSolid;
                    return 0;
                })
                {
                    HighText = "T",
                    LowText = "F",
                    DoText = true
                });

                //IsVisible
                pos.Y += 60;
                mapBrushProperties.AddPropertyWidget(new PropertyWidget.ButtonWidget(_graphics, pos, siz, "IsVisible", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    (widg as PropertyWidget.ButtonWidget).State = map.mapBrushes[map.mapBrushViewer.curIndex].IsVisible;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    map.mapBrushes[map.mapBrushViewer.curIndex].IsVisible = !map.mapBrushes[map.mapBrushViewer.curIndex].IsVisible;
                    return 0;
                })
                {
                    HighText = "T",
                    LowText = "F",
                    DoText = true
                });

                //IsDynamic
                pos.Y += 60;
                mapBrushProperties.AddPropertyWidget(new PropertyWidget.ButtonWidget(_graphics, pos, siz, "IsDynamic", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    (widg as PropertyWidget.ButtonWidget).State = map.mapBrushes[map.mapBrushViewer.curIndex].IsDynamic;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    map.mapBrushes[map.mapBrushViewer.curIndex].IsDynamic = !map.mapBrushes[map.mapBrushViewer.curIndex].IsDynamic;
                    return 0;
                })
                {
                    HighText = "T",
                    LowText = "F",
                    DoText = true
                });

                //IsTrigger
                pos.Y += 60;
                mapBrushProperties.AddPropertyWidget(new PropertyWidget.ButtonWidget(_graphics, pos, siz, "IsTrigger", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    (widg as PropertyWidget.ButtonWidget).State = map.mapBrushes[map.mapBrushViewer.curIndex].IsTrigger;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    map.mapBrushes[map.mapBrushViewer.curIndex].IsTrigger = !map.mapBrushes[map.mapBrushViewer.curIndex].IsTrigger;
                    return 0;
                })
                {
                    HighText = "T",
                    LowText = "F",
                    DoText = true
                });

                //IsLighting
                pos.Y += 60;
                mapBrushProperties.AddPropertyWidget(new PropertyWidget.ButtonWidget(_graphics, pos, siz, "IsLighting", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    (widg as PropertyWidget.ButtonWidget).State = map.mapBrushes[map.mapBrushViewer.curIndex].IsLighting;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    map.mapBrushes[map.mapBrushViewer.curIndex].IsLighting = !map.mapBrushes[map.mapBrushViewer.curIndex].IsLighting;
                    return 0;
                })
                {
                    HighText = "T",
                    LowText = "F",
                    DoText = true
                });

                //IsAnchor
                pos.Y += 60;
                mapBrushProperties.AddPropertyWidget(new PropertyWidget.ButtonWidget(_graphics, pos, siz, "IsAnchor", (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    (widg as PropertyWidget.ButtonWidget).State = map.mapBrushes[map.mapBrushViewer.curIndex].IsAnchor;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (map.mapBrushViewer.curIndex == -1) return -1;
                    map.mapBrushes[map.mapBrushViewer.curIndex].IsAnchor = !map.mapBrushes[map.mapBrushViewer.curIndex].IsAnchor;
                    return 0;
                })
                {
                    HighText = "T",
                    LowText = "F",
                    DoText = true
                });
            }
        }
        public override void Destroy()
        {
            base.Destroy();
            map.Destroy();
            mapBrushProperties.Destroy();
        }
        public override void Poll()
        {
            base.Poll();
            map.Poll();
            library.Poll();
            mapBrushProperties.Poll();

            if (library.button.ButtonDown)
            {
                if (library.brushViewer.curIndex != -1 && Input.IsKeyPressed(Keys.C))
                {
                    //paste into mapfile
                    Brush tmp = new Brush(library.brushes[library.brushViewer.curIndex]);
                    //Find pos to place it
                    tmp.physical.position = Vector3.Zero;

                    map.mapBrushes.Add(tmp);
                }
            }

            if (map.mapBrushViewer.ButtonActive)
            {
                if (map.mapBrushViewer.curIndex != -1)
                {
                    var pr = (_parent as MapEdit);
                    if (pr != null)
                    {
                        pr.brushEditor.BindBrush(map.mapBrushes[map.mapBrushViewer.curIndex]);
                    }
                }
            }
        }
        public override void AddlRender()
        {
            Program.SpritesBeginDefault(_sprites);
            map.Render(false);
            library.Render(false);
            mapBrushProperties.Render(false);

            _sprites.End();
        }

        public override void DefaultRender(PerspectiveViewerWindow window)
        {
            _graphics.Clear(Color.CornflowerBlue);
            _graphics.DepthStencilState = _dss;

            window.Camera.SetupDraw();

            if (map.mapBrushViewer.curIndex != -1 && !viewAll)
            {
                //render specific model
            }
            else
            {
                for (int i = 0; i < map.mapBrushes.Count; i++)
                {
                    map.mapBrushes[i].physical.Draw(window.Camera);
                }
            }
            base.DefaultRender(window);
        }
        public override void OrthoRender(PerspectiveViewerWindow window, WindowContext context)
        {
            _graphics.Clear(Color.Black);

            window.Camera.SetupDraw();

            if (map.mapBrushViewer.curIndex != -1 && !viewAll)
            {
                //render specific model
            }
            else
            {
                for (int i = 0; i < map.mapBrushes.Count; i++)
                {
                    map.mapBrushes[i].physical.Draw(window.Camera);
                }
            }

            base.OrthoRender(window, context);
        }
    }
    public class BrushEditor : PerspectiveViewer
    {
        public Brush CurrentBrush;
        public BrushLibrary library;

        protected ListViewer<VertexPositionColorTexture> polyList;
        protected ListViewer<VertexModelAdvanced.Polygon> brushContentsList;
        protected PropertyViewer vertexPropertyViewer;

        protected PropertyViewer brushPropertyViewer;

        protected PropertyViewer brushPolygonPropertyViewer;

        protected PropertyViewer libraryNewDialog;
        protected PropertyViewer polygonNewDialog;

        protected VertexModelAdvanced[] xyzPlanes;
        protected Texture2D circleTex;
        protected Vector2 circleOffset;
        private bool renderPlanes;
        public BrushEditor(GameScene parent, BrushLibrary lib) : base(parent)
        {
            library = lib;
            Vector2 p = new Vector2(Program.InternalScreen.Y, 0);

            //The size of a single "small" scroller
            Vector2 s = new Vector2((Program.InternalScreen.X - Program.InternalScreen.Y) / 3f, Program.InternalScreen.Y / 3f);
            
            brushContentsList = new ListViewer<VertexModelAdvanced.Polygon>(new List<VertexModelAdvanced.Polygon>(), 17, p, s, Program.Game.Content);

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

            p = Program.InternalScreen / 4;
            s = Program.InternalScreen / 2;

            libraryNewDialog = new PropertyViewer(_graphics, p, s, 0);
            polygonNewDialog = new PropertyViewer(_graphics, p, s, 0);

            SetupDialogBoxes();

            CurrentBrush = null;

            windows[PERSPECTIVE_ID].Camera.TargetAngle = new Vector3(-1, -1, -1);
            windows[PERSPECTIVE_ID].Camera.Position = windows[PERSPECTIVE_ID].Camera.TargetAngle * -d_Distance;

            xyzPlanes = new VertexModelAdvanced[4];

            int opac = 64;
            
            //X facing (SIDE)
            xyzPlanes[SIDE_ID] = new VertexModelAdvanced(_graphics);
            xyzPlanes[SIDE_ID].AddBasicPoly(new VertexModelAdvanced.Polygon(_graphics, 4, 2, PrimitiveType.TriangleStrip, 
                VertexModelAdvanced.Generate4Point(Vector3.Zero, Vector3.UnitY * 5, Vector3.UnitZ * 5, new Color(colors[SIDE_ID], opac))));

            //Z facing (FORWARD)
            xyzPlanes[FORWARD_ID] = new VertexModelAdvanced(_graphics);
            xyzPlanes[FORWARD_ID].AddBasicPoly(new VertexModelAdvanced.Polygon(_graphics, 4, 2, PrimitiveType.TriangleStrip,
                VertexModelAdvanced.Generate4Point(Vector3.Zero, Vector3.UnitY * 5, Vector3.UnitX * 5, new Color(colors[FORWARD_ID], opac))));

            //Y facing (TOP)
            xyzPlanes[BIRDSEYE_ID] = new VertexModelAdvanced(_graphics);
            xyzPlanes[BIRDSEYE_ID].AddBasicPoly(new VertexModelAdvanced.Polygon(_graphics, 4, 2, PrimitiveType.TriangleStrip,
                VertexModelAdvanced.Generate4Point(Vector3.Zero, Vector3.UnitZ * 5, Vector3.UnitX * 5, new Color(colors[BIRDSEYE_ID], opac))));

            for (int i = 1; i < 4; i++)
            {
                xyzPlanes[i].Recompile(VertexModelAdvanced.COMPILE_POLYGONS);
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
                parent.DisplayedText = String.Format("{0:0.00}", CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Position.X);
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    Vector3 tmp = CurrentBrush.physical.GetPoly(brushContentsList.curIndex).GetVertexPoint(selVert);

                    tmp.X = temp;

                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).SetVertexPoint(selVert, tmp);
                    CurrentBrush.physical.Recompile(VertexModelAdvanced.COMPILE_POLYGONS);
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = String.Format("{0:0.00}", CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Position.Y);
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    Vector3 tmp = CurrentBrush.physical.GetPoly(brushContentsList.curIndex).GetVertexPoint(selVert);

                    tmp.Y = temp;

                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).SetVertexPoint(selVert, tmp);
                    CurrentBrush.physical.Recompile(VertexModelAdvanced.COMPILE_POLYGONS);
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = String.Format("{0:0.00}", CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Position.Z);
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    Vector3 tmp = CurrentBrush.physical.GetPoly(brushContentsList.curIndex).GetVertexPoint(selVert);

                    tmp.Z = temp;

                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).SetVertexPoint(selVert, tmp);
                    CurrentBrush.physical.Recompile(VertexModelAdvanced.COMPILE_POLYGONS);
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = $"{CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.R}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.R = temp;
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).Compile();
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = $"{CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.G}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.G = temp;
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).Compile();
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = $"{CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.B}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.B = temp;
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).Compile();
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = $"{CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.A}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                byte temp = 0;
                if (byte.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].Color.A = temp;
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).Compile();
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = $"{CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].TextureCoordinate.X}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].TextureCoordinate.X = temp;
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).Compile();
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                parent.DisplayedText = $"{CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].TextureCoordinate.Y}";
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //Set
            {
                if (brushContentsList.curIndex == -1 || selVert == -1) return -1;
                float temp = 0;
                if (float.TryParse(parent.TempText, out temp))
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies[selVert].TextureCoordinate.Y = temp;
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).Compile();
                    polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);
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
                if (!CurrentBrush.physical.GetPoly(brushContentsList.curIndex).textureBuilt)
                {
                    parent.DisplayedText = "Default";
                }
                else
                {
                    parent.DisplayedText = CurrentBrush.physical.GetPoly(brushContentsList.curIndex).texture.Name;
                }
                return 0;
            }, (PropertyViewer.PropertyWidget parent) => //set
            {
                if (brushContentsList.curIndex == -1 || library.brushViewer.curIndex == -1) return -1;
                if (parent.TempText.ToLower() == "default")
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).textureBuilt = false;
                    return 0;
                }
                try
                {
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).texture = Program.Game.Content.Load<Texture2D>(parent.TempText);
                    CurrentBrush.physical.GetPoly(brushContentsList.curIndex).textureBuilt = true;
                    parent.TempText = "";
                }
                catch (Exception e) { }
                return 0;
            }));

            pos = new Vector2(15, 30);
            siz = new Vector2(250, 30);

            //brush properties
            {
                brushPropertyViewer.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Name", (PropertyWidget widg) =>
                {
                    if (CurrentBrush == null)
                    {
                        widg.DisplayedText = "UNDF";
                        return -1;
                    }
                    widg.DisplayedText = CurrentBrush.name;
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    if (CurrentBrush == null) return -1;
                    CurrentBrush.name = widg.TempText;
                    return 0;
                }));
            }
        }

        //library new
        protected bool lib_isMesh, lib_isComplx;
        protected float lib_x, lib_y, lib_z;
        //polygon new
        protected bool poly_isComplx, poly_Is3Side;
        protected float poly_complex_snapto;
        protected Vector3[] poly_points = new Vector3[4];
        public void SetupDialogBoxes()
        {
            Vector2 pos = new Vector2(15, 30);
            Vector2 siz = new Vector2(60, 30);

            //library new dialog : libraryNewDialog
            {
                //lib_isMesh
                lib_isMesh = false; //if true, mesh: if false, cube
                libraryNewDialog.AddPropertyWidget(new PropertyViewer.PropertyWidget.ButtonWidget(_graphics, pos, siz, "GenMesh?", (PropertyViewer.PropertyWidget widg) => //GET
                {
                    (widg as PropertyViewer.PropertyWidget.ButtonWidget).State = lib_isMesh;
                    return 0;
                }, (PropertyViewer.PropertyWidget widg) => //SET
                {
                    lib_isMesh = !lib_isMesh;
                    return 0;
                })
                {
                    HighText = "Mesh", //display for high
                    LowText = "Cube", //temp for low
                    DoText = true
                });

                //lib_isComplx
                pos.X += 175;
                lib_isComplx = false; //if true, complex: if false, simple
                libraryNewDialog.AddPropertyWidget(new PropertyViewer.PropertyWidget.ButtonWidget(_graphics, pos, siz, "Complex?", (PropertyViewer.PropertyWidget widg) => //GET
                {
                    (widg as PropertyViewer.PropertyWidget.ButtonWidget).State = lib_isComplx;
                    return 0;
                }, (PropertyViewer.PropertyWidget widg) => //SET
                {
                    lib_isComplx = !lib_isComplx;
                    return 0;
                })
                {
                    HighText = "C", //display for high
                    LowText = "S", //temp for low
                    DoText = true
                });

                //generate
                pos.X += 175;
                libraryNewDialog.AddPropertyWidget(new PropertyViewer.PropertyWidget.ButtonWidget(_graphics, pos, siz, "Generate", (PropertyViewer.PropertyWidget widg) => //GET
                {
                    return 0;
                }, (PropertyViewer.PropertyWidget widg) => //SET
                {
                    //GENERATE
                    libraryNewDialog.ForceAllWidgets();
                    Brush newBrush = new Brush();
                    if (!lib_isMesh)
                    {
                        newBrush.physical = VertexModelAdvanced.CreateGenericCuboid(_graphics, lib_isComplx, new Vector3(lib_x, lib_y, lib_z));
                        if (lib_isComplx)
                            newBrush.name = "ComplexCuboid";
                        else
                            newBrush.name = "SimpleCuboid";
                    }
                    else
                    {
                        newBrush.physical = VertexModelAdvanced.CreateGenericMesh(_graphics, lib_isComplx, lib_x, lib_y, lib_z);
                        if (lib_isComplx)
                            newBrush.name = "ComplexMesh";
                        else
                            newBrush.name = "SimpleMesh";
                    }

                    library.brushes.Add(newBrush);

                    //RESET VALS
                    ResetDialogBoxes();
                    Attention_Level = 0;
                    return 0;
                }));

                siz.X = 200;
                pos.X = 15;
                pos.Y = 100;

                //positions X
                lib_x = 1;
                libraryNewDialog.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Dimensions (XYZ)", (PropertyWidget widg) =>
                {
                    widg.DisplayedText = String.Format("{0:0.00}", lib_x);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    float tmp = 0;
                    if (float.TryParse(widg.TempText, out tmp))
                    {
                        lib_x = tmp;
                    }
                    widg.TempText = "";
                    return 0;
                }));

                pos.X += 220;
                //positions Y
                lib_y = 1;
                libraryNewDialog.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyWidget widg) =>
                {
                    widg.DisplayedText = String.Format("{0:0.00}", lib_y);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    float tmp = 0;
                    if (float.TryParse(widg.TempText, out tmp))
                    {
                        lib_y = tmp;
                    }
                    widg.TempText = "";
                    return 0;
                }));

                pos.X += 220;
                //positions Z
                lib_z = 1;
                libraryNewDialog.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyWidget widg) =>
                {
                    widg.DisplayedText = String.Format("{0:0.00}", lib_z);
                    return 0;
                }, (PropertyWidget widg) =>
                {
                    float tmp = 0;
                    if (float.TryParse(widg.TempText, out tmp))
                    {
                        lib_z = tmp;
                    }
                    widg.TempText = "";
                    return 0;
                }));
            }

            poly_points = new Vector3[4];
            pos = new Vector2(15, 30);
            siz = new Vector2(60, 30);
            //polygon new dialog : polyNewDialog 
            {
                //generate
                polygonNewDialog.AddPropertyWidget(new PropertyViewer.PropertyWidget.ButtonWidget(_graphics, pos, siz, "Generate", (PropertyViewer.PropertyWidget widg) => //GET
                {
                    return 0;
                }, (PropertyViewer.PropertyWidget widg) => //SET
                {
                    polygonNewDialog.ForceAllWidgets();

                    VertexModelAdvanced.Polygon polygon = new VertexModelAdvanced.Polygon(_graphics, (poly_Is3Side ? 3 : 4), (poly_Is3Side ? 1 : 2), PrimitiveType.TriangleStrip);
                    VertexPositionColorTexture[] verts = new VertexPositionColorTexture[poly_Is3Side ? 3 : 4];
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = new VertexPositionColorTexture(poly_points[i], new Color(poly_points[i]), Vector2.Zero);
                    }

                    if (poly_isComplx)
                    {
                        CurrentBrush.physical.AddComplexPoly(polygon, poly_complex_snapto);
                    }
                    else
                    {
                        CurrentBrush.physical.AddBasicPoly(polygon);
                    }

                    ResetDialogBoxes();
                    Attention_Level = 0;
                    return 0;
                })
                {
                    HighText = "Mesh", //display for high
                    LowText = "Cube", //temp for low
                    DoText = false
                });

                pos.X += 150;
                //poly_isComplx
                polygonNewDialog.AddPropertyWidget(new PropertyViewer.PropertyWidget.ButtonWidget(_graphics, pos, siz, "Complex?", (PropertyViewer.PropertyWidget widg) => //GET
                {
                    (widg as PropertyWidget.ButtonWidget).State = poly_isComplx;
                    return 0;
                }, (PropertyViewer.PropertyWidget widg) => //SET
                {
                    poly_isComplx = !poly_isComplx;
                    return 0;
                })
                {
                    HighText = "Cmplx", //display for high
                    LowText = "Smpl", //temp for low
                    DoText = true
                });

                pos.X += 150;
                //poly_is3side
                polygonNewDialog.AddPropertyWidget(new PropertyViewer.PropertyWidget.ButtonWidget(_graphics, pos, siz, "points?", (PropertyViewer.PropertyWidget widg) => //GET
                {
                    (widg as PropertyWidget.ButtonWidget).State = poly_Is3Side;
                    return 0;
                }, (PropertyViewer.PropertyWidget widg) => //SET
                {
                    poly_Is3Side = !poly_Is3Side;
                    return 0;
                })
                {
                    HighText = "4", //display for high
                    LowText = "3", //temp for low
                    DoText = true
                });

                pos.X -= 300;
                pos.Y += 80;
                siz.X = 100;
                //poly_complex_snapto
                polygonNewDialog.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, "Snap", (PropertyViewer.PropertyWidget widg) => //GET
                {
                    widg.DisplayedText = String.Format("{0:0.00}", poly_complex_snapto);
                    return 0;
                }, (PropertyViewer.PropertyWidget widg) => //SET
                {
                    float tmp = 0;
                    if (float.TryParse(widg.TempText, out tmp))
                    {
                        poly_complex_snapto = tmp; 
                    }
                    return 0;
                }));

                for (int i = 0; i < poly_points.Length; i++)
                {
                    pos.Y += 80;
                    pos.X = 15;

                    //x with header for this point
                    polygonNewDialog.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, $"Point {i}", (PropertyViewer.PropertyWidget widg) => //GET
                    {
                        widg.DisplayedText = String.Format("{0:0.00}", poly_points[widg.UsableValue].X);
                        return 0;
                    }, (PropertyViewer.PropertyWidget widg) => //SET
                    {
                        float tmp = 0;
                        if (float.TryParse(widg.TempText, out tmp))
                        {
                            poly_points[widg.UsableValue].X = tmp;
                        }
                        return 0;
                    })
                    {
                        UsableValue = i
                    });

                    //y no header
                    pos.X += 140;
                    polygonNewDialog.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyViewer.PropertyWidget widg) => //GET
                    {
                        widg.DisplayedText = String.Format("{0:0.00}", poly_points[widg.UsableValue].Y);
                        return 0;
                    }, (PropertyViewer.PropertyWidget widg) => //SET
                    {
                        float tmp = 0;
                        if (float.TryParse(widg.TempText, out tmp))
                        {
                            poly_points[widg.UsableValue].Y = tmp;
                        }
                        return 0;
                    })
                    {
                        UsableValue = i
                    });

                    //z no header
                    pos.X += 140;
                    polygonNewDialog.AddPropertyWidget(new PropertyWidget(_graphics, pos, siz, (PropertyViewer.PropertyWidget widg) => //GET
                    {
                        widg.DisplayedText = String.Format("{0:0.00}", poly_points[widg.UsableValue].Z);
                        return 0;
                    }, (PropertyViewer.PropertyWidget widg) => //SET
                    {
                        float tmp = 0;
                        if (float.TryParse(widg.TempText, out tmp))
                        {
                            poly_points[widg.UsableValue].Z = tmp;
                        }
                        return 0;
                    })
                    {
                        UsableValue = i
                    });
                }
            }

            ResetDialogBoxes();
        }
        private void ResetDialogBoxes()
        {
            lib_isMesh = false;
            lib_isComplx = false;
            lib_x = 1;
            lib_y = 1;
            lib_z = 1;

            poly_isComplx = false;
            poly_complex_snapto = 0.1f;
            poly_Is3Side = true;
            for (int i = 0; i < 4; i++)
            {
                poly_points[i] = Vector3.Zero;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            circleTex.Dispose();
            brushContentsList.Destroy();
            vertexPropertyViewer.Destroy();
            brushPropertyViewer.Destroy();
            brushPolygonPropertyViewer.Destroy();
            polyList.Destroy();
            libraryNewDialog.Destroy();
            polygonNewDialog.Destroy();
            circleTex.Dispose();
            for (int i = 1; i < xyzPlanes.Length; i++)
            {
                xyzPlanes[i].Destroy();
            }
        }

        int lastIndexLib = -1;
        int lastIndexBrush = -1;
        public void BindBrush(Brush brush)
        {
            CurrentBrush = brush;
            brushContentsList.viewedList = CurrentBrush.physical.Polygons;
            rendersides = true;
            brushContentsList.curIndex = -1;
        }
        public override void Poll()
        {
            if (Attention_Level == 0)
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
                        BindBrush(library.brushViewer.viewedList[lastIndexLib]);
                    }
                    else
                    {
                        CurrentBrush = null;
                        brushContentsList.viewedList = new List<VertexModelAdvanced.Polygon>();
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
                        polyList.SetData(CurrentBrush.physical.GetPoly(lastIndexBrush).verticies);
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

                //brushlibrary create new 
                if (library.button.ButtonDown)
                {
                    if (Input.IsKeyPressed(Keys.N))
                        Attention_Level = 1;
                }

                
                if (brushContentsList.ButtonActive)
                {
                    //brush contents delete/create polys
                    if (brushContentsList.curIndex != -1 && (Input.IsKeyDown(Keys.LeftControl) || Input.IsKeyDown(Keys.RightControl)) && Input.IsKeyPressed(Keys.Delete)) //deletes
                    {
                        CurrentBrush.physical.RemovePoly(brushContentsList.curIndex);
                        CurrentBrush.physical.Recompile(VertexModelAdvanced.COMPILE_POLYGONS);
                        brushContentsList.curIndex = -1;
                        selVert = -1;
                    }

                    //change this code to change the attention level to a higher value, open dialog to generate a poly
                    if (Input.IsKeyPressed(Keys.N))
                        Attention_Level = 2;
                }
            }
            else
            {
                if (Input.IsKeyPressed(Keys.Escape))
                {
                    ResetDialogBoxes();
                    Attention_Level = 0;
                }

                if (Attention_Level == 1) //libraryNewDialog
                {
                    libraryNewDialog.Poll();
                }
                else if (Attention_Level == 2) //polyNewDialog
                {
                    polygonNewDialog.Poll();
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

            //Render special dialog boxes
            if (Attention_Level == 1)
            {
                libraryNewDialog.Render(false);
            }
            else if (Attention_Level == 2)
            {
                polygonNewDialog.Render(false);
            }

            _sprites.End();
        }

        //DEFAULT PERSPECTIVE
        float d_Distance = 5;
        bool rendersides = true;
        
        public override void DefaultProcess(PerspectiveViewerWindow window)
        {
            base.DefaultProcess(window);

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
                Vector2 tmpr = CurrentBrush.physical.profile.ang2d;
                CurrentBrush.physical.position = Vector3.Zero;
                CurrentBrush.physical.profile.ang2d = Vector2.Zero;
                
                if (brushContentsList.curIndex != -1 && !rendersides)
                {
                    CurrentBrush.physical.Draw(window.Camera, brushContentsList.curIndex);
                }
                else
                {
                    CurrentBrush.physical.Draw(window.Camera);
                }

                CurrentBrush.physical.position = temp;
                CurrentBrush.physical.profile.ang2d = tmpr;
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
        bool outlineShapes = false;
        public override void OrthoProcess(PerspectiveViewerWindow window, WindowContext context)
        {
            base.OrthoProcess(window, context);

            if (Attention_Level != ATTENTION_UNFOCUSED) return;

            if (Input.IsKeyPressed(Keys.R))
            {
                rendersides = !rendersides;
            }
            if (Input.IsKeyPressed(Keys.O))
            {
                outlineShapes = !outlineShapes;
            }

            if (brushContentsList.curIndex != -1)
            {
                Vector2 mp = Input.GetMousePosition() - window.TargetOrigin;
                Vector2 pos;

                VertexModelAdvanced.Polygon poly = CurrentBrush.physical.GetPoly(brushContentsList.curIndex);
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

                                pos = window.GetVector2(poly.GetVertexPoint(i)) - window.GetVector2(window.Position);

                                pos.X *= (window.Target.Width / window.WindowScale.X);
                                pos.Y *= (window.Target.Height / window.WindowScale.Y);

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

                        pos = window.GetVector2(poly.GetVertexPoint(selVert)) - window.GetVector2(window.Position);

                        pos.X *= (window.Target.Width / window.WindowScale.X);
                        pos.Y *= (window.Target.Height / window.WindowScale.Y);

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

                        MouseOrigin.Y /= (window.Target.Height / window.WindowScale.X);
                        MouseOrigin.X /= (window.Target.Width / window.WindowScale.Y);

                        MouseProjection = window.GetVector3(MouseOrigin + window.GetVector2(window.Position), poly.verticies[selVert].Position);

                        poly.SetVertexPoint(selVert, MouseProjection);
                        CurrentBrush.physical.Recompile(VertexModelAdvanced.COMPILE_POLYGONS);
                        polyList.SetData(CurrentBrush.physical.GetPoly(brushContentsList.curIndex).verticies);

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
                Vector2 tmpr = CurrentBrush.physical.profile.ang2d;
                CurrentBrush.physical.position = Vector3.Zero;
                CurrentBrush.physical.profile.ang2d = Vector2.Zero;

                if (rendersides)
                    CurrentBrush.physical.Draw(window.Camera);
                else if (brushContentsList.curIndex != -1)
                    CurrentBrush.physical.Draw(window.Camera, brushContentsList.curIndex);

                Program.SpritesBeginDefault(_sprites);

                if (outlineShapes) //draw lines teehee (only for visible shapes)
                {
                    List<VertexModelAdvanced.Polygon> polygons = CurrentBrush.physical.Polygons;
                    int closestID = -1;
                    float closestLength = -1;
                    //At the same time, check for point collisions
                    for (int i = 0; i < polygons.Count; i++)
                    {
                        Vector2 center = Vector2.Zero;
                        Vector2 firstPos = Vector2.Zero;
                        for (int k = 0; k < polygons[i].vertexCount; k++)
                        {
                            Vector2 vert = window.MapToReal(window.GetVector2(polygons[i].GetVertexPoint(k)) - window.GetVector2(window.Position));
                            if (k == 0)
                                firstPos = vert;
                            center += vert;
                            for (int n = k+1, l = 0; n < polygons[i].vertexCount && l < 2; n++, l++)
                            {
                                Vector2 end = window.MapToReal(window.GetVector2(polygons[i].GetVertexPoint(n)) - window.GetVector2(window.Position));
                                Program.DrawLine(_graphics, _sprites, vert, end, 3, false);
                            }
                        }
                        center /= polygons[i].vertexCount;

                        Vector2 msps = Input.GetMousePosition() - window.TargetOrigin;
                        float centMSPS = (msps - center).LengthSquared();
                        float meanLen = (firstPos - center).LengthSquared();
                        meanLen /= 2;

                        if (closestLength == -1 && centMSPS <= meanLen)
                        {
                            closestLength = centMSPS;
                            closestID = i;
                        }
                        else if (centMSPS < closestLength && centMSPS <= meanLen)
                        {
                            closestID = i;
                            closestLength = centMSPS;
                        }
                    }

                    //Raise flag to lock to this pos
                    if (closestID != -1)
                    {
                        if (Input.IsMousePressed(Input.MouseButton.LeftMouse) && selVert == -1)
                        {
                            brushContentsList.curIndex = closestID;
                        }
                    }
                }

                if (brushContentsList.curIndex != -1) //draw vert dots
                {
                    VertexModelAdvanced.Polygon poly = CurrentBrush.physical.GetPoly(brushContentsList.curIndex);
                    for (int i = 0; i < poly.verticies.Length; i++)
                    {
                        VertexPositionColorTexture det = poly.verticies[i];

                        Vector2 pos = window.GetVector2(det.Position) - window.GetVector2(window.Position);

                        pos.X *= (window.Target.Width / window.WindowScale.X);
                        pos.Y *= (window.Target.Height / window.WindowScale.Y);

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
                }
                
                _sprites.End();
                CurrentBrush.physical.position = temp;
                CurrentBrush.physical.profile.ang2d = tmpr;
            }

            base.OrthoRender(window, context);
        }
    }
}
