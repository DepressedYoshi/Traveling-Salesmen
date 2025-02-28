using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphManager : MonoBehaviour
{    public GameObject vertexPrefab; 
    public GameObject edgePrefab;   

    private MyGraph<GameObject, float> graph; 
    private List<GameObject> vertices;
    private GameObject selectedVertex = null; 

    private LineRenderer tempLine;
    private GameObject tempEdgeObj;

    private void Start()
    {
        graph = new MyGraph<GameObject, float>(false); 
        vertices = new List<GameObject>();
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

    private void CreateNewVertex(Vector2 position)
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

    private void CreateEdge(GameObject vertexA, GameObject vertexB)
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
            //display the weight 
            GameObject weightLabel = CreateEdgeLabel(vertexA.transform.position, vertexB.transform.position, randomWeight);
            weightLabel.transform.parent = edgeObj.transform;
            Debug.Log($"Created edge between {vertexA.name} and {vertexB.name} with weight {randomWeight}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error creating edge: " + e.Message);
        }
    }

    GameObject CreateEdgeLabel(Vector3 start, Vector3 end, float weight)
{
    GameObject textObj = new GameObject("EdgeWeight");
    textObj.transform.position = (start + end) / 2; // Set position to the middle of the edge

    TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
    textMesh.text = weight.ToString("F1"); // Display weight with one decimal place
    textMesh.fontSize = 3;
    textMesh.alignment = TextAlignmentOptions.Center;
    textMesh.color = Color.black; // Change to any color you like
    textMesh.sortingOrder = 5;
    return textObj;
}


    private void SetupEdgeLineRenderer(LineRenderer lr, Vector3 start, Vector3 end){
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Make the line thicker
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;

        // Assign a brighter color (like blue)
        Color lineColor = new Color(0.2f, 0.6f, 1f); // Light blue
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        // Improve appearance
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.numCapVertices = 10; // Makes the line ends rounded
        lr.numCornerVertices = 10; // Smooths the corners
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

    private void SelectVertex(GameObject vertex)
    {
        selectedVertex = vertex;
        HighlightVertex(selectedVertex, true);
    }

   private void DeselectVertex()
    {
        if (selectedVertex != null)
        {
            HighlightVertex(selectedVertex, false);
            selectedVertex = null;
        }
    }

    private void HighlightVertex(GameObject vertex, bool highlight)
{
    Transform outline = vertex.transform.Find("Outline"); // Check if the outline already exists

    if (highlight)
    {
        if (outline == null) // If no outline exists, create one
        {
            GameObject outlineObj = new GameObject("Outline");
            outlineObj.transform.parent = vertex.transform;
            outlineObj.transform.position = vertex.transform.position;

            SpriteRenderer originalSR = vertex.GetComponent<SpriteRenderer>();
            SpriteRenderer outlineSR = outlineObj.AddComponent<SpriteRenderer>();

            outlineSR.sprite = originalSR.sprite; // Copy the same sprite
            outlineSR.sortingOrder = originalSR.sortingOrder - 1; // Render behind the main vertex
            outlineSR.color = Color.yellow; // Set outline color to yellow

            outlineObj.transform.localScale = Vector3.one * 1.2f; // Slightly scale up
        }
    }
    else
    {
        if (outline != null)
        {
            Destroy(outline.gameObject); // Remove the outline when deselecting
        }
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
            tempLine.SetPosition(0, selectedVertex.transform.position);
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
