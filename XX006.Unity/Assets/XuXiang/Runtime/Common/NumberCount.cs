using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 整数统计。
    /// </summary>
    public class NumberCountInt
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="n">统计数量[1, 1024]。</param>
        public NumberCountInt(int n)
        {
            m_Numbers = new int[Mathf.Clamp(n, 1, 1024)];
            for (int i=0; i< m_Numbers.Length; ++i)
            {
                m_Numbers[i] = 0;
            }
        }

        /// <summary>
        /// 添加一个值。
        /// </summary>
        /// <param name="v">值大小。</param>
        public void Add(int v)
        {
            m_Sum += v - m_Numbers[m_Index];
            m_Numbers[m_Index++] = v;
            if (m_Index >= m_Numbers.Length)
            {
                m_Index = 0;
            }
            m_Count = Math.Min(m_Count + 1, m_Numbers.Length);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取平均值。
        /// </summary>
        public int Average
        {
            get
            {
                return m_Count > 0 ? (m_Sum / m_Count) : 0;
            }
        }

        /// <summary>
        /// 获取和。
        /// </summary>
        public int Sum
        {
            get { return m_Sum; }
        }

        /// <summary>
        /// 获取值的数量。
        /// </summary>
        public int Count
        {
            get { return m_Count; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 当前统计的值序列。
        /// </summary>
        private int[] m_Numbers = null;

        /// <summary>
        /// 当前索引。
        /// </summary>
        private int m_Index = 0;

        /// <summary>
        /// 数量。
        /// </summary>
        private int m_Count = 0;

        /// <summary>
        /// 当前的和。
        /// </summary>
        private int m_Sum = 0;

        #endregion
    }
}