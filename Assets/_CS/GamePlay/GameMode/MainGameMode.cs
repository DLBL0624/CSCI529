﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainGameMode : GameModeBase
{

	IUIMgr pUIMgr;
    IRoleModule rm;
    ISpeEventMgr pEventMgr;


    Queue<SpecialEvent> UnHandledEvent = new Queue<SpecialEvent>();
    public override void Tick(float dTime){

        if (Input.GetKeyDown(KeyCode.A))
        {
            pUIMgr.ShowMsgBox();
        }
    }

	public override void Init(){
		pUIMgr = GameMain.GetInstance ().GetModule<UIMgr> ();
        rm = GameMain.GetInstance().GetModule<RoleModule>();
        pEventMgr = GameMain.GetInstance().GetModule<SpeEventMgr>();


        pUIMgr.ShowPanel("UIMain");
    }

    public void NextTurn()
    {
        HandleEvents(pEventMgr.CheckEvent());
        GameMain.GetInstance().GetModule<CardDeckModule>().CheckOverdue();
    }

    public void HandleEvents(List<SpecialEvent> list)
    {
        UnHandledEvent.Clear();
        foreach (SpecialEvent e in list)
        {
            UnHandledEvent.Enqueue(e);

        }
        HandleNextEvent();
    }





    public void HandleNextEvent()
    {
        if (UnHandledEvent.Count > 0)
        {

            SpecialEvent head = UnHandledEvent.Dequeue();

            foreach(string action in head.actions)
            {

                DialogManager dm = pUIMgr.ShowPanel("DialogManager") as DialogManager;
                if (dm == null)
                {
                    Debug.LogError("dialog mgr load fail");
                }
                dm.StartDialog(action, delegate (string[] args)
                {
                    if (head.EventId == "e0")
                    {
                        pEventMgr.AddListener("e1");
                    }
                    pEventMgr.RemoveListener(head.EventId);
                    HandleNextEvent();
                });
            }

        }

    }


}

