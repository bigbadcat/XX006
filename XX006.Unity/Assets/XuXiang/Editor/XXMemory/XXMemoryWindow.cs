using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using RawMemorySnapshot = UnityEditor.Profiling.Memory.Experimental.PackedMemorySnapshot;

namespace XX
{
    public class MemoryWindow : EditorWindow
    {
        /// <summary>
        /// 快照选取项。
        /// </summary>
        public class MemorySnapshotItem
        {
            public MemorySnapshotItem(string name)
            {
                Name = name;
            }

            public void OnDrawGUI()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(Name, GUILayout.MaxWidth(40));
                Path = EditorGUILayout.TextField(Path);
                if (GUILayout.Button("Load", GUILayout.MaxWidth(50)) && !IsTaking)
                {
                    var filepath = EditorUtility.OpenFilePanelWithFilters("Load Snapshot", string.Empty, new[] { "Snapshots", "memsnap" });
                    if (!string.IsNullOrEmpty(filepath))
                    {
                        Path = filepath;
                    }
                }
                if (GUILayout.Button("Take", GUILayout.MaxWidth(50)) && !IsTaking)
                {
                    var filepath = EditorUtility.SaveFilePanel("Save Snapshot", string.Empty, "MP_" + Name, "memsnap");
                    if (!string.IsNullOrEmpty(filepath))
                    {
                        Path = filepath;
                        IsTaking = true;
                        EditorUtility.DisplayProgressBar("MemoryProfiler", "Taking snap...", 0);
                        MemoryProfiler.TakeSnapshot(Path, (str, b) =>
                        {
                            IsTaking = false;
                            Debug.LogFormat("SaveMemSnap:{0} - {1}", Path, b);
                            EditorUtility.ClearProgressBar();
                        });
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            /// <summary>
            /// 快照名称。
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// 快照文件路径。
            /// </summary>
            public string Path { get; private set; }

            /// <summary>
            /// 是否在获取快照中。
            /// </summary>
            public bool IsTaking { get; private set; }
        }

        [MenuItem("Tools/MemoryProfiler...")]
        public static void ShowAtlasPacker()
        {
            EditorWindow.GetWindow(typeof(MemoryWindow), true, "Memory Profiler");
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public MemoryWindow()
        {
            this.minSize = new Vector2(400, 300);
            m_SnapItem1 = new MemorySnapshotItem("Snap1");
            m_SnapItem2 = new MemorySnapshotItem("Snap2");
        }

        /// <summary>
        /// 界面绘制。
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            m_SnapItem1.OnDrawGUI();
            m_SnapItem2.OnDrawGUI();
            EditorGUILayout.EndVertical();

            string title = string.IsNullOrEmpty(m_SnapItem2.Path) ? "Analyze" : "Compare";
            if (GUILayout.Button(title, GUILayout.MaxWidth(80), GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40)))
            {
                EditorApplication.update += DoProfiler;
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Box(string.Empty, GUILayout.Height(4), GUILayout.ExpandWidth(true));
            OnDrawSplit();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制窗口分割。
        /// </summary>
        private void OnDrawSplit()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(0));
            OnDrawLeft();
            EditorGUILayout.EndVertical();

            //分割线
            GUILayout.Box(string.Empty, GUILayout.Width(m_SplitterWidth), GUILayout.ExpandHeight(true));
            Rect splitterRect = GUILayoutUtility.GetLastRect();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(Math.Min(m_RightWidth, 800)));
            OnDrawRight();
            EditorGUILayout.EndVertical();            
            EditorGUILayout.EndHorizontal();

