using Unity.VisualScripting;
using UnityEngine;

namespace AGV
{
    public class OnMouseClick : MonoBehaviour
    {
        ImportObject parent;
        GameObject assemblyPart;
        GameObject finishedPart;
        GameObject partsPart;
        bool assembled = false;
        Renderer renderer;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            renderer = GetComponent<Renderer>();
            parent = transform.parent.parent.gameObject.GetComponent<ImportObject>();
            assemblyPart = GameObject.Find("Assembly").transform.Find(gameObject.name).gameObject;
            partsPart = GameObject.Find("Parts").transform.Find(gameObject.name).gameObject;
            finishedPart = GameObject.Find("Finished").transform.Find(gameObject.name).gameObject;
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
            if (Input.GetMouseButton(1))
            {
                return;
            }
            if (parent.canBeAssembled(gameObject.name) || parent.isAssembled(gameObject.name))
            {
                assemblyPart.SetActive(true);
            }
            if (parent.activePart != assemblyPart)
            {
                assemblyPart.GetComponent<Renderer>().material.color = partsPart.GetComponent<Renderer>().material.color;
            }
            assemblyPart.GetComponent<Outline>().enabled = true;
            finishedPart.GetComponent<Outline>().enabled = true;
            partsPart.GetComponent<Outline>().enabled = true;
        }

        void OnMouseExit()
        {
            if (parent.canBeAssembled(gameObject.name) && ((parent.activePart != assemblyPart) || (parent.reverse && !parent.play)))
            {
                assemblyPart.SetActive(false);
                Debug.Log(gameObject.name);
            }
            else if (parent.isAssembled(gameObject.name))
            {
                if (parent.activePart == assemblyPart)
                {
                    assemblyPart.GetComponent<Renderer>().material.color = Color.yellow;
                }
                else
                {
                    assemblyPart.GetComponent<Renderer>().material.color = Color.white;
                    assemblyPart.GetComponent<Outline>().enabled = false;
                }
            }
            if (parent.activePart != assemblyPart)
            {
                finishedPart.GetComponent<Outline>().enabled = false;
                partsPart.GetComponent<Outline>().enabled = false;
            }
        }
    }
}