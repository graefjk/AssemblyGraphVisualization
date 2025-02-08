using Css.Properties;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    public float speed = 1;
    [SerializeField]
    public float rotationSpeed = 1;
    [SerializeField]
    public float distanceFactor = 3;
    ImportObject importer;

    GameObject assembly;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        assembly = GameObject.Find("Assembly");
        importer = GameObject.Find("Importer").GetComponent<ImportObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Bounds bounds = importer.getAssemblyBounds();
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



        if (Input.GetKey("1"))
        {
            transform.position = bounds.center - new Vector3(distanceFactor * bounds.extents.x, 0, 0);
            transform.rotation = Quaternion.Euler(0,90,0);
        }
        if (Input.GetKey("2"))
        {
            transform.position = bounds.center + new Vector3(distanceFactor * bounds.extents.x, 0, 0);
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }
        if (Input.GetKey("3"))
        {
            transform.position = bounds.center - new Vector3(0, distanceFactor * bounds.extents.y, 0);
            transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
        if (Input.GetKey("4"))
        {
            transform.position = bounds.center + new Vector3(0, distanceFactor * bounds.extents.y, 0);
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        if (Input.GetKey("5"))
        {
            transform.position = bounds.center - new Vector3(0, 0, distanceFactor * bounds.extents.z);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (Input.GetKey("6"))
        {
            transform.position = bounds.center + new Vector3(0, 0, distanceFactor * bounds.extents.z);
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
