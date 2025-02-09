using UnityEngine;

public class OnMouseClick : MonoBehaviour
{
    ImportObject parent;
    GameObject assemblyPart;
    bool assembled = false;
    Renderer renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<Renderer>();
        parent = transform.parent.parent.gameObject.GetComponent<ImportObject>();
        assemblyPart = GameObject.Find("Assembly").transform.Find(gameObject.name + "(Clone)").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        parent.mouseClick(gameObject.name);
        assembled = true;
//      renderer.material.color = Color.black;
    }


    void OnMouseEnter()
    {
        if (parent.canBeAssembled(gameObject.name))
        {
            assemblyPart.SetActive(true);
            assemblyPart.GetComponent<Renderer>().material.color = Color.blue;
        }
    }

    void OnMouseExit()
    {
        if (parent.canBeAssembled(gameObject.name))
        {
            assemblyPart.SetActive(false);
            Debug.Log(gameObject.name);
        }
    }
}
