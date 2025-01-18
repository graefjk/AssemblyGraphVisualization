using UnityEngine;

public class OnMouseClick : MonoBehaviour
{
    ImportObject parent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parent = transform.parent.gameObject.GetComponent<ImportObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        parent.mouseClick(gameObject.name);
    }
}
