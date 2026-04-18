using UnityEngine;

namespace Services.Cursor
{
    [CreateAssetMenu(fileName = "CursorStyle", menuName = "Scriptables/Cursor Style", order = 1)]
    public class CursorStyle : ScriptableObject
    {
        public Texture2D cursorTexture;
        public Vector2 hotSpot = Vector2.zero;

        public void Apply()
        {
            UnityEngine.Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
    }
}