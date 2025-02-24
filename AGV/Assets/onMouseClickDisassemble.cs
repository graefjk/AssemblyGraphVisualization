using UnityEngine;

namespace AGV
{
    public class OnMouseClickDisassemble : MonoBehaviour
    {
        ImportObject parent;

        void Start()
        {
            parent = transform.parent.parent.gameObject.GetComponent<ImportObject>();
        }
        void OnMouseDown()
        {
            parent.mouseClick(gameObject.name);
            //      renderer.material.color = Color.black;
        }
    }
}
