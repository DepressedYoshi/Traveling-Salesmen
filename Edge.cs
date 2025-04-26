using UnityEngine;

public class Edge
{
    public LineRenderer line;
    public float weight;

    public Edge(LineRenderer line, float weight)
    {
        this.line = line;
        this.weight = weight;
    }

    public void Highlight(bool on)
    {
        if (on)
        {
            line.startColor = Color.red;
            line.endColor = Color.red;
        }
        else
        {
            line.startColor = Color.blue;
            line.endColor = Color.blue;
        }
    }
    public void SetColor(Color color)
{
    if (line != null)
    {
        line.startColor = color;
        line.endColor = color;
    }
}

public void HighlightSelected()
{
    SetColor(Color.yellow); // Selected connection
}

public void HighlightPath()
{
    SetColor(Color.green); // Final path
}

public void ResetColor()
{
    SetColor(Color.blue); // Default connection color
}

}
