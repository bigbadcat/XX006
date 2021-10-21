using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XuXiang
{
    /// <summary>
    /// 不进行任何绘制的图像，只用于接收事件。
    /// </summary>
	public class UIEventReceiver : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }

#if UNITY_EDITOR

    /// <summary>
    /// UIEventReceiver的编辑页面。
    /// </summary>
    [CustomEditor(typeof(UIEventReceiver))]
    [ExecuteInEditMode]
    public class UIEventReceiverEditor : Editor
    {
        /// <summary>
        /// 绘制面板。
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        }
    }

#endif
}
