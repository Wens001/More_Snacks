using UnityEngine;
using System.Collections;
using DG.Tweening;
[ExecuteInEditMode]
public class PostProcessing : Singleton<PostProcessing> {
    public Material mat;

    private void Awake()
    {
        mat.SetFloat("_ColorMul", 1);
    }

    public void Blink(float time )
    {
        mat.DOKill();
        mat.DOFloat(-1, "_ColorMul", time / 2).onComplete = 
            ()=> mat.DOFloat(1, "_ColorMul", time / 2);
    }

    //[ImageEffectOpaque] //在渲染完不透明物体后马上执行
    protected virtual void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(mat != null)
        {
            Graphics.Blit(src, dest, mat);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
