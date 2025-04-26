using System;
using System.Collections.Generic;
using TMPro;

public class MyGraph<T, E>
{
    // Adjacency Map: Each vertex maps to a dictionary of adjacent vertices and edge values
    private Dictionary<T, Dictionary<T, E>> adjacencyMap;
    private bool isDirected;

    // ==================== Constructors ====================
    public MyGraph(bool directed = false)
    {
        adjacencyMap = new Dictionary<T, Dictionary<T, E>>();
        isDirected = directed;
    }

    // ==================== Core Graph Operations ====================

    public int NumVertices() => adjacencyMap.Count;

    public IEnumerable<T> Vertices() => adjacencyMap.Keys;

    public int NumEdges()
    {
        int count = 0;
        foreach (var vertex in adjacencyMap.Values)
            count += vertex.Count;
        return isDirected ? count : count / 2;
    }

    public IEnumerable<(T, T, E)> Edges()
    {
        HashSet<(T, T)> seenEdges = new HashSet<(T, T)>();
        foreach (var vertex in adjacencyMap)
        {
            foreach (var neighbor in vertex.Value)
            {
                if (isDirected || !seenEdges.Contains((neighbor.Key, vertex.Key)))
                {
                    yield return (vertex.Key, neighbor.Key, neighbor.Value);
                    seenEdges.Add((vertex.Key, neighbor.Key));
                }
            }
        }
    }

    public E GetEdge(T u, T v) => adjacencyMap.ContainsKey(u) && adjacencyMap[u].ContainsKey(v) ? adjacencyMap[u][v] : default;

    public (T, T) EndVertices(T u, T v) => (u, v);

    public T Opposite(T v, T neighbor)
    {
        if (adjacencyMap.ContainsKey(v) && adjacencyMap[v].ContainsKey(neighbor))
            return neighbor;
        throw new ArgumentException("Edge does not exist.");
    }

    public int OutDegree(T v) => adjacencyMap.ContainsKey(v) ? adjacencyMap[v].Count : 0;

    public int InDegree(T v)
    {
        if (!isDirected) return OutDegree(v);
        int count = 0;
        foreach (var vertex in adjacencyMap)
            if (vertex.Value.ContainsKey(v)) count++;
        return count;
    }

    public IEnumerable<(T, E)> OutgoingEdges(T v)
    {
        if (adjacencyMap.ContainsKey(v))
            foreach (var neighbor in adjacencyMap[v])
                yield return (neighbor.Key, neighbor.Value);
    }

    public IEnumerable<(T, E)> IncomingEdges(T v)
    {
        if (isDirected)
        {
            foreach (var vertex in adjacencyMap)
                if (vertex.Value.ContainsKey(v))
                    yield return (vertex.Key, vertex.Value[v]);
        }
        else
        {
            foreach (var edge in OutgoingEdges(v))
                yield return edge;
        }
    }

    public void InsertVertex(T x)
    {
        if (!adjacencyMap.ContainsKey(x))
            adjacencyMap[x] = new Dictionary<T, E>();
    }

    public void InsertEdge(T u, T v, E value)
    {
        ValidateVertex(u);
        ValidateVertex(v);
        if (adjacencyMap[u].ContainsKey(v))
            throw new ArgumentException("Edge already exists.");

        adjacencyMap[u][v] = value;
        if (!isDirected)
            adjacencyMap[v][u] = value;
    }

    public void RemoveVertex(T v)
    {
        if (!adjacencyMap.ContainsKey(v)) return;

        foreach (var neighbor in new List<T>(adjacencyMap[v].Keys))
            adjacencyMap[neighbor].Remove(v);
        adjacencyMap.Remove(v);
    }

    public void RemoveEdge(T u, T v)
    {
        if (adjacencyMap.ContainsKey(u)) adjacencyMap[u].Remove(v);
        if (!isDirected && adjacencyMap.ContainsKey(v)) adjacencyMap[v].Remove(u);
    }

    // ==================== Traversals ====================

    public List<T> DFSTraversal(T start, T goal)
    {
        List<T> bestPath = null;
        float bestWeight = float.MaxValue;
        HashSet<T> visited = new HashSet<T>();

        void DFS(T current, List<T> path, float currentWeight)
        {
            if (currentWeight >= bestWeight) return;

            path.Add(current);
            visited.Add(current);

            if (current.Equals(goal))
            {
                if (currentWeight < bestWeight)
                {
                    bestWeight = currentWeight;
                    bestPath = new List<T>(path);
                }
            }
            else
            {
                foreach (var neighbor in GetNeighbors(current))
                    if (!visited.Contains(neighbor.Key))
                        DFS(neighbor.Key, path, currentWeight + GetEdgeWeight(neighbor.Value));
            }

            path.RemoveAt(path.Count - 1);
            visited.Remove(current);
        }

        DFS(start, new List<T>(), 0);
        return bestPath;
    }

    public List<T> DijkstraTraversal(T start, T goal)
    {
        var (distances, previous) = InitializeDijkstra(start);
        var pq = CreatePriorityQueue(start);
        var visited = new HashSet<T>();

        while (pq.Count > 0)
        {
            var (dist, current) = pq.Min;
            pq.Remove(pq.Min);

            if (visited.Contains(current)) continue;
            visited.Add(current);

            if (current.Equals(goal)) break;

            foreach (var neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor.Key)) continue;

                float alt = dist + GetEdgeWeight(neighbor.Value);
                if (alt < distances[neighbor.Key])
                {
                    distances[neighbor.Key] = alt;
                    previous[neighbor.Key] = current;
                    pq.Add((alt, neighbor.Key));
                }
            }
        }

        return ReconstructPath(start, goal, previous);
    }

    // ==================== Helpers ====================

    private void ValidateVertex(T v)
    {
        if (!adjacencyMap.ContainsKey(v))
            throw new ArgumentException("Vertex does not exist.");
    }

    private Dictionary<T, E> GetNeighbors(T v) => adjacencyMap.ContainsKey(v) ? adjacencyMap[v] : new Dictionary<T, E>();

    private float GetEdgeWeight(E edgeValue)
    {
        if (edgeValue is Edge e)
            return e.weight;
        return Convert.ToSingle(edgeValue);
    }

    private (Dictionary<T, float>, Dictionary<T, T>) InitializeDijkstra(T start)
    {
        var distances = new Dictionary<T, float>();
        var previous = new Dictionary<T, T>();

        foreach (var vertex in adjacencyMap.Keys)
        {
            distances[vertex] = float.MaxValue;
            previous[vertex] = default;
        }
        distances[start] = 0;

        return (distances, previous);
    }

    private SortedSet<(float, T)> CreatePriorityQueue(T start)
    {
        return new SortedSet<(float, T)>(Comparer<(float, T)>.Create((a, b) =>
        {
            int cmp = a.Item1.CompareTo(b.Item1);
            return cmp == 0 ? Comparer<T>.Default.Compare(a.Item2, b.Item2) : cmp;
        })) { (0, start) };
    }

    private List<T> ReconstructPath(T start, T goal, Dictionary<T, T> previous)
    {
        var path = new List<T>();

        if (!previous.ContainsKey(goal) || previous[goal] == null)
            return path;

        for (T at = goal; !at.Equals(default(T)); at = previous[at])
        {
            path.Insert(0, at);
            if (at.Equals(start)) break;
        }

        return path;
    }

    // ==================== Debugging ====================

    public void PrintGraph()
    {
        foreach (var vertex in adjacencyMap)
        {
            Console.Write($"Vertex {vertex.Key}: ");
            foreach (var neighbor in vertex.Value)
                Console.Write($"({neighbor.Key}, {neighbor.Value}) ");
            Console.WriteLine();
        }
    }
}
