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
    }

    public void OnScale(float d)
    {
        CameraController.CurCamera?.ChangeZoom(-d);
    }

    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
    Application.targetFrameRate = 60;
#endif

        this.GetComponentInChildren<TouchReceiver>().Processer = this;

        
    }
}