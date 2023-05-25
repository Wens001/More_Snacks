using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSnack : MonoBehaviour
{
    public GameObject[] snack1s;
    public GameObject[] snack2s;
    public float buildY;
    public Vector2 buildXZ;
    public int SnackNum = 60;
    private List<GameObject> snackList;
    public Vector3 camPos;
    public Vector3 camEuler;
    public Transform cam;
    private Transform root;
    // Start is called before the first frame update
    void Start()
    {
        root = new GameObject("SnackRoot").transform;
        if (cam == null) {
            cam = Camera.main.transform;
        }
        snackList = new List<GameObject>();
        //SetPrefabMat();
    }
    private void OnEnable()
    {
        Physics.gravity = new Vector3(0, -5, 0);
    }

    void SetPrefabMat() {
        int snacksLength = snack1s.Length;
        for (int i = 0; i < snacksLength; i++) {
            Renderer[] renders = snack1s[i].GetComponentsInChildren<Renderer>();
            foreach (var child in renders) {
                foreach (var mat in child.materials) {
                    if (mat.shader.name == "Matcap_XRay") {
                        mat.shader = Shader.Find("Matcap");
                    }
                }
            }
            Debug.Log("change mat");
        }
    }

    // Update is called once per frame
    void Update()
    {
        cam.position = camPos;
        cam.eulerAngles = camEuler;
        if (Input.GetMouseButtonDown(0)) {

            StartCoroutine(BuildSnack(snack1s,SnackNum));
        }
        if (Input.GetMouseButtonDown(1))
        {

            StartCoroutine(BuildSnack(snack2s, SnackNum));
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            var length = snackList.Count;
            for (int i = 0; i < length; i++) {
                Destroy(snackList[0]);
                snackList.RemoveAt(0);
            }
        }
    }
    IEnumerator BuildSnack(GameObject[] objs,int num) {       
        for (int i = 0; i < num; i++) {
            Vector3 build = transform.position + new Vector3(Random.Range(-buildXZ.x, buildXZ.x), buildY, Random.Range(-buildXZ.y, buildXZ.y));
            var prefab = objs[Random.Range(0, objs.Length)];
            GameObject snack = Instantiate(prefab, build,Random.rotation,root);
            snackList.Add(snack);
            snack.SetActive(true);
            if(i%4==0)
                yield return null;
        }
    }
    private void OnDrawGizmos()
    {
        Vector3 pos1 = transform.position+ new Vector3(-buildXZ.x, buildY, buildXZ.y);
        Vector3 pos2 = transform.position + new Vector3(buildXZ.x, buildY, buildXZ.y);
        Vector3 pos3 = transform.position + new Vector3(buildXZ.x, buildY, -buildXZ.y);
        Vector3 pos4 = transform.position + new Vector3(-buildXZ.x, buildY, -buildXZ.y);
        Debug.DrawLine(pos1, pos2, Color.green);
        Debug.DrawLine(pos2, pos3, Color.green);
        Debug.DrawLine(pos3, pos4, Color.green);
        Debug.DrawLine(pos4, pos1, Color.green);
    }
}
