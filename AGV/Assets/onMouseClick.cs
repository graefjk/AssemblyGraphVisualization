using SimpleWebBrowser;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AGV
{
    public class OnMouseClick : MonoBehaviour
    {
        ImportObject parent;
        GameObject assemblyPart;
        GameObject finishedPart;
        GameObject partsPart;
        RawImage ui;

        public WebBrowser2D MainBrowser;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            parent = transform.parent.parent.gameObject.GetComponent<ImportObject>();
            assemblyPart = GameObject.Find("Assembly").transform.Find(gameObject.name).gameObject;
            partsPart = GameObject.Find("Parts").transform.Find(gameObject.name).gameObject;
            finishedPart = GameObject.Find("Finished").transform.Find(gameObject.name).gameObject;
            MainBrowser = GameObject.Find("Browser2D").GetComponent<WebBrowser2D>();
            ui = GameObject.Find("Browser2D").GetComponent<RawImage>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnMouseDown()
        {
            if (!isUI())
            {
                parent.mouseClick(gameObject.name);
            }
            //      renderer.material.color = Color.black;
        }


        bool mouseEntered = false;
        public void OnMouseEnter()
        {
            if (UnityEngine.Input.GetMouseButton(1) || isUI())
            {
                return;
            }
            OnMouseEnterDontCheckUI();
        }

        public void OnMouseEnterDontCheckUI()
        {
            if (parent.canBeAssembled(gameObject.name) || parent.isAssembled(gameObject.name) || parent.previewPartsThatCannotBeAssembledRightNow)
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
            MainBrowser.RunJavaScript("document.getElementById(" + gameObject.name + ").style.borderColor = 'black'");
            mouseEntered = true;
        }


        public void OnMouseOver()
        {
            if (isUI())
            {
                if (mouseEntered)
                    OnMouseExit();
            }
            else
            {
                if (!mouseEntered)
                    OnMouseEnter();
            }

        }

        public void OnMouseExit()
        {
            if ((parent.canBeAssembled(gameObject.name) || parent.previewPartsThatCannotBeAssembledRightNow) && ((parent.activePart != assemblyPart) || (parent.reverse && !parent.play)) && !parent.isAssembled(gameObject.name))
            {
                assemblyPart.SetActive(false);
                //Debug.Log(gameObject.name);
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
            MainBrowser.RunJavaScript("document.getElementById(" + gameObject.name + ").style.borderColor = document.getElementById(" + gameObject.name + ").style.backgroundColor");
            mouseEntered = false;
        }

        public bool isUI()
        {
            Vector2 screenCords = MainBrowser.GetScreenCoords();
            return (((Texture2D)ui.texture).GetPixel((int)screenCords.x, (int)screenCords.y).a > 0);
        }
    }
}