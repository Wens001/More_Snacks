using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTransparent : MonoBehaviour
{

    public BaseControl childPlayer { get { return GameManager.Instance.childPlayer; } }

    private Collider targetWall;
    private Vector3 ChildPos {
        get { return childPlayer.transform.position
                + Vector3.up * .25f; } }
    private int layerMask;

    void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Wall");
    }

    private void ClearTargetWallTrans()
    {
        if (!targetWall)
            return;
        targetWall.TryGetComponent(out MeshRenderer mr);
        SetWallAlpha(mr,1);
        targetWall = null;
    }

    private void SetWallAlpha(MeshRenderer mr,float value)
    {
        for (int i = 0; i < mr.materials.Length; i++)
        {
            Color color = Color.white;
            if (mr.sharedMaterials[i].HasProperty(ConstDefine.Mater._MainColor))
                color = mr.sharedMaterials[i].GetColor(ConstDefine.Mater._MainColor);
            else
                color = mr.sharedMaterials[i].GetColor("_Color");
            color.a = value;
            if (mr.sharedMaterials[i].HasProperty(ConstDefine.Mater._MainColor))
                mr.sharedMaterials[i].SetColor(ConstDefine.Mater._MainColor, color);
            else
                mr.sharedMaterials[i].SetColor("_Color", color);
        }
    }

    private const float minAlpha = .5f;
    private const float Distance = 1.25f;
    void Update()
    {
        if (targetWall && targetWall.TryGetComponent(out MeshRenderer mr))
        {
            float alpha = 1;
            if (mr.sharedMaterial.HasProperty(ConstDefine.Mater._MainColor))
                alpha = mr.material.GetColor(ConstDefine.Mater._MainColor).a;
            else
                alpha = mr.sharedMaterial.GetColor("_Color").a;
            alpha = Mathf.Lerp(alpha, minAlpha, Time.deltaTime * 10);
            SetWallAlpha(mr, alpha);
        }

        if (!childPlayer)
            return;
        Ray ray = new Ray(ChildPos + .1f * Vector3.up, Vector3.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, Distance, layerMask))
        {
            if (hit.collider != targetWall)
            {
                ClearTargetWallTrans();
                targetWall = hit.collider;
            }
        }
        else
        {
            ClearTargetWallTrans();
        }
    }

    private void OnDrawGizmos()
    {
        if ( !Application.isPlaying || !GameManager.Instance  || !childPlayer )
            return;
        Gizmos.DrawLine(ChildPos , ChildPos + Vector3.forward * Distance);
    }


}
