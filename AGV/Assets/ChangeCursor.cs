using System.IO;
using UnityEngine;

namespace AGV
{
    public static class ChangeCursor
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        static public CursorMode cursorMode = CursorMode.Auto;
        static public Texture2D resizeTexture = new Texture2D(16, 16);

        // Update is called once per frame
        static ChangeCursor()
        {
#if UNITY_STANDALONE_WIN
            resizeTexture.LoadImage(File.ReadAllBytes(@"resize-cursor.png"));
            Debug.Log("Windows");
#else
                Debug.Log("not Windows");
#endif
        }


        public static void changeToResizeCursor()
        {
            Cursor.SetCursor(resizeTexture, new Vector2(11,2), cursorMode);
        }

        public static void changeToNormalCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }
}