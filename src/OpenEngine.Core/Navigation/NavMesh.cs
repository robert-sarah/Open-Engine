// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Navigation
{
    public class NavMesh
    {
        public string Name { get; set; }
        public List<NavMeshPolygon> Polygons { get; set; }
        public List<NavMeshLink> Links { get; set; }
        public float AgentRadius { get; set; }
        public float AgentHeight { get; set; }
        public float MaxSlope { get; set; }
        public float StepHeight { get; set; }
        public Bounds Bounds { get; set; }

        public NavMesh()
        {
            Polygons = new List<NavMeshPolygon>();
            Links = new List<NavMeshLink>();
            AgentRadius = 0.5f;
            AgentHeight = 2f;
            MaxSlope = 60f;
            StepHeight = 0.4f;
            Bounds = new Bounds();
        }

        public void BuildFromMesh(Mesh mesh)
        {
            // Convert mesh to navigation polygons
            Polygons.Clear();
            
            for (int i = 0; i < mesh.Triangles.Count; i += 3)
            {
                if (i + 2 < mesh.Triangles.Count)
                {
                    var poly = new NavMeshPolygon
                    {
                        Vertices = new List<Vector3>
                        {
                            mesh.Vertices[mesh.Triangles[i]],
                            mesh.Vertices[mesh.Triangles[i + 1]],
                            mesh.Vertices[mesh.Triangles[i + 2]]
                        }
                    };
                    Polygons.Add(poly);
                }
            }

            BuildConnections();
            CalculateBounds();
        }

        private void BuildConnections()
        {
            // Build adjacency between polygons
            foreach (var poly in Polygons)
            {
                poly.Neighbors.Clear();
                
                foreach (var other in Polygons)
                {
                    if (poly == other) continue;
                    
                    if (ShareEdge(poly, other))
                    {
                        poly.Neighbors.Add(other);
                    }
                }
            }
        }

        private bool ShareEdge(NavMeshPolygon a, NavMeshPolygon b)
        {
            // Check if two polygons share an edge
            int sharedVertices = 0;
            foreach (var va in a.Vertices)
            {
                foreach (var vb in b.Vertices)
                {
                    if (Vector3.Distance(va, vb) < 0.001f)
                    {
                        sharedVertices++;
                        break;
                    }
                }
            }
            return sharedVertices >= 2;
        }

        private void CalculateBounds()
        {
            if (Polygons.Count == 0) return;

            var min = Polygons[0].Vertices[0];
            var max = Polygons[0].Vertices[0];

            foreach (var poly in Polygons)
            {
                foreach (var vertex in poly.Vertices)
                {
                    min = Vector3.Min(min, vertex);
                    max = Vector3.Max(max, vertex);
                }
            }

            Bounds = new Bounds((min + max) * 0.5f, max - min);
        }

        public NavMeshPolygon? FindPolygonAt(Vector3 position)
        {
            foreach (var poly in Polygons)
            {
                if (poly.ContainsPoint(position))
                    return poly;
            }
            return null;
        }

        public List<Vector3> FindPath(Vector3 start, Vector3 end)
        {
            var startPoly = FindPolygonAt(start);
            var endPoly = FindPolygonAt(end);

            if (startPoly == null || endPoly == null)
                return new List<Vector3> { end };

            return AStarPath(startPoly, endPoly, start, end);
        }

        private List<Vector3> AStarPath(NavMeshPolygon start, NavMeshPolygon end, Vector3 startPos, Vector3 endPos)
        {
            var openSet = new PriorityQueue<NavMeshPolygon, float>();
            var cameFrom = new Dictionary<NavMeshPolygon, NavMeshPolygon>();
            var gScore = new Dictionary<NavMeshPolygon, float>();
            var fScore = new Dictionary<NavMeshPolygon, float>();

            gScore[start] = 0f;
            fScore[start] = Heuristic(start, end, startPos, endPos);
            openSet.Enqueue(start, fScore[start]);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current, startPos, endPos);
                }

                foreach (var neighbor in current.Neighbors)
                {
                    var tentativeGScore = gScore[current] + Vector3.Distance(current.GetCenter(), neighbor.GetCenter());

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end, neighbor.GetCenter(), endPos);
                        
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }

            return new List<Vector3> { endPos };
        }

        private float Heuristic(NavMeshPolygon a, NavMeshPolygon b, Vector3 pointA, Vector3 pointB)
        {
            return Vector3.Distance(pointA, pointB);
        }

        private List<Vector3> ReconstructPath(Dictionary<NavMeshPolygon, NavMeshPolygon> cameFrom, NavMeshPolygon current, Vector3 startPos, Vector3 endPos)
        {
            var path = new List<Vector3> { endPos };
            
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current.GetCenter());
            }
            
            path[0] = startPos;
            return path;
        }
    }

    public class NavMeshPolygon
    {
        public List<Vector3> Vertices { get; set; }
        public List<NavMeshPolygon> Neighbors { get; set; }
        public int Area { get; set; }
        public int Cost { get; set; }

        public NavMeshPolygon()
        {
            Vertices = new List<Vector3>();
            Neighbors = new List<NavMeshPolygon>();
            Area = 0;
            Cost = 1;
        }

        public Vector3 GetCenter()
        {
            if (Vertices.Count == 0) return Vector3.Zero;
            
            var center = Vector3.Zero;
            foreach (var vertex in Vertices)
            {
                center += vertex;
            }
            return center / Vertices.Count;
        }

        public bool ContainsPoint(Vector3 point)
        {
            // Ray casting algorithm for point in polygon
            int intersections = 0;
            for (int i = 0; i < Vertices.Count; i++)
            {
                var j = (i + 1) % Vertices.Count;
                if ((Vertices[i].Z > point.Z) != (Vertices[j].Z > point.Z))
                {
                    var intersectX = (Vertices[j].X - Vertices[i].X) * (point.Z - Vertices[i].Z) / (Vertices[j].Z - Vertices[i].Z) + Vertices[i].X;
                    if (point.X < intersectX)
                    {
                        intersections++;
                    }
                }
            }
            return (intersections % 2) == 1;
        }
    }

    public class NavMeshLink
    {
        public NavMeshPolygon Start { get; set; }
        public NavMeshPolygon End { get; set; }
        public float Cost { get; set; }
        public bool Bidirectional { get; set; }

        public NavMeshLink()
        {
            Cost = 1f;
            Bidirectional = true;
        }
    }

    public class PriorityQueue<T, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<(T item, TPriority priority)> _elements = new List<(T, TPriority)>();

        public int Count => _elements.Count;

        public void Enqueue(T item, TPriority priority)
        {
            _elements.Add((item, priority));
            var i = _elements.Count - 1;
            while (i > 0)
            {
                var parent = (i - 1) / 2;
                if (_elements[i].priority.CompareTo(_elements[parent].priority) >= 0)
                    break;
                (_elements[i], _elements[parent]) = (_elements[parent], _elements[i]);
                i = parent;
            }
        }

        public T Dequeue()
        {
            var result = _elements[0].item;
            var last = _elements.Count - 1;
            _elements[0] = _elements[last];
            _elements.RemoveAt(last);
            
            var i = 0;
            while (true)
            {
                var left = 2 * i + 1;
                var right = 2 * i + 2;
                if (left >= _elements.Count) break;
                
                var smallest = left;
                if (right < _elements.Count && _elements[right].priority.CompareTo(_elements[left].priority) < 0)
                    smallest = right;
                
                if (_elements[i].priority.CompareTo(_elements[smallest].priority) <= 0)
                    break;
                
                (_elements[i], _elements[smallest]) = (_elements[smallest], _elements[i]);
                i = smallest;
            }
            
            return result;
        }

        public bool Contains(T item)
        {
            return _elements.Exists(e => e.item.Equals(item));
        }
    }
}
