using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace XX
{
    /// <summary>
    /// 有关提供内存快照的虚拟机的信息。
    /// </summary>
    public class XXVirtualMachineInformation
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="raw">原始数据。</param>
        public void Init(VirtualMachineInformation raw)
        {
            AllocationGranularity = raw.allocationGranularity;
            ArrayBoundsOffsetInHeader = raw.arrayBoundsOffsetInHeader;
            ArrayHeaderSize = raw.arrayHeaderSize;
            ArraySizeOffsetInHeader = raw.arraySizeOffsetInHeader;
            HeapFormatVersion = raw.heapFormatVersion;
            ObjectHeaderSize = raw.objectHeaderSize;
            PointerSize = raw.pointerSize;
        }

        /// <summary>
        /// 虚拟机分配器使用的字节分配粒度。
        /// </summary>
        public int AllocationGranularity { get; private set; }

        /// <summary>
        /// 存储数组边界的数组对象头部内的字节偏移量。
        /// </summary>
        public int ArrayBoundsOffsetInHeader { get; private set; }

        /// <summary>
        /// 数组对象头的大小(以字节为单位)。
        /// </summary>
        public int ArrayHeaderSize { get; private set; }

        /// <summary>
        /// 存储数组大小的数组对象头部内的字节偏移量。
        /// </summary>
        public int ArraySizeOffsetInHeader { get; private set; }

        /// <summary>
        /// 当托管堆内的对象布局发生变化时将发生变化的版本号。
        /// </summary>
        public int HeapFormatVersion { get; private set; }

        /// <summary>
        /// 每个托管对象头的大小(以字节为单位)。
        /// </summary>
        public int ObjectHeaderSize { get; private set; }

        /// <summary>
        /// 指针的大小(以字节计)。
        /// </summary>
        public int PointerSize { get; private set; }
    }
}
