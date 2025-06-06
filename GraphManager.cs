using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

/*
TO DO 
UI: 
- select.switch path fidnning mode 
- message board to see what happend - see message in debug log 
- button - clear screen 
- button - reset all esisting highlight and selection 
- sliding bar adjust animation speed
*/


public class GraphManager : MonoBehaviour
{

    public enum TraversalMode { Dijkstra, DFS }
    public TraversalMode currentMode = TraversalMode.Dijkstra;
//
    public TextMeshProUGUI messageText;

    public GameObject vertexPrefab;
    public GameObject edgePrefab;
    private MyGraph<Node, Edge> graph;
    private List<Node> vertices;
    private List<GameObject> edgeObjects =  new List<GameObject>(); //track the edges objects in unity, not the graph data stucture - controll the highlighting 
    private Node selectedVertex = null;
    private LineRenderer tempLine;
    private GameObject tempEdgeObj;
    //for traversal 
    private Node startNode = null;
    private Node endNode = null;
    public float animationDelay = 0.05f;


    private void Start()
    {
        graph = new MyGraph<Node, Edge>(false);
        vertices = new List<Node>();
        LogMessage("okay I really hope it dont crash this time");
    }

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
                {
                    HandleMouseClick(mousePos);
                 }
        }
        if (selectedVertex != null && Input.GetKeyDown(KeyCode.Delete)){    DeleteSelectedVertex();}


        UpdateTempLine(mousePos);
        

    }

   private void DeleteSelectedVertex()
{
    if (selectedVertex == null) return;

    // Find all connected neighbors
    List<Node> neighbors = new List<Node>();
    foreach (var neighbor in graph.OutgoingEdges(selectedVertex))
    {
        neighbors.Add(neighbor.Item1);
    }

    // Destroy all edge GameObjects between selectedVertex and neighbors
    foreach (var neighborNode in neighbors)
    {
        // Find the edge object visually between selectedVertex and neighborNode
        foreach (var edgeObj in edgeObjects)
        {
            if (edgeObj == null) continue; // Skip already destroyed

            LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
            if (lr == null) continue;

            Vector3 start = lr.GetPosition(0);
            Vector3 end = lr.GetPosition(1);
            Vector3 selectedPos = selectedVertex.vertexObj.transform.position;
            Vector3 neighborPos = neighborNode.vertexObj.transform.position;

            // Compare positions (order doesn't matter because undirected)
            if ((ApproximatelyEqual(start, selectedPos) && ApproximatelyEqual(end, neighborPos)) ||
                (ApproximatelyEqual(start, neighborPos) && ApproximatelyEqual(end, selectedPos)))
            {
                Destroy(edgeObj);
            }
        }

        graph.RemoveEdge(selectedVertex, neighborNode);
    }

    // Clean up the edge list to remove destroyed objects
    edgeObjects.RemoveAll(e => e == null);

    // Remove vertex from graph
    graph.RemoveVertex(selectedVertex);

    // Destroy vertex GameObject
    Destroy(selectedVertex.vertexObj);

    vertices.Remove(selectedVertex);

    selectedVertex = null;

    LogMessage("Vertex and its connected edges fully deleted.");
}



    private void HandleMouseClick(Vector2 mousePos)
    {
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null)
        {
            HandleVertexClick(hit.gameObject);
        }
        else
        {
            CreateNewVertex(mousePos);
        }
    }

