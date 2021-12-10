using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using XuXiang;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 获取文件路径。
    /// </summary>
	public class GetFilePath
	{
        /// <summary>
        /// 资源存放根目录。
        /// </summary>
        public static string ResourceFolder = "ResourcesEx/";

        /// <summary>
        /// 复制资源路径到剪切板。
        /// </summary>
        [MenuItem("Assets/Copy Resource Path" , priority = 3)]
		public static void CopyResourcePath()
		{
            UnityEngine.Object obj = Selection.activeObject;
            if (obj != null)
            {
                string assetpath = AssetDatabase.GetAssetPath(obj);
                int sindex = assetpath.IndexOf(ResourceFolder);
                int eindex = assetpath.LastIndexOf('.');
                if (sindex >= 0 && eindex >= 0)
                {
                    int subindex = sindex + ResourceFolder.Length;
                    string respath = assetpath.Substring(subindex, eindex - subindex);
                    EditorGUIUtility.systemCopyBuffer = respath;

                    Debug.LogFormat("Resource path is {0}", respath);
                }
                else
                {
                    Debug.LogErrorFormat("Error resource path {0}", assetpath);
                }
            }
            else
            {
                Debug.LogError("This is not a resource file!");
            }
		}

        ///// <summary>
        ///// 复制Lua路径到剪切板。
        ///// </summary>
        //[MenuItem("Assets/Copy Lua Path", priority = 3)]
        //public static void CopyLuaPath()
        //{
        //    UnityEngine.Object obj = Selection.activeObject;
        //    if (obj != null)
        //    {
        //        string assetpath = AssetDatabase.GetAssetPath(obj);
        //        int sindex = assetpath.IndexOf(ResourceFolder);
        //        if (sindex >= 0)
        //        {
        //            int subindex = sindex + ResourceFolder.Length;
        //            string respath = assetpath.Substring(subindex);
        //            if (respath.StartsWith("Lua/") && respath.EndsWith(".lua"))
        //            {
        //                string luapath = respath.Substring(4, respath.Length - 8);      //去掉头和尾
        //                EditorGUIUtility.systemCopyBuffer = luapath;
        //                Debug.LogFormat("Lua path is {0}", luapath);
        //            }
        //            else
        //            {
        //                Debug.LogError("This is not a lua file!");
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogErrorFormat("Error resource path {0}", assetpath);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("This is not a resource file!");
        //    }
        //}
    }
}

