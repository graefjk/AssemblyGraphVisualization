using SimpleWebBrowser;
using UnityEngine;
using UnityEngine.UI;


namespace AGV
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField]
        public float speed = 1;
        [SerializeField]
        public float rotationSpeed = 1;
        [SerializeField]
        public float distanceFactor = 3;
        AGVManager manager;
        WebBrowser2D MainBrowser;
        RawImage ui;
        public float scrollSpeed;

        GameObject assembly;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            assembly = GameObject.Find("Assembly");
            manager = GameObject.Find("AGVManager").GetComponent<AGVManager>();
            MainBrowser = GameObject.Find("Browser2D").GetComponent<WebBrowser2D>();
            ui = GameObject.Find("Browser2D").GetComponent<RawImage>();
        }

        // Update is called once per frame
        void Update()
        {
            if (manager.textAreaHasFocus)
            {
                return;
            }

            Bounds bounds = manager.getAssemblyBounds();
            Vector3 center = assembly.GetComponent<RotateAssembly>().centerPosition;
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftControl))
            {
                speed *= 2;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftShift))
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

            if (Input.GetMouseButton(1) && !isUI())
            {
                if (UnityEngine.Cursor.lockState == CursorLockMode.Locked)
                {
                    transform.eulerAngles += new Vector3(-Input.mousePositionDelta.y, Input.mousePositionDelta.x, 0) * rotationSpeed * Time.deltaTime;
                }
                else
                {
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                }

            }
            else
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
            }



            if (Input.GetKey("1"))
            {
                transform.position = bounds.center + assembly.transform.position - new Vector3(distanceFactor * bounds.extents.x, 0, 0);
                transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            if (Input.GetKey("2"))
            {
                transform.position = bounds.center + assembly.transform.position + new Vector3(distanceFactor * bounds.extents.x, 0, 0);
                transform.rotation = Quaternion.Euler(0, -90, 0);
            }
            if (Input.GetKey("3"))
            {
                transform.position = bounds.center + assembly.transform.position - new Vector3(0, distanceFactor * bounds.extents.y, 0);
                transform.rotation = Quaternion.Euler(-90, 0, 0);
            }
            if (Input.GetKey("4"))
            {
                transform.position = bounds.center + assembly.transform.position + new Vector3(0, distanceFactor * bounds.extents.y, 0);
                transform.rotation = Quaternion.Euler(90, 0, 0);
            }
            if (Input.GetKey("5"))
            {
                transform.position = bounds.center + assembly.transform.position - new Vector3(0, 0, distanceFactor * bounds.extents.z);
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            if (Input.GetKey("6"))
            {
                transform.position = bounds.center + assembly.transform.position + new Vector3(0, 0, distanceFactor * bounds.extents.z);
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }

            if (!manager.textDivHover) {
                Camera.main.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            }
        }

        public bool isUI()
        {
            Vector2 screenCords = MainBrowser.GetScreenCoords();
            return (((Texture2D)ui.texture).GetPixel((int)screenCords.x, (int)screenCords.y).a > 0);
        }
    }
}