private void HandleVertexClick(GameObject clickedVertex)
{
    Node clickedNode = vertices.Find(node => node.vertexObj == clickedVertex);
    if (clickedNode == null) return;

    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
    {
        // Pathfinding mode
        if (startNode == null)
        {
            startNode = clickedNode;
            startNode.HighlightSelectedForPathfinding();
            LogMessage("Start node selected: " + startNode.vertexObj.name);
        }
        else if (endNode == null && clickedNode != startNode)
        {
            endNode = clickedNode;
            endNode.HighlightSelectedForPathfinding();
            LogMessage("End node selected: " + endNode.vertexObj.name);

            StartCoroutine(ComputeAndAnimate());
        }
    }
    else
    {
        // Default behavior: create edges
        if (selectedVertex == null)
        {
            SelectVertex(clickedNode);
        }
        else if (selectedVertex == clickedNode)
        {
            DeselectVertex();
        }
        else
        {
            CreateEdge(selectedVertex, clickedNode);
            DeselectVertex();
        }
    }
}

    private void CreateNewVertex(Vector2 position)
    {
        GameObject vertexObj = Instantiate(vertexPrefab, position, Quaternion.identity);
        vertexObj.name = "Vertex_" + graph.NumVertices();

        Node newNode = new Node(vertexObj);
        graph.InsertVertex(newNode);
        vertices.Add(newNode);

        SetRandomColor(vertexObj);

        if (selectedVertex != null)
        {
            CreateEdge(selectedVertex, newNode);
            DeselectVertex();
        }
    }

    private void CreateEdge(Node nodeA, Node nodeB)
    {
        try
        {
            float randomWeight = UnityEngine.Random.Range(0f, 100f);
            GameObject edgeObj = Instantiate(edgePrefab);
            LineRenderer lr = edgeObj.GetComponent<LineRenderer>() ?? edgeObj.AddComponent<LineRenderer>();

            SetupEdgeLineRenderer(lr, nodeA.vertexObj.transform.position, nodeB.vertexObj.transform.position);
            GameObject weightLabel = CreateEdgeLabel(nodeA.vertexObj.transform.position, nodeB.vertexObj.transform.position, randomWeight);
            weightLabel.transform.parent = edgeObj.transform;

            Edge newEdge = new Edge(lr, randomWeight);
            graph.InsertEdge(nodeA, nodeB, newEdge);

            edgeObjects.Add(edgeObj);
            LogMessage($"Created edge between {nodeA.vertexObj.name} and {nodeB.vertexObj.name} with weight {randomWeight}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error creating edge: " + e.Message);
        }
    }

    private GameObject CreateEdgeLabel(Vector3 start, Vector3 end, float weight)
    {
        GameObject textObj = new GameObject("EdgeWeight");
        textObj.transform.position = (start + end) / 2;

        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = weight.ToString("F1");
        textMesh.fontSize = 3;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        textMesh.sortingOrder = 5;
        return textObj;
    }

    private void SetupEdgeLineRenderer(LineRenderer lr, Vector3 start, Vector3 end)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.startColor = Color.blue;
        lr.endColor = Color.blue;

        lr.numCapVertices = 10;
        lr.numCornerVertices = 10;
    }

    private void SetRandomColor(GameObject vertex)
    {
        Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        SpriteRenderer sr = vertex.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = randomColor;
        }
    }

    private void SelectVertex(Node node)
    {
        selectedVertex = node;
        selectedVertex.SetOutline(true);
    }

    private void DeselectVertex()
    {
        if (selectedVertex != null)
        {
            selectedVertex.SetOutline(false);
            selectedVertex = null;
        }
    }

    private void UpdateTempLine(Vector2 mousePos)
    {
        if (selectedVertex != null)
        {
            if (tempLine == null)
            {
                SetupTempLine();
            }
            tempLine.SetPosition(0, selectedVertex.vertexObj.transform.position);
            tempLine.SetPosition(1, mousePos);
        }
        else
        {
            RemoveTempLine();
        }
    }

    private void SetupTempLine()
    {
        tempEdgeObj = new GameObject("TempEdge");
        tempLine = tempEdgeObj.AddComponent<LineRenderer>();
        tempLine.positionCount = 2;
        tempLine.startWidth = 0.05f;
        tempLine.endWidth = 0.05f;
        tempLine.material = new Material(Shader.Find("Sprites/Default"));
        tempLine.startColor = Color.red;
        tempLine.endColor = Color.red;
    }

    private void RemoveTempLine()
    {
        if (tempLine != null)
        {
            Destroy(tempEdgeObj);
            tempLine = null;
        }
    }


