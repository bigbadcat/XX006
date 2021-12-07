using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            //FileStream stream = File.Open(path, FileMode.OpenOrCreate);
            //StreamWriter writer = new StreamWriter(stream);
            //writer.Write(text);
            //writer.Dispose();
            //writer = null;

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