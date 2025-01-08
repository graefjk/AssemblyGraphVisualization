using UnityEngine;
using AsImpL;

public class ImportObject : MonoBehaviour
{
    [SerializeField]
    public GameObject obj;
    protected ImportOptions importOptions = new ImportOptions();
    ObjectImporter objImporter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        importOptions.litDiffuse = true;
        objImporter = gameObject.GetComponent<ObjectImporter>();
        objImporter.ImportModelAsync("0", "C:\\Users\\janni\\Documents\\GitHub\\ATM-AGV\\assets\\joint_assembly\\00000\\0.obj", null, importOptions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
