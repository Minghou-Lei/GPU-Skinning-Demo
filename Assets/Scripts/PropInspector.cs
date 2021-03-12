using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Baker))]
public class PropInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var baker = target as Baker;

        EditorGUILayout.BeginHorizontal();
        baker.Bakemode = (Baker.BAKEMODE) EditorGUILayout.EnumPopup("渲染模式：", baker.Bakemode);
        EditorGUILayout.EndHorizontal();


        if (baker.Bakemode == Baker.BAKEMODE.bone)
        {
            EditorGUILayout.BeginHorizontal();
            baker.outI = (Baker.UVCHANNEL) EditorGUILayout.EnumPopup("索引输出通道：", baker.outI);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            baker.outW = (Baker.UVCHANNEL) EditorGUILayout.EnumPopup("权重输出通道：", baker.outW);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            baker.mesh = (Mesh) EditorGUILayout.ObjectField("Mesh：", baker.mesh, typeof(Mesh), true);
            EditorGUILayout.EndHorizontal();
        }

        if (baker.Bakemode == Baker.BAKEMODE.vertex)
        {
            EditorGUILayout.BeginHorizontal();
            baker.computeShader = (ComputeShader) EditorGUILayout.ObjectField("Compute Shader:", baker.computeShader,
                typeof(ComputeShader), true);
            EditorGUILayout.EndHorizontal();
        }
    }
}