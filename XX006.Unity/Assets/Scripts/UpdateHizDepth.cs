using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XX006
{
    /// <summary>
    /// 用于在摄像机渲染后更新深度纹理。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class UpdateHizDepth : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        }

        private void OnPostRender()
        {
            HizManager.Instance.UpdateDepthTexture();
        }
    }
}