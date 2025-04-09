using TransformHandles;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using UnityEngine.UIElements;

public class onMouseClickExtraPart : MonoBehaviour
{
    private TransformHandleManager _manager;
    TransformHandles.Handle handle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _manager = TransformHandleManager.Instance;

    }

    void OnMouseDown()
    {

        if (UnityEngine.Input.GetKey(KeyCode.LeftShift)) // maybe add && currentVertex.Contains('"'+name+'"')
        {
            handle = _manager.CreateHandle(transform);
            _manager.AddTarget(transform, handle);
        }
        //      renderer.material.color = Color.black;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
