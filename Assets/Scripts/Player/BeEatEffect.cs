using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VLB;

public class BeEatEffect : MonoBehaviour
{
    public bool OneEat;
    public GameObject[] bodyParts;
    public GameObject[] emojis;
    public GameObject[] emojis2;
    public GameObject deadEffect;
    private int index ;

    private void Start()
    {
        index = 0;
    }

    public void HidePart() {
        if (index >= 0 && index < bodyParts.Length) {
            bodyParts[index].SetActive(false);
            index++;
        }
    }

    Vector3[] showPos = { new Vector3(-0.45f, 0.76f, 0.38f), new Vector3(0.45f, 0.76f, 0.38f), new Vector3(0, 0.85f, 1.12f) };
    public void ShowEmoji(Transform _parent,Vector3[] _localPoss, float _showTime=2) {
        if (_localPoss == null)
            return;
        int choseIndex;
        List<Vector3> showPosList = _localPoss.ToList();
        choseIndex = Random.Range(0, showPosList.Count);
        ShowEffect(emojis[Random.Range(0, emojis.Length)], _parent, showPosList[choseIndex], _showTime);
        showPosList.RemoveAt(choseIndex);
        choseIndex = Random.Range(0, showPosList.Count);
        ShowEffect(emojis[Random.Range(0, emojis.Length)], _parent, showPosList[choseIndex], _showTime);
        showPosList = null;
    }
    public void ShowEmoji2(Transform _parent, Vector3[] _localPoss, float _showTime = 2)
    {
        if (_localPoss == null)
            return;
        int choseIndex;
        List<Vector3> showPosList = _localPoss.ToList();
        choseIndex = Random.Range(0, showPosList.Count);
        ShowEffect(emojis2[Random.Range(0, emojis2.Length)], _parent, showPosList[choseIndex], _showTime);
        showPosList.RemoveAt(choseIndex);
        choseIndex = Random.Range(0, showPosList.Count);
        ShowEffect(emojis2[Random.Range(0, emojis2.Length)], _parent, showPosList[choseIndex], _showTime);
        showPosList = null;
    }
    public void ShowDeadEffect(Transform _parent, Vector3 _localPos, float _showTime=2)
    {
        ShowEffect(deadEffect, _parent, _localPos, _showTime);
    }

    void ShowEffect(GameObject _effect, Transform _parent, Vector3 _localPos, float _showTime) {
        if (_effect == null)
            return;
        GameObject go = Instantiate(_effect, _parent);
        go.transform.localPosition = _localPos;
        if(_showTime>=0)
            this.AttachTimer(_showTime, () => { Destroy(go); });
    }

    public bool IsLastPart() {
        if (index == bodyParts.Length-1) {
            return true;
        }
        return false;
    }
}
