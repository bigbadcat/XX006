using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace XX
{
    /// <summary>
    /// 托管类型的字段的描述。
    /// </summary>
    public class XXFieldDescription
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="raw">原始数据。</param>
        public void Init(FieldDescription raw)
        {
            IsStatic = raw.isStatic;
            Name = raw.name;
            Offset = raw.offset;
            TypeIndex = raw.typeIndex;
        }

        /// <summary>
        /// 这个字段是静态的吗?
        /// </summary>
        public bool IsStatic { get; private set; }

        /// <summary>
        /// 此字段的名称。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 该字段的偏移量。
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// 此字段所属类型的类型描述索引。
        /// </summary>
        public int TypeIndex { get; private set; }
    }

    /// <summary>
    /// 类型描述。
    /// </summary>
    public class XXTypeDescription
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="raw">原始数据。</param>
        /// <param name="snap">所属快照。</param>
        public void Init(TypeDescription raw, XXMemorySnapshot snap)
        {
            Assembly = raw.assembly;
            TypeDescriptionName = raw.name;
            TypeIndex = raw.typeIndex;
            BaseOrElementTypeIndex = raw.baseOrElementTypeIndex;
            m_Fields.Clear();
            m_StaticFields.Clear();
            m_InstanceFields.Clear();
            foreach (var raw_field in raw.fields)
            {
                XXFieldDescription field = new XXFieldDescription();
                field.Init(raw_field);
                m_Fields.Add(field);
                var add_to = field.IsStatic ? m_StaticFields : m_InstanceFields;
                add_to.Add(field);
            }

            IsValueType = raw.isValueType;
            IsArray = raw.isArray;
            ArrayRank = raw.arrayRank;
            Size = raw.size;
            TypeInfoAddress = snap.VMInfo.PointerSize == 4 ? (raw.typeInfoAddress & 0xFFFFFFFF) : raw.typeInfoAddress;
            StaticFieldBytes = raw.staticFieldBytes;
            BelongSnap = snap;

            //只有一个字段，且字段类型等于自身的值类型，则认为是不可解析的原子类型
            IsAtomicType = IsValueType && m_InstanceFields.Count == 1 && m_InstanceFields[0].TypeIndex == TypeIndex;
        }

        /// <summary>
        /// 建立完整的字段列表。
        /// </summary>
        /// <param name="type_descriptions">所有类型列表。</param>
        public void BuildFullField(List<XXTypeDescription> type_descriptions)
        {
            //基类字段
            if (IsArray || IsValueType || BaseOrElementTypeIndex == -1)
            {
                return;
            }

            XXTypeDescription base_type = type_descriptions[BaseOrElementTypeIndex];
            while (base_type != null && !base_type.IsValueType && !base_type.IsArray)
            {
                foreach (var field in base_type.Fields)
                {
                    if (!field.IsStatic)
                    {
                        m_InstanceFields.Add(field);
                    }
                }

                //下一个父类
                if (base_type.BaseOrElementTypeIndex != -1)
                {
                    base_type = type_descriptions[base_type.BaseOrElementTypeIndex];
                }
                else
                {
                    break;
                }                
            }
        }

        /// <summary>
        /// 加载此类型的程序集的名称。
        /// </summary>
        public string Assembly { get; private set; }

        /// <summary>
        /// 此类型的名称。
        /// </summary>
        public string TypeDescriptionName { get; private set; }

        /// <summary>
        /// 此类型的类型索引。
        /// </summary>
        public int TypeIndex { get; private set; }

        /// <summary>
        /// 基类原始索引。
        /// </summary>
        public int BaseOrElementTypeIndex { get; private set; }

        /// <summary>
        /// 字段列表。
        /// </summary>
        public List<XXFieldDescription> Fields
        {
            get { return m_Fields; }
        }
        private List<XXFieldDescription> m_Fields = new List<XXFieldDescription>();

        /// <summary>
        /// 静态字段列表。
        /// </summary>
        public List<XXFieldDescription> StaticFields
        {
            get { return m_StaticFields; }
        }
        private List<XXFieldDescription> m_StaticFields = new List<XXFieldDescription>();

        /// <summary>
        /// 实例字段列表。
        /// </summary>
        public List<XXFieldDescription> InstanceFields
        {
            get { return m_InstanceFields; }
        }
        private List<XXFieldDescription> m_InstanceFields = new List<XXFieldDescription>();

        /// <summary>
        /// 是否值类型。
        /// </summary>
        public bool IsValueType { get; private set; }

        /// <summary>
        /// 是否数组。
        /// </summary>
        public bool IsArray { get; private set; }

        /// <summary>
        /// 如果是数组，则返回数组维度。
        /// </summary>
        public int ArrayRank { get; private set; }

        /// <summary>
        /// 该类型实例的大小(以字节计)。如果该类型是数组类型，则描述数组中单个元素所占用的字节量。
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// 是否内核类型。
        /// </summary>
        public bool IsAtomicType { get; private set; }

        /// <summary>
        /// 内存中的地址，它包含虚拟机中这种类型的描述。这可用于将堆中的托管对象与其对应的类型描述匹配，因为托管对象的第一个指针指向其类型描述。
        /// </summary>
        public ulong TypeInfoAddress { get; private set; }

        /// <summary>
        /// 在拍摄快照时，存储此类型静态字段的字节的实际内容。
        /// </summary>
        public byte[] StaticFieldBytes { get; private set; }

        /// <summary>
        /// 获取所属快照。
        /// </summary>
        public XXMemorySnapshot BelongSnap { get; private set; }
    }

    /// <summary>
    /// 类型快照。
    /// </summary>
    public class XXTypeDescriptionSnapshot
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="raw">原始数据。</param>
        public void Init(TypeDescription[] raw, XXMemorySnapshot mem_snap)
        {
            //生成自己的数据
            m_TypeDescriptions.Clear();
            m_TypeDescriptions.Capacity = raw.Length;
            m_AddressToTypeDescription.Clear();
            for (int i = 0; i < raw.Length; ++i)
            {
                XXTypeDescription item = new XXTypeDescription();
                item.Init(raw[i], mem_snap);
                m_TypeDescriptions.Add(item);
                m_AddressToTypeDescription.Add(item.TypeInfoAddress, item);
            }

            //建立完整的字段列表
            foreach (var type_des in m_TypeDescriptions)
            {
                type_des.BuildFullField(m_TypeDescriptions);
            }
        }

        /// <summary>
        /// 获取类型。
        /// </summary>
        /// <param name="index">类型索引。</param>
        /// <returns>类型信息。</returns>
        public XXTypeDescription GetType(int index)
        {
            if (index < 0 || index >= m_TypeDescriptions.Count)
            {
                return null;
            }
            return m_TypeDescriptions[index];
        }

        /// <summary>
        /// 通过地址获取类型信息。
        /// </summary>
        /// <param name="address">类型地址。</param>
        /// <returns>类型信息。</returns>
        public XXTypeDescription GetTypeByAddress(ulong address)
        {
            XXTypeDescription ret;
            if (m_AddressToTypeDescription.TryGetValue(address, out ret))
            {
                return ret;
            }
            return null;
        }

        /// <summary>
        /// 获取类型列表。
        /// </summary>
        public List<XXTypeDescription> TypesDescriptions
        {
            get { return m_TypeDescriptions; }
        }
        
        /// <summary>
        /// 类型序列，按原始索引排列。
        /// </summary>
        private List<XXTypeDescription> m_TypeDescriptions = new List<XXTypeDescription>();

        /// <summary>
        /// 通过地址获取类型信息。
        /// </summary>
        private Dictionary<ulong, XXTypeDescription> m_AddressToTypeDescription = new Dictionary<ulong, XXTypeDescription>();
    }
}