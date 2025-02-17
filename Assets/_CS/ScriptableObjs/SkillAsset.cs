﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SkillPrerequist
{
    public string skillId;
    public int level;
}


[CreateAssetMenu(fileName = "new skill", menuName = "Ctm/skill")]
[System.Serializable]
public class SkillAsset : ScriptableObject
{
    public string SkillId;

    public string SkillName;

    [TextArea(3, 10)]
    public string SkingDesp;


    public int MaxLevel;

    public string SkillType;

    //每个等级都会有相应的附加卡片
    public List<string> AttachCards = new List<string>();

    //与等级无关的前置条件，即前置技能
    public List<SkillPrerequist> PrerequistSkills = new List<SkillPrerequist>();

    //每个等级的技能还有额外的属性要求
    public List<string> PrerequistStatus = new List<string>();

    //每个等级的难度
    public List<int> Difficulties = new List<int>();

}
