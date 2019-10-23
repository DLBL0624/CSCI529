﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum eReactorType
{
    PRESENT,
    CAUTION,
    TUHAO
}


public class CardInZhibo
{
    public string CardId;
    public float TimeLeft;
    public int UseLeft;
    public CardAsset ca;
    public bool NeedDiscard;

    public CardInZhibo()
    {

    }

    public CardInZhibo(string CardId, float TimeLfet, int UseLeft)
    {
        this.CardId = CardId;
        this.TimeLeft = TimeLfet;
        this.UseLeft = UseLeft;
    }
    public CardInZhibo(CardAsset ca)
    {
        this.CardId = ca.CardId;
        this.TimeLeft = ca.ValidTime;
        this.UseLeft = ca.UseTime;
        this.ca = ca;
    }
}

public class ZhiboGameState
{
    public RoleStats stats;

    public float TimeLeft = 1200;

    public List<ZhiboBuff> ZhiboBuffs = new List<ZhiboBuff>();

    public List<Danmu> Danmus = new List<Danmu>();

    public List<CardInZhibo> Cards = new List<CardInZhibo>();
    public List<CardInZhibo> CardDeck = new List<CardInZhibo>();
    public List<CardInZhibo> CardUsed = new List<CardInZhibo>();

    public List<ZhiboSpecial> Specials = new List<ZhiboSpecial>();

    public float Score = 0;
    public int MaxHot = 100;

    public float ChoukaValue = 0;
    public int ChoukaYuzhi = 100;
    public float Tili = 0;

    public float DanmuFreq { get { return danmuFreq * AccelerateRate; } }
    private float danmuFreq = 5f;

    public float DanmuSpd { get { return danmuSpd * AccelerateRate; } }

    private float danmuSpd = 160.0f;


    public float AccelerateRate = 1.0f;
    public float AccelerateDur = 0f;

    public int[] BuffAddValue = new int[5];
    public int[] BuffAddPercent = new int[5];

    public List<string> ComingEmergencies = new List<string>();
}
public class ZhiboGameMode : GameModeBase
{


    IUIMgr mUIMgr;
    IResLoader mResLoader;
    IRoleModule pRoleMgr;
    ICardDeckModule mCardMdl;
    public ZhiboUI mUICtrl;

    public ZhiboGameState state;

    public float spdRate = 1.0f;


    public float lastTick = 0;
    public float nextTick = 0;

    private int bigOneNext = 3;
    private int bigOneCount = 0;

    public int CardMax = 10;
    private float DiscardTimer = 0;

    private float DanmuLeft = 0;
    private float DanmuSpringSpd = 20;

    private int BadDanmuFreq = 5;
    private int badCounter = 0;

    private float choukaPerSec = 5;

    private EmergencyAsset nowEmergency = null;

    private Dictionary<string, List<string>> DanmuDict = new Dictionary<string, List<string>>();

    private Dictionary<string, string> BuffDesp = new Dictionary<string, string>();


    public override void Init()
    {
        mUIMgr = GameMain.GetInstance().GetModule<UIMgr>();
        mResLoader = GameMain.GetInstance().GetModule<ResLoader>();
        mCardMdl = GameMain.GetInstance().GetModule<CardDeckModule>();
        pRoleMgr = GameMain.GetInstance().GetModule<RoleModule>();

        state = new ZhiboGameState();

        state.stats = new RoleStats(pRoleMgr.GetStats());

        mUIMgr.ShowPanel("ZhiboPanel");
        mUICtrl = mUIMgr.GetCtrl("ZhiboPanel") as ZhiboUI;

        state.ZhiboBuffs.Clear();
        state.Cards.Clear();
        state.Danmus.Clear();

        state.Score = 0;
        state.ChoukaValue = 0;
        state.Tili = 10;

        spdRate = 1.0f;
        lastTick = 0;
        nextTick = 0;
        bigOneNext = 3;
        bigOneCount = 0;

        BadDanmuFreq = 12;
        badCounter = 0;

        LoadDanmuDict();
        LoadCard();
        LoadBuff();
        InitEmergency();

        for (int i = 0; i < 3; i++)
        {
            AddCardFromDeck();
        }
    }


