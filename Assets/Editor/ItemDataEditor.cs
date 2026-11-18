using Invent;
using UnityEditor;

[CustomEditor(typeof(Invent.ItemData))]
[CanEditMultipleObjects]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "weaponDamage", "consumableType", "amount");

        SerializedProperty itemType = serializedObject.FindProperty("itemType");
        if (!itemType.hasMultipleDifferentValues)
        {
            if (itemType.enumValueIndex == (int)ItemType.Weapon)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponDamage"));
            else if (itemType.enumValueIndex == (int)ItemType.Consumable)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("consumableType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("amount"));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
