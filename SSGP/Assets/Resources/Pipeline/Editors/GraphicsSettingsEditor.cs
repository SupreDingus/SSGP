using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditorForRenderPipeline(typeof(GraphicsSettings), typeof(LightingPipelineAsset))]
public class GraphicsSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //EditorGUILayout.LabelField("does this work yet");
    }
}
