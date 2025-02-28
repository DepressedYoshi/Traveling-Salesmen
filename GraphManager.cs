using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject vertexPrefab; 
    public GameObject edgePrefab;   

    private MyGraph<GameObject, float> graph; 
    private List<GameObject> vertices;
    private GameObject selectedVertex = null; 

    private LineRenderer tempLine;
    private GameObject tempEdgeObj;

    void Start()
    {
        graph = new MyGraph<GameObject, float>(false); 
        vertices = new List<GameObject>();
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick(mousePos);
        }

        UpdateTempLine(mousePos);
    }

    void HandleMouseClick(Vector2 mousePos)
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

    void HandleVertexClick(GameObject clickedVertex)
    {
        if (selectedVertex == null)
        {
            SelectVertex(clickedVertex);
        }
        else if (selectedVertex == clickedVertex)
        {
            DeselectVertex();
        }
        else
        {
            CreateEdge(selectedVertex, clickedVertex);
            DeselectVertex();
        }
    }

    void CreateNewVertex(Vector2 position)
    {
        GameObject newVertex = CreateVertex(position);
        
        if (selectedVertex != null)
        {
            CreateEdge(selectedVertex, newVertex);
            DeselectVertex();
        }
    }

    GameObject CreateVertex(Vector2 position)
    {
        GameObject vertex = Instantiate(vertexPrefab, position, Quaternion.identity);
        vertex.name = "Vertex_" + graph.NumVertices().ToString();
        graph.InsertVertex(vertex);
        vertices.Add(vertex);

        SetRandomColor(vertex);

        return vertex;
    }

    void CreateEdge(GameObject vertexA, GameObject vertexB)
    {
        try
        {
            float randomWeight = UnityEngine.Random.Range(0f, 100f); 
            graph.InsertEdge(vertexA, vertexB, randomWeight);

            GameObject edgeObj = Instantiate(edgePrefab);
            LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
            if (lr == null)
            {
                lr = edgeObj.AddComponent<LineRenderer>();
            }

            SetupEdgeLineRenderer(lr, vertexA.transform.position, vertexB.transform.position);
            Debug.Log($"Created edge between {vertexA.name} and {vertexB.name} with weight {randomWeight}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error creating edge: " + e.Message);
        }
    }

    void SetupEdgeLineRenderer(LineRenderer lr, Vector3 start, Vector3 end)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.black;
        lr.endColor = Color.black;
    }

    void SetRandomColor(GameObject vertex)
    {
        Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        SpriteRenderer sr = vertex.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = randomColor;
        }
    }

    void SelectVertex(GameObject vertex)
    {
        selectedVertex = vertex;
        HighlightVertex(selectedVertex, true);
    }

    void DeselectVertex()
    {
        if (selectedVertex != null)
        {
            HighlightVertex(selectedVertex, false);
            selectedVertex = null;
        }
    }

    void HighlightVertex(GameObject vertex, bool highlight)
    {
        SpriteRenderer sr = vertex.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = highlight ? Color.yellow : new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        }
    }

    void UpdateTempLine(Vector2 mousePos)
    {
        if (selectedVertex != null)
        {
            if (tempLine == null)
            {
                SetupTempLine();
            }
            tempLine.SetPosition(0, selectedVertex.transform.position);
            tempLine.SetPosition(1, mousePos);
        }
        else
        {
            RemoveTempLine();
        }
    }

    void SetupTempLine()
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

    void RemoveTempLine()
    {
        if (tempLine != null)
        {
            Destroy(tempEdgeObj);
            tempLine = null;
        }
    }
}
