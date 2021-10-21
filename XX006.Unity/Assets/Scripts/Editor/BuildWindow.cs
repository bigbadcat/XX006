using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XuXiang;

namespace XX006.EditorTools
{
    public class BuildWindow : EditorWindow
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 显示打包窗口。
        /// </summary>
        [MenuItem("Tools/Builder...")]
        public static void ShowBuildWindow()
        {
            EditorWindow.GetWindow(typeof(BuildWindow), true, "Builder");
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        BuildWindow()
        {
            this.minSize = new Vector2(300, 200);
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 界面绘制。
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