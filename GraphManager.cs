using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphManager : MonoBehaviour
{
    public GameObject vertexPrefab;
    public GameObject edgePrefab;

    private MyGraph<Node, Edge> graph;
    private List<Node> vertices;
    private Node selectedVertex = null;

    private LineRenderer tempLine;
    private GameObject tempEdgeObj;

    private void Start()
    {
        graph = new MyGraph<Node, Edge>(false);
        vertices = new List<Node>();
    }

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick(mousePos);
        }

        UpdateTempLine(mousePos);
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

            Debug.Log($"Created edge between {nodeA.vertexObj.name} and {nodeB.vertexObj.name} with weight {randomWeight}");
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
        selectedVertex.Highlight(true);
    }

    private void DeselectVertex()
    {
        if (selectedVertex != null)
        {
            selectedVertex.Highlight(false);
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
}
