using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 用于从字节流中读取数据。
    /// </summary>
    public class DataReader
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="data">保存数据字节数组。</param>
        /// <param name="len">可读取数据长度。</param>
        /// <param name="start">读取的起始位置。</param>
        public DataReader(byte[] data, int len, int start = 0)
        {
            if (data == null)
            {
                throw new Exception("The data is null!");
            }
            if (start + len > data.Length)
            {
                throw new Exception("Bad data length! (start+len>=data.Length)");
            }

            m_baData = data;
            m_iLength = len;
            m_iPosition = start;
        }

        /// <summary>
        /// 读取一个字节。
        /// </summary>
        /// <returns>字节值。</returns>
        public byte ReadByte()
        {
            if (m_iPosition +1 > m_iLength)
            {
                throw new Exception("Not enough bytes!");
            }
            return m_baData[m_iPosition++];
        }

        /// <summary>
        /// 读取一个16位整数。
        /// </summary>
        /// <returns>16位整数值。</returns>
        public short ReadInt16()
        {
            if (m_iPosition + 2 > m_iLength)
            {
                throw new Exception("Not enough bytes!");
            }
            int i1 = m_baData[m_iPosition];
            int i2 = m_baData[m_iPosition + 1];
            int i = (i1 << 8) | i2;
            m_iPosition += 2;
            return (short)(i);
        }

        /// <summary>
        /// 读取一个32位整数。
        /// </summary>
        /// <returns>32位整数值。</returns>
        public int ReadInt32()
        {
            if (m_iPosition + 4 > m_iLength)
            {
                throw new Exception("Not enough bytes!");
            }
            int i1 = m_baData[m_iPosition];
            int i2 = m_baData[m_iPosition + 1];
            int i3 = m_baData[m_iPosition + 2];
            int i4 = m_baData[m_iPosition + 3];
            int i = (i1 << 24) | (i2 << 16) | (i3 << 8) | i4;
            m_iPosition += 4;
            return i;
        }

        /// <summary>
        /// 读取一个字符串。
        /// </summary>
        /// <returns>字符串值。</returns>
        public string ReadString()
        {
            int len = ReadInt32();
            if (m_iPosition + len > m_iLength)
            {
                throw new Exception(string.Format("Bad string length! {0}", len));
            }

            string str = Encoding.UTF8.GetString(m_baData, m_iPosition, len);
            m_iPosition += len;

            return str;
        }

        /// <summary>
        /// 读取一个布尔值。
        /// </summary>
        /// <returns>布尔值。</returns>
        public bool ReadBoolean()
        {
            return ReadByte() > 0;
        }

        /// <summary>
        /// 读取一个浮点数。
        /// </summary>
        /// <returns>浮点数值。</returns>
        public float ReadFloat()
        {
            return ReadInt32() / 1000.0f;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 数据源。
        /// </summary>
        private byte[] m_baData = null;

        /// <summary>
        /// 可读取数据长度。
        /// </summary>
        private int m_iLength = 0;

        /// <summary>
        /// 当前读取位置。
        /// </summary>
        private int m_iPosition = 0;

        #endregion
    }
}