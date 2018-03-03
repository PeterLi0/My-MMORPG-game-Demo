using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecastNavMeshQuery))]
public class RecastNavMeshQueryEditor : Editor
{
    private Vector2 _scrollPosition;

    public override void OnInspectorGUI()
    {
        var catagoryStyle = new GUIStyle();
        catagoryStyle.fontStyle = FontStyle.Bold;

		RecastNavMeshQuery recastNavMeshQuery = target as RecastNavMeshQuery;

		if (GUILayout.Button("Import NavMesh XML"))
		{
            recastNavMeshQuery.FilePath = EditorUtility.OpenFilePanel("Import NavMesh", Application.dataPath, "xml").Replace(Application.dataPath, "Assets");
		}
        if (GUILayout.Button("Import NavMesh Json"))
        {
            recastNavMeshQuery.FilePath = EditorUtility.OpenFilePanel("Import NavMesh", Application.dataPath, "json").Replace(Application.dataPath, "Assets");
        }
        if (GUILayout.Button("Import NavMesh Binary"))
        {
            recastNavMeshQuery.FilePath = EditorUtility.OpenFilePanel("Import NavMesh", Application.dataPath, "dat").Replace(Application.dataPath, "Assets");
        }
        if (GUILayout.Button("Import NavMesh for WebPlayer"))
        {
            recastNavMeshQuery.FilePath = EditorUtility.OpenFilePanel("Import NavMesh", Application.dataPath, "bytes").Replace(Application.dataPath, "Assets");
        }

        if (recastNavMeshQuery.FilePath.Contains("Resources"))
        {
            recastNavMeshQuery.FilePath = Path.GetFileNameWithoutExtension(recastNavMeshQuery.FilePath);
        }

        EditorGUILayout.LabelField("File", recastNavMeshQuery.FilePath);

        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Debug", GUIStyle.none, catagoryStyle);
        recastNavMeshQuery.StartPosition = EditorGUILayout.ObjectField("Starting Object", recastNavMeshQuery.StartPosition, typeof(GameObject), true) as GameObject;
        recastNavMeshQuery.EndPosition = EditorGUILayout.ObjectField("Ending Object", recastNavMeshQuery.EndPosition, typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("Draw Path"))
        {
            recastNavMeshQuery.Initialize();
            recastNavMeshQuery.SmoothGeneratedPath();
        }

        EditorGUILayout.LabelField("Points on Path", ""+recastNavMeshQuery.SmoothPathNum);

    }

}