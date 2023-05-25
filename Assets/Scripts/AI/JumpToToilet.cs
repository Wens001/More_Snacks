using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpToToilet : MonoBehaviour
{
    protected SnackControl control;
    public Transform[] toilets;
    public Transform jumpPoint;
    public float UpSpeed = 2.4f;
    public float MaxH = 2;
    private float addA;

    // Start is called before the first frame update
    void Start()
    {
        var toiletObjs = GameObject.FindGameObjectsWithTag("Toilet");
        toilets = new Transform[toiletObjs.Length];
        for (int i = 0; i < toilets.Length; i++) {
            toilets[i] = toiletObjs[i].transform;
        }
        TryGetComponent(out control);
        if(control && toilets.Length>0)
            StartCoroutine(FindNearToilet());
    }


    IEnumerator FindNearToilet() {
        Vector3 dir;
        float dis = (transform.lossyScale.x - 1) * 1.55f + 1.5f;
        while (!GameManager.Instance.IsGameOver && control.IsSnack && !control.IsDirty ) {
            if (!control.IsHiding)
            {
                for (int i = 0; i < toilets.Length; i++) {
                    if (toilets[i] !=null && Vector3.Distance(transform.position, toilets[i].position) < dis) {
                        dir = toilets[i].position - transform.position;
                        dir.y = 0;
                        Transform target = null;
                        GameObject camTemp = new GameObject();
                        FllowObject fllowObject = null;
                        if (control.IsLocalPlayer) {
                            camTemp.transform.position = transform.position;
                            JoystickPanel.Instance.gameObject.SetActive(false);
                            fllowObject = FindObjectOfType<FllowObject>();                           
                            if (fllowObject)
                            {
                                target = fllowObject.target;
                                fllowObject.target = camTemp.transform;
                                camTemp.transform.DOMove(toilets[i].position,1).onComplete=()=> {
                                    camTemp.transform.DOMove(transform.position, 0.5f).SetDelay(2.2f).onComplete = () =>
                                    {
                                        fllowObject.target = target;
                                    };
                                };
                            }
                        }
                        
                        control.IsInvincible = true;
                        control.agent.enabled = false;
                        transform.rotation = Quaternion.LookRotation(dir);
                        transform.position = toilets[i].position - dir.normalized * dis;                                              
                        control.SetJumpAnim();
                        this.AttachTimer(2, () =>
                        {
                            //control.IsDirty = true;
                            control.PlayerHealth++;
                            control.SetDirtyMat();                          
                        });
                        this.AttachTimer(4.7f, () =>
                        {
                            control.IsInvincible = false;
                            control.agent.enabled = true;
                            if (target) {
                                JoystickPanel.Instance.gameObject.SetActive(true);
                                fllowObject.target = target;
                            }
                            BeEatEffect effect = control.GetComponent<BeEatEffect>();
                            if (effect) {
                                effect.ShowDeadEffect(transform, new Vector3(0,0.74f,0.43f), -1);
                            }
                        });
                                        
                        yield break;
                    }
                    yield return null;
                }
            }
            yield return null;
        }
    }

//    private void Update()
//    {
//#if UNITY_EDITOR
//        if (Input.GetKeyDown(KeyCode.J)) {
//            JumpToPoint();
//        }
//#endif
//    }

    void JumpToPoint() {
        if (jumpPoint != null) {
            addA = UpSpeed * UpSpeed / (2 * MaxH);
            Debug.Log("addA:" + addA);           
            float allTime = UpSpeed / addA;
            Debug.Log("T:" + allTime);
            allTime += Mathf.Sqrt((MaxH - jumpPoint.position.y) * 2 / addA);
            Debug.Log("allTime:" + allTime);
            StartCoroutine(JumpAnim(allTime));
        }
    }

    IEnumerator JumpAnim(float _allTime) {
        //实时竖直速度
        float v = UpSpeed;
        //初始竖直速度
        float startSpeed = UpSpeed;
        //初始点
        float startPosY = transform.position.y;
        Vector3 pos = transform.position;
        var toiletPos = jumpPoint.position;
        toiletPos.y = 0;
        var dis = Vector3.Distance(toiletPos, transform.position);
        //Debug.Log("dis:" + dis);
        float h_v = dis / _allTime;
        //Debug.Log("h_v:" + h_v);
        var dir= (toiletPos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
        for (float t = 0; t < _allTime; t += Time.deltaTime) {
            v -= addA * Time.deltaTime;
            pos.y= startPosY+ startSpeed * t - 0.5f * addA * t * t;
            pos += dir * h_v * Time.deltaTime;
            transform.position = pos;
            yield return null;
        }

        float rotateSp = 10*360/2;
        for (float t = 0; t < 2; t += Time.deltaTime) {
            transform.Rotate(Vector3.up, rotateSp * Time.deltaTime);
            yield return null;
        }
        //Debug.Log("v:"+v);
        startSpeed = -v;
        startPosY = transform.position.y;
        transform.rotation = Quaternion.LookRotation(dir);
        for (float t = 0; t < _allTime; t += Time.deltaTime)
        {
            v -= addA * t;
            pos.y = startPosY + startSpeed * t - 0.5f * addA * t * t;
            pos -= dir * h_v * Time.deltaTime;
            transform.position = pos;
            yield return null;
        }
    }
}