            //拖拽调整左右窗口大小
            if (Event.current != null)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                        if (splitterRect.Contains(Event.current.mousePosition))
                        {
                            //Debug.Log("Start dragging");
                            m_SplitterDragging = true;
                        }
                        break;
                    case EventType.MouseDrag:
                        if (m_SplitterDragging)
                        {
                            //调整的是右侧窗口，所以用减法
                            m_RightWidth -= Event.current.delta.x;
                            Repaint();
                        }
                        break;
                    case EventType.MouseUp:
                        if (m_SplitterDragging)
                        {
                            //Debug.Log("Done dragging");
                            m_SplitterDragging = false;
                            m_RightWidth = Math.Min(m_RightWidth, 800);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 绘制左侧内容。
        /// </summary>
        private void OnDrawLeft()
        {
            if (m_Sanp1 == null || m_Sanp1 == null)
            {
                GUILayout.Label("No compare result");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("ShowType:", GUILayout.MaxWidth(70));
            int old_index = m_ShowTypeIndex;
            m_ShowTypeIndex = EditorGUILayout.Popup(m_ShowTypeIndex, CompreResultTypeString, GUILayout.MaxWidth(70));
            if (old_index != m_ShowTypeIndex)
            {
                m_ScrollViewPos = Vector2.zero;
                m_CurPageIndex = 0;
                m_ShowObjectsType = null;
                m_ShowObjects.Clear();
            }

            //分页切换
            m_CurPageIndex = Mathf.Max(0, Mathf.Min(m_CurPageIndex, m_PageNumber-1));
            if (GUILayout.Button(string.Empty, "ArrowNavigationLeft") && m_CurPageIndex > 0)
            {
                m_CurPageIndex--;
            }
            GUILayout.Label(string.Format("{0}/{1}", (m_CurPageIndex + 1), Mathf.Max(1, m_PageNumber)));
            if (GUILayout.Button(string.Empty, "ArrowNavigationRight") && m_CurPageIndex < m_PageNumber-1)
            {
                m_CurPageIndex++;
            }

            GUILayout.FlexibleSpace();
            if (m_ShowObjectsType == null)
            {
                bool old_set = m_FullAssembly;
                m_FullAssembly = GUILayout.Toggle(m_FullAssembly, "FullAssembly");
                if (old_set != m_FullAssembly)
                {
                    EditorUtility.DisplayProgressBar("Compare", string.Empty, 0);
                    m_Result.Init(m_Sanp1, m_Snap2, m_FullAssembly ? null : Assemblys);
                    EditorUtility.ClearProgressBar();
                }
            }
            else
            {
                GUILayout.Label(m_ShowObjectsType.Assembly + ":" + m_ShowObjectsType.TypeDescriptionName);
                if (GUILayout.Button("Back"))
                {
                    m_ShowObjectsType = null;
                    m_ShowObjects.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            m_ScrollViewPos = EditorGUILayout.BeginScrollView(m_ScrollViewPos, "AppToolbar");
            if (m_ShowObjectsType == null)
            {
                switch ((CompreResultType)m_ShowTypeIndex)
                {
                    case CompreResultType.Add:
                        OnDrawObjectList(m_Result.AddObjects);
                        break;
                    case CompreResultType.Remove:
                        OnDrawObjectList(m_Result.RemoveObjects);
                        break;
                    case CompreResultType.Diff:
                        OnDrawDiffList();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                OnDrawObjectList(m_ShowObjects);
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制对象列表。
        /// </summary>
        private void OnDrawObjectList(List<XXManagedObject> objects)
        {
            int start = m_CurPageIndex * OBJECT_PAGE_SHOW_NUMBER;
            m_PageNumber = (int)Mathf.Ceil(objects.Count * 1.0f / OBJECT_PAGE_SHOW_NUMBER);
            for (int i = 0; i < OBJECT_PAGE_SHOW_NUMBER && (start + i < objects.Count); ++i)
            {
                int index = start + i;
                XXManagedObject obj = objects[index];
                if (i % 2 == 0)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                }
                else
                {
                    EditorGUILayout.BeginHorizontal("IN BigTitle Inner", GUILayout.ExpandWidth(true));
                }
                
                GUILayout.Label(string.Format("0x{0:X}", obj.Address), GUILayout.MaxWidth(100));                
                if (string.IsNullOrEmpty(obj.ShortValueText))
                {
                    GUILayout.Label(obj.TypeDescription.TypeDescriptionName);                
                }
                else
                {
                    string txt = obj.TypeDescription.TypeDescriptionName + "   " + obj.ShortValueText;
                    GUILayout.Label(txt);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show", GUILayout.MaxWidth(50)))
                {
                    ShowObject(obj);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 显示对象。
        /// </summary>
        /// <param name="obj">对象信息。</param>
        private void ShowObject(XXManagedObject obj)
        {
            if (m_CurShowObject != null && m_PreObjects.Count < 100)
            {
                m_PreObjects.Add(m_CurShowObject);
            }
            m_NextObjects.Clear();
            m_CurShowObject = obj;
            m_ObjectScrollViewPos = Vector2.zero;
        }

        /// <summary>
        /// 绘制差异。
        /// </summary>
        private void OnDrawDiffList()
        {
            var diffs = m_Result.DiffInfos;
            int start = m_CurPageIndex * OBJECT_PAGE_SHOW_NUMBER;
            m_PageNumber = (int)Mathf.Ceil(diffs.Count * 1.0f / OBJECT_PAGE_SHOW_NUMBER);
            for (int i = 0; i < OBJECT_PAGE_SHOW_NUMBER && (start + i < diffs.Count); ++i)
            {
                int index = start + i;
                var diff = diffs[index];
                XXTypeDescription type_des = m_Snap2.TypeDescriptions.GetTypeByAddress(diff.TypeAddress);
                if (i % 2 == 0)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                }
                else
                {
                    EditorGUILayout.BeginHorizontal("IN BigTitle Inner", GUILayout.ExpandWidth(true));
                }
                GUILayout.Label(type_des.TypeDescriptionName);
                GUILayout.FlexibleSpace();
                GUILayout.Label(diff.Count.ToString(), GUILayout.Width(50));                
                if (diff.Count > 0 && GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    m_ShowObjectsType = type_des;
                    m_ShowObjects.Clear();
                    foreach (var obj in m_Result.AddObjects)
                    {
                        if (obj.TypeDescription == m_ShowObjectsType)
                        {
                            m_ShowObjects.Add(obj);
                        }
                    }
                }                
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制右侧内容。
        /// </summary>
        private void OnDrawRight()
        {
            if (m_CurShowObject == null)
            {
                GUILayout.Label("No object");
                return;
            }

            if (m_PreObjects.Count + m_NextObjects.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(m_PreObjects.Count <= 0);
                if (GUILayout.Button("Pre", GUILayout.MaxWidth(100)))
                {
                    m_NextObjects.Add(m_CurShowObject);
                    m_CurShowObject = m_PreObjects[m_PreObjects.Count - 1];
                    m_PreObjects.RemoveAt(m_PreObjects.Count - 1);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(m_NextObjects.Count <= 0);
                if (GUILayout.Button("Next", GUILayout.MaxWidth(100)))
                {
                    m_PreObjects.Add(m_CurShowObject);
                    m_CurShowObject = m_NextObjects[m_NextObjects.Count - 1];
                    m_NextObjects.RemoveAt(m_NextObjects.Count - 1);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            XXTypeDescription type_des = m_CurShowObject.TypeDescription;
            GUILayout.Label(type_des.TypeDescriptionName);            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Address:0x{0:X}", m_CurShowObject.Address), GUILayout.MaxWidth(150));
            GUILayout.Label(string.Format("Size:{0}", m_CurShowObject.Size));                      
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(m_CurShowObject.ShortValueText))
            {
                m_TextPos = EditorGUILayout.BeginScrollView(m_TextPos, GUILayout.MaxHeight(200));
                GUILayout.TextArea(m_CurShowObject.ValueText, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            //引用关系
            m_ObjectScrollViewPos = EditorGUILayout.BeginScrollView(m_ObjectScrollViewPos);
            DrawObjectReference(m_CurShowObject);            
            GUILayout.Space(4);
            DrawObjectReferenceBy(m_CurShowObject);
            EditorGUILayout.EndScrollView();
        }

        private Vector2 m_TextPos = Vector2.zero;
        
        /// <summary>
        /// 绘制对象引用的信息。
        /// </summary>
        /// <param name="obj">对象信息。</param>
        private void DrawObjectReference(XXManagedObject obj)
        {
            //成员引用除了数组外全部显示
            int n = obj.ReferenceTo.Count;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Reference:{0}", n));
            EditorGUILayout.EndHorizontal();

            int start = 0 * FIELD_PAGE_SHOW_NUMBER;
            for (int i = 0; i < FIELD_PAGE_SHOW_NUMBER && (start + i < n); ++i)
            {
                int index = start + i;
                var ref_to = obj.ReferenceTo[index];
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                string title = string.Format("{0} {1}", ref_to.Name, ref_to.Address > 0 ? ref_to.Address.ToString("X") : "null");
                if (GUILayout.Button(title) && ref_to.Address != 0)
                {
                    ShowObject(obj.BelongSnap.ManagedObjects.GetObject(ref_to.Address));
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制对象被引用的信息。
        /// </summary>
        /// <param name="obj">对象信息。</param>
        private void DrawObjectReferenceBy(XXManagedObject obj)
        {
            //被引用分页显示
            int n = obj.ReferenceFrom.Count;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Reference by:{0}", n));
            EditorGUILayout.EndHorizontal();

            int start = 0 * FIELD_PAGE_SHOW_NUMBER;
            for (int i = 0; i < FIELD_PAGE_SHOW_NUMBER && (start + i < n); ++i)
            {
                int index = start + i;
                var ref_from = obj.ReferenceFrom[index];
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                string title = string.Empty;
                switch (ref_from.Type)
                {
                    case XXObjectReferenceFrom.FromType.GCHandle:
                        title = string.Format("GCHandle Index:{0}", ref_from.GCHandleIndex);
                        break;
                    case XXObjectReferenceFrom.FromType.Field:
                        if (ref_from.ObjectAddress != 0)
                        {
                            title = string.Format("Object 0x{0:X} {1}", ref_from.ObjectAddress, ref_from.FieldPath);
                        }
                        else
                        {
                            title = string.Format("Static {0}", ref_from.FieldPath);
                        }
                        break;
                    default:
                        break;
                }
                if (GUILayout.Button(title) && ref_from.ObjectAddress != 0)
                {
                    ShowObject(obj.BelongSnap.ManagedObjects.GetObject(ref_from.ObjectAddress));
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 开始分析工作。
        /// </summary>
        private void DoProfiler()
        {
            EditorApplication.update -= DoProfiler;
            ClearData();

            EditorUtility.DisplayProgressBar("LoadSnap", m_SnapItem1.Path, 0);
            m_Sanp1 = XXMemorySnapshot.Load(m_SnapItem1.Path);
            if (m_Sanp1 == null)
            {
                Debug.LogErrorFormat("Load snap1 failed. path:{0}", m_SnapItem1.Path);
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayProgressBar("LoadSnap", m_SnapItem2.Path, 0.4f);
            m_Snap2 = XXMemorySnapshot.Load(m_SnapItem2.Path);
            if (m_Snap2 == null)
            {
                m_Sanp1 = null;
                Debug.LogErrorFormat("Load snap2 failed. path:{0}", m_SnapItem2.Path);
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayProgressBar("Compare", string.Empty, 0.8f);
            m_Result.Init(m_Sanp1, m_Snap2, m_FullAssembly ? null : Assemblys);
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 清除数据。
        /// </summary>
        private void ClearData()
        {
            m_CurShowObject = null;
            m_PreObjects.Clear();
            m_NextObjects.Clear();
            m_Result.Clear();
            m_ShowObjects.Clear();
            m_ShowObjectsType = null;
            m_Sanp1 = null;
            m_Snap2 = null;

            m_CurPageIndex = 0;
            m_ShowTypeIndex = 0;
            m_ScrollViewPos = Vector2.zero;
        }

        /// <summary>
        /// 比较类型显示数组。
        /// </summary>
        private static string[] CompreResultTypeString
        {
            get
            {
                if (s_CompreResultTypeString == null)
                {
                    s_CompreResultTypeString = new string[3];
                    for (int i=0; i< s_CompreResultTypeString.Length; ++i)
                    {
                        s_CompreResultTypeString[i] = ((CompreResultType)i).ToString();
                    }
                }
                return s_CompreResultTypeString;
            }
        }
        private static string[] s_CompreResultTypeString = null;

        /// <summary>
        /// 获取需要过滤的程序集名称列表。
        /// </summary>
        private static List<string> Assemblys
        {
            get
            {
                if (s_Assemblys == null)
                {
                    s_Assemblys = new List<string>();
                    s_Assemblys.Add("Assembly-CSharp");
                    s_Assemblys.Add("spine-unity");
                }
                return s_Assemblys;
            }
        }
        private static List<string> s_Assemblys = null;

        /// <summary>
        /// 比较结果类型。
        /// </summary>
        private enum CompreResultType
        {
            /// <summary>
            /// 添加。
            /// </summary>
            Add = 0,

            /// <summary>
            /// 移除。
            /// </summary>
            Remove = 1,

            /// <summary>
            /// 差异。
            /// </summary>
            Diff = 2,
        }

        /// <summary>
        /// 一页显示的对象数量。
        /// </summary>
        public const int OBJECT_PAGE_SHOW_NUMBER = 100;

        /// <summary>
        /// 一页显示的字段数量。
        /// </summary>
        public const int FIELD_PAGE_SHOW_NUMBER = 100;

        #region 界面控制 --------------------

        private MemorySnapshotItem m_SnapItem1 = null;
        private MemorySnapshotItem m_SnapItem2 = null;

        private float m_RightWidth = 300;
        private bool m_SplitterDragging = false;
        private float m_SplitterWidth = 4;

        /// <summary>
        /// 滚动位置。
        /// </summary>
        private Vector2 m_ScrollViewPos = Vector2.zero;

        /// <summary>
        /// 对象滚动位置。
        /// </summary>
        private Vector2 m_ObjectScrollViewPos = Vector2.zero;

        /// <summary>
        /// 显示类型。
        /// </summary>
        private int m_ShowTypeIndex = 0;

        /// <summary>
        /// 内容页数。
        /// </summary>
        private int m_PageNumber = 0;

        /// <summary>
        /// 当前页面索引。
        /// </summary>
        private int m_CurPageIndex = 0;

        /// <summary>
        /// 是否完整程序集。
        /// </summary>
        private bool m_FullAssembly = false;

        #endregion

        #region 快照数据 --------------------

        /// <summary>
        /// 快照1。
        /// </summary>
        private XXMemorySnapshot m_Sanp1 = null;

        /// <summary>
        /// 快照2.
        /// </summary>
        private XXMemorySnapshot m_Snap2 = null;

        /// <summary>
        /// 对比结果。
        /// </summary>
        private XXMemorySnapshotCompreResult m_Result = new XXMemorySnapshotCompreResult();

        /// <summary>
        /// 当前查看的对象。
        /// </summary>
        private XXManagedObject m_CurShowObject = null;

        /// <summary>
        /// 上次查看的对象列表。
        /// </summary>
        private List<XXManagedObject> m_PreObjects = new List<XXManagedObject>();

        /// <summary>
        /// 后续查看的对象列表。
        /// </summary>
        private List<XXManagedObject> m_NextObjects = new List<XXManagedObject>();

        /// <summary>
        /// 当前查看的对象列表。
        /// </summary>
        private List<XXManagedObject> m_ShowObjects = new List<XXManagedObject>();

        /// <summary>
        /// 当前查看的对象列表类型。
        /// </summary>
        private XXTypeDescription m_ShowObjectsType = null;

        #endregion
    }
}