using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XX006.Fight
{
    public enum SkillActionType
    {
        #region 数据类----------

        /// <summary>
        /// 移动。
        /// </summary>
        Shift,

        #endregion

        #region 表现类----------

        /// <summary>
        /// 播放动画。
        /// </summary>
        Animation,

        /// <summary>
        /// 播放效果。
        /// </summary>
        Effect,

        #endregion
    }

    /// <summary>
    /// 技能行为运行数据。
    /// </summary>
    public class SkillActionEntry
    {
        /// <summary>
        /// 生效延迟。
        /// </summary>
        public float delay = 0;

        /// <summary>
        /// 技能行为。
        /// </summary>
        public SkillAction action = null;
    }

    /// <summary>
    /// 技能行为。(逻辑对象)
    /// </summary>
    public abstract class SkillAction
    {
        #region 对外操作----------------------------------------------------------------

        public abstract void DoAction(Player player);

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

        #endregion

        #region 内部数据----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 时间计数。
        /// </summary>
        protected float m_Duration = 0;

        #endregion
    }

    public class SkillActionMove : SkillAction
    {
        public override void DoAction(Player player)
        {
            SkillEffectMove mv_eft = new SkillEffectMove();
            mv_eft.Direction = player.transform.forward;
            mv_eft.Speed = player.m_MoveSpeed * SpeedRate;
            player.StartEffect(mv_eft, 0, Duration);
        }

        public float SpeedRate = 4;
    }

    public class SkillActionAnimation : SkillAction
    {
        public override void DoAction(Player player)
        {
            player.PlayAnimtion(AniName, AniMix);
        }

        public string AniName = string.Empty;

        public float AniMix = 0.1f;
    }
}