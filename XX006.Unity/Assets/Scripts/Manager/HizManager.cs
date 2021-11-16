using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    public class HizManager : Singleton<HizManager>
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 更新深度纹理。
        /// </summary>
        public void UpdateDepthTexture()
        {
            if (m_DepthUpdateFrame == Time.frameCount || m_MipmapMat == null)
            {
                return;
            }

            RenderTexture depth_tex = DepthTexture;
            m_DepthUpdateFrame = Time.frameCount;

            Graphics.Blit(Shader.GetGlobalTexture(m_PID_CameraDepthTexture), m_DepthTextureTmp[0]);
            Graphics.CopyTexture(m_DepthTextureTmp[0], 0, 0, depth_tex, 0, 0);
            for (int i = 1; i < m_DepthTextureTmp.Length; ++i)
            {
                Graphics.Blit(m_DepthTextureTmp[i - 1], m_DepthTextureTmp[i], m_MipmapMat);
                Graphics.CopyTexture(m_DepthTextureTmp[i], 0, 0, depth_tex, 0, i);
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取带mipmap的深度图。
        /// </summary>
        public RenderTexture DepthTexture
        {
            get
            {
                if (m_DepthTexture == null)
                {
                    int size = 256;     //256够了，太大性价比不高
                    m_DepthTexture = new RenderTexture(size, size, 0, RenderTextureFormat.RHalf);           //16的红色通道保持深度值
                    m_DepthTexture.autoGenerateMips = false;        //Mipmap手动生成
                    m_DepthTexture.useMipMap = true;
                    m_DepthTexture.filterMode = FilterMode.Point;
                    m_DepthTexture.Create();

                    for (int i = 0; i < m_DepthTextureTmp.Length; ++i)
                    {
                        int w = size / (1 << i);
                        RenderTexture rt = RenderTexture.GetTemporary(w, w, 0, RenderTextureFormat.RHalf);
                        rt.useMipMap = false;
                        rt.autoGenerateMips = false;
                        rt.filterMode = FilterMode.Point;
                        m_DepthTextureTmp[i] = rt;
                    }
                }
                return m_DepthTexture;
            }
        }

        /// <summary>
        /// 获取或设置mipmap用的材质。
        /// </summary>
        public Material MipmapMat
        {
            get { return m_MipmapMat; }
            set { m_MipmapMat = value; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 初始化。
        /// </summary>
        protected override void Init()
        {
            m_PID_CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        }

        /// <summary>
        /// 释放。
        /// </summary>
        protected override void Release()
        {
            Destroy(m_DepthTexture);
            m_DepthTexture = null;
            for (int i = 0; i < m_DepthTextureTmp.Length; ++i)
            {
                RenderTexture.ReleaseTemporary(m_DepthTextureTmp[i]);
                m_DepthTextureTmp[i] = null;
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 带mipmap的深度图。
        /// </summary>
        private RenderTexture m_DepthTexture = null;

        /// <summary>
        /// 生成mipmap的临时纹理。[256, 128, 64, 32, 16]5个尺寸。
        /// </summary>
        private RenderTexture[] m_DepthTextureTmp = new RenderTexture[5];

        /// <summary>
        /// 生成mipmap用的材质。
        /// </summary>
        private Material m_MipmapMat;

        /// <summary>
        /// 当前深度图对应的帧序号。
        /// </summary>
        private int m_DepthUpdateFrame = -1;

        /// <summary>
        /// 摄像机深度纹理属性id。
        /// </summary>
        private int m_PID_CameraDepthTexture = 0;

        #endregion

    }
}