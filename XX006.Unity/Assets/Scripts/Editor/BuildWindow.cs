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
                //Log.Info("Build AssetBundle");
                EditorApplication.update += DoBuildAssetBundle;
            }

            if (GUILayout.Button("Copy AssetBundle", GUILayout.MinHeight(40)))
            {
                //Log.Info("Copy AssetBundle");
                EditorApplication.update += DoCopyAssetBundle;
            }

            if (GUILayout.Button("Release Package", GUILayout.MinHeight(40)))
            {
                Log.Info("Release Package");
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 进行打包资源操作。
        /// </summary>
        private void DoBuildAssetBundle()
        {
            EditorApplication.update -= DoBuildAssetBundle;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                case BuildTarget.iOS:
                    break;
                default:
                    Log.Error("The active build target is not support. cur:{0}", EditorUserBuildSettings.activeBuildTarget);
                    return;
            }
            BuildHelper.BuildAssetBundle();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 进行拷贝AB包操作。
        /// </summary>
        private void DoCopyAssetBundle()
        {
            EditorApplication.update -= DoCopyAssetBundle;
            BuildTools.CopyAssetBundleDepend(BuildHelper.BundleFolder);
            BuildHelper.CopyAssetBundle();
            AssetDatabase.Refresh();
        }

        #endregion
    }
}