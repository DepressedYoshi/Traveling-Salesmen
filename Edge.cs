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
}
