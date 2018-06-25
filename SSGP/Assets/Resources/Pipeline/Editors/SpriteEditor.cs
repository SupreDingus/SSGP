using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

//[CustomEditorForRenderPipeline(typeof(SpriteRenderer), typeof(LightingPipelineAsset))]
//public class SpriteEditor : Editor
//{
//    SerializedProperty[] defaultProperties;
//    string[] propertyNames = {
//        "m_Sprite",
//        "m_Color",
//        "m_FlipX",
//        "m_FlipY",
//        "m_SharedMaterials",
//        "m_DrawMode",
//        "m_SortingLayerID",
//        "m_SortingOrder",
//    };
//    string[] displayPropertyNames = {
//        "Sprite",
//        "Color",
//        "Flip X",
//        "Flip Y",
//        "Material",
//        "Draw Mode",
//        "Sorting Layer",
//        "Order in Layer",
//    };
//
//    private void OnEnable()
//    {
//        defaultProperties = new SerializedProperty[8];
//
//        //serializedObject.
//
//        for (int i = 0; i < 8; i++)
//        {
//            defaultProperties[i] = serializedObject.FindProperty(propertyNames[i]);
//        }
//    }
//
//    public override void OnInspectorGUI()
//    {
//        SpriteRenderer r = target as SpriteRenderer;
//
//        //r.sharedMaterials
//        
//        EditorGUILayout.PropertyField(defaultProperties[0], new GUIContent(displayPropertyNames[0]));
//        EditorGUILayout.PropertyField(defaultProperties[1], new GUIContent(displayPropertyNames[1]));
//
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.PropertyField(defaultProperties[2], new GUIContent(displayPropertyNames[2]));
//        EditorGUILayout.PropertyField(defaultProperties[3], new GUIContent(displayPropertyNames[3]));
//        EditorGUILayout.EndHorizontal();
//
//        EditorGUILayout.PropertyField(defaultProperties[4], new GUIContent(displayPropertyNames[4]));
//        EditorGUILayout.PropertyField(defaultProperties[5], new GUIContent(displayPropertyNames[5]));
//        EditorGUILayout.PropertyField(defaultProperties[6], new GUIContent(displayPropertyNames[6]));
//        EditorGUILayout.PropertyField(defaultProperties[7], new GUIContent(displayPropertyNames[7]));
//
//        serializedObject.ApplyModifiedProperties();
//        
//
//        //base.OnInspectorGUI();
//    }
//}
