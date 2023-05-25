using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public class NavMeshSourceTag : MonoBehaviour
{
    public static List<MeshFilter> m_Meshes = new List<MeshFilter>();
    public static List<Terrain> m_Terrains = new List<Terrain>();

    public static List<NavMeshModifierVolume> 
        VolumeModifiers = new List<NavMeshModifierVolume>();

    public static NavMeshModifierVolume IsInVolumeRange(Vector3 pos,float _size=0)
    {
        foreach (var volume in VolumeModifiers)
        {
            if (volume.area == 1)
                continue;
            var volTra = volume.transform;
            var vpos = volTra.position + volume.center;
            //判断点是否在长方体内，如果长方体有旋转，可逆转思路，让点绕长方体中心逆向旋转。
            var direction = pos - vpos;
            var rotatedDirection = Quaternion.Euler(-volTra.eulerAngles) * direction;
            var rotatedPoint = rotatedDirection + vpos;
            //因为有时零食太靠障碍物边缘躲起来，看上去在外面，所以减去零食本身的包围盒长度，让零食尽量能在障碍物里躲起来
            var sizex = volume.size.x * volTra.lossyScale.x / 2 -_size;
            var sizez = volume.size.z * volTra.lossyScale.z / 2 - _size;
            
            if (vpos.x + sizex < rotatedPoint.x)
                continue;
            if (vpos.x - sizex > rotatedPoint.x)
                continue;
            if (vpos.z + sizez < rotatedPoint.z)
                continue;
            if (vpos.z - sizez > rotatedPoint.z)
                continue;
            return volume;
        }
        return null;
    }

    private MeshFilter[] mf;
    private NavMeshModifierVolume volumes;

    private void Awake()
    {
        mf = GetComponentsInChildren<MeshFilter>();
        volumes = GetComponent<NavMeshModifierVolume>();
        Init();
    }

    #region Object

    public float OnUpdateTime(Transform target)
    {
        if (enabled == false)
            return 0;
        timer += Time.deltaTime;
        return timer / MaxTimer;
    }

    public bool IsAtTime()
    {
        return timer >= MaxTimer;
    }

    private void Init()
    {
        if (volumes != null && volumes.area == 3)
        {
            parent = transform.parent.gameObject;
            coll = parent.GetComponent<Collider>();
            if (coll == null) {
                MeshCollider coll1 = parent.AddComponent<MeshCollider>();
                coll1.sharedMesh = parent.GetComponent<MeshFilter>().sharedMesh;
                coll1.convex = true;
                coll = coll1;
            }
            //coll = parent.AddComponent<MeshCollider>();
            //coll.sharedMesh = parent.GetComponent<MeshFilter>().sharedMesh;
            //coll.convex = true;
            parent.layer = TargetLayer;
        }
    }
    private static List<NavMeshModifierVolume> collList = new List<NavMeshModifierVolume>();

    public static Vector3? FindNearCollPosition(Vector3 pos )
    {
        Vector3? res = null ;
        float minDis = Mathf.Infinity;
        for (int i = 0; i < collList.Count; i++)
        {
            var dis = Vector3.Distance(pos, collList[i].transform.position);
            if (dis < minDis)
            {
                minDis = dis;
                res = collList[i].transform.position;
            }
        }
        return res;
    }

    public static Vector3? FindGreaterCollPosition(Vector3 pos , float min = 0)
    {
        Vector3? res = null;
        float maxDis = Mathf.Infinity;
        for (int i = 0; i < collList.Count; i++)
        {
            var dis = Vector3.Distance(pos, collList[i].transform.position);
            if (min < dis && dis < maxDis)
            {
                maxDis = dis;
                res = collList[i].transform.position;
            }
        }
        return res;
    }

    private float timer = 0;
    private float MaxTimer = 1.2f;
    private int TargetLayer = 1;
    private GameObject parent;
    //MeshCollider Collider
    private Collider coll;
    private Rigidbody rigi;
    [Range(0.25f, 100f)]
    public float Mass = 1;


    public Rigidbody AddForce(Vector3 force)
    {
        if (gameObject == null)
            return null;
        gameObject.layer = 0;
        if (rigi == null)
        {
            rigi = parent.GetComponent<Rigidbody>();
            if (rigi == null)
                rigi = parent.AddComponent<Rigidbody>();
            rigi.mass = Mass;
            StartCoroutine(DelayDisable());
        }
        rigi.AddForce(force);
        //rigi.AddTorque(force.normalized);
        enabled = false;
        return rigi;
    }

    IEnumerator DelayDisable()
    {
        if (nmmv && collList.Contains(nmmv))
            collList.Remove(nmmv);
        yield return new WaitForSeconds(2f);
        coll.enabled = false;
        yield return new WaitForSeconds(2);
        rigi.gameObject.SetActive(false);
    }

    #endregion

    private NavMeshModifierVolume _nmmv;
    private NavMeshModifierVolume nmmv
    {
        get
        {
            if (_nmmv == null)
                _nmmv = GetComponent<NavMeshModifierVolume>();
            return _nmmv;
        }
    }
    void OnEnable()
    {
        if (mf != null)
        {
            for (int i = 0; i < mf.Length; i++)
                m_Meshes.Add(mf[i]);
        }

        var t = GetComponent<Terrain>();
        if (t != null)
        {
            m_Terrains.Add(t);
        }


        if (volumes != null)
            VolumeModifiers.Add(volumes);
        
        if (nmmv && collList.Contains(nmmv) == false && nmmv.area != 1 )
            collList.Add(nmmv);
    }

    void OnDisable()
    {
        if (volumes != null)
            VolumeModifiers.Remove(volumes);
        if (mf != null)
        {
            for (int i = 0; i < mf.Length; i++)
                m_Meshes.Remove(mf[i]);
        }
        var t = GetComponent<Terrain>();
        if (t != null)
        {
            m_Terrains.Remove(t);
        }
        if (nmmv && collList.Contains(nmmv) )
            collList.Remove(nmmv);
    }

    public static void Collect(ref List<NavMeshBuildSource> sources)
    {
        sources.Clear();
        for (var i = 0; i < m_Meshes.Count; ++i)
        {
            var mf = m_Meshes[i];
            if (mf == null)
                continue;

            var m = mf.sharedMesh;
            if (m == null)
                continue;

            var s = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = m,
                transform = mf.transform.localToWorldMatrix,
                area = 0
            };
            sources.Add(s);
        }
    }

    //----------------------------------------------------------------------------------------
    public static void CollectModifierVolumes(ref List<NavMeshBuildSource> _sources)
    {

        for (var i = 0; i < m_Terrains.Count; ++i)
        {
            var t = m_Terrains[i];
            if (t == null) continue;

            var s = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Terrain,
                sourceObject = t.terrainData,
                // Terrain system only supports translation - so we pass translation only to back-end
                transform = Matrix4x4.TRS(t.transform.position, Quaternion.identity, Vector3.one),
                area = 0
            };
            _sources.Add(s);
        }

        foreach (var m in VolumeModifiers)
        {
            var mcenter = m.transform.TransformPoint(m.center);
            var scale = m.transform.lossyScale;
            var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

            var src = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.ModifierBox,
                transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one),
                size = msize,
                area = m.area,

            };

            _sources.Add(src);
        }
    }
}
