using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XX006.UI
{
    public class ShowFPS : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            m_DT6 = m_DT5 = m_DT4 = m_DT3 = m_DT2 = m_DT1 = m_CurDT = 166;
            m_FPS = 60;
            DTValue.text = (m_CurDT / 10.0f).ToString();
            FPSValue.text = m_FPS.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            float dt = Time.deltaTime;
            m_DT6 = m_DT5;
            m_DT5 = m_DT4;
            m_DT4 = m_DT3;
            m_DT3 = m_DT2;
            m_DT2 = m_DT1;
            m_DT1 = m_CurDT;
            m_CurDT = (int)(dt * 1000 * 10);
            if (m_DT1 != m_CurDT)
            {
                DTValue.text = (m_CurDT / 10.0f).ToString();
            }
            int fps = (int)(70000.0f / (m_CurDT + m_DT1 + m_DT2 + m_DT3 + m_DT4 + m_DT5 + m_DT6) + 0.2f);  //0.8½ø1
            if (m_FPS != fps)
            {
                m_FPS = fps;
                FPSValue.text = m_FPS.ToString();
            }
        }

        public TMPro.TextMeshProUGUI DTValue;
        public TMPro.TextMeshProUGUI FPSValue;

        private int m_CurDT = 0;
        private int m_DT1 = 0;
        private int m_DT2 = 0;
        private int m_DT3 = 0;
        private int m_DT4 = 0;
        private int m_DT5 = 0;
        private int m_DT6 = 0;
        private int m_FPS = 0;
    }
}