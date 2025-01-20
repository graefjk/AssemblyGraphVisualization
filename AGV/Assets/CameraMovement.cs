using Css.Properties;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    public float speed = 1;
    [SerializeField]
    public float rotationSpeed = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed *= 2;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed /= 2;
        }

        if (Input.GetKey("a"))
        {
            transform.position -= transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey("d"))
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey("w"))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey("s"))
        {
            transform.position -= transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey("e") || Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
        if (Input.GetKey("c"))
        {
            transform.position -= transform.up * speed * Time.deltaTime;
        }

        if (Input.GetMouseButton(1))
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            transform.eulerAngles += new Vector3(-Input.mousePositionDelta.y, Input.mousePositionDelta.x, 0) * rotationSpeed * Time.deltaTime;
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
    }
}
