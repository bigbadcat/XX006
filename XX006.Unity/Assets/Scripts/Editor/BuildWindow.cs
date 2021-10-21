using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XuXiang;

namespace XX006.EditorTools
{
    public class BuildWindow : EditorWindow
    {
        #region �������----------------------------------------------------------------

        /// <summary>
        /// ��ʾ������ڡ�
        /// </summary>
        [MenuItem("Tools/Builder...")]
        public static void ShowBuildWindow()
        {
            EditorWindow.GetWindow(typeof(BuildWindow), true, "Builder");
        }

        /// <summary>
        /// ���캯����
        /// </summary>
        BuildWindow()
        {
            this.minSize = new Vector2(300, 200);
        }

        #endregion

        #region �ڲ�����----------------------------------------------------------------

        /// <summary>
        /// ������ơ�
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Build AssetBundle", GUILayout.MinHeight(40)))
            {
                Log.Info("Build AssetBundle");
            }

            if (GUILayout.Button("Copy AssetBundle", GUILayout.MinHeight(40)))
            {
                Log.Info("Copy AssetBundle");
            }

            if (GUILayout.Button("Release Package", GUILayout.MinHeight(40)))
            {
                Log.Info("Release Package");
            }

            GUILayout.EndVertical();
        }

        #endregion
    }
}