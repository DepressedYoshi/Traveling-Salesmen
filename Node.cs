using UnityEngine;

public class Node
{
    public GameObject vertexObj;

    public Node(GameObject obj)
    {
        vertexObj = obj;
    }

    public void Highlight(bool on)
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
                outlineSR.color = Color.yellow;
                outlineObj.transform.localScale = Vector3.one * 1.2f;
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
}
