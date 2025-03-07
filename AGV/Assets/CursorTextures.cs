using AGV;
using UnityEngine;

public class CursorTextures : MonoBehaviour
{
    [SerializeField]
    public Texture2D resizeCursor;
    [SerializeField]
    public Texture2D resizeCursorBottom;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeCursor.resizeTexture = resizeCursor;
        ChangeCursor.resizeTextureBottom = resizeCursorBottom;
    }
}
