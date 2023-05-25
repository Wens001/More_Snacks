using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnackHeadUI : MonoBehaviour
{
    public Transform fllowObject;
    public Image countryImg;
    public Text snackName;
    public bool canFollow = true;
    // Start is called before the first frame update
    void Start()
    {
        var info = CountryManager.Instance.GetInfo(fllowObject.GetComponent<BaseControl>());
        countryImg.sprite = info.countryIcon;
        snackName.text = info.name;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canFollow)
            return;
        if (!fllowObject.gameObject.activeSelf)
            gameObject.SetActive(false);
        transform.position = fllowObject.position + Vector3.up * 1.3f;
    }
}
