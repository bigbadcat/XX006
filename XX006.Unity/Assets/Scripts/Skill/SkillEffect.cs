using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XX006.Fight
{
    /// <summary>
    /// 技能效果。(运行类对象)
    /// </summary>
    public abstract class SkillEffect
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 开始效果。
        /// </summary>
        /// <param name="player"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        public void Start(Player player, float delay, float duration)
        {
            m_Target = player;
            m_Delay = delay;
            m_Count = 0;
            m_Duration = duration;
            if (m_Delay <= 0)
            {
                OnStart();
            }            

            //无延迟无持续时间类效果开始即结束
            if (IsEnd)
            {
                OnEnd();
            }
        }

        /// <summary>
        /// 效果更新。
        /// </summary>
        /// <param name="dt">更新间隔。</param>
        public void Update(float dt)
        {
            if (IsEnd)
            {
                return;
            }

            if (m_Delay > 0)
            {
                m_Delay -= dt;
                if (m_Delay <= 0)
                {
                    OnStart();
                }
                return;
            }
            m_Count += dt;
            OnUpdate(dt);
            if (IsEnd)
            {
                OnEnd();
            }
        }

        /// <summary>
        /// 强制结束效果。
        /// </summary>
        public void ForcedEnd()
        {
            if (IsEnd)
            {
                return;
            }

            m_Delay = 0;
            m_Count = m_Duration;
            OnEnd();
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取或设置行为持续时间。
        /// </summary>
        public float Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        /// <summary>
        /// 判断效果是否结束。
        /// </summary>
        public bool IsEnd
        {
            get { return m_Delay <= 0 && m_Count >= Duration; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdate(float dt)
        {
        }

        protected virtual void OnEnd()
        {
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 效果延迟倒计时。
        /// </summary>
        public float m_Delay = 0;

        /// <summary>
        /// 效果计数。
        /// </summary>
        public float m_Count = 0;

        /// <summary>
        /// 时间计数。
        /// </summary>
        protected float m_Duration = 0;

        /// <summary>
        /// 作用目标。
        /// </summary>
        protected Player m_Target = null;

        #endregion
    }

    public class SkillEffectMove : SkillEffect
    {
        protected override void OnUpdate(float dt)
        {
            Vector3 mv = Direction * Speed * dt;
            m_Target.transform.position += mv;
        }

        public Vector3 Direction = Vector3.forward;

        public float Speed = 1;
    }

    public class SkillEffectChangeBend : SkillEffect
    {
        protected override void OnStart()
        {
            m_BendArea = m_Target?.GetComponent<BendArea>();
            if (m_BendArea != null)
            {
                m_BendArea.Range = ChangeCurve.Evaluate(0);
            }
        }

        protected override void OnUpdate(float dt)
        {
            if (m_BendArea != null)
            {
                m_BendArea.Range = ChangeCurve.Evaluate(m_Count);
            }
        }

        protected override void OnEnd()
        {
            if (m_BendArea != null)
            {
                m_BendArea.Range = 1;
            }
            m_BendArea = null;
        }

        public AnimationCurve ChangeCurve = null;

        private BendArea m_BendArea = null;
    }
}