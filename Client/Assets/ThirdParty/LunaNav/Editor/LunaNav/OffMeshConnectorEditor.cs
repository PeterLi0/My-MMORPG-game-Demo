using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OffMeshConnector))]
public class OffMeshConnectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OffMeshConnector offMesh = target as OffMeshConnector;
        if (offMesh != null)
        {
            offMesh.EndPosition = EditorGUILayout.Vector3Field("End Position", offMesh.EndPosition);
            offMesh.Radius = EditorGUILayout.FloatField("Radius", offMesh.Radius);
            offMesh.Bidirectional = EditorGUILayout.Toggle("Bidirectional", offMesh.Bidirectional);
        }
    }

    public void OnSceneGUI()
    {
        OffMeshConnector offMesh = target as OffMeshConnector;
        if (offMesh != null)
        {
            offMesh.EndPosition = Handles.PositionHandle(offMesh.EndPosition, Quaternion.identity);
        }
    }
}
