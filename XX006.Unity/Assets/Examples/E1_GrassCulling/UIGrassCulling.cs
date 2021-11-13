using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XX006;

public class UIGrassCulling : MonoBehaviour
{
    public RawImage RawImg;
    public TMPro.TextMeshProUGUI TiTle;

    public Button SetFPS30;
    public Button SetFPS60;
    public Button SetFPSMax;

    public Slider ViewSlider;
    public Transform ViewCamera;

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

        RawImg.texture = GrassChunk.HizDepthTexture;
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
