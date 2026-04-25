using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // V? t?t c? field m?c ??nh tr? cropData
        DrawPropertiesExcluding(serializedObject, "cropData");

        // Ch? hi?n cropData khi ItemType = Seed
        Item item = (Item)target;
        if (item.itemType == ItemType.Seed)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Seed Settings", EditorStyles.boldLabel);
            SerializedProperty cropDataProp = serializedObject.FindProperty("cropData");
            EditorGUILayout.PropertyField(cropDataProp, new GUIContent("Crop Data"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
