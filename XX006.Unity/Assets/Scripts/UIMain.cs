using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XX006;

public class UIMain : MonoBehaviour
{
    public RawImage RawImg;
    public TMPro.TextMeshProUGUI TiTle;
    public TMPro.TextMeshProUGUI DTValue;
    public TMPro.TextMeshProUGUI FPSValue;

    public Button SetFPS30;
    public Button SetFPS60;
    public Button SetFPSMax;

    public Slider ViewSlider;
    public Transform ViewCamera;

    private int m_CurDT = 0;
    private int m_DT1 = 0;
    private int m_DT2 = 0;
    private int m_DT3 = 0;
    private int m_DT4 = 0;
    private int m_DT5 = 0;
    private int m_DT6 = 0;
    private int m_FPS = 0;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        OnSetFPSMaxClick();
#else
        OnSetFPS60Click();
#endif

        TiTle.text = string.Format("SupportsComputeShaders:{0}", SystemInfo.supportsComputeShaders);
        ViewCamera.localRotation = Quaternion.Euler(0, ViewSlider.value * 360, 0);
        m_DT6 = m_DT5 = m_DT4 = m_DT3 = m_DT2 = m_DT1 = m_CurDT = 166;
        m_FPS = 60;

        RawImg.texture = GrassChunk.HizDepthTexture;
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
        int fps = (int)(70000.0f / (m_CurDT + m_DT1 + m_DT2 + m_DT3 + m_DT4 + m_DT5 + m_DT6) + 0.2f);  //0.8进1
        if (m_FPS != fps)
        {
            m_FPS = fps;
            FPSValue.text = m_FPS.ToString();
        }
    }

    public void OnViewSliderChange(float t)
    {
        ViewCamera.localRotation = Quaternion.Euler(0, t * 360, 0);
    }

    private void OnDestroy()
    {
    }

    public void OnSetFPS30Click()
    {
        Application.targetFrameRate = 30;
        SetFPS30.interactable = false;
        SetFPS60.interactable = true;
        SetFPSMax.interactable = true;
    }

    public void OnSetFPS60Click()
    {
        Application.targetFrameRate = 60;
        SetFPS30.interactable = true;
        SetFPS60.interactable = false;
        SetFPSMax.interactable = true;
    }

    public void OnSetFPSMaxClick()
    {
        Application.targetFrameRate = 0;
        SetFPS30.interactable = true;
        SetFPS60.interactable = true;
        SetFPSMax.interactable = false;
    }
}
