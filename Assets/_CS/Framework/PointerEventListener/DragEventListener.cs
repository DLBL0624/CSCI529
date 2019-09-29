﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class DragEventListener : ClickEventListerner,IDragHandler,IBeginDragHandler,IEndDragHandler
{
    

    public delegate void OnDragDlg(PointerEventData eventData);
    public event OnDragDlg OnDragEvent;

    public delegate void OnBeginDragDlg(PointerEventData eventData);
    public event OnBeginDragDlg OnBeginDragEvent;

    public delegate void OnEndDragDlg(PointerEventData eventData);
    public event OnEndDragDlg OnEndDragEvent;

    


    public void OnDrag(PointerEventData eventData)
    {
        if (OnDragEvent != null)
        {
            OnDragEvent(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (OnBeginDragEvent != null)
        {
            OnBeginDragEvent(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (OnEndDragEvent != null)
        {
            OnEndDragEvent(eventData);
        }
		//PassEvent<IPointerClickHandler>(eventData,ExecuteEvents.pointerClickHandler);
    }


	public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T:IEventSystemHandler{
		List<RaycastResult> results = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (data,results);
		GameObject current = data.pointerCurrentRaycast.gameObject;
		for (int i = 0; i < results.Count; i++) {
			if (current != results [i].gameObject) {
				ExecuteEvents.Execute (results[i].gameObject,data,function);
			}
		}

	}

	public void ClearDragEvent(){
		{
			if (OnDragEvent != null) {
				Delegate[] invokeList = OnDragEvent.GetInvocationList ();
				if (invokeList != null)
				{
					foreach (Delegate del in invokeList)
					{
						OnDragEvent -= (OnDragDlg)del;
					}
				}
			}

		}
		{
			if (OnBeginDragEvent != null) {
				Delegate[] invokeList = OnBeginDragEvent.GetInvocationList ();
				if (invokeList != null)
				{
					foreach (Delegate del in invokeList)
					{
						OnBeginDragEvent -= (OnBeginDragDlg)del;
					}
				}
			}


		}
		{
			if (OnEndDragEvent != null) {
				Delegate[] invokeList = OnEndDragEvent.GetInvocationList ();
				if (invokeList != null)
				{
					foreach (Delegate del in invokeList)
					{
						OnEndDragEvent -= (OnEndDragDlg)del;
					}
				}
			
			}
		}

	}
}
