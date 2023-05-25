using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

public class Localization
{
    private static List<LocalizedText> Texts ;

    private static Dictionary<string, Dictionary<string, string>> _phrases;
    //language,key,value

    private static string _language;


    public static string CurrentLanguage
    {
        get
        {
            return _language;
        }
    }

    private static void Init()
    {
        _phrases = new Dictionary<string, Dictionary<string, string>>(0);


        TextAsset file = AssetsLoader.Load<TextAsset>("Localization");
        //获取全部行
        List<string> AllRows = new List<string>(0);
        byte[] array = Encoding.UTF8.GetBytes(file.text);
        MemoryStream stream = new MemoryStream(array);
        StreamReader sr = new StreamReader(stream, Encoding.Default);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            AllRows.Add(line);
        }
        sr.Close();

        string pattern = ",(?=(?:[^\\" + '"' + "]*\\" + '"' + "[^\\" + '"' + "]*\\" + '"' + ")*[^\\" + '"' + "]*$)";

        //获取语言
        var header = Regex.Split(AllRows[0], pattern);

        for (int i = 1; i < header.Length; i++)
        {
            //某语言
            string lang = _getExactValue(header[i]).ToLower();
            //获取key和value
            Dictionary<string, string> _p = new Dictionary<string, string>();
            for (int j = 1; j < AllRows.Count; j++)
            {
                //某一行
                string row = AllRows[j];
                //切割
                var cells = Regex.Split(row, pattern);
                //添加key-value
                _p.Add(_getExactValue(cells[0]), _getExactValue(cells[i]));
            }
            _phrases.Add(lang, _p);
        }
        ChangeLanguage(PlayerPrefs.GetString("Localization.language",
            System.Globalization.CultureInfo.InstalledUICulture.Name));
    }

    private static string _getExactValue(string val)
    {
        if (val[0] == '"' && val[val.Length - 1] == '"')
        {
            val = val.Substring(1, val.Length - 2);
        }

        char p = '"';
        string pattern = p.ToString() + p.ToString();
        string p2 = p + "";
        val = val.Replace(pattern, p2);
        return val;
    }

    /// <summary>
    /// 更改语言
    /// Change language
    /// </summary>
    /// <param name="lang"></param>
    public static void ChangeLanguage(string lang)
    {
        var newlang = lang.ToLower();
        if (_language == newlang)
            return;
        _language = newlang;
        PlayerPrefs.SetString("Localization.language", _language);

        foreach (var text in Texts)
            text?.ChangeLanguage();
    }

    public static void AddText(LocalizedText lt)
    {
        if (Texts == null)
            Texts = new List<LocalizedText>();
        if (!Texts.Contains(lt))
            Texts.Add(lt);
    }

    public static void RemoveText(LocalizedText lt)
    {
        if (Texts == null)
            Texts = new List<LocalizedText>();
        if (Texts.Contains(lt))
            Texts.Remove(lt);
    }

    /// <summary>
    /// 获取key的文字
    /// Find string of key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetString(string key)
    {
        if (_phrases == null)
        {
            Init();
        }

        if (!_phrases.ContainsKey(_language))
        {
            string newLang = _phrases.Keys.ToList().Find(k => k.Split('-')[0] == _language.Split('-')[0]);
            if (newLang == null)
            {
                newLang = "zh-cn";
            }
            Debug.LogError($"不存在语言{_language}，自动替换为{newLang}");
            ChangeLanguage(newLang);
        }

        if (!_phrases[_language].ContainsKey(key))
        {
            return $"[invalid key: {key}]";
        }

        return _phrases[_language][key];
    }
}