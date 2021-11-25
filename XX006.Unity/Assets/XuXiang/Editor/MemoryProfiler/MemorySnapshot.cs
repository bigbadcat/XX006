using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using RawMemorySnapshot = UnityEditor.Profiling.Memory.Experimental.PackedMemorySnapshot;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 内存快照对比结果。
    /// </summary>
    public class XXMemorySnapshotCompreResult
    {
        /// <summary>
        /// 差异信息。
        /// </summary>
        public class DiffInfo
        {
            /// <summary>
            /// 构造函数。
            /// </summary>
            /// <param name="type_address">类型地址。</param>
            /// <param name="count">差异计数。</param>
            public DiffInfo(ulong type_address, int count)
            {
                TypeAddress = type_address;
                Count = count;
            }

            /// <summary>
            /// 排序比较函数。
            /// </summary>
            public static int Compare(DiffInfo a, DiffInfo b)
            {
                return b.Count - a.Count;
            }

            /// <summary>
            /// 类型地址。
            /// </summary>
            public ulong TypeAddress { get; private set; }

            /// <summary>
            /// 差异数量。
            /// </summary>
            public int Count { get; private set; }
        }

        /// <summary>
        /// 初始化对比快照。
        /// </summary>
        /// <param name="before">前一份内存快照。</param>
        /// <param name="after">后一份内存快照。</param>
        /// <param name="assemblys">要过滤的整程序集，null表示不过滤。</param>
        public void Init(XXMemorySnapshot before, XXMemorySnapshot after, List<string> assemblys = null)
        {
            Clear();
            foreach (var kvp in after.ManagedObjects.ManagedObjects)
            {
                //程序集过滤
                if (assemblys != null && !assemblys.Contains(kvp.Value.TypeDescription.Assembly))
                {
                    continue;
                }

                XXManagedObject obj = before.ManagedObjects.GetObject(kvp.Key);
                if (obj == null)
                {
                    m_AddObjects.Add(kvp.Value);
                    //AddObjectDiffCount(kvp.Value.TypeDescription.TypeInfoAddress, 1);
                }
            }
            foreach (var kvp in before.ManagedObjects.ManagedObjects)
            {
                //程序集过滤
                if (assemblys != null && !assemblys.Contains(kvp.Value.TypeDescription.Assembly))
                {
                    continue;
                }

                XXManagedObject obj = after.ManagedObjects.GetObject(kvp.Key);
                if (obj == null)
                {
                    m_RemoveObjects.Add(kvp.Value);
                    //AddObjectDiffCount(kvp.Value.TypeDescription.TypeInfoAddress, -1);
                }
            }

            List<int> del_add = new List<int>();
            List<int> del_reomve = new List<int>();
            for (int i=0; i<m_AddObjects.Count; ++i)
            {
                XXManagedObject obj = m_AddObjects[i];
                for (int j=0; j<m_RemoveObjects.Count; ++j)
                {
                    if (obj.IsSameValue(m_RemoveObjects[j]))
                    {
                        del_add.Add(i);
                        del_reomve.Add(j);
                        break;
                    }
                }
            }
            for (int i = del_add.Count-1; i>=0; --i)
            {
                m_AddObjects.RemoveAt(del_add[i]);
            }
            for (int i = del_reomve.Count - 1; i >= 0; --i)
            {
                m_RemoveObjects.RemoveAt(del_reomve[i]);
            }

            m_AddObjects.Sort(CompareObject);
            m_RemoveObjects.Sort(CompareObject);
            foreach (var obj in m_AddObjects)
            {
                AddObjectDiffCount(obj.TypeDescription.TypeInfoAddress, 1);
            }
            foreach (var obj in m_RemoveObjects)
            {
                AddObjectDiffCount(obj.TypeDescription.TypeInfoAddress, -1);
            }
            RemoveZeroDiff();
        }

        /// <summary>
        /// 对象比较。
        /// </summary>
        /// <param name="a">对象A。</param>
        /// <param name="b">对象B。</param>
        /// <returns>优先按类型名，其次按地址。</returns>
        public static int CompareObject(XXManagedObject a, XXManagedObject b)
        {
            if (a.TypeDescription.TypeInfoAddress != b.TypeDescription.TypeInfoAddress)
            {
                return a.TypeDescription.TypeDescriptionName.CompareTo(b.TypeDescription.TypeDescriptionName);
            }

            //无符号数不能直接用减法比较
            return a.Address < b.Address ? -1 : (a.Address == b.Address ? 0 : 1);
        }

        /// <summary>
        /// 清除比较结果。
        /// </summary>
        public void Clear()
        {
            m_AddObjects.Clear();
            m_RemoveObjects.Clear();
            m_ObjectDiff.Clear();
        }

        /// <summary>
        /// 添加对象差异统计。
        /// </summary>
        /// <param name="type_address">类型地址。</param>
        /// <param name="count">数量。</param>
        private void AddObjectDiffCount(ulong type_address, int count)
        {
            int n;
            if (!m_ObjectDiff.TryGetValue(type_address, out n))
            {
                n = 0;
                m_ObjectDiff.Add(type_address, n);
            }
            n += count;
            m_ObjectDiff[type_address] = n;
        }

        /// <summary>
        /// 异常0差异统计。
        /// </summary>
        private void RemoveZeroDiff()
        {
            m_DiffInfos.Clear();
            List<ulong> no_diff = new List<ulong>(m_ObjectDiff.Count);
            foreach (var kvp in m_ObjectDiff)
            {
                if (kvp.Value == 0)
                {
                    no_diff.Add(kvp.Key);
                }
                else
                {
                    m_DiffInfos.Add(new DiffInfo(kvp.Key, kvp.Value));
                }
            }
            foreach (var k in no_diff)
            {
                m_ObjectDiff.Remove(k);
            }
            m_DiffInfos.Sort(DiffInfo.Compare);
        }

        /// <summary>
        /// 获取添加的对象(归属after快照)。
        /// </summary>
        public List<XXManagedObject> AddObjects
        {
            get { return m_AddObjects; }
        }

        /// <summary>
        /// 获取移除的对象(归属before快照)。
        /// </summary>
        public List<XXManagedObject> RemoveObjects
        {
            get { return m_RemoveObjects; }
        }

        /// <summary>
        /// 获取对象差异。
        /// </summary>
        public Dictionary<ulong, int> ObjectDiff
        {
            get { return m_ObjectDiff; }
        }

        /// <summary>
        /// 差异信息列表。
        /// </summary>
        public List<DiffInfo> DiffInfos
        {
            get { return m_DiffInfos; }
        }

        /// <summary>
        /// 添加的对象(归属after快照)。
        /// </summary>
        private List<XXManagedObject> m_AddObjects = new List<XXManagedObject>();

        /// <summary>
        /// 移除的对象(归属before快照)。
        /// </summary>
        private List<XXManagedObject> m_RemoveObjects = new List<XXManagedObject>();

        /// <summary>
        /// 对象差异。
        /// </summary>
        private Dictionary<ulong, int> m_ObjectDiff = new Dictionary<ulong, int>();

        /// <summary>
        /// 差异信息。
        /// </summary>
        private List<DiffInfo> m_DiffInfos = new List<DiffInfo>();
    }

    /// <summary>
    /// 内存快照数据。
    /// </summary>
    public class XXMemorySnapshot
    {
        /// <summary>
        /// 保存内存快照。
        /// </summary>
        /// <param name="raw">Unity接口返回的最原始内存快照数据。</param>
        /// <param name="path">保存路径。</param>
        public static void Save(RawMemorySnapshot raw, string path)
        {
            RawMemorySnapshot.Save(raw, path);
        }

        /// <summary>
        /// 加载内存快照。
        /// </summary>
        /// <param name="path">快照保存路径。</param>
        /// <returns></returns>
        public static XXMemorySnapshot Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }

            RawMemorySnapshot raw = RawMemorySnapshot.Load(path);
            XXMemorySnapshot snap = Create(raw);
            raw.Dispose();
            raw = null;
            return snap;
        }

        /// <summary>
        /// 创建内存快照。
        /// </summary>
        /// <param name="raw">Unity接口返回的最原始内存快照数据。</param>
        /// <returns>分析好的内存快照数据。</returns>
        public static XXMemorySnapshot Create(RawMemorySnapshot raw)
        {
            PackedMemorySnapshot packed_raw = new PackedMemorySnapshot(raw);
            XXMemorySnapshot snapshot = new XXMemorySnapshot();
            DateTime start = DateTime.Now;
            snapshot.Init(packed_raw);
            long ms = (long)(DateTime.Now - start).TotalMilliseconds;
            Debug.LogFormat("XXMemorySnapshot.Init {0}ms", ms);
            return snapshot;
        }

        /// <summary>
        /// 初始化内存快照。
        /// </summary>
        /// <param name="raw">Unity接口返回的最原始内存快照数据。</param>
        public void Init(PackedMemorySnapshot raw)
        {
            //原始数据解析
            m_VMInfo.Init(raw.virtualMachineInformation);
            m_TypeDescriptions.Init(raw.typeDescriptions, this);
            m_MemorySection.Init(raw.managedHeapSections, this);
            m_ManagedObjects.Analyze(raw.gcHandles, this);
        }

        /// <summary>
        /// 获取虚拟机信息。
        /// </summary>
        public XXVirtualMachineInformation VMInfo
        {
            get { return m_VMInfo; }
        }

        /// <summary>
        /// 获取类型信息。
        /// </summary>
        public XXTypeDescriptionSnapshot TypeDescriptions
        {
            get { return m_TypeDescriptions; }
        }

        /// <summary>
        /// 获取托管堆数据。
        /// </summary>
        public XXMemorySectionSnapshot MemorySections
        {
            get { return m_MemorySection; }
        }

        /// <summary>
        /// 获取托管对象。
        /// </summary>
        public XXManagedObjectSnapshot ManagedObjects
        {
            get { return m_ManagedObjects; }
        }

        /// <summary>
        /// 虚拟机信息。
        /// </summary>
        XXVirtualMachineInformation m_VMInfo = new XXVirtualMachineInformation();

        /// <summary>
        /// 类型信息。
        /// </summary>
        XXTypeDescriptionSnapshot m_TypeDescriptions = new XXTypeDescriptionSnapshot();

        /// <summary>
        /// 托管内存扇区。
        /// </summary>
        XXMemorySectionSnapshot m_MemorySection = new XXMemorySectionSnapshot();

        /// <summary>
        /// 托管对象。
        /// </summary>
        XXManagedObjectSnapshot m_ManagedObjects = new XXManagedObjectSnapshot();
    }
}