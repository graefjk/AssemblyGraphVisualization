using UnityEngine;

namespace AGV
{
    public static class ChangeCursor
    {
        static public CursorMode cursorMode = CursorMode.Auto;
        static public Texture2D resizeTexture;
        static public Texture2D resizeTextureBottom;

        public static void changeToResizeCursor()
        {
            Cursor.SetCursor(resizeTexture, new Vector2(11,2), cursorMode);
        }

        public static void changeToResizeCursorBottom()
        {
            Cursor.SetCursor(resizeTextureBottom, new Vector2(2, 11), cursorMode);
        }

        public static void changeToNormalCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }
}