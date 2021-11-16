using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 控制中心，用于传递控制操作到角色。
    /// </summary>
    public class ControllerCenter : Singleton<ControllerCenter>
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 进行移动操作。
        /// </summary>
        /// <param name="dir">移动方向。</param>
        public void DoMove(Vector3 dir)
        {
            if (m_Target != null)
            {
                float d = ShapeUtil.NormalizeAngle(90 - (Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg));
                m_Target.TryMove(d);
            }            
        }

        /// <summary>
        /// 进行待机操作。
        /// </summary>
        public void DoIdle()
        {
            m_Target?.TryIdle();
        }

        /// <summary>
        /// 进行使用技能操作。
        /// </summary>
        /// <param name="code">技能编号。</param>
        public void DoCastSkill(int code)
        {
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        public Player Target
        {
            get { return m_Target; }
            set
            {
                m_Target = value;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        private Player m_Target = null;

        #endregion
    }
}