    private void LoadBuff()
    {
        BuffDesp.Add("m+", "增加{0}点魅力");
        BuffDesp.Add("t+", "增加{0}点体力");
        BuffDesp.Add("k+", "增加{0}点口才");
        BuffDesp.Add("j+", "增加{0}点技艺");
        BuffDesp.Add("f+", "增加{0}点反应");

        BuffDesp.Add("m+%", "增加百分比{0}的魅力");
        BuffDesp.Add("t+%", "增加百分比{0}的体力");
        BuffDesp.Add("k+%", "增加百分比{0}的口才");
        BuffDesp.Add("j+%", "增加百分比{0}的技艺");
        BuffDesp.Add("f+%", "增加百分比{0}的反应");
    }

    public string GetBuffDesp(string buffname)
    {
        return BuffDesp[buffname];
    }
    private void LoadCard()
    {
        List<CardInfo> infoList = mCardMdl.GetAllCards();
        state.CardDeck.Clear();
        foreach (CardInfo info in infoList)
        {
            string eid = info.CardId;
            CardAsset ca = mCardMdl.GetCardInfo(eid);
            CardInZhibo card = new CardInZhibo(eid, ca.ValidTime, ca.UseTime);
            card.ca = ca;
            state.CardDeck.Add(card);
        }

        shuffle<CardInZhibo>(state.CardDeck);
    }


