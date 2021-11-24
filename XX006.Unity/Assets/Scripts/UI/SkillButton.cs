using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XuXiang;

namespace XX006.UI
{
    public class SkillButton : MonoBehaviour
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 按下。
        /// </summary>
        /// <param name="data">按下的信息。</param>
        public void OnPointDown(BaseEventData data)
        {
            if (m_TouchDown || IsCD)
            {
                return;
            }

            PointerEventData pdata = data as PointerEventData;
            //Vector3 wv = PanelManager.Instance.UICamera.ScreenToWorldPoint(pdata.position);
            Vector3 wv = this.GetComponentInParent<Canvas>().worldCamera.ScreenToWorldPoint(pdata.position);
            Vector3 v = transform.InverseTransformPoint(wv);
            v.z = 0;
            if (v.sqrMagnitude > 50 * 50)
            {
                return;
            }

            //Log.Info("Down:{0}", v);
            m_TouchDown = true;

            Player p = ControllerCenter.Instance.Target;
            if (p != null && p.DoSkill(m_SkillID))
            {
                StartCD();
            }            
        }

        /// <summary>
        /// 弹起。
        /// </summary>
        /// <param name="data">弹起的信息。</param>
        public void OnPointUp(BaseEventData data)
        {
            if (!m_TouchDown)
            {
                return;
            }

            m_TouchDown = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            CDProgress.enabled = false;
            CDValue.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (m_CDValue > 0)
            {
                m_CDValue -= Time.deltaTime;                
                if (m_CDValue > 0)
                {
                    //更新CD显示
                    int cds = (int)(m_CDValue * 10);
                    if (cds != m_CDShow)
                    {
                        m_CDShow = cds;
                        CDValue.text = m_CDShow >= 10 ? (m_CDShow / 10).ToString() : (m_CDShow / 10.0f).ToString("N1");
                    }
                    CDProgress.fillAmount = m_CDValue / m_CD;
                }
                else
                {
                    EndCD();
                }
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        public Image Icon;
        public Image CDProgress;
        public TMPro.TextMeshProUGUI CDValue;

        /// <summary>
        /// 判断是否在CD中。
        /// </summary>
        public bool IsCD
        {
            get { return m_CDValue > 0; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        private void StartCD()
        {
            m_CDValue = m_CD;
            CDProgress.enabled = true;
            CDProgress.fillAmount = 1;
            CDValue.enabled = true;
            m_CDShow = (int)(m_CDValue * 10);
            CDValue.text = m_CDShow >= 10 ? (m_CDShow / 10).ToString() : (m_CDShow / 10.0f).ToString("N1");
        }

        private void EndCD()
        {
            //播放结束特效

            //隐藏CD组件
            CDProgress.enabled = false;
            CDValue.enabled = false;
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 是否按下了。
        /// </summary>
        private bool m_TouchDown = false;

        /// <summary>
        /// 技能编号。
        /// </summary>
        [SerializeField]
        private int m_SkillID = 0;

        /// <summary>
        /// CD时长。(实际是通过技能ID从控制目标中获取)
        /// </summary>
        [SerializeField]
        private float m_CD = 5;

        /// <summary>
        /// CD计数。(实际是通过技能ID从控制目标中获取)
        /// </summary>
        private float m_CDValue = 0;

        /// <summary>
        /// 显示的CD值。(减少CD文本生成频率)
        /// </summary>
        private int m_CDShow = 0;

        #endregion
    }
}