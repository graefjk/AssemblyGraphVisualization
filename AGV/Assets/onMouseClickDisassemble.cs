using UnityEngine;

namespace AGV
{
    public class OnMouseClickDisassemble : MonoBehaviour
    {
        AGVManager parent;

        void Start()
        {
            parent = transform.parent.parent.gameObject.GetComponent<AGVManager>();
        }
        void OnMouseDown()
        {
            parent.mouseClick(gameObject.name);
            //      renderer.material.color = Color.black;
        }
    }
}
