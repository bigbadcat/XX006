using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XuXiang
{
    /// <summary>
    /// 对日志模块进行封装。
    /// </summary>
    public class Log
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 输出信息。
        /// </summary>
        /// <param name="str">信息内容。</param>
        public static void Info(string str)
        {
            if (Enable)
            {
                Debug.Log(str);
            }
        }

        /// <summary>
        /// 输出信息。
        /// </summary>
        /// <param name="format">信息格式。</param>
        /// <param name="args">信息参数。</param>
        public static void Info(string format, params object[] args)
        {
            Info(string.Format(format, args));
        }

        /// <summary>
        /// 输出警告信息。
        /// </summary>
        /// <param name="str">信息内容。</param>
        public static void Warning(string str)
        {
            if (Enable)
            {
                Debug.LogWarning(str);
            }
        }

        /// <summary>
        /// 输出警告信息。
        /// </summary>
        /// <param name="format">信息格式。</param>
        /// <param name="args">信息参数。</param>
        public static void Warning(string format, params object[] args)
        {
            Warning(string.Format(format, args));
        }

        /// <summary>
        /// 输出错误信息。
        /// </summary>
        /// <param name="str">信息内容。</param>
        public static void Error(string str)
        {
            if (Enable)
            {
                Debug.LogError(str);
            }
        }

        /// <summary>
        /// 输出错误信息。
        /// </summary>
        /// <param name="format">信息格式。</param>
        /// <param name="args">信息参数。</param>
        public static void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        /// <summary>
        /// 保存堆栈。
        /// </summary>
        /// <param name="text">堆栈内容。</param>
        public static void SaveStackTrace(string text)
        {
#if UNITY_EDITOR
            //追加到文件
            string file = Application.dataPath + "/../StackTrace.log";
            FileStream stream = new FileStream(file, FileMode.OpenOrCreate);
            stream.Position = stream.Length;            //跳到末尾
            StreamWriter writer = new StreamWriter(stream);
            string t = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            writer.WriteLine(t);
            writer.WriteLine();
            writer.Write(text);
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine();
            writer.Flush();
            writer.Close();
            writer.Dispose();
            writer = null;
            stream = null;
#endif
        }

#if UNITY_EDITOR

        [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
        static bool OnOpenAsset(int instanceID, int line)
        {
            //判断选中的文本是否还有类名
            string stackTrace = GetSourceText();
            if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains(ClassName))
            {
                //匹配[文件路径:行号]部分 过滤出调用堆栈
                Match matches = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
                while (matches.Success)
                {
                    //找到不是自身文件输出的那行即为日志输出调用行
                    string pathline = matches.Groups[1].Value;
                    if (!pathline.Contains(FileName))
                    {
                        //分析出文件路径与行号并用编辑器打开并指定对应行
                        int splitIndex = pathline.LastIndexOf(":");
                        string path = pathline.Substring(0, splitIndex);        //文件相对路径
                        line = System.Convert.ToInt32(pathline.Substring(splitIndex + 1));      //行号
                        string root = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));    //根路径
                        string fullPath = (root + path).Replace('/', '\\');         //完整路径
                        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath, line);   //打开并标记
                        break;
                    }
                    matches = matches.NextMatch();
                }
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取控制台选中的文本。
        /// </summary>
        /// <returns>制台选中的文本。</returns>
        public static string GetSourceText()
        {
            //获取控制台窗口
            Type ConsoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            FieldInfo info = ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            object wnd = info.GetValue(null);
            if (wnd != null && (object)EditorWindow.focusedWindow == wnd)
            {
                //获取当前选中的文本
                info = ConsoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                string activeText = info.GetValue(wnd).ToString();

                return activeText;
            }

            return null;
        }
#endif

        #endregion

        #region 对外属性----------------------------------------------------------------

        public static string ClassName = "XuXiang.Log";

        /// <summary>
        /// 文件名称。
        /// </summary>
        public static string FileName = "Log.cs";

        /// <summary>
        /// 获取或设置是否启用日志。
        /// </summary>
        public static bool Enable = true;

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------
        #endregion
    }
}