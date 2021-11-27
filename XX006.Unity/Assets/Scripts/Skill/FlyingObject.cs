 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XX006.Fight
{
    /// <summary>
    /// 飞行物对象。
    /// </summary>
    public abstract class FlyingObject
    {
        public void Start(Vector3 pos, float duration, GameObject show)
        {
            m_Position = pos;
            m_Count = 0;
            m_Duration = duration;
            m_ShowObject = show;
            if (m_ShowObject != null)
            {
                m_ShowObject.transform.position = pos;
            }
            FlyingObjectManager.Instance.AddFlyingObject(this);
            OnStart();
        }

        public void Update(float dt)
        {
            if (IsEnd)
            {
                return;
            }
            m_Count += dt;
            OnUpdate(dt);
            if (m_ShowObject != null)
            {
                m_ShowObject.transform.position = m_Position;
            }
            if (IsEnd)
            {
                OnEnd();
                GameObject.Destroy(m_ShowObject);
                m_ShowObject = null;
                return;
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

            m_Count = m_Duration;
            OnEnd();
            GameObject.Destroy(m_ShowObject);
            m_ShowObject = null;
        }

        public bool IsEnd
        {
            get { return m_Count >= m_Duration; }
        }

        /// <summary>
        /// 开始飞行。
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// 帧更新。
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void OnUpdate(float dt);

        /// <summary>
        /// 飞行结束。
        /// </summary>
        protected abstract void OnEnd();

        /// <summary>
        /// 时间计数。
        /// </summary>
        protected float m_Count = 0;

        /// <summary>
        /// 持续时间。
        /// </summary>
        protected float m_Duration = 0;

        /// <summary>
        /// 飞行位置。
        /// </summary>
        protected Vector3 m_Position = Vector3.zero;

        /// <summary>
        /// 显示对象。
        /// </summary>
        protected GameObject m_ShowObject = null;
    }

    /// <summary>
    /// 直线飞行物。
    /// </summary>
    public class StraightFlyingObject : FlyingObject
    {
        protected override void OnStart()
        {

        }

        protected override void OnUpdate(float dt)
        {
            m_Position += Direction * Speed * dt;
        }

        protected override void OnEnd()
        {

        }

        /// <summary>
        /// 飞行方向。
        /// </summary>
        public Vector3 Direction = Vector3.forward;

        /// <summary>
        /// 飞行速度。
        /// </summary>
        public float Speed = 1;
    }
}