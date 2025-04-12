using UnityEngine;

namespace AGV
{
    public class RotateAssembly : MonoBehaviour
    {
        [SerializeField]
        float rotationSpeed;
        public Vector3 centerPosition;
        public Vector3 initialPosition;
        Transform extraParts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
        {
            extraParts = transform.parent.Find("ExtraParts");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Keypad4))
            {
                transform.RotateAround(centerPosition, new Vector3(1, 0, 0), rotationSpeed);
                extraParts.RotateAround(centerPosition, new Vector3(1, 0, 0), rotationSpeed);
            }
            if (Input.GetKey(KeyCode.Keypad6))
            {
                transform.RotateAround(centerPosition, new Vector3(-1, 0, 0), rotationSpeed);
                extraParts.RotateAround(centerPosition, new Vector3(-1, 0, 0), rotationSpeed);
            }
            if (Input.GetKey(KeyCode.Keypad7))
            {
                transform.RotateAround(centerPosition, new Vector3(0, 1, 0), rotationSpeed);
                extraParts.RotateAround(centerPosition, new Vector3(0, 1, 0), rotationSpeed);
            }
            if (Input.GetKey(KeyCode.Keypad9))
            {
                transform.RotateAround(centerPosition, new Vector3(0, -1, 0), rotationSpeed);
                extraParts.RotateAround(centerPosition, new Vector3(0, -1, 0), rotationSpeed);
            }
            if (Input.GetKey(KeyCode.Keypad8))
            {
                transform.RotateAround(centerPosition, new Vector3(0, 0, -1), rotationSpeed);
                extraParts.RotateAround(centerPosition, new Vector3(0, 0, -1), rotationSpeed);
            }
            if (Input.GetKey(KeyCode.Keypad5))
            {
                transform.RotateAround(centerPosition, new Vector3(0, 0, 1), rotationSpeed);
                extraParts.RotateAround(centerPosition, new Vector3(0, 0, 1), rotationSpeed);
            }


            if (Input.GetKey(KeyCode.Keypad0))
            {
                transform.position = initialPosition;
                transform.localRotation = Quaternion.identity;
                extraParts.rotation = Quaternion.identity;
                extraParts.position = Vector3.zero;
            }
        }
    } }