public IEnumerator AnimatePath(List<Node> path)
{
    // Step 1: Reset all outlines first
    foreach (var node in vertices)
    {
        node.ResetOutline();
    }
    foreach (var edgeObj in edgeObjects)
    {
        if (edgeObj == null) continue;
        LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.startColor = Color.blue;
            lr.endColor = Color.blue;
        }
    }

    // Step 2: Animate the path node-by-node
    for (int i = 0; i < path.Count - 1; i++)
    {
        Node current = path[i];
        Node next = path[i + 1];

        // Highlight the outline of the node
        current.HighlightPath();

        // Highlight the edge between current -> next
        Edge connectingEdge = graph.GetEdge(current, next);
        if (connectingEdge != null)
        {
            connectingEdge.HighlightPath();
        }

        yield return new WaitForSeconds(animationDelay);
    }

    // Step 3: Highlight the final destination node outline
    path[^1].HighlightPath();
}


private IEnumerator ComputeAndAnimate()
{
    List<Node> path = null;

    if (currentMode == TraversalMode.Dijkstra)
    {
        path = new List<Node>(graph.DijkstraTraversal(startNode, endNode));
    }
    else if (currentMode == TraversalMode.DFS)
    {
        path = new List<Node>(graph.DFSTraversal(startNode, endNode));
    }

    if (path != null && path.Count > 0)
    {
        yield return AnimatePath(path);
    }
    else
    {
        LogMessage("No path found.");
        
    }

    startNode.ResetOutline();
    endNode.ResetOutline();
    startNode = null;
    endNode = null;
}

private bool ApproximatelyEqual(Vector3 a, Vector3 b, float tolerance = 0.1f)
{
    return Vector3.Distance(a, b) < tolerance;
}

public void LogMessage(string message)
{
    if (messageText == null)
    {
        Debug.LogWarning("Message Text not assigned!");
        return;
    }

    messageText.text += message + "\n";

    Debug.Log(message); // Optional: still print to Unity console
}
public void ClearGraph()
{
    // Destroy all node GameObjects
    foreach (var node in vertices)
    {
        if (node != null && node.vertexObj != null)
        {
            Destroy(node.vertexObj);
        }
    }

    // Destroy all edge GameObjects
    foreach (var edgeObj in edgeObjects)
    {
        if (edgeObj != null)
        {
            Destroy(edgeObj);
        }
    }

    // Clear data structures
    vertices.Clear();
    edgeObjects.Clear();
    graph = new MyGraph<Node, Edge>(false); // Create new empty graph

    // Clear temp selections
    selectedVertex = null;
    startNode = null;
    endNode = null;

    LogMessage("Graph cleared.");
}
public void ResetHighlights()
{
    // Reset outlines on all nodes
    foreach (var node in vertices)
    {
        if (node != null)
        {
            node.ResetOutline();
        }
    }

    // Reset colors on all edges
    foreach (var edgeObj in edgeObjects)
    {
        if (edgeObj == null) continue;
        LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.startColor = Color.blue; // Default edge color
            lr.endColor = Color.blue;
        }
    }

    // Reset selections
    selectedVertex = null;
    startNode = null;
    endNode = null;

    LogMessage("Highlights reset.");
}

public void GenerateButton(){
    GenerateRandomGraph(20,40);
}
public void GenerateRandomGraph(int numNodes, int numEdges)
{
    ClearGraph(); // Always start fresh!

    float spawnRadius = 10f; // How far apart nodes are spread (adjustable)

    // Step 1: Create random nodes
    for (int i = 0; i < numNodes; i++)
    {
        Vector2 randomPos = UnityEngine.Random.insideUnitCircle * spawnRadius;
        CreateNewVertex(randomPos);
    }

    // Step 2: Create random edges between nodes
    for (int i = 0; i < numEdges; i++)
    {
        Node nodeA = vertices[UnityEngine.Random.Range(0, vertices.Count)];
        Node nodeB = vertices[UnityEngine.Random.Range(0, vertices.Count)];

        if (nodeA != nodeB && graph.GetEdge(nodeA, nodeB) == null)
        {
            CreateEdge(nodeA, nodeB);
        }
    }

    LogMessage($"Generated random graph with {numNodes} nodes and {numEdges} edges.");
}


}

