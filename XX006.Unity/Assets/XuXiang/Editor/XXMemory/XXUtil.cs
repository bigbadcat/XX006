using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

namespace XX
{
    /// <summary>
    /// 工具函数。
    /// </summary>
    public static class XXUtil
    {
        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="entries">数据源。</param>
        /// <param name="count">数据数量。</param>
        /// <returns>数据内容。</returns>
        public static T[] GetEntries<T>(ArrayEntries<T> entries, uint count)
        {
            T[] ret = new T[count];
            entries.GetEntries(0, count, ref ret);
            return ret;
        }

        /// <summary>
        /// 读取一个指针。
        /// </summary>
        /// <param name="bytes">内存数据。</param>
        /// <param name="offset">偏移。</param>
        /// <param name="psize">指针大小。</param>
        /// <returns>指针值。</returns>
        public static ulong ReadPointer(byte[] bytes, int offset, int psize)
        {
            if (psize == 4)
            {
                return BitConverter.ToUInt32(bytes, offset);
            }
            if (psize == 8)
            {
                return BitConverter.ToUInt64(bytes, offset);
            }
            throw new Exception("Unexpected pointersize: " + psize);
        }

        /// <summary>
        /// 获取数组长度。
        /// </summary>
        /// <param name="bytes">数组所在存储区域。</param>
        /// <param name="offset">起始偏移。</param>
        /// <param name="array_type">数组元素类型。</param>
        /// <param name="vmi">虚拟机信息。</param>
        /// <returns>数组长度。</returns>
        public static int ReadArrayLength(byte[] bytes, int offset, XXTypeDescription array_type, XXMemorySnapshot mem_snap)
        {
            XXVirtualMachineInformation vmi = mem_snap.VMInfo;
            ulong bounds = XXUtil.ReadPointer(bytes, offset + vmi.ArrayBoundsOffsetInHeader, vmi.PointerSize);
            if (bounds == 0)
            {
                return (int)XXUtil.ReadPointer(bytes, offset + vmi.ArraySizeOffsetInHeader, vmi.PointerSize);
            }

            XXMemorySection section = mem_snap.MemorySections.Find(bounds);
            int cursor = (int)(bounds - section.StartAddress);
            int length = 1;
            for (int i=0; i<array_type.ArrayRank; ++i)
            {
                length *= (int)XXUtil.ReadPointer(section.Bytes, cursor, vmi.PointerSize);
                cursor += vmi.PointerSize == 4 ? 8 : 16;
            }
            return length;
        }

        /// <summary>
        /// 获取数组大小。
        /// </summary>
        /// <param name="bytes">数组所在存储区域。</param>
        /// <param name="offset">起始偏移。</param>
        /// <param name="array_type">数组元素类型。</param>
        /// <param name="vmi">虚拟机信息。</param>
        /// <returns>数组长度。</returns>
        public static int ReadArrayObjectSize(byte[] bytes, int offset, XXTypeDescription array_type, XXMemorySnapshot mem_snap)
        {
            XXVirtualMachineInformation vmi = mem_snap.VMInfo;
            int len = ReadArrayLength(bytes, offset, array_type, mem_snap);
            int esize = array_type.IsValueType ? array_type.Size : vmi.PointerSize;
            int size = vmi.ArrayHeaderSize + esize * len;
            return size;
        }

        /// <summary>
        /// 读取字符串对象大小。
        /// </summary>
        /// <param name="bytes">所在字节数组。</param>
        /// <param name="offset">对象起始地址偏移。</param>
        /// <param name="vmi">虚拟机信息。</param>
        /// <returns>字符串对象大小。</returns>
        public static int ReadStringObjectSize(byte[] bytes, int offset, XXVirtualMachineInformation vmi)
        {
            //数据区的前4个字节表示长度
            int len = BitConverter.ToInt32(bytes, offset + vmi.ObjectHeaderSize);
            int size = vmi.ObjectHeaderSize + 4 + (len + 1) * 2;    //utf16=2bytes per char
            return size;
        }

        /// <summary>
        /// 读取字符串的值。
        /// </summary>
        /// <param name="bytes">所在字节数组。</param>
        /// <param name="offset">对象起始地址偏移。</param>
        /// <param name="vmi">虚拟机信息。</param>
        /// <returns>字符串的值。</returns>
        public static string ReadStringValue(byte[] bytes, int offset, XXVirtualMachineInformation vmi)
        {
            int len = BitConverter.ToInt32(bytes, offset + vmi.ObjectHeaderSize);
            string text = System.Text.Encoding.Unicode.GetString(bytes, offset + vmi.ObjectHeaderSize + 4, len * 2);
            return text;
        }
    }
}