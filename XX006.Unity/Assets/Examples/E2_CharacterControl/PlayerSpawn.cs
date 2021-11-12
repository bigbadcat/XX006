using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject PlayerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefab == null)
        {
            return;
        }

        GameObject obj = Instantiate(PlayerPrefab) as GameObject;
        obj.transform.SetParent(this.transform.parent);
        obj.transform.localPosition = this.transform.localPosition;
        obj.transform.localScale = this.transform.localScale;
        obj.transform.localRotation = this.transform.localRotation;
        Destroy(this.gameObject);
    }
}
