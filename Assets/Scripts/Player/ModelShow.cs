using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VLB;

public class ModelShow : MonoBehaviour
{
    public Transform handRoot;
    public Transform mouseRoot;
    private Transform model;
    private Animator anim;
    private SkinnedMeshRenderer bodyRenderer;
    private Camera cam;
    private Transform AITra;
    private Animator AIAnim;
    private Transform buff;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        model = transform.Find("Model");
        bodyRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
        cam = transform.GetComponentInChildren<Camera>(true);
        if (handRoot != null) {
            foreach (Transform child in handRoot)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.shadowCastingMode = ShadowCastingMode.Off;
            bodyRenderer.receiveShadows = false;
        }
    }


    [ContextMenu("SetHandRootHideWeapom")]
    public void SetHandRootHideWeapom() {
        FindAndSetRoot(transform, "Root");
    }

    public void FindAndSetRoot(Transform _parent, string _parentName) {
        foreach (Transform child in _parent)
        {
            if (child.name == _parentName)
            {
                handRoot = child;
                foreach (Transform weapon in child) {
                    weapon.gameObject.SetActive(false);
                }
                return;
            }
            else
            {
                FindAndSetRoot(child, _parentName);
            }
        }
    }

    public static ModelShow GetModelShow(ChildOrSnack roleType,int _id) {
        GameObject go = Instantiate(Resources.Load<GameObject>($"ModelShow/{roleType}{_id}"));
        ModelShow ms = go.GetComponent<ModelShow>();
        return ms;
    }

    //身体换shader，原shader有透视效果
    void ChangeNoRayShader() {
        bodyRenderer.materials[0].shader = Shader.Find("Matcap");
    }

    void ChangeWeapon(EWeapon weapon) {
        if (weapon <= EWeapon.Wrench) {
            foreach (Transform child in handRoot) {
                child.gameObject.SetActive(false);
            }
            GameObject wea_obj = Instantiate(Resources.Load<GameObject>("Weapon/" + weapon.ToString()));
            wea_obj.transform.SetParent(handRoot);
            wea_obj.transform.localPosition = new Vector3(-0.052f,0.049f,0);
            wea_obj.transform.localEulerAngles = new Vector3(-80, 158.4f, 36);
        }
        else
        {
            Debug.LogWarning("不是小孩武器");
        }
    }

    public void ShowPropAnim(EWeapon weapon) {
        if (weapon < EWeapon.Pepper) {
            ShowChildUsePropAnim(weapon);
        }
        else
        {
            ShowSnackUsePropAnim(weapon);
        }
        ChangeNoRayShader();
    }

    //展示小孩武器
    void ShowChildUsePropAnim(EWeapon weapon) {
        ChangeWeapon(weapon);
        if (weapon > EWeapon.Wrench)
        {
            return;
        }
        model.localPosition = new Vector3(0.5f, 0, 0);
        model.localEulerAngles = new Vector3(0, 326, 0);
        GameObject parent = Instantiate(Resources.Load<GameObject>("ModelShow/Parent"), transform);
        AITra = parent.transform;
        AITra.localPosition = new Vector3(-0.6f, 0, 0);
        AITra.localEulerAngles = Vector3.zero;
        cam.transform.localPosition = new Vector3(0, 1.17f, 2.1f);
        cam.orthographicSize = 1.5f;
        AIAnim = parent.GetComponentInChildren<Animator>();
        cam.gameObject.SetActive(true);
        StartCoroutine(AttackAnim());
    }
    IEnumerator AttackAnim()
    {
        WaitForSeconds hitTime = new WaitForSeconds(0.4f);
        WaitForSeconds VertigoTime = new WaitForSeconds(2);
        GameObject effect = AITra.GetComponentInChildren<ParticleSystem>(true).gameObject;
        ChangeEffectShader(effect.transform);
        while (true) {
            anim.SetTrigger("HammerAttack");
            yield return hitTime;
            if (AIAnim)
            {
                effect.SetActive(true);
                AIAnim.SetTrigger("Vertigo");
                yield return VertigoTime;
                AIAnim.SetTrigger("ToNormal");
                effect.SetActive(false);
                yield return hitTime;
            }                     
        }
    }

    //展示零食BUFF
    void ShowSnackUsePropAnim(EWeapon weapon) {
        if (weapon < EWeapon.Pepper) {
            Debug.LogWarning("不是零食buff");
            return;
        }
        GameObject parent = Instantiate(Resources.Load<GameObject>("ModelShow/Child0"),transform);
        AITra = parent.transform;      
        AIAnim = parent.GetComponentInChildren<Animator>();
        buff = Instantiate(Resources.Load<GameObject>("Weapon/"+ weapon.ToString())).transform;
        buff.SetParent(transform);       
        cam.transform.localPosition = new Vector3(0, .9f, 2.1f);
        cam.orthographic = false;
        cam.gameObject.SetActive(true);
        StartCoroutine(SnackBuffAnim(weapon));
    }

    IEnumerator SnackBuffAnim(EWeapon weapon)
    {
        WaitForSeconds hitTime = new WaitForSeconds(0.4f);
        WaitForSeconds VertigoTime = new WaitForSeconds(2);
        GameObject effect = null;
        Vector3 snackStartPos = Vector3.zero;
        Vector3 childStartPos = Vector3.zero;
        Vector3 snackMovePos = Vector3.zero;
        Vector3 childMovePos = Vector3.zero;
        Texture normalTex = bodyRenderer.materials[0].GetTexture("_DiffuseTex");
        Texture dirtyTex = Resources.Load<Texture>("UI/Donut_dirty");
        //初始化位置什么的
        if (weapon == EWeapon.Pepper)
        {
            buff.localPosition = new Vector3(-0.4f, 0.4f, 0.22f);
            var buffPos = buff.localPosition;
            buffPos.y = 0;
            //小孩，零食起始点
            snackStartPos = new Vector3(0.3f, 0, -0.8f);
            childStartPos = new Vector3(1, 0, -2);
            snackMovePos = buffPos;
            childMovePos = buffPos + new Vector3(0.3f, 0, -0.3f);
            //初始化喷火特效
            effect = Instantiate(Resources.Load<GameObject>("FireEffect"));
            mouseRoot = AITra.GetComponent<ModelShow>().mouseRoot;
            effect.transform.SetParent(mouseRoot);
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = Quaternion.identity;
        }
        else
        {
            buff.localPosition = new Vector3(0, 0, -1);
            snackStartPos = new Vector3(-0.4f, 0, 0.4f);
            childStartPos = new Vector3(0.9f, 0, 0);
            snackMovePos = buff.localPosition;
            childMovePos = snackStartPos;           
            effect = AITra.Find("EmojiPos").gameObject;
        }
        //处理特效
        effect.gameObject.SetActive(false);
        ChangeEffectShader(effect.transform);
        //循环播放
        while (true)
        {
            buff.gameObject.SetActive(true);
            model.transform.localPosition = snackStartPos;
            Vector3 dir = snackMovePos - snackStartPos;
            model.transform.rotation = Quaternion.LookRotation(dir);
            AITra.localPosition = childStartPos;
            AITra.rotation = Quaternion.LookRotation(snackStartPos - childStartPos);
            if (weapon == EWeapon.Pepper)
            {
                anim.SetFloat("Speed", 1);
                //零食去BUFF
                model.DOLocalMove(snackMovePos, 1).SetEase(Ease.Linear).onComplete = () =>
                {
                    buff.gameObject.SetActive(false);
                    anim.SetFloat("Speed", 0);
                    bodyRenderer.materials[0].SetColor("_DiffuseColor", Color.red);
                };
                AIAnim.SetFloat("Speed", 1);
                //小孩追
                AITra.DOLocalMove(childMovePos, 2).SetEase(Ease.Linear).
                    OnUpdate(() => { AITra.rotation = AITra.transform.rotation = Quaternion.LookRotation(model.position - AITra.position); }).
                    onComplete = () =>
                    {
                        AIAnim.SetFloat("Speed", 0);
                        bodyRenderer.materials[0].SetColor("_DiffuseColor", Color.white);
                        AIAnim.SetTrigger("EatPepper");
                        effect.SetActive(true);

                    };
                yield return VertigoTime;
                anim.SetFloat("Speed", 1);
                model.transform.rotation = Quaternion.LookRotation(Vector3.left);
                model.DOLocalMoveX(-2, 1);
                yield return VertigoTime;
                effect.SetActive(false);               
            }
            else
            {
                anim.SetTrigger("Jump");
                yield return VertigoTime;
                bodyRenderer.materials[0].SetTexture("_DiffuseTex", dirtyTex);
                yield return new WaitForSeconds(2.7f);
                AIAnim.SetFloat("Speed", 1);
                AITra.DOLocalMove(childMovePos, 1).SetEase(Ease.Linear).
                    OnUpdate(() => { AITra.rotation = AITra.transform.rotation = Quaternion.LookRotation(model.position - AITra.position); }).
                    onComplete = () =>
                    {
                        AIAnim.SetFloat("Speed", 0);
                        bodyRenderer.materials[0].SetTexture("_DiffuseTex", normalTex);
                        AIAnim.SetTrigger("Vomit");
                        effect.SetActive(true);
                    };
                yield return new WaitForSeconds(1);
                anim.SetFloat("Speed", 1);
                model.transform.rotation = Quaternion.LookRotation(Vector3.left);
                model.DOLocalMoveX(-2, 1);
                yield return new WaitForSeconds(2.5f);
                effect.SetActive(false);
            }
        }
    }

    void ChangeEffectShader(Transform _effect) {
        var shaderScript = _effect.GetComponent<ChangeShader>();
        if (shaderScript) {
            shaderScript.ChangeTargetShader();
        }

        //Transform[] temp = _effect.GetComponentsInChildren<Transform>();
        //foreach (Transform child in temp)
        //{
        //    child.gameObject.layer = LayerMask.NameToLayer("Player");
        //    var rend = child.GetComponent<Renderer>();
        //    if (rend)
        //        rend.material.shader = Shader.Find("Mobile/Particles/Alpha Blended");
        //}
    }

    //斗兽场展示动画
    public void ShowCoinLvAnim() {
        GameObject parent = Instantiate(Resources.Load<GameObject>("ModelShow/Child0"), transform);
        AITra = parent.transform;
        AIAnim = parent.GetComponentInChildren<Animator>();
        cam.transform.localPosition = new Vector3(0, .9f, 2.1f);
        cam.orthographic = false;
        cam.gameObject.SetActive(true);
        model.localPosition = new Vector3(-0.3f, 0.5f, 1.06f);
        model.localEulerAngles = new Vector3(0, -24.1f, 0);
        anim.SetFloat("Speed", 1);
        AITra.localPosition = new Vector3(0.64f, 0.11f, 0);
        AITra.localEulerAngles = new Vector3(0, -41.6f, 0);
        AIAnim.SetFloat("Speed", 1);
    }
    //肌肉零食展现动画
    Tween scaleTween;
    Tween strongTween;
    public void ShowMuscleSnack() {
        StartCoroutine(MuscleSnackStrongAiam());
    }

    IEnumerator MuscleSnackStrongAiam() {
        cam.gameObject.SetActive(true);
        var bocaiAnim = transform.Find("Spinach").GetComponent<Animator>();
        WaitForSeconds delaySecond = new WaitForSeconds(3.7f);
        while (true) {
            transform.localScale = Vector3.one;
            bodyRenderer.SetBlendShapeWeight(0, 0);
            anim.SetTrigger("Strong");
            bocaiAnim.SetTrigger("Eat");
            yield return delaySecond;
            float strongV = 0;

            scaleTween = DOTween.To(() => transform.localScale, x => transform.localScale = x, new Vector3(2.2f, 2.2f, 2.2f), 0.4f);
            strongTween = DOTween.To(() => strongV, x => strongV = x, 36.5f, 0.3f);
            strongTween.OnUpdate(() =>
            {
                bodyRenderer.SetBlendShapeWeight(0, strongV);
            });
            yield return delaySecond;
        }
    }

    private void OnDisable()
    {
        if (scaleTween != null) {
            scaleTween.Kill();
        }
        if (strongTween != null)
        {
            strongTween.Kill();
        }
    }
}
