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
            parent.mouseClick(gameObject.name.Replace("(Clone)", ""));
            //      renderer.material.color = Color.black;
        }
    }
}
