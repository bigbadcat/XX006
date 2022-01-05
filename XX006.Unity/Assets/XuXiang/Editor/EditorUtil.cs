using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XuXiang;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// EditorUtil。
    /// </summary>
    public static class EditorUtil
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 检测文件夹，如果不存在则创建。
        /// </summary>
        /// <param name="path">文件路径。</param>
        public static void CheckOrCreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 加载文本文件内容。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>文本内容。</returns>
        public static string LoadTextFile(string path)
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }

            FileStream stream = File.OpenRead(path);
            StreamReader reader = new StreamReader(stream);
            string ret = reader.ReadToEnd();
            reader.Dispose();
            reader = null;
            return ret;
        }

        /// <summary>
        /// 加载文本文件内容。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>文本行内容。</returns>
        public static string[] LoadTextFileLines(string path)
        {
            string text = LoadTextFile(path);
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            return lines;
        }

        /// <summary>
        /// 保存文本文件。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="text">文本内容。</param>
        public static void SaveTextFile(string path, string text)
        {
            System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(false);
            File.WriteAllText(path, text, utf8);
        }

        /// <summary>
        /// 获取文件夹下的预设列表。
        /// </summary>
        /// <param name="prefabs"></param>
        /// <param name="folder"></param>
        public static void GetPrefabs(List<string> prefabs, string folder)
        {
            string path = Path.Combine(Application.dataPath, folder);
            string[] files = System.IO.Directory.GetFiles(path, "*.prefab", System.IO.SearchOption.AllDirectories);
            int index = path.IndexOf("Assets");
            for (int i = 0; i < files.Length; ++i)
            {
                string file = files[i].Substring(index).Replace("\\", "/");
                prefabs.Add(file);
            }
        }

        /// <summary>
        /// 以GUID为资产标识保存资产。
        /// </summary>
        /// <param name="key">保存的标识。</param>
        /// <param name="asset">资产对象。</param>
        public static void SaveAssetToPrefs(string key, UnityEngine.Object asset)
        {
            
            string guid = string.Empty;
            if (asset != null)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            }
            EditorPrefs.SetString(key, guid);
        }

        /// <summary>
        /// 加载prefs保存的资产。
        /// </summary>
        /// <typeparam name="T">资产类型。</typeparam>
        /// <param name="key">保存标识。</param>
        /// <returns>资产对象。</returns>
        public static T LoadAssetFromPrefs<T>(string key) where T : UnityEngine.Object
        {
            string guid = EditorPrefs.GetString(key, string.Empty);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T t = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
            return t;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 项目路径。
        /// </summary>
        public static string ProjectPath
        {
            get
            {
                return Directory.GetParent(Application.dataPath).FullName;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------
        #endregion

        #region 内部数据----------------------------------------------------------------
        #endregion
    }
}