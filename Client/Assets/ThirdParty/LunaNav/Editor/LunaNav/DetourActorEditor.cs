using System;
using LunaNav;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DetourActor))]
public class DetourActorEditor : Editor
{
    private Vector2 _scrollPosition;

    public override void OnInspectorGUI()
    {
        DetourActor detourActor = target as DetourActor;
        if (detourActor == null)
            return;

        var catagoryStyle = new GUIStyle();
        catagoryStyle.fontStyle = FontStyle.Bold;
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false); //Last two bools are if we want to show the scroll bar always.
        EditorGUILayout.BeginVertical();

        EditorGUILayout.PrefixLabel("Properties", GUIStyle.none, catagoryStyle);

        detourActor.Radius = EditorGUILayout.Slider("Actor Radius", detourActor.Radius, 0.1f, 5f);
        detourActor.Height = EditorGUILayout.Slider("Actor Height", detourActor.Height, 0.1f, 5f);
        detourActor.MaxAcceleration = EditorGUILayout.Slider("Max Acceleration", detourActor.MaxAcceleration, 0.1f, 90f);
        detourActor.MaxSpeed = EditorGUILayout.Slider("Max Speed", detourActor.MaxSpeed, 0.1f, 90f);
        detourActor.CollisionQueryRange = EditorGUILayout.Slider("Collision Check Range", detourActor.CollisionQueryRange, 0.1f, 90f);
        detourActor.PathOptimizationRange = EditorGUILayout.Slider("Path Optimization Range", detourActor.PathOptimizationRange, 0.1f, 90f);

        detourActor.UpdateFlags = (UpdateFlags)EditorGUILayout.MaskField("Update Flags", (int)detourActor.UpdateFlags, Enum.GetNames(typeof(UpdateFlags)));
        //detourActor.ObstacleAvoidanceType = (ObstacleAvoidanceType)EditorGUILayout.MaskField("Update Flags", (short)detourActor.ObstacleAvoidanceType, Enum.GetNames(typeof(ObstacleAvoidanceType)));

        detourActor.SeparationWeight = EditorGUILayout.Slider("Separation", detourActor.SeparationWeight, 0.1f, 5f);
        detourActor.WaitForUpdate = EditorGUILayout.Slider(new GUIContent("Update Time", "Time between updates in seconds for the target position - 0 = always update"), detourActor.WaitForUpdate, 0.0f, 5f, new GUILayoutOption[]{});

        if (GUILayout.Button("Reset"))
        {
            detourActor.ResetValues();
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Setup", GUIStyle.none, catagoryStyle);
        detourActor.Target = EditorGUILayout.ObjectField("Target", detourActor.Target, typeof(GameObject), true) as GameObject;

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

    }
}