    public void shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int idx = Random.Range(0, list.Count - 1 - i);
            T tmp = list[idx];
            list[idx] = list[list.Count - 1 - i];
            list[list.Count - 1 - i] = tmp;
        }
    }

    private void InitEmergency()
    {

    }

    private void LoadDanmuDict()
    {
        {
            List<string> ll = new List<string>();
            ll.Add("主播什么时候开播的");
            ll.Add("日常打卡");
            ll.Add("主播晚上好啊");
            DanmuDict.Add("common", ll);
        }
    }
    public override void Tick(float dTime)
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddCardFromDeck();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            FinishZhibo();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            GenSpecial("Special");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            mUIMgr.ShowHint("这是一段提示测试行");
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            //GenBuff("e");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowEmergency();
        }

        state.TimeLeft -= dTime * spdRate;
        if (state.TimeLeft < 0)
        {
            FinishZhibo();
        }
        mUICtrl.UpdateTimeLeft(state.TimeLeft);


        state.AccelerateDur -= spdRate * dTime;
        if(state.AccelerateDur < 0)
        {
            state.AccelerateRate = 1f;
        }

        if (state.ChoukaValue > 100)
        {
            int cardNum = (int)(state.ChoukaValue / 100);
            state.ChoukaValue -= cardNum * 100f;
            mUICtrl.ChangeChouka(state.ChoukaValue);
            for(int i = 0; i < cardNum; i++)
            {
                AddCardFromDeck();
            }
        }


        for (int i = state.Danmus.Count - 1; i >= 0; i--)
        {
            state.Danmus[i].Tick(dTime* spdRate);
            if (state.Danmus[i].NeedDestroy)
            {
                AutoDisappear(state.Danmus[i]);
            }
        }

        for (int i = state.Specials.Count - 1; i >= 0; i--)
        {
            state.Specials[i].Tick(dTime * spdRate);
        }

        bool buffChanged = false;
        for (int i = state.ZhiboBuffs.Count -1; i >= 0; i--)
        {
            state.ZhiboBuffs[i].Tick(dTime * spdRate);

            if (state.ZhiboBuffs[i].leftTime <= 0)
            {
                RemoveBuff(state.ZhiboBuffs[i]);
                buffChanged = true;
            }
        }

        if (buffChanged)
        {
            CalculateBuffExtras();
        }

        DiscardTimer += dTime * spdRate;

        for (int i = state.Cards.Count - 1; i >= 0; i--)
        {
            if (state.Cards[i].TimeLeft > 0)
            {
                state.Cards[i].TimeLeft -= dTime * spdRate;
                if(state.Cards[i].TimeLeft <= 0)
                {
                    state.Cards[i].NeedDiscard = true;
                }
            }

        }
        if (DiscardTimer > 1f)
        {

            for (int i = state.Cards.Count - 1; i >= 0; i--)
            {
                if (state.Cards[i].NeedDiscard)
                {
                    DiscardCard(state.Cards[i],false);
                    mUICtrl.GetCardContainer().RemoveCard(i);
                }
                else
                {
                    mUICtrl.GetCardContainer().UpdateCard(i, state.Cards[i]);
                }
            }

            DiscardTimer -= 1;
        }




        lastTick += dTime * spdRate;
        if (lastTick > nextTick)
        {
            GenDanmu();
            lastTick = 0;
            nextTick = 1.0f / state.DanmuFreq * Random.Range(0.7f, 1.3f);
            //nextTick = Random.Range(0.1f, 0.3f);
        }


        if(DanmuLeft > 0)
        {
            int oldV = (int)DanmuLeft;
            DanmuLeft -= dTime * DanmuSpringSpd * spdRate;
            if(oldV != (int)DanmuLeft)
            {
                GenDanmu();
            }
        }

        GetChoukaValue(choukaPerSec * dTime * spdRate);
    }


    public void ShowEmergency()
    {
        spdRate = 0.1f;
        mUIMgr.ShowPanel("ActBranch");
        ActBranchCtrl actrl = mUIMgr.GetCtrl("ActBranch") as ActBranchCtrl;
        EmergencyAsset ea = mResLoader.LoadResource<EmergencyAsset>("Emergencies/choufeng");
        actrl.SetEmergency(ea);
        actrl.ActBranchEvent += delegate (int idx) {
            spdRate = 1f;
            EmergencyChoice c = ea.Choices[idx];
            Debug.Log(c.Content);
            if(c.NextEmId != null && c.NextEmId != string.Empty)
            {

            }
            if (c.Ret=="Hot")
            {

            }
        };
    }

    public void GetChoukaValue(float v)
    {
        state.ChoukaValue += v;
        state.ChoukaValue = state.ChoukaValue < 0 ? 0 : state.ChoukaValue;
        mUICtrl.ChangeChouka(state.ChoukaValue);
        //mUIMgr.showHint("获得抽卡值" + v);
    }

    public float GetScoreFromFormulation(string formulation)
    {
        string[] comps = formulation.Split('+');
        float finalValue = 0;
        foreach(string comp in comps)
        {
            if (comp.Contains("*"))
            {
                string[] ss = comp.Split('*');
                float rate = float.Parse(ss[0]);
                string pname = ss[1];
                switch (pname)
                {
                    case "m":
                        finalValue += state.stats.meili * rate;
                        break;
                    case "k":
                        finalValue += state.stats.koucai * rate;
                        break;
                    case "t":
                        finalValue += state.stats.tili * rate;
                        break;
                    case "f":
                        finalValue += state.stats.fanying * rate;
                        break;
                    case "":
                        finalValue += state.stats.jiyi * rate;
                        break;
                    default:
                        Debug.Log("unknown property");
                        break;
                }
            }
            else
            {
                finalValue += int.Parse(comp);
            }
        }
        return finalValue;

    }

    public void GainScore(float score)
    {
        state.Score += score;
        //mUIMgr.ShowHint("获得热度" + (int)score);
        mUICtrl.UpdateScore(state.Score);



    }
    public void GenTili(int v)
    {
        state.Tili += v;
        mUIMgr.ShowHint("获得热度" + v);
        mUICtrl.UpdateScore(state.Score);
    }



    private void RemoveBuff(ZhiboBuff obj)
    {
        state.ZhiboBuffs.Remove(obj);
        mResLoader.ReleaseGO("Zhibo/Buff", obj.gameObject);
    }

    private void AutoDisappear(Danmu danmu)
    {
        RecycleDanmu(danmu);
        state.Danmus.Remove(danmu);
        if (danmu.isBad)
        {
            GetChoukaValue(-2);
        }
        else if (danmu.isBig)
        {
            GetChoukaValue(4);
        }
        else
        {
            GetChoukaValue(2);
        }

    }

   


    public void DestroyRandomly(int num)
    {
        List<Danmu> toClean = randomPickDanmu(num);
        foreach (Danmu danmu in toClean)
        {
            danmu.OnDestroy();
            state.Danmus.Remove(danmu);
            GainScore(10);
        }
    }






    
    //IEnumerator GenMultiDanmu(int num)
    //{
    //    int nn = num;
    //    while (nn > 0)
    //    {
    //        Danmu danmu = mUICtrl.GenDanmu();
    //        state.Danmus.Add(danmu);
    //        yield return null;
    //    }
    //}


    private void RefreshUsedCards()
    {
        for (int i = state.CardUsed.Count - 1; i >= 0; i--)
        {
            state.CardDeck.Add(state.CardUsed[i]);
            state.CardUsed.RemoveAt(i);
        }
        //shuffle
        shuffle<CardInZhibo>(state.CardDeck);
    }

    public void GainNewCard(string cardId)
    {
        CardAsset ca = mCardMdl.GetCardInfo(cardId);
        if (ca == null)
        {
            return;
        }
        CardInZhibo info = new CardInZhibo(ca);


        bool ret = mUICtrl.AddNewCard(cardId);
        state.Cards.Add(info);
    }

    public void GainNewCardWithPossiblity(string cardId, int possibility)
    {
        int randInt = Random.Range(0, 100);
        if(randInt >= possibility)
        {
            return;
        }
        GainNewCard(cardId);
    }

    public void AddCardFromDeck()
    {

        if(state.CardDeck.Count == 0)
        {
            RefreshUsedCards();
        }
        if(state.CardDeck.Count == 0)
        {
            return;
        }

        if(state.Cards.Count >= CardMax)
        {
            return;
        }
        CardInZhibo info = state.CardDeck[0];
        bool ret = mUICtrl.AddNewCard(info.CardId);
        if (!ret)
        {
            Debug.Log("add card Fail");
            return;
        }

        state.CardDeck.RemoveAt(0);
        state.Cards.Add(info);

        if(info.ca.ValidTime > 0)
        {
            info.TimeLeft = info.ca.ValidTime;
        }
        //mUICtrl.GetCardContainer().UpdateCard(state.Cards.Count-1,info);
    }

    public void RecycleDanmu(Danmu danmu)
    {
        mResLoader.ReleaseGO("Zhibo/Danmu", danmu.gameObject);
    }

    public string getRandomDanmu()
    {
        return "你麻痹死了";
    }

    public void FinishZhibo()
    {
        mUIMgr.CloseCertainPanel(mUICtrl);
        GameMain.GetInstance().GetModule<CoreManager>().ChangeScene("Main");
    }


    private void DiscardCard(CardInZhibo cinfo, bool costUseTime=true)
    {
        state.Cards.Remove(cinfo);
        if (costUseTime && cinfo.UseLeft  > 0)
        {
            cinfo.UseLeft -= 1;
            if (cinfo.UseLeft == 0)
            {
                return;
            }
        }

        state.CardUsed.Add(cinfo);
        cinfo.TimeLeft = 0;
        cinfo.NeedDiscard = false;
    }

    public bool TryUseCard(int cardIdx)
    {
        if(cardIdx < 0|| cardIdx >= state.Cards.Count)
        {
            return false;
        }
        CardInZhibo cinfo = state.Cards[cardIdx];

        CardAsset ca = cinfo.ca;
        if (state.Tili < ca.cost)
        {
            return false;
        }
        ExcuteUseCard(cinfo);
        DiscardCard(cinfo);
        return true;
    }


    private void GenSpeedUp(float duration = 5f)
    {
        state.AccelerateRate = 2f;
        state.AccelerateDur = (state.AccelerateDur < 0 ? 0 : state.AccelerateDur) + duration;
    }

    private CardInZhibo NowExecuteCard;

    public void ExcuteUseCard(CardInZhibo card)
    {

        CardAsset cardAsset = card.ca;
        if (cardAsset != null)
        {
            NowExecuteCard = card;
            if (cardAsset.CardType == eCardType.GENG)
            {
                mUICtrl.ShowGengEffect();
            }

            //几率触发或条件触发的效果 将放入该列表中后处理
            List<CardEffect> extraEffects = new List<CardEffect>();

            foreach(CardEffect ce in cardAsset.Effects)
            {
                HandleOneCardEffect(ce, extraEffects);
            }

            for(int i=0;i< extraEffects.Count;i++)
            {
                HandleOneCardEffect(extraEffects[i], extraEffects);
            }
            NowExecuteCard = null;

        }
    }

    private void HandleOneCardEffect(CardEffect ce, List<CardEffect> extraEffects)
    {
        string[] args = ce.effectString.Split(',');
        switch (ce.effect)
        {
            case "SpawnGift":
                for (int i = 0; i < 3; i++)
                {
                    GenSpecial(args[0]);
                }
                break;
            case "SpeedUp":
                GenSpeedUp(float.Parse(args[0]));
                break;
            case "GenGoodDanmu":
                GenDanmu(args[0]);
                break;
            case "GenBadDanmu":
                GenDanmu(args[0]);
                break;
            case "GetScore":

                GainScore(GetScoreFromFormulation(args[0]));
                mUICtrl.ShowNewAudience();


                
                mUICtrl.ShowDanmuEffect(mUICtrl.GetCardContainer().cards[state.Cards.IndexOf(NowExecuteCard)].transform.position);
                break;
            case "GetChouka":
                GetChoukaValue(int.Parse(args[0]));
                break;
            case "GetTili":
                GenTili(int.Parse(args[0]));
                break;
            case "AddStatus":
                GenBuff(args[0], int.Parse(args[1]), 10);
                break;
            case "AddRemoveAward":

                GenBuff(args[0], int.Parse(args[1]), 10);
                break;
            case "ClearDanmu":

                DestroyRandomly(int.Parse(args[0]));
                break;
            case "AddCardToDeck":
                AddCardToDeck(args[0], int.Parse(args[1]));
                break;
            case "Chongzhu":
                AddCardFromDeck();
                break;
            case "GainCardWithPossibility":
                GainNewCardWithPossiblity(args[0], int.Parse(args[1]));
                break;
            case "Branches":
                int randNum = Random.Range(0, 100);
                int baseNum = 0;
                string[] cmds = ce.effectString.Split(';');
                foreach (string cmd in cmds)
                {
                    if (cmd == "")
                    {
                        continue;
                    }
                    string s = cmd;
                    int p = int.Parse(s.Substring(0, s.IndexOf(',')));
                    if (randNum <= baseNum + p)
                    {
                        //生效
                        s = s.Substring(s.IndexOf(',') + 1);
                        string effect = s.Substring(0, s.IndexOf(','));
                        string effectString = s.Substring(s.IndexOf(',') + 1);
                        extraEffects.Add(new CardEffect(effect, effectString));
                        break;
                    }
                    else
                    {
                        baseNum += p;
                    }

                }

                break;
            default:
                break;
        }

    }

    private void AddCardToDeck(string cardId, int level)
    {

        string eid = cardId;
        CardAsset ca = mCardMdl.GetCardInfo(eid);
        CardInZhibo card = new CardInZhibo(eid, ca.ValidTime, ca.UseTime);
        card.ca = ca;
        state.CardDeck.Add(card);
    }

    private List<Danmu> randomPickDanmu(int n)
    {
        if (state.Danmus.Count <= n)
        {
            return new List<Danmu>(state.Danmus);
        }
        List<Danmu> ret = new List<Danmu>();
        List<int> choosed = new List<int>();
        int nowC = 0;
        while (nowC < n)
        {
            int randIdx = Random.Range(0, state.Danmus.Count);
            if (!choosed.Contains(randIdx))
            {
                choosed.Add(randIdx);
                nowC++;
            }
        }
        foreach (int idx in choosed)
        {
            ret.Add(state.Danmus[idx]);
        }
        return ret;
    }


    public void GenDanmu(string fengxian, int num = 50)
    {
        DanmuLeft += num;
    }

    public void GenBuff(string BuffId, int value, float duration)
    {

        ZhiboBuff buff = mUICtrl.GenBuff();
        buff.Init(BuffId, value, duration, this);
        state.ZhiboBuffs.Add(buff);
        CalculateBuffExtras();
    }

    private void CalculateBuffExtras()
    {
        for(int i = 0; i < 5; i++)
        {
            state.BuffAddValue[i] = 0;
            state.BuffAddPercent[i] = 0;
        }
        foreach (ZhiboBuff buff in state.ZhiboBuffs)
        {
            switch (buff.buffId)
            {
                case "m+":
                    state.BuffAddValue[0] += buff.buffLevel;
                    break;
                case "m+%":
                    state.BuffAddPercent[0] += buff.buffLevel;
                    break;
                case "t+":
                    state.BuffAddValue[1] += buff.buffLevel;
                    break;
                case "t+%":
                    state.BuffAddPercent[1] += buff.buffLevel;
                    break;
                case "k+":
                    state.BuffAddValue[2] += buff.buffLevel;
                    break;
                case "k+%":
                    state.BuffAddPercent[2] += buff.buffLevel;
                    break;
                case "f+":
                    state.BuffAddValue[3] += buff.buffLevel;
                    break;
                case "f+%":
                    state.BuffAddPercent[3] += buff.buffLevel;
                    break;
                case "j+":
                    state.BuffAddValue[4] += buff.buffLevel;
                    break;
                case "j+%":
                    state.BuffAddPercent[4] += buff.buffLevel;
                    break;
                default:
                    break;
            }
        }
    }


    public void GenSpecial(string specialType)
    {
        ZhiboSpecial spe = mUICtrl.GenSpecial(specialType);
        state.Specials.Add(spe);
    }


    public void GenDanmu()
    {

        bool bad = false;
        if(Random.Range(0, badCounter)<1)
        {
            bad = true;
            badCounter = BadDanmuFreq;
        }
        else
        {
            badCounter -= 1;
        }
        Danmu danmu = mUICtrl.GenDanmu(bad);
        bigOneCount++;
        if (bigOneCount > bigOneNext)
        {
            danmu.SetAsBig();
            bigOneCount = 0;
            bigOneNext = Random.Range(5, 8);
        }
        state.Danmus.Add(danmu);
    }





    public void HitSpecial(ZhiboSpecial spe)
    {
        if (spe.type == "gift")
        {
            state.Score += 100;
            mUICtrl.UpdateScore(state.Score);
        }
        state.Specials.Remove(spe);
        mResLoader.ReleaseGO("Zhibo/Special/Special" , spe.gameObject);
    }


    public override void OnRelease()
    {
        base.OnRelease();
        if (GameFinishedCallback != null)
        {
            GameFinishedCallback();
        }
    }

}
