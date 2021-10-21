using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleCombine : MonoBehaviour
{
    public Transform BoneRoot;

    public string MeshSavePath = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        RefreshPart();

        Animator animator = this.GetComponent<Animator>();
        
    }

    public void Hit()
    {

    }

    void RefreshPart()
    {
        GameObject combine_obj = new GameObject("CombineSkin");
        combine_obj.transform.SetParent(this.transform);
        combine_obj.transform.localPosition = Vector3.zero;
        combine_obj.transform.localScale = Vector3.one;
        combine_obj.transform.localRotation = Quaternion.Euler(Vector3.zero);

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<Transform> combine_bones = new List<Transform>();
        SkinnedMeshRenderer[] part_renders = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var part_render in part_renders)
        {
            //part_render.transform.parent.gameObject.SetActive(false);
            Transform[] bones = part_render.bones;
            Transform[] new_bones = new Transform[bones.Length];
            for (int i=0; i<bones.Length; ++i)
            {
                new_bones[i] = GetChild(BoneRoot, bones[i].name);
            }
            part_render.bones = new_bones;
            combine_bones.AddRange(new_bones);

            for (int sub = 0; sub < part_render.sharedMesh.subMeshCount; sub++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = part_render.sharedMesh;
                ci.subMeshIndex = sub;
                combineInstances.Add(ci);
            }
            materials.Add(part_render.sharedMaterial);
            GameObject.Destroy(part_render.gameObject);
        }

        SkinnedMeshRenderer combine_render = combine_obj.AddComponent<SkinnedMeshRenderer>();
        Mesh combine_mesh = new Mesh();
        combine_mesh.name = "combine_mesh";
        combine_mesh.CombineMeshes(combineInstances.ToArray(), true, false);
        combine_render.sharedMesh = combine_mesh;
        //combine_render.materials = materials.ToArray();
        combine_render.sharedMaterial = materials[0];
        combine_render.rootBone = BoneRoot;
        combine_render.bones = combine_bones.ToArray();



        //UnityEditor.AssetDatabase.CreateAsset(combine_mesh, "Assets/ResourcesEx/Actor/Sword/FullMesh.asset");
        //UnityEditor.PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/ResourcesEx/Actor/Sword/Pack.prefab");

    }

    Transform GetChild(Transform p, string name)
    {
        if (string.CompareOrdinal(p.name, name) == 0)
        {
            return p;
        }

        int count = p.childCount;
        for (int i=0; i<count; ++i)
        {
            Transform child = GetChild(p.GetChild(i), name);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
