using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XX006.Fight
{
    /// <summary>
    /// 技能运行数据。
    /// </summary>
    public class SkillRunTime
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 添加技能动作。
        /// </summary>
        /// <param name="action">技能动作。</param>
        /// <param name="delay">动作延迟。</param>
        public void AddAction(SkillAction action, float delay)
        {
            SkillActionEntry entry = new SkillActionEntry();
            entry.delay = delay;
            entry.action = action;
            m_Actions.Add(entry);
            m_Duration = Mathf.Max(m_Duration, delay + action.Duration);
        }

        /// <summary>
        /// 更新技能运行。
        /// </summary>
        /// <param name="dt">更新时长。</param>
        public void Update(float dt)
        {
            m_Count += dt;
            s_CacheIndex.Clear();
            for (int i=0; i< m_Actions.Count; ++i)
            {
                SkillActionEntry entry = m_Actions[i];
                entry.delay -= dt;
                if (entry.delay <= 0)
                {
                    entry.action.DoAction(m_Owner);
                    s_CacheIndex.Add(i);
                }
            }

            //移除掉已结束的行为
            for (int i = s_CacheIndex.Count - 1; i >= 0; --i)
            {
                m_Actions.RemoveAt(s_CacheIndex[i]);
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取是否结束。
        /// </summary>
        public bool IsEnd
        {
            get { return m_Count >= m_Duration; }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        private static List<int> s_CacheIndex = new List<int>();

        public Player m_Owner = null;

        /// <summary>
        /// 技能持续时间。
        /// </summary>
        private float m_Duration = 0;

        /// <summary>
        /// 时间计数。
        /// </summary>
        private float m_Count = 0;

        /// <summary>
        /// 技能行为列表。
        /// </summary>
        private List<SkillActionEntry> m_Actions = new List<SkillActionEntry>();

        #endregion
    }
}