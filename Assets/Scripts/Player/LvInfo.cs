using System;
using System.Collections.Generic;

[Serializable]
public class LvInfo 
{
    public int Lv;
    public string Type;
    public string Prop;
    public string SpScene;

    public LvInfo()
    {}
}

[Serializable]
public class LvInfoList
{
    public List<LvInfo> lvInfoList;

    public LvInfoList() { }
}