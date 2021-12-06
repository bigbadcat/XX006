using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 用于模型动画播放控制。
    /// </summary>
    public class Avatar : MonoBehaviourCache
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 播放动画。
        /// </summary>
        /// <param name="ani">动画名称。</param>
        /// <param name="fade">动画融合时长。</param>
        public void PlayAnimation(string ani, float fade = ANIMATION_FADE_DURATION)
        {
            if (LoadAnimation(ani))
            {
                m_Animator.CrossFade(ani, fade);
            }
        }

        /// <summary>
        /// 加载动画。(提前加载动画可以在播放动画时有效缓解卡顿)
        /// </summary>
        /// <param name="ani">动画名称。</param>
        /// <returns>是否有该动画。</returns>
        public bool LoadAnimation(string ani)
        {
            AnimationClip clip;
            if (m_Animations.TryGetValue(ani, out clip))
            {
                return clip != null;
            }
            if (string.IsNullOrEmpty(m_AnimationFolder))
            {
                Log.Error("The animation folder is empty. GameObject:{0}", this.name);
            }
            else
            {
                clip = ResourceManager.Instance.LoadAnimation(m_AnimationFolder + ani);
            }

            m_Animations[ani] = clip;
            Controller[ani] = clip;
            if (clip == null)
            {
                Log.Warning("LoadAnimation failed. GameObject:{0} ani:{1}", this.name, ani);
            }
            return clip != null;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 默认动画融合时长。
        /// </summary>
        public const float ANIMATION_FADE_DURATION = 0.1f;

        /// <summary>
        /// 获取或设置动画路径。获取时非空路径将会自动在末尾添加‘/’，设置时可以带‘/’结尾也可以不带。
        /// </summary>
        public string AnimationFolder
        {
            get { return m_AnimationFolder; }
            set
            {
                m_AnimationFolder = value;
                if (!string.IsNullOrEmpty(m_AnimationFolder) && !m_AnimationFolder.EndsWith("/"))
                {
                    m_AnimationFolder = m_AnimationFolder + "/";
                }
            }
        }

        /// <summary>
        /// 获取动画控制器。
        /// </summary>
        public AnimatorOverrideController Controller
        {
            get
            {
                if (m_Controller == null)
                {
                    //覆盖原本的动画控制器
                    m_Animator = GetComponent<Animator>();
                    m_Controller = new AnimatorOverrideController(m_Animator.runtimeAnimatorController);
                    m_Controller.name = "AnimatorOverrideController";
                    m_Animator.runtimeAnimatorController = m_Controller;
                }
                return m_Controller;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        private void OnDestroy()
        {
            ReleaseAnimation();
        }

        /// <summary>
        /// 释放动画。
        /// </summary>
        private void ReleaseAnimation()
        {
            if (ResourceManager.Instance != null)
            {
                foreach (var kvp in m_Animations)
                {
                    ResourceManager.Instance.UnloadAsset(m_AnimationFolder + kvp.Key);
                }
            }
            m_Animations.Clear();
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 动画存放路径。
        /// </summary>
        [SerializeField]
        private string m_AnimationFolder = string.Empty;

        /// <summary>
        /// 动画集合。
        /// </summary>
        private Dictionary<string, AnimationClip> m_Animations = new Dictionary<string, AnimationClip>();

        /// <summary>
        /// 动画播放器。
        /// </summary>
        private Animator m_Animator;

        /// <summary>
        /// 动画控制器。
        /// </summary>
        private AnimatorOverrideController m_Controller;

        #endregion
    }
}