using UnityEngine;

public class Node
{
    public GameObject vertexObj;

    public Node(GameObject obj)
    {
        vertexObj = obj;
    }

    // Existing Outline Highlighting (for connecting edges)
   public void SetOutline(bool on, Color? color = null)
{
    Transform outline = vertexObj.transform.Find("Outline");

    if (on)
    {
        if (outline == null)
        {
            GameObject outlineObj = new GameObject("Outline");
            outlineObj.transform.parent = vertexObj.transform;
            outlineObj.transform.position = vertexObj.transform.position;

            SpriteRenderer originalSR = vertexObj.GetComponent<SpriteRenderer>();
            SpriteRenderer outlineSR = outlineObj.AddComponent<SpriteRenderer>();

            outlineSR.sprite = originalSR.sprite;
            outlineSR.sortingOrder = originalSR.sortingOrder - 1;
            outlineObj.transform.localScale = Vector3.one * 1.2f;

            if (color.HasValue)
            {
                outlineSR.color = color.Value;
            }
            else
            {
                outlineSR.color = Color.yellow; // Default yellow if no color given
            }
        }
        else
        {
            SpriteRenderer outlineSR = outline.GetComponent<SpriteRenderer>();
            if (outlineSR != null && color.HasValue)
            {
                outlineSR.color = color.Value;
            }
        }
    }
    else
    {
        if (outline != null)
        {
            GameObject.Destroy(outline.gameObject);
        }
    }
}


    // --- New Methods for Fill Color Control ---
public void HighlightSelectedForPathfinding()
{
    SetOutline(true, new Color(1.0f, 0.5f, 0.0f)); // Orange outline
}

public void HighlightPath()
{
    SetOutline(true, Color.green); // Green outline for final path
}

public void ResetOutline()
{
    SetOutline(false); // Remove the outline
}


}
