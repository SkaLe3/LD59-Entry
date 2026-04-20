#if UNITY_EDITOR

using Services.Cursor;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(CursorStyle))]
public class CursorStyleEditor : Editor
{

    private const float DotSize = 5f;
    private int Size = 300;

    public override void OnInspectorGUI()
    {
        CursorStyle cursorSetting = (CursorStyle)target;

        EditorGUI.BeginChangeCheck();
        cursorSetting.cursorTexture = (Texture2D)EditorGUILayout.ObjectField("Cursor Texture", cursorSetting.cursorTexture, typeof(Texture2D), false);

        if (cursorSetting.cursorTexture != null)
        {
            GUILayout.Label("Preview:");
            Rect rect = GUILayoutUtility.GetRect(Size, Size);
            EditorGUI.DrawTextureTransparent(rect, cursorSetting.cursorTexture, ScaleMode.ScaleToFit);
            float x;
            float y;

            // Calculate the position of the dot relative to the texture size
            float offsetX = (rect.width - rect.height) / 2;
            offsetX = Mathf.Clamp(offsetX, 0, offsetX);
            float offsetY = (rect.width - rect.height) / 2;

            if (rect.width > rect.height)
            {
                offsetY = Mathf.Clamp(offsetY, offsetY, 0);
            }

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
            {
                Vector2 mousePos = currentEvent.mousePosition - new Vector2(rect.x, rect.y);
                x = (-offsetX + mousePos.x) * (cursorSetting.cursorTexture.width / Mathf.Clamp(rect.width, 0, Size));
                y = (-offsetY + mousePos.y) * (cursorSetting.cursorTexture.width / Mathf.Clamp(rect.width, 0, Size));
                cursorSetting.hotSpot = new Vector2(x, y);
                GUI.changed = true; // Mark GUI as changed so changes are saved
            }

            x = offsetX + cursorSetting.hotSpot.x * (Mathf.Clamp(rect.width, 0, Size) / cursorSetting.cursorTexture.width);
            y = -offsetY + cursorSetting.hotSpot.y * (Mathf.Clamp(rect.width, 0, Size) / cursorSetting.cursorTexture.width);
            Vector2 dotPosition = new Vector2(rect.x + x - DotSize / 2, rect.y + y - DotSize / 2);

            // Draw red dot
            EditorGUI.DrawRect(new Rect(dotPosition.x, dotPosition.y, DotSize, DotSize), Color.red);

            // Draw label with hot spot coordinates
            string label = $"Hot Spot: ({cursorSetting.hotSpot.x}, {cursorSetting.hotSpot.y})";
            GUIContent content = new GUIContent(label);
            GUIStyle style = GUI.skin.GetStyle("Label");
            Vector2 labelSize = style.CalcSize(content);
            Rect labelRect = new Rect(dotPosition.x + DotSize, dotPosition.y, labelSize.x, labelSize.y);
            EditorGUI.LabelField(labelRect, content, style);


        EditorGUILayout.Space(20);

        cursorSetting.hotSpot = EditorGUILayout.Vector2Field("Hot Spot", cursorSetting.hotSpot);
        cursorSetting.hotSpot = new Vector2(Mathf.RoundToInt(cursorSetting.hotSpot.x), Mathf.RoundToInt(cursorSetting.hotSpot.y));
        cursorSetting.hotSpot = new Vector2(Mathf.Clamp(cursorSetting.hotSpot.x, 0, cursorSetting.cursorTexture.width), Mathf.Clamp(cursorSetting.hotSpot.y, 0, cursorSetting.cursorTexture.height));
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(cursorSetting);
        }
    }
}

#endif
