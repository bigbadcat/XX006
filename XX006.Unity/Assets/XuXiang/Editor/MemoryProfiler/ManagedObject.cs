using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 保存对象被引用的信息。
    /// </summary>
    public class ObjectReferenceFrom
    {
        /// <summary>
        /// 引用来源类型。
        /// </summary>
        public enum FromType
        {
            /// <summary>
            /// Native层。
            /// </summary>
            GCHandle,

            /// <summary>
            /// 成员字段。
            /// </summary>
            Field,
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="gc_index">GCHandle索引。</param>
        public ObjectReferenceFrom(int gc_index)
        {
            Type = FromType.GCHandle;
            GCHandleIndex = gc_index;
            FieldPath = string.Empty;
            ObjectAddress = 0;
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="field_path">字段路径，若为静态字段则包含类名前缀。</param>
        /// <param name="address">对象地址，0标识静态字段。</param>
        public ObjectReferenceFrom(string field_path, ulong address)
        {
            Type = FromType.Field;
            GCHandleIndex = 0;
            FieldPath = field_path;
            ObjectAddress = address;
        }

        /// <summary>
        /// 获取引用来源类型。
        /// </summary>
        public FromType Type { get; private set; }

        /// <summary>
        /// 获取GCHandle索引。
        /// </summary>
        public int GCHandleIndex { get; private set; }

        /// <summary>
        /// 获取静态字段完整路径。
        /// </summary>
        public string FieldPath { get; private set; }

        /// <summary>
        /// 获取源对象地址。
        /// </summary>
        public ulong ObjectAddress { get; private set; }
    }

    /// <summary>
    /// 保存对象引用的信息。
    /// </summary>
    public class ObjectReferenceTo
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="name">字段名称。</param>
        /// <param name="address">对象地址。</param>
        public ObjectReferenceTo(string name, ulong address)
        {
            Name = name;
            Address = address;
        }

        /// <summary>
        /// 成员名称。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 引用的对象地址。
        /// </summary>
        public ulong Address { get; private set; }
    }

    /// <summary>
    /// 托管对象(值类型数据不统计)。
    /// </summary>
    public class ManagedObject
    {
        /// <summary>
        /// 初始化对象。
        /// </summary>
        /// <param name="address">对象地址。</param>
        /// <param name="type_description">类型信息。</param>
        /// <param name="section">所在内存扇区。</param>
        /// <param name="snap">所属快照。</param>
        public void Init(ulong address, TypeDescription type_description, MemorySection section, MemorySnapshot snap)
        {
            Address = address;
            TypeDescription = type_description;
            Size = TypeDescription.Size;        //数组和字符串需要特殊处理
            MemorySection = section;
            BelongSnap = snap;
            ReferenceFrom = new List<ObjectReferenceFrom>();
            ReferenceTo = new List<ObjectReferenceTo>();
            if (type_description.IsArray)
            {
                int offset = (int)(address - MemorySection.StartAddress);
                Size = MemoryUtil.ReadArrayObjectSize(MemorySection.Bytes, offset, type_description, snap);
            }

            if (TypeDescription.TypeDescriptionName.CompareTo("System.String") == 0)
            {
                int offset = (int)(address - MemorySection.StartAddress);
                Size = MemoryUtil.ReadStringObjectSize(MemorySection.Bytes, offset, snap.VMInfo);
                ValueText = MemoryUtil.ReadStringValue(MemorySection.Bytes, offset, snap.VMInfo);
            }
            else
            {
                ValueText = string.Empty;
            }
        }

        /// <summary>
        /// 判断对象的值是否一样。
        /// </summary>
        /// <param name="to">要比较的。可能与自身不是来着同一份快照的对象，但必须是同一运行时的。</param>
        /// <returns>其它类型对象只比较地址。</returns>
        public bool IsSameValue(ManagedObject to)
        {
            //必须是同类型
            if (this.TypeDescription.TypeInfoAddress != to.TypeDescription.TypeInfoAddress)
            {
                return false;
            }

            //没有内容就比较地址
            if (string.IsNullOrEmpty(this.ValueText) || string.IsNullOrEmpty(to.ValueText))
            {
                return this.Address == to.Address;
            }
            return this.ValueText.CompareTo(to.ValueText) == 0;
        }

        /// <summary>
        /// 对象的内存地址。
        /// </summary>
        public ulong Address { get; private set; }

        /// <summary>
        /// 类型信息。
        /// </summary>
        public TypeDescription TypeDescription { get; private set; }

        /// <summary>
        /// 对象占用的内存大小。
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// 所在的内存扇区。
        /// </summary>
        public MemorySection MemorySection { get; private set; }

        /// <summary>
        /// 获取所属快照。
        /// </summary>
        public MemorySnapshot BelongSnap { get; private set; }

        /// <summary>
        /// 值内容。
        /// </summary>
        public string ValueText
        {
            get { return m_ValueText; }
            private set
            {
                m_ValueText = value;
                ShortValueText = m_ValueText.Length < 80 ? m_ValueText : m_ValueText.Substring(0, 80);
            }
        }
        private string m_ValueText = string.Empty;

        /// <summary>
        /// 简短的值内容。
        /// </summary>
        public string ShortValueText { get; private set; }

        /// <summary>
        /// 该对象被谁引用。
        /// </summary>
        public List<ObjectReferenceFrom> ReferenceFrom { get; private set; }

        /// <summary>
        /// 该对象引用的其它对象。
        /// </summary>
        public List<ObjectReferenceTo> ReferenceTo { get; private set; }
    }

    /// <summary>
    /// 托管对象快照。
    /// </summary>
    public class ManagedObjectSnapshot
    {
        /// <summary>
        /// 分析对象。
        /// </summary>
        /// <param name="raw_gc">原始GCHandle列表。</param>
        /// <param name="mem_snap">所属内存快照对象。</param>
        public void Analyze(PackedGCHandle[] raw_gc, MemorySnapshot mem_snap)
        {
            int psize = mem_snap.VMInfo.PointerSize;
            m_ManagedObjects.Clear();
            m_BadObjectAddress.Clear();
            for (int i=0; i<raw_gc.Length; ++i)
            {
                ulong address = psize == 4 ? (raw_gc[i].target & 0xFFFFFFFF) : raw_gc[i].target;
                ManagedObject obj = AnalyzeManagedObject(address, mem_snap);
                if (obj != null)
                {
                    var from = new ObjectReferenceFrom(i);
                    obj.ReferenceFrom.Add(from);
                }              
            }

            foreach (var type_des in mem_snap.TypeDescriptions.TypesDescriptions)
            {
                if (type_des.StaticFieldBytes.Length > 0)
                {
                    AnalyzeManagedObject(type_des.StaticFieldBytes, 0, type_des.StaticFields, mem_snap, null, type_des.TypeDescriptionName);
                }                
            }
        }

        /// <summary>
        /// 根据地址获取对象。
        /// </summary>
        /// <param name="address">对象地址。</param>
        /// <returns>对象信息。</returns>
        public ManagedObject GetObject(ulong address)
        {
            ManagedObject obj;
            if (m_ManagedObjects.TryGetValue(address, out obj))
            {
                return obj;
            }
            return null;
        }
        
        /// <summary>
        /// 获取对象集合。
        /// </summary>
        public Dictionary<ulong, ManagedObject> ManagedObjects
        {
            get { return m_ManagedObjects; }
        }

        /// <summary>
        /// 分析托管对象。
        /// </summary>
        /// <param name="address">对象地址。</param>
        /// <param name="mem_snap">所属内存快照对象。</param>
        private ManagedObject AnalyzeManagedObject(ulong address, MemorySnapshot mem_snap)
        {
            //判断空指针或者是否已经分析过了
            if (address <= 0 || m_BadObjectAddress.ContainsKey(address))
            {
                return null;
            }
            ManagedObject obj;
            if (m_ManagedObjects.TryGetValue(address, out obj))
            {
                return obj;
            }

            //对象数据第一个指针指向vtable，vatable第一个指针指向类型数据
            VirtualMachineInformation vmi = mem_snap.VMInfo;
            MemorySection section_obj = mem_snap.MemorySections.Find(address);
            if (section_obj == null)
            {
                m_BadObjectAddress.Add(address, 0);
                return null;
            }

            int offset_obj = (int)(address - section_obj.StartAddress);
            ulong address_vtable = MemoryUtil.ReadPointer(section_obj.Bytes, offset_obj, vmi.PointerSize);
            MemorySection section_vtable = mem_snap.MemorySections.Find(address_vtable);
            if (section_vtable == null)
            {
                m_BadObjectAddress.Add(address, 1);
                return null;
            }
            int offset_vtable = (int)(address_vtable - section_vtable.StartAddress);
            ulong type_address = MemoryUtil.ReadPointer(section_vtable.Bytes, offset_vtable, vmi.PointerSize);
            TypeDescription type_description = mem_snap.TypeDescriptions.GetTypeByAddress(type_address);
            if (type_description == null)
            {
                m_BadObjectAddress.Add(address, 2);
                return null;
            }

            obj = new ManagedObject();
            obj.Init(address, type_description, section_obj, mem_snap);
            m_ManagedObjects.Add(obj.Address, obj);

            //分析成员
            if (type_description.IsArray)
            {
                if (type_description.BaseOrElementTypeIndex != -1)
                {
                    TypeDescription ele_type = mem_snap.TypeDescriptions.TypesDescriptions[type_description.BaseOrElementTypeIndex];
                    int array_length = MemoryUtil.ReadArrayLength(section_obj.Bytes, offset_obj, type_description, mem_snap);
                    int cursor = offset_obj + vmi.ArrayHeaderSize;
                    for (int i = 0; i < array_length; ++i)
                    {
                        if (ele_type.IsValueType)
                        {
                            AnalyzeManagedObject(section_obj.Bytes, cursor, ele_type.InstanceFields, mem_snap, obj, i.ToString());
                            cursor += ele_type.Size;
                        }
                        else
                        {
                            ulong e_address = MemoryUtil.ReadPointer(section_obj.Bytes, cursor, vmi.PointerSize);
                            cursor += vmi.PointerSize;
                            ManagedObject to_obj = AnalyzeManagedObject(e_address, mem_snap);
                            AddReferenceInfo(obj, i.ToString(), to_obj);
                        }
                    }
                }
            }
            else
            {
                AnalyzeManagedObject(section_obj.Bytes, offset_obj + vmi.ObjectHeaderSize, type_description.InstanceFields, mem_snap, obj, string.Empty);
            }
            return obj;
        }

        /// <summary>
        /// 分析托管对象。
        /// </summary>
        /// <param name="bytes">成员数据。</param>
        /// <param name="offset">起始偏移。</param>
        /// <param name="fields">字段列表。</param>
        /// <param name="mem_snap">所属内存快照对象。</param>
        /// <param name="belong">所属对象。</param>
        /// <param name="field_prefix">成员前缀。</param>
        private void AnalyzeManagedObject(byte[] bytes, int offset, List<FieldDescription> fields, MemorySnapshot mem_snap, ManagedObject belong, string field_prefix)
        {
            int hs = mem_snap.VMInfo.ObjectHeaderSize;
            int psize = mem_snap.VMInfo.PointerSize;
            foreach (var field in fields)
            {
                if (field.Offset < 0)
                {
                    continue;
                }

                TypeDescription type_des = mem_snap.TypeDescriptions.GetType(field.TypeIndex);
                string field_path = string.IsNullOrEmpty(field_prefix) ? field.Name : string.Format("{0}.{1}", field_prefix, field.Name);
                if (type_des.IsValueType)
                {
                    //非原子值类型则进一步分析
                    if (!type_des.IsAtomicType)
                    {
                        AnalyzeManagedObject(bytes, offset + field.Offset - ((field.IsStatic) ? 0 : hs), type_des.InstanceFields, mem_snap, belong, field_path);
                    }                    
                }
                else
                {
                    ulong address = address = MemoryUtil.ReadPointer(bytes, offset + field.Offset - ((field.IsStatic) ? 0 : hs), psize);
                    ManagedObject obj = AnalyzeManagedObject(address, mem_snap);
                    AddReferenceInfo(belong, field_path, obj);
                }
            }
        }

        /// <summary>
        /// 添加引用信息。
        /// </summary>
        /// <param name="from">引用源对象。</param>
        /// <param name="field_name">字段名称。</param>
        /// <param name="to">被引用的对象。</param>
        private static void AddReferenceInfo(ManagedObject from, string field_name, ManagedObject to)
        {
            if (to != null)
            {
                to.ReferenceFrom.Add(new ObjectReferenceFrom(field_name, from != null ? from.Address : 0));
            }
            if (from != null)
            {
                from.ReferenceTo.Add(new ObjectReferenceTo(field_name, to != null ? to.Address : 0));
            }
        }

        /// <summary>
        /// 错误的对象地址。 0:对象扇区没找到 1:vtable的扇区没找到 2:类型未找到
        /// </summary>
        private Dictionary<ulong, int> m_BadObjectAddress = new Dictionary<ulong, int>();

        /// <summary>
        /// 托管对象列表。
        /// </summary>
        private Dictionary<ulong, ManagedObject> m_ManagedObjects = new Dictionary<ulong, ManagedObject>();
    }
}