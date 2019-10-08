﻿using UnityEngine;
using System.Collections.Generic;

public class CardContainerLayout : MonoBehaviour
{
    public List<MiniCard> cards = new List<MiniCard>();


    //public GameObject cardPrefab;


    [HideInInspector]
    public int CardNow;


    RectTransform rt;


    float Width = 1000f;

    float R = 0;
    float MaxDegree = 10;
    float CardMoveSpd = 5f;
    float DefaultIntervalDegree = 2f;

    public ZhiboGameMode gameMode;

    public void Init(ZhiboGameMode gameMode)
    {
        rt = (RectTransform)transform;
        Width = rt.rect.width;
        this.gameMode = gameMode;
        R = Width * 0.5f / Mathf.Sin(MaxDegree * 0.5f * Mathf.Deg2Rad);
        Debug.Log(R);
    }

    public void PutToInitPos(MiniCard card)
    {
        card.rt.anchoredPosition = new Vector3(Mathf.Sin(MaxDegree*0.5f*Mathf.Deg2Rad) * R, Mathf.Cos(MaxDegree * 0.5f * Mathf.Deg2Rad) * R);
        card.rt.localEulerAngles = new Vector3(0, 0, -MaxDegree);
    }

    public bool AddCard(string cardId)
    {


        IResLoader loader = GameMain.GetInstance().GetModule<ResLoader>();

        GameObject cardGo = loader.Instantiate("Zhibo/Card");
        if(cardGo == null)
        {
            return false;
        }
        MiniCard card = cardGo.GetComponent<MiniCard>();
        card.Init(cardId,this);
        cards.Add(card);
        Adjust();
        return true;
    }

    private void Adjust()
    {
        float interval = DefaultIntervalDegree;
        if (cards.Count > 6)
        {
            interval = MaxDegree / cards.Count;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            float angleDegree = i * interval - 10*0.5f;
            cards[i].transform.SetSiblingIndex(i);
            cards[i].targetDegree = angleDegree;
            //Vector2 posInWorld = transform.localToWorldMatrix * new Vector4(i * interval, 0, 0, 1);
            //cards[i].setTargetPosition(posInWorld);
        }
    }

    public void Tick(float dTime)
    {
        foreach(MiniCard card in cards)
        {
            card.Tick(dTime);
            if (Mathf.Abs(card.targetDegree - card.nowDegree) <= 1e-6)
            {
                continue;
            }
            card.nowDegree += (card.targetDegree - card.nowDegree) * dTime * CardMoveSpd;
            card.rt.anchoredPosition = new Vector3(Mathf.Sin(card.nowDegree * Mathf.Deg2Rad) * R, Mathf.Cos(card.nowDegree * Mathf.Deg2Rad) * R - R);
            card.rt.localEulerAngles = new Vector3(0, 0, -card.nowDegree);
        }
    }


    public bool UseCard(MiniCard toUse)
    {
        int cardIdx = cards.IndexOf(toUse);
        if (gameMode.TryUseCard(cardIdx))
        {
            removeCard(toUse);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void removeCard(MiniCard toRemove)
    {
        cards.Remove(toRemove);
        Adjust();
    }

    public void RemoveCard(int CardIdx)
    {
        if(CardIdx<0 || CardIdx >= cards.Count)
        {
            return;
        }
        cards[CardIdx].Disappaer();
        cards.RemoveAt(CardIdx);
        Adjust();
    }

    public void UpdateCard(int CardIdx, CardInZhibo cinfo)
    {
        if (CardIdx < 0 || CardIdx >= cards.Count)
        {
            return;
        }
        cards[CardIdx].UpdateView(cinfo);
    }


}
