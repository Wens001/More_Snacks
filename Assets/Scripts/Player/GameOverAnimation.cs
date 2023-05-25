using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverAnimation : MonoBehaviour
{

    public GameObject sceneGround;
    private GameObject Root;
    public GameObject Quad;
    public Transform CameraRoot;
    public Transform MainCamera;

    private void Awake()
    {
        Root = transform.Find("Root").gameObject;
        Root.SetActive(false);
    }


    public void OnGameOver()
    {
        Quad.SetActive(false);
        this.AttachTimer(1f,()=>
        {
            MainCamera.SetParent(CameraRoot);
            MainCamera.localPosition = Vector3.zero;
            MainCamera.localRotation = Quaternion.identity;
            sceneGround.SetActive(false);
            Root.SetActive(true);
            this.AttachTimer(2.1f, () => Quad.SetActive(true));
        });
    }



}
