using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LunaNav;
using Recast.Json;
using Recast.Json.Bson;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Formatting = System.Xml.Formatting;

[CustomEditor(typeof(RecastNavMesh))]
public class RecastNavMeshEditor : Editor {

    private Vector2 _scrollPosition;
    private bool[] fold;
        
    /// <summary>
    /// Sets up the GUI so users can export the NavMesh data into XML or Binary formats
    /// </summary>
	public override void OnInspectorGUI()
	{
        var catagoryStyle = new GUIStyle();
        catagoryStyle.fontStyle = FontStyle.Bold;
        RecastNavMesh recastNavMesh = target as RecastNavMesh;
        if (recastNavMesh == null || recastNavMesh.NavMesh == null)
            return;
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false); //Last two bools are if we want to show the scroll bar always.

        EditorGUILayout.BeginVertical();
        if (fold == null)
        {
            fold = new bool[recastNavMesh.NavMesh._tiles.Length];
        }
        if (fold.Length != recastNavMesh.NavMesh._tiles.Length)
        {
            var temp = new bool[recastNavMesh.NavMesh._tiles.Length];
            Array.Copy(fold, 0, temp, 0, fold.Length);
            fold = temp;
        }
        for(int i = 0; i < recastNavMesh.NavMesh._tiles.Length; i++)
        {
            if (recastNavMesh.NavMesh._tiles[i] != null && recastNavMesh.NavMesh._tiles[i].Verts != null && recastNavMesh.NavMesh._tiles[i].Verts.Length != 0)
            {
                fold[i] = EditorGUILayout.Foldout(fold[i], new GUIContent(string.Format("{0}, {1}", recastNavMesh.NavMesh._tiles[i].Header.X, recastNavMesh.NavMesh._tiles[i].Header.Y)));
                if (fold[i])
                {
                    if (GUILayout.Button("Remove Tile"))
                    {
                        recastNavMesh.RemoveTile(recastNavMesh.NavMesh._tiles[i].Header.X, recastNavMesh.NavMesh._tiles[i].Header.Y);
                        recastNavMesh.BuildGeometry();
                    }
                    if (GUILayout.Button("Rebuild Tile"))
                    {
                        recastNavMesh.BuildTile(recastNavMesh.NavMesh._tiles[i].Header.X, recastNavMesh.NavMesh._tiles[i].Header.Y);
                        recastNavMesh.BuildGeometry();
                    }
                }
            }
        }
        EditorGUILayout.Separator();
        EditorGUILayout.PrefixLabel("Rebuild", GUIStyle.none, catagoryStyle);

        if (GUILayout.Button("Rebuild Geometry"))
        {
            recastNavMesh.RebuildTiles();
        }
        EditorGUILayout.Separator();
        EditorGUILayout.PrefixLabel("Visualize", GUIStyle.none, catagoryStyle);

        recastNavMesh.Mat = EditorGUILayout.ObjectField("Material", recastNavMesh.Mat, typeof(Material), true) as Material;
        if (GUILayout.Button("Toggle Geometry"))
        {
            recastNavMesh.Toggle();
        }

        //if (GUILayout.Button("Draw Voxel Geometry"))
        //{
        //    recastNavMesh.BuildVoxelGeometry();
        //    recastNavMesh.DrawVoxelGeometry();
        //}
        //if (GUILayout.Button("Create Walkable Voxel Geometry"))
        //{
        //    recastNavMesh.BuildWalkableVoxelGeometry();
        //    recastNavMesh.DrawWalkableVoxelGeometry();
        //}

        //if (GUILayout.Button("Create PolyMesh"))
        //{
        //    recastNavMesh.BuildPolyMeshGeometry();
        //    recastNavMesh.DrawPolyMeshGeometry();
        //}

	    EditorGUILayout.Separator();
        EditorGUILayout.PrefixLabel("Output", GUIStyle.none, catagoryStyle);
        if (GUILayout.Button("Export XML"))
        {
            var path = EditorUtility.SaveFilePanel("Export NavMesh", "", "NavMesh.xml", "xml");

            if (path.Length > 0)
            {
                FileStream f = null;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                f = File.Create(path);
                NavMeshSerializer serializer = new NavMeshSerializer(recastNavMesh.NavMesh);
                
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavMeshSerializer));
                xmlSerializer.Serialize(f, serializer);
                
                f.Close();
                
            }
        }
        if (GUILayout.Button("Export JSON"))
        {
            var path = EditorUtility.SaveFilePanel("Export NavMesh", "", "NavMesh.json", "json");

            if (path.Length > 0)
            {
                FileStream f = null;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                f = File.Create(path);
                NavMeshSerializer serializer = new NavMeshSerializer(recastNavMesh.NavMesh);

                using (StreamWriter sw = new StreamWriter(f))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = (Recast.Json.Formatting) Formatting.Indented;

                    JsonSerializer b = new JsonSerializer();
                    b.Serialize(jw, serializer);
                }
                f.Close();

            }
        }
        if (GUILayout.Button("Export Binary JSON"))
        {
            var path = EditorUtility.SaveFilePanel("Export NavMesh", "", "NavMesh.dat", "dat");

            if (path.Length > 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                NavMeshSerializer serializer = new NavMeshSerializer(recastNavMesh.NavMesh);

                using(FileStream f = File.Create(path))
                using (BsonWriter writer = new BsonWriter(f))
                {
                    JsonSerializer b = new JsonSerializer();
                    b.Serialize(writer, serializer);
                }
            }
        }

        if (GUILayout.Button("Export for WebPlayer"))
        {
            var path = EditorUtility.SaveFilePanel("Export NavMesh", "", "NavMesh.bytes", "bytes");

            if (path.Length > 0)
            {
                FileStream f = null;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                f = File.Create(path);
                NavMeshSerializer serializer = new NavMeshSerializer(recastNavMesh.NavMesh);

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavMeshSerializer));
                var xmlWriter = new XmlTextWriter(f, Encoding.UTF8);
                xmlSerializer.Serialize(xmlWriter, serializer);

                f.Close();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

	}
}
