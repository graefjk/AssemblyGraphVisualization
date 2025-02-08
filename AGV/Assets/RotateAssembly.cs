using UnityEngine;

public class rotateAssembly : MonoBehaviour
{
    [SerializeField]
    float rotationSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Keypad4))
        {
            transform.Rotate(new Vector3(0, 0, -1) * rotationSpeed);
        }
        if (Input.GetKey(KeyCode.Keypad6))
        {
            transform.Rotate(new Vector3(0, 0, 1) * rotationSpeed);
        }
        if (Input.GetKey(KeyCode.Keypad7))
        {
            transform.Rotate(new Vector3(1, 0, 0) * rotationSpeed);
        }
        if (Input.GetKey(KeyCode.Keypad9))
        {
            transform.Rotate(new Vector3(-1, 0, 0) * rotationSpeed);
        }
        if (Input.GetKey(KeyCode.Keypad8))
        {
            transform.Rotate(new Vector3(0, -1, 0) * rotationSpeed);
        }
        if (Input.GetKey(KeyCode.Keypad5))
        {
            transform.Rotate(new Vector3(0, 1, 0) * rotationSpeed);
        }

        if (Input.GetKey(KeyCode.Keypad0))
        {
            transform.localRotation = Quaternion.identity;
        }
    }
}
