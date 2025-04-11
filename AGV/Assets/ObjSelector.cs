using System.Collections.Generic;
using Mono.WebBrowser;
using SimpleWebBrowser;
using TransformHandles;
using TransformHandles.Utils;
using UnityEngine;

public class ObjSelector : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;
    [SerializeField] private bool changeColor = false;

    private Camera _camera;

    private TransformHandleManager _manager;
    
    private Handle _lastHandle;
    private Dictionary<Transform, Handle> _handleDictionary;
    public WebBrowser2D MainBrowser;

    private void Awake()
    {
        _camera = Camera.main;
        
        _manager = TransformHandleManager.Instance;
        _handleDictionary = new Dictionary<Transform, Handle>();

        MainBrowser = MainBrowser = GameObject.Find("Browser2D").GetComponent<WebBrowser2D>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, layerMask))
            {
                var hitTransform = hit.transform;
                var children = hitTransform.GetComponentsInChildren<Transform>();
                if (_handleDictionary.ContainsKey(hitTransform))
                {
                    RemoveTarget(hitTransform);
                    DeselectObject(hitTransform);
                    foreach (var child in children)
                    {
                        DeselectObject(child);
                    }
                    return;
                }
                CreateHandle(hitTransform);
                foreach (var child in children)
                {
                    SelectObject(child);
                }
            }
        }
        // Add the object to handle if exists, else create a new handle
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, layerMask))
            {
                var hitTransform = hit.transform;
                var children = hitTransform.GetComponentsInChildren<Transform>();
                if (_handleDictionary.ContainsKey(hitTransform)) //remove
                {
                    RemoveTarget(hitTransform);
                    DeselectObject(hitTransform);
                    foreach (var child in children)
                    {
                        DeselectObject(child);
                    }
                    return;
                }
                ;
                if (_lastHandle == null) { CreateHandle(hitTransform); }
                else { AddTarget(hitTransform); }
                

                foreach (var child in children)
                {
                    SelectObject(child);
                }
            }
        }
        // Remove the object from handle
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(1))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var hitTransform = hit.transform;
                if(!_handleDictionary.ContainsKey(hitTransform)) return;
                RemoveTarget(hitTransform);
                DeselectObject(hitTransform);
                var children = hitTransform.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    DeselectObject(child);
                }
            }
        }

        // Create new handle for object
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(2))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 1000f, layerMask)) return;
            if(_handleDictionary.ContainsKey(hit.transform)) return;
            var hitTransform = hit.transform;
            CreateHandle(hitTransform);
            SelectObject(hitTransform);
        }
    }

    private void DeselectObject(Transform hitInfoTransform)
    {
        _handleDictionary.Remove(hitInfoTransform);

        hitInfoTransform.tag = "Untagged";
        var rendererComponent = hitInfoTransform.gameObject.GetComponent<Renderer>();
        if (rendererComponent == null) rendererComponent = hitInfoTransform.GetComponentInChildren<Renderer>();
        if(changeColor) rendererComponent.material.color = unselectedColor;
    }

    private void SelectObject(Transform hitInfoTransform)
    {
        _handleDictionary.Add(hitInfoTransform, _lastHandle);

        hitInfoTransform.tag = "Selected";
        var rendererComponent = hitInfoTransform.gameObject.GetComponent<Renderer>();
        if (rendererComponent == null) rendererComponent =  hitInfoTransform.GetComponentInChildren<Renderer>();
        if (changeColor) rendererComponent.material.color = selectedColor;
    }
    
    private void CreateHandle(Transform hitTransform)
    {
        Handle handle = _manager.CreateHandle(hitTransform);
        handle.type = HandleType.All;
        _manager.ChangeHandleSpace(handle, Space.Self);
        TransformHandleManager.ChangeHandleType(handle, HandleType.PositionRotation);
        handle.target.transform.position = hitTransform.GetComponent<MeshCollider>().bounds.center;
        _lastHandle = handle;

        handle.OnInteractionEvent += OnHandleInteraction;
        handle.OnHandleDestroyedEvent += OnHandleDestroyed;
        handle.OnInteractionStartEvent += OnInteractionStart;
        handle.OnInteractionEndEvent += OnInteractionEnd;
    }

    private void AddTarget(Transform hitTransform)
    {
        _manager.AddTarget(hitTransform, _lastHandle);
    }
    
    private void RemoveTarget(Transform hitTransform)
    {
        var handle = _handleDictionary[hitTransform];
        if (_lastHandle == handle) _lastHandle = null;

        _manager.RemoveTarget(hitTransform, handle);
    }


    private static void OnHandleInteraction(Handle handle)
    {
        Debug.Log($"{handle.name} is being interacted with");
    }
    
    
    private void OnHandleDestroyed(Handle handle)
    {
        handle.OnInteractionEvent -= OnHandleInteraction;
        handle.OnHandleDestroyedEvent -= OnHandleDestroyed;
    }

    private void OnInteractionStart(Handle handle)
    {
        MainBrowser.RunJavaScript("document.body.style.userSelect = 'none'");
    }

    private void OnInteractionEnd(Handle handle)
    {
        MainBrowser.RunJavaScript("document.body.style.userSelect = 'auto'");
    }
}