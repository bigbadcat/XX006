using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XX006;

/// <summary>
/// 用于在摄像机渲染后更新深度纹理。
/// </summary>
public class UpdateDepth : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnPostRender()
    {
        GrassChunk.UpdateHizDepthTexture();
    }
}

