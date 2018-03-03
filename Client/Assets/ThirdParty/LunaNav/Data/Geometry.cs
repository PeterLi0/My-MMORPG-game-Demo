using System;
using System.Collections.Generic;
using System.IO;
using LunaNav;

namespace LunaNav
{
    public class Geometry
    {
        public static short WalkableArea = 63;

        public List<RecastVertex> Vertexes { get; set; }
        public int NumVertexes { get; set; }
        public List<int> Triangles { get; set; }
        public int NumTriangles { get; set; }
        public RecastVertex MaxBounds { get; set; }
        public RecastVertex MinBounds { get; set; }
        private int _walkableAreas;
        public ChunkyTriMesh ChunkyTriMesh { get; set; }

        // OffMesh Connection
        public List<float> OffMeshConnectionVerts { get; set; }
        public List<float> OffMeshConnectionRadii { get; set; }
        public List<int> OffMeshConnectionDirections { get; set; }
        public List<int> OffMeshConnectionAreas { get; set; }
        public List<int> OffMeshConnectionFlags { get; set; }
        public List<long> OffMeshConnectionIds { get; set; }
        public long OffMeshConnectionCount { get; set; }

        public int WalkableAreas
        {
            get { return _walkableAreas; }
        }

        public Geometry()
        {
            Vertexes = new List<RecastVertex>();
            Triangles = new List<int>();
            OffMeshConnectionVerts = new List<float>();
            OffMeshConnectionRadii = new List<float>();
            OffMeshConnectionDirections = new List<int>();
            OffMeshConnectionAreas = new List<int>();
            OffMeshConnectionFlags = new List<int>();
            OffMeshConnectionIds = new List<long>();
        }

        public Geometry(string filename) : this()
        {
            Vertexes = new List<RecastVertex>();
            Triangles = new List<int>();

            string[] file = File.ReadAllLines(filename);

            foreach (string line in file)
            {
                if (line[0] == '#') continue;
                if (line[0] == 'v' && line[1] != 'n' && line[1] != 't')
                {
                    string[] positions = line.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    // Vertex pos
                    Vertexes.Add(new RecastVertex(float.Parse(positions[1]), float.Parse(positions[2]),
                                                  float.Parse(positions[3])));
                    NumVertexes++;
                }
                if (line[0] == 'f')
                {
                    string[] points = line.Substring(1).Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    //// Faces
                    for (int i = 2; i < points.Length; i++)
                    {
                        Triangles.Add(int.Parse(points[0].Split('/')[0]) - 1);
                        Triangles.Add(int.Parse(points[i - 1].Split('/')[0]) - 1);
                        Triangles.Add(int.Parse(points[i].Split('/')[0]) - 1);
                        NumTriangles++;
                    }
                }

            }
            CalculateBounds();
        }

        public void CalculateBounds()
        {
            if (Vertexes.Count != 0)
            {
                MinBounds = Vertexes[0];
                MaxBounds = Vertexes[0];
                foreach (RecastVertex recastVertex in Vertexes)
                {
                    MinBounds = RecastVertex.Min(MinBounds, recastVertex);
                    MaxBounds = RecastVertex.Max(MaxBounds, recastVertex);
                }
            }
        }

        public void MarkWalkableTriangles(float walkableSlopeAngle, ref short[] areas)
        {
            MarkWalkableTriangles(walkableSlopeAngle, Triangles, NumTriangles, ref areas);
        }

        public void MarkWalkableTriangles(float walkableSlopeAngle, List<int> triangles, int numTriangles,
                                          ref short[] areas)
        {
            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * Math.PI);
            float[] norm;
            _walkableAreas = 0;
            for (int i = 0; i < numTriangles; i++)
            {
                int tri = i * 3;
                CalcTriNormal(Vertexes[triangles[tri + 0]], Vertexes[triangles[tri + 1]],
                                             Vertexes[triangles[tri + 2]], out norm);
                if (norm[1].CompareTo(walkableThr) > 0)
                    areas[i] = WalkableArea;
            }            
        }

        private void CalcTriNormal(RecastVertex v0, RecastVertex v1, RecastVertex v2, out float[] norm)
        {
            RecastVertex e0, e1, n;
            e0 = RecastVertex.Sub(v1, v0);
            e1 = RecastVertex.Sub(v2, v0);
            n = RecastVertex.Cross(e0, e1);
            n.Normalize();
            norm = n.ToArray();
        }

        public void CreateChunkyTriMesh()
        {
            ChunkyTriMesh = new ChunkyTriMesh(Vertexes.ToArray(), Triangles.ToArray(), NumTriangles, 256);
        }

        public void AddOffMeshConnection(RecastVertex start, RecastVertex end, float radius, bool biDirectional,
                                         short area, int flags)
        {
            OffMeshConnectionVerts.AddRange(new []{start.X, start.Y, start.Z, end.X, end.Y, end.Z});
            OffMeshConnectionRadii.Add(radius);
            OffMeshConnectionDirections.Add(biDirectional ? 1 : 0);
            OffMeshConnectionAreas.Add(area);
            OffMeshConnectionFlags.Add(flags);
            OffMeshConnectionIds.Add(1000+OffMeshConnectionCount++);
        }

    }
}