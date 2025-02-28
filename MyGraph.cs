using System;
using System.Collections.Generic;
using TMPro;

public class MyGraph<T, E>
{
    // Adjacency Map: Each vertex maps to a dictionary of adjacent vertices and edge values
    private Dictionary<T, Dictionary<T, E>> adjacencyMap;
    private bool isDirected;

    public MyGraph()
    {
        adjacencyMap = new Dictionary<T, Dictionary<T, E>>();
        isDirected = false;
    }
    public MyGraph(bool directed)
    {
        adjacencyMap = new Dictionary<T, Dictionary<T, E>>();
        isDirected = directed;
    }

    // Returns the number of vertices in the graph
    public int NumVertices()
    {
        return adjacencyMap.Count;
    }

    // Returns an iteration of all vertices
    public IEnumerable<T> Vertices()
    {
        return adjacencyMap.Keys;
    }

    // Returns the number of edges in the graph
    public int NumEdges()
    {
        int count = 0;
        foreach (var vertex in adjacencyMap.Values)
        {
            count += vertex.Count;
        }
        return isDirected ? count : count / 2; // Avoid double-counting in undirected graphs
    }

    // Returns an iteration of all edges in the graph
    public IEnumerable<(T, T, E)> Edges()
    {
        HashSet<(T, T)> seenEdges = new HashSet<(T, T)>();
        foreach (var vertex in adjacencyMap)
        {
            foreach (var neighbor in vertex.Value)
            {
                var edge = (vertex.Key, neighbor.Key);
                var reverseEdge = (neighbor.Key, vertex.Key);
                if (isDirected || !seenEdges.Contains(reverseEdge))
                {
                    yield return (vertex.Key, neighbor.Key, neighbor.Value);
                    seenEdges.Add(edge);
                }
            }
        }
    }

    // Returns the edge between two vertices, or default(E) if not found
    public E GetEdge(T u, T v)
    {
        if (adjacencyMap.ContainsKey(u) && adjacencyMap[u].ContainsKey(v))
        {
            return adjacencyMap[u][v];
        }
        return default;
    }

    // Returns an array containing the two endpoint vertices of an edge
    public (T, T) EndVertices(T u, T v)
    {
        return (u, v);
    }

    // Returns the opposite vertex of an edge given one endpoint
    public T Opposite(T v, T neighbor)
    {
        if (adjacencyMap.ContainsKey(v) && adjacencyMap[v].ContainsKey(neighbor))
        {
            return neighbor;
        }
        throw new ArgumentException("Edge does not exist.");
    }

    // Returns the number of outgoing edges from a vertex
    public int OutDegree(T v)
    {
        if (adjacencyMap.ContainsKey(v))
        {
            return adjacencyMap[v].Count;
        }
        return 0;
    }

    // Returns the number of incoming edges to a vertex (same as OutDegree for undirected graphs)
    public int InDegree(T v)
    {
        if (isDirected)
        {
            int count = 0;
            foreach (var vertex in adjacencyMap)
            {
                if (vertex.Value.ContainsKey(v))
                {
                    count++;
                }
            }
            return count;
        }
        return OutDegree(v);
    }

    // Returns an iteration of all outgoing edges from a vertex
    public IEnumerable<(T, E)> OutgoingEdges(T v)
    {
        if (adjacencyMap.ContainsKey(v))
        {
            foreach (var neighbor in adjacencyMap[v])
            {
                yield return (neighbor.Key, neighbor.Value);
            }
        }
    }

    // Returns an iteration of all incoming edges to a vertex (same as OutgoingEdges for undirected graphs)
    public IEnumerable<(T, E)> IncomingEdges(T v)
    {
        if (isDirected)
        {
            foreach (var vertex in adjacencyMap)
            {
                if (vertex.Value.ContainsKey(v))
                {
                    yield return (vertex.Key, vertex.Value[v]);
                }
            }
        }
        else
        {
            foreach (var edge in OutgoingEdges(v))
            {
                yield return edge;
            }
        }
    }

    // Inserts a new vertex into the graph
    public void InsertVertex(T x)
    {
        if (!adjacencyMap.ContainsKey(x))
        {
            adjacencyMap[x] = new Dictionary<T, E>();
        }
    }

    // Inserts a new edge between two vertices with a given edge value
    public void InsertEdge(T u, T v, E value)
    {
        if (!adjacencyMap.ContainsKey(u) || !adjacencyMap.ContainsKey(v))
        {
            throw new ArgumentException("One or both vertices do not exist.");
        }
        if (!adjacencyMap[u].ContainsKey(v))
        {
            adjacencyMap[u][v] = value;
            if (!isDirected)
            {
                adjacencyMap[v][u] = value; // Undirected graph: Add reverse edge
            }
        }
        else
        {
            throw new ArgumentException("Edge already exists.");
        }
    }

    // Removes a vertex and all incident edges
    public void RemoveVertex(T v)
    {
        if (adjacencyMap.ContainsKey(v))
        {
            foreach (var neighbor in adjacencyMap[v].Keys)
            {
                adjacencyMap[neighbor].Remove(v);
            }
            adjacencyMap.Remove(v);
        }
    }

    // Removes an edge from the graph
    public void RemoveEdge(T u, T v)
    {
        if (adjacencyMap.ContainsKey(u) && adjacencyMap[u].ContainsKey(v))
        {
            adjacencyMap[u].Remove(v);
            if (!isDirected)
            {
                adjacencyMap[v].Remove(u);
            }
        }
    }

    // Debugging: Print adjacency map
    public void PrintGraph()
    {
        foreach (var vertex in adjacencyMap)
        {
            Console.Write($"Vertex {vertex.Key}: ");
            foreach (var neighbor in vertex.Value)
            {
                Console.Write($"({neighbor.Key}, {neighbor.Value}) ");
            }
            Console.WriteLine();
        }
    }
}
    