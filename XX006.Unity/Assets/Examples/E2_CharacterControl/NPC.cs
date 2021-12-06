using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        XuXiang.Avatar avater = GetComponent<XuXiang.Avatar>();
        if (avater != null)
        {
            avater.PlayAnimation("stand");
        }
    }
}
