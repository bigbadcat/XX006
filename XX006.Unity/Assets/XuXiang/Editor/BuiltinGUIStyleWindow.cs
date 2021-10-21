using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XuXiang;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 内置的GUIStyleWindow窗口。
    /// </summary>
    public class BuiltinGUIStyleWindow : EditorWindow
    {
        #region 对外操作----------------------------------------------------------------

        [MenuItem("Help/Builtin GUIStyle")]
        public static void ShowBuiltinGUIStyleWindow()
        {
            EditorWindow.GetWindow(typeof(BuiltinGUIStyleWindow), true, "Builtin GUIStyle Window");
        }

        BuiltinGUIStyleWindow()
        {
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        #endregion

        #region 内部操作----------------------------------------------------------------

        private void OnGUI()
        {
            GUILayout.BeginHorizontal("HelpBox");
            GUILayout.Space(30);
            search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(position.x / 3));
            GUILayout.Label("", "SearchCancelButtonEmpty");
            GUILayout.EndHorizontal();
            scrollVector2 = GUILayout.BeginScrollView(scrollVector2);
            foreach (GUIStyle style in GUI.skin.customStyles)
            {
                if (style.name.ToLower().Contains(search.ToLower()))
                {
                    DrawStyleItem(style);
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawStyleItem(GUIStyle style)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(40);
            EditorGUILayout.SelectableLabel(style.name);
            GUILayout.FlexibleSpace();
            EditorGUILayout.SelectableLabel(style.name, style);
            GUILayout.Space(40);
            EditorGUILayout.SelectableLabel("", style, GUILayout.Height(40), GUILayout.Width(40));
            GUILayout.Space(50);
            if (GUILayout.Button("复制到剪贴板"))
            {
                TextEditor textEditor = new TextEditor();
                textEditor.text = style.name;
                textEditor.OnFocus();
                textEditor.Copy();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        private Vector2 scrollVector2 = Vector2.zero;

        private string search = "";

        #endregion
    }
}