using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CountryManager : MonoBehaviour
{
    private static CountryManager instance;
    public static CountryManager Instance {
        get {
            if (instance == null) {
                GameObject go = new GameObject("CountryManager");
                instance = go.AddComponent<CountryManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    public static Sprite[] AllCountrySp;
    public static string[] AllchildNameStrs = { "Cheems", "Tony", "Andy", "Frank", "Wang", "Li", "Smith", "Johnson", "Williams", "James", "Linda" };
    public struct PlayerInfo{
        public Sprite countryIcon;
        public string name;
    }
    //各人物的信息
    public Dictionary<BaseControl, PlayerInfo> PlayerInfoDict;
    //剩余的名字
    private List<string> remainChildNameStrs;
    //剩余的名字
    private List<Sprite> remainCountry;
    //记录各国家的人物控制器数
    private Dictionary<int, int> countrySpDict;
    private bool isInit = false;
    private void Awake()
    {
        instance = this;
        Init();
    }

    void Init() {
        if (isInit)
            return;
        if (AllCountrySp == null)
        {
            AllCountrySp = Resources.LoadAll<Sprite>("UI/CountryIcon");
        }
        countrySpDict = new Dictionary<int, int>();
        remainChildNameStrs = AllchildNameStrs.ToList();
        remainCountry = AllCountrySp.ToList();
        PlayerInfoDict = new Dictionary<BaseControl, PlayerInfo>();
        isInit = true;
    }

    public PlayerInfo GetInfo(BaseControl _control,int _defaultCountry=-1) {
        Init();
        if (!PlayerInfoDict.ContainsKey(_control)) {
            PlayerInfo info = new PlayerInfo();
            //国家
            var choseCountryId=0;
            if (_defaultCountry >= 0 && _defaultCountry <= (int)Country.Usa)
            {
                choseCountryId = _defaultCountry;
            }
            else {
                choseCountryId = Random.Range(0, remainCountry.Count);
            }
            info.countryIcon = remainCountry[choseCountryId];
            if (!countrySpDict.ContainsKey(choseCountryId)) {
                countrySpDict[choseCountryId] = 0;
            }
            countrySpDict[choseCountryId]++;
            if (countrySpDict[choseCountryId] >= 2)
                remainCountry.Remove(info.countryIcon);
            //名字
            var name = remainChildNameStrs[Random.Range(0, remainChildNameStrs.Count)];
            info.name = name;
            remainChildNameStrs.Remove(name);
            if (_defaultCountry >= 0) {
                info.name = "You";
            }
            PlayerInfoDict[_control] = info;
        }
        return PlayerInfoDict[_control];
    }    
}

public enum Country { 
China,
France,
Germany,
Japan,
Korea,
Kingdom,
Usa
}
