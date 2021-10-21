using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 监听者容器。
    /// </summary>
    public class ListenerContainer<T1, T2>
    {
        /// <summary>
        /// 监听者操作。
        /// </summary>
        private struct ListenerOperate
        {
            /// <summary>
            /// 添加操作类型值。
            /// </summary>
            public const int OPERATE_TYPE_ADD = 1;

            /// <summary>
            /// 移除操作类型值。
            /// </summary>
            public const int OPERATE_TYPE_REMOVE = 2;

            /// <summary>
            /// 构造函数。
            /// </summary>
            /// <param name="operate">操作类型。</param>
            /// <param name="listener">监听者。</param>
            public ListenerOperate(int operate, Action<T1, T2> listener)
            {
                Operate = operate;
                Listener = listener;
            }

            /// <summary>
            /// 操作类型。
            /// </summary>
            public int Operate;

            /// <summary>
            /// 监听者。
            /// </summary>
            public Action<T1, T2> Listener;
        }

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 添加监听者。
        /// </summary>
        /// <param name="listener">监听者。</param>
        public void AddListener(Action<T1, T2> listener)
        {
            if (m_IsInvoking)
            {
                m_Operates.Add(new ListenerOperate(ListenerOperate.OPERATE_TYPE_ADD, listener));
            }
            else
            {
                m_Listeners.Add(listener);
            }
        }

        /// <summary>
        /// 移除监听者。
        /// </summary>
        /// <param name="listener">监听者。</param>
        public void RemoveEndListener(Action<T1, T2> listener)
        {
            if (m_IsInvoking)
            {
                m_Operates.Add(new ListenerOperate(ListenerOperate.OPERATE_TYPE_REMOVE, listener));
            }
            else
            {
                m_Listeners.Remove(listener);
            }
        }

        /// <summary>
        /// 清空所有回调。
        /// </summary>
        public void ClearListener()
        {
            if (m_IsInvoking)
            {
                foreach (var tmp in m_Listeners)
                {
                    m_Operates.Add(new ListenerOperate(ListenerOperate.OPERATE_TYPE_REMOVE, tmp));
                }
            }
            else
            {
                m_Listeners.Clear();
            }
        }

        /// <summary>
        /// 触发回调。
        /// </summary>
        /// <param name="p1">参数1。</param>
        /// <param name="p2">参数2。</param>
        public void Invoke(T1 p1, T2 p2)
        {
            //触发回调
            m_IsInvoking = true;
            foreach (var tmp in m_Listeners)
            {
                tmp.Invoke(p1, p2);
            }
            m_IsInvoking = false;

            //处理操作
            foreach (var tmp in m_Operates)
            {
                if (tmp.Operate == ListenerOperate.OPERATE_TYPE_ADD)
                {
                    m_Listeners.Add(tmp.Listener);
                }
                else if (tmp.Operate == ListenerOperate.OPERATE_TYPE_REMOVE)
                {
                    m_Listeners.Remove(tmp.Listener);
                }
            }
            m_Operates.Clear();
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 是否回调中。
        /// </summary>
        private bool m_IsInvoking = false;

        /// <summary>
        /// 监听者列表。
        /// </summary>
        private List<Action<T1, T2>> m_Listeners = new List<Action<T1, T2>>();

        /// <summary>
        /// 监听者操作。
        /// </summary>
        private List<ListenerOperate> m_Operates = new List<ListenerOperate>();

        #endregion
    }
}