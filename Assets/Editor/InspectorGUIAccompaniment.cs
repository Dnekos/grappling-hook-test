using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Cloner))]
public class InspectorGUIAccompaniment : Editor 
{
    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        if (GUILayout.Button("Apply Above to This Object's Transform"))
        {
            Cloner cloner = (Cloner)target;
                cloner.ApplyChanges();
        }
    }

}
