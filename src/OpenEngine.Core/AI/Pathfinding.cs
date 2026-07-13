// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.AI
{
    public class PathfindingNode
    {
        public System.Numerics.Vector3 Position { get; set; }
        public List<PathfindingNode> Neighbors { get; set; }
        public bool IsWalkable { get; set; }
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;
        public PathfindingNode Parent { get; set; }

        public PathfindingNode()
        {
            Neighbors = new List<PathfindingNode>();
            IsWalkable = true;
        }
    }

    public class PathfindingGrid
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float CellSize { get; set; }
        public PathfindingNode[,] Grid { get; set; }

        public PathfindingGrid(int width, int height, float cellSize)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            Grid = new PathfindingNode[width, height];
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Grid[x, y] = new PathfindingNode
                    {
                        Position = new System.Numerics.Vector3(x * CellSize, 0, y * CellSize)
                    };
                }
            }

            // Connect neighbors
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var neighbors = GetNeighbors(x, y);
                    Grid[x, y].Neighbors = neighbors;
                }
            }
        }

        private List<PathfindingNode> GetNeighbors(int x, int y)
        {
            var neighbors = new List<PathfindingNode>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                    {
                        neighbors.Add(Grid[nx, ny]);
                    }
                }
            }

            return neighbors;
        }

        public PathfindingNode GetNodeFromWorldPosition(System.Numerics.Vector3 worldPosition)
        {
            int x = (int)(worldPosition.X / CellSize);
            int y = (int)(worldPosition.Z / CellSize);

            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return Grid[x, y];
            }

            return null;
        }

        public void SetNodeWalkable(int x, int y, bool walkable)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Grid[x, y].IsWalkable = walkable;
            }
        }
    }

    public class Pathfinding
    {
        public static List<System.Numerics.Vector3> FindPath(PathfindingGrid grid, PathfindingNode startNode, PathfindingNode targetNode)
        {
            if (startNode == null || targetNode == null || !startNode.IsWalkable || !targetNode.IsWalkable)
            {
                return new List<System.Numerics.Vector3>();
            }

            var openSet = new List<PathfindingNode> { startNode };
            var closedSet = new HashSet<PathfindingNode>();

            startNode.GCost = 0;
            startNode.HCost = GetDistance(startNode, targetNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                foreach (var neighbor in currentNode.Neighbors)
                {
                    if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float newGCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                    if (newGCost < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newGCost;
                        neighbor.HCost = GetDistance(neighbor, targetNode);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return new List<System.Numerics.Vector3>();
        }

        private static List<System.Numerics.Vector3> RetracePath(PathfindingNode startNode, PathfindingNode endNode)
        {
            var path = new List<System.Numerics.Vector3>();
            var currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private static float GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
        {
            float dx = Math.Abs(nodeA.Position.X - nodeB.Position.X);
            float dz = Math.Abs(nodeA.Position.Z - nodeB.Position.Z);

            if (dx > dz)
            {
                return 14 * dz + 10 * (dx - dz);
            }

            return 14 * dx + 10 * (dz - dx);
        }
    }

    public class NavMeshAgent
    {
        public System.Numerics.Vector3 Position { get; set; }
        public float Speed { get; set; }
        public float StoppingDistance { get; set; }
        public List<System.Numerics.Vector3> CurrentPath { get; set; }
        public int CurrentPathIndex { get; set; }
        public bool IsMoving { get; set; }
        public PathfindingGrid Grid { get; set; }

        public NavMeshAgent()
        {
            Speed = 3.5f;
            StoppingDistance = 0.1f;
            CurrentPath = new List<System.Numerics.Vector3>();
            CurrentPathIndex = 0;
            IsMoving = false;
        }

        public void SetDestination(System.Numerics.Vector3 destination)
        {
            if (Grid == null) return;

            var startNode = Grid.GetNodeFromWorldPosition(Position);
            var targetNode = Grid.GetNodeFromWorldPosition(destination);

            CurrentPath = Pathfinding.FindPath(Grid, startNode, targetNode);
            CurrentPathIndex = 0;
            IsMoving = CurrentPath.Count > 0;
        }

        public void Update(float deltaTime)
        {
            if (!IsMoving || CurrentPath == null || CurrentPath.Count == 0)
            {
                IsMoving = false;
                return;
            }

            if (CurrentPathIndex >= CurrentPath.Count)
            {
                IsMoving = false;
                return;
            }

            var targetPosition = CurrentPath[CurrentPathIndex];
            var direction = System.Numerics.Vector3.Normalize(targetPosition - Position);
            var distance = System.Numerics.Vector3.Distance(Position, targetPosition);

            if (distance < StoppingDistance)
            {
                CurrentPathIndex++;
                if (CurrentPathIndex >= CurrentPath.Count)
                {
                    IsMoving = false;
                }
            }
            else
            {
                Position += direction * Speed * deltaTime;
            }
        }

        public void Stop()
        {
            IsMoving = false;
            CurrentPath.Clear();
            CurrentPathIndex = 0;
        }
    }
}
