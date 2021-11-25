using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 一个内存扇区数据。
    /// </summary>
    public class MemorySection
    {
        /// <summary>
        /// 排序比较。
        /// </summary>
        /// <param name="a">扇区A。</param>
        /// <param name="b">扇区B。</param>
        /// <returns>起始地址比较结果。</returns>
        public static int SortCompare(MemorySection a, MemorySection b)
        {
            //无符号数不能直接用减法比较
            return a.StartAddress < b.StartAddress ? -1 : (a.StartAddress == b.StartAddress ? 0 : 1);
        }

        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="raw">原始数据。</param>
        public void Init(UnityEditor.MemoryProfiler.MemorySection raw, int psize)
        {
            StartAddress = psize == 4 ? (raw.startAddress & 0xFFFFFFFF) : raw.startAddress;
            Bytes = raw.bytes;
            EndAddress = StartAddress + (ulong)Bytes.Length;
        }

        /// <summary>
        /// 包含内存转储的字节数组。
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// 内存中的起始位置(包括此地址)。
        /// </summary>
        public ulong StartAddress { get; private set; }

        /// <summary>
        /// 内存中的结束位置(不包括此地址)。
        /// </summary>
        public ulong EndAddress { get; private set; }
    }

    /// <summary>
    /// 扇区查找类。
    /// </summary>
    class ManagedMemorySectionFinder : IComparer<MemorySection>
    {
        /// <summary>
        /// 查找比较。
        /// </summary>
        /// <param name="x">迭代比较元素。</param>
        /// <param name="y">无用，比较需要的参数封装在Finder中。</param>
        /// <returns>比较结果。</returns>
        public int Compare(MemorySection x, MemorySection y)
        {
            //无符号数不能直接用减法比较
            return x.EndAddress <= Address ? -1 : (x.StartAddress <= Address ? 0 : 1);
        }

        /// <summary>
        /// 缓存对象。
        /// </summary>
        public static ManagedMemorySectionFinder Cache = new ManagedMemorySectionFinder();

        /// <summary>
        /// 要查找的地址。
        /// </summary>
        public ulong Address { get; set; }
    }

    /// <summary>
    /// 内存扇区快照。
    /// </summary>
    public class MemorySectionSnapshot
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="entries">原始数据。</param>
        public void Init(UnityEditor.MemoryProfiler.MemorySection[] raw, MemorySnapshot mem_snap)
        {
            //生成自己的数据
            int psize = mem_snap.VMInfo.PointerSize;
            m_Sections.Clear();
            m_Sections.Capacity = raw.Length;
            for (int i = 0; i < raw.Length; ++i)
            {
                MemorySection item = new MemorySection();
                item.Init(raw[i], psize);
                m_Sections.Add(item);
            }
            m_Sections.Sort(MemorySection.SortCompare);        //排序后可以进行二分查找
        }

        /// <summary>
        /// 获取扇区序列。
        /// </summary>
        public List<MemorySection> Sections
        {
            get { return m_Sections; }
        }

        /// <summary>
        /// 查找地址所在的扇区。
        /// </summary>
        /// <param name="address">地址值。</param>
        /// <returns>扇区对象。</returns>
        public MemorySection Find(ulong address)
        {
            ManagedMemorySectionFinder finder = ManagedMemorySectionFinder.Cache;
            finder.Address = address;
            int index = m_Sections.BinarySearch(null, finder);
            return (index >= 0 && index < m_Sections.Count) ? m_Sections[index] : null;
        }

        /// <summary>
        /// 扇区序列。
        /// </summary>
        private List<MemorySection> m_Sections = new List<MemorySection>();
    }
}