using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;
using XX006;

public class UICharacterControl : MonoBehaviour, TouchReceiver.ITouchProcesser
{
    public void OnClick(float x, float y)
    {
    }

    public void OnMove(float dx, float dy)
    {
    }

    public void OnRotate(float dx, float dy)
    {
        CameraController.CurCamera?.ChangeRotate(-dx);
        CameraController.CurCamera?.ChangePitch(-dy);

        //m_RPD.x -= dx;
        //m_RPD.y = Mathf.Clamp(m_RPD.y - dy, 10, 80);
        //RotateText.text = m_RPD.x.ToString("N1");
        //PitchText.text = m_RPD.y.ToString("N1");
    }

    public void OnScale(float d)
    {
        CameraController.CurCamera?.ChangeZoom(-d);
        //m_RPD.z = Mathf.Clamp(m_RPD.z - d, 3, 12);
        //DistanceText.text = m_RPD.z.ToString("N1");
    }

    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
    Application.targetFrameRate = 60;
#endif

        this.GetComponentInChildren<TouchReceiver>().Processer = this;

        
    }

    //void LateUpdate()
    //{
    //    Vector3 pos = PositionTarget.position;
    //    PositionXText.text = pos.x.ToString("N1");
    //    PositionYText.text = pos.z.ToString("N1");
    //}

    public Transform PositionTarget;
    public TMPro.TextMeshProUGUI PositionXText;
    public TMPro.TextMeshProUGUI PositionYText;
    public TMPro.TextMeshProUGUI RotateText;
    public TMPro.TextMeshProUGUI PitchText;
    public TMPro.TextMeshProUGUI DistanceText;

    [SerializeField]
    private Vector3 m_RPD = new Vector3(0, 30, 4);
}
