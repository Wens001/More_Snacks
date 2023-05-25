using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    public string key;
    private Text _text;

    private void Awake()
    {
        _text = gameObject.GetComponent<Text>();
    }

    public void ChangeLanguage()
    {
        _text.text = Localization.GetString(key);
    }

    private void OnEnable()
    {
        Localization.AddText(this);
        ChangeLanguage();
    }

    private void OnDisable()
    {
        Localization.RemoveText(this);
    }

}
