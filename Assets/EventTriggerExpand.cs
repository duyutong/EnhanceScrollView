using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum EventTriggerTypeExpand
{
    LongPress,
    PointerUpExpand
}
public class EventTriggerExpand : EventTrigger
{
    public delegate void EventTriggerHandle(BaseEventData eventData);
    readonly Dictionary<string, List<MyEntry>> triggerDic = new Dictionary<string, List<MyEntry>>();
    public float longPressTime = 1f;
    private float pressTime = 0;
    private bool isPress = false;
    private bool isEnter = false;
    public class MyEntry : Entry { public string methodName = null; }
    public override void OnBeginDrag(PointerEventData eventData) { DoMethod(EventTriggerType.BeginDrag, eventData); }
    public override void OnCancel(BaseEventData eventData) { DoMethod(EventTriggerType.Cancel, eventData); }
    public override void OnDeselect(BaseEventData eventData) { DoMethod(EventTriggerType.Deselect, eventData); }
    public override void OnDrag(PointerEventData eventData) { DoMethod(EventTriggerType.Drag, eventData); }
    public override void OnDrop(PointerEventData eventData) { DoMethod(EventTriggerType.Drop, eventData); }
    public override void OnEndDrag(PointerEventData eventData) { DoMethod(EventTriggerType.EndDrag, eventData); }
    public override void OnInitializePotentialDrag(PointerEventData eventData) { DoMethod(EventTriggerType.InitializePotentialDrag, eventData); }
    public override void OnMove(AxisEventData eventData) { DoMethod(EventTriggerType.Move, eventData); }
    public override void OnPointerClick(PointerEventData eventData) { DoMethod(EventTriggerType.PointerClick, eventData); }
    public override void OnPointerDown(PointerEventData eventData) { isPress = true; DoMethod(EventTriggerType.PointerDown, eventData); }
    public override void OnPointerEnter(PointerEventData eventData) { isEnter = true; DoMethod(EventTriggerType.PointerEnter, eventData); }
    public override void OnPointerExit(PointerEventData eventData) { isEnter = false; DoMethod(EventTriggerType.PointerExit, eventData); }
    public override void OnPointerUp(PointerEventData eventData) { isPress = false; DoMethod(EventTriggerType.PointerUp, eventData); }
    public override void OnScroll(PointerEventData eventData) { DoMethod(EventTriggerType.Scroll, eventData); }
    public override void OnSelect(BaseEventData eventData) { DoMethod(EventTriggerType.Select, eventData); }
    public override void OnSubmit(BaseEventData eventData) { DoMethod(EventTriggerType.Submit, eventData); }
    public override void OnUpdateSelected(BaseEventData eventData) { DoMethod(EventTriggerType.UpdateSelected, eventData); }
    //扩展方法(所有扩展方法因为不知道BaseEventData传什么所以都传的空，未来可以补上)
    private void Update()
    {
        if (Input.GetMouseButtonUp(0)) { isPress = false; if (isEnter) DoMethod(EventTriggerTypeExpand.PointerUpExpand, null); }
        if (pressTime >= longPressTime) DoMethod(EventTriggerTypeExpand.LongPress, null);
        pressTime = isPress ? pressTime >= longPressTime ? -99999 : pressTime + Time.deltaTime : 0;
    }
    private void DoMethod(EventTriggerTypeExpand eventID, BaseEventData eventData)
    {
        if (!triggerDic.ContainsKey(eventID.ToString())) return;
        foreach (MyEntry entry in triggerDic[eventID.ToString()])
        {
            entry.callback.Invoke(eventData);
        }
    }
    private void DoMethod(EventTriggerType eventID, BaseEventData eventData)
    {
        if (!triggerDic.ContainsKey(eventID.ToString())) return;
        foreach (MyEntry entry in triggerDic[eventID.ToString()])
        {
            entry.callback.Invoke(eventData);
        }
    }
    public void AddTrggerEventListener(EventTriggerTypeExpand eventID, EventTriggerHandle call, string methodName = null)
    {
        TriggerEvent callback = new TriggerEvent();
        callback.AddListener((data) => { call(data); });
        if (triggerDic.ContainsKey(eventID.ToString()))
            triggerDic[eventID.ToString()].Add(new MyEntry() { eventID = EventTriggerType.EndDrag, callback = callback, methodName = methodName });
        else
            triggerDic.Add(eventID.ToString(), new List<MyEntry>() { new MyEntry() { eventID = EventTriggerType.EndDrag, callback = callback, methodName = methodName } });
    }
    public void AddTrggerEventListener(EventTriggerType eventID, EventTriggerHandle call, string methodName = null)
    {
        TriggerEvent callback = new TriggerEvent();
        callback.AddListener((data) => { call(data); });
        if (triggerDic.ContainsKey(eventID.ToString()))
            triggerDic[eventID.ToString()].Add(new MyEntry() { eventID = EventTriggerType.EndDrag, callback = callback, methodName = methodName });
        else
            triggerDic.Add(eventID.ToString(), new List<MyEntry>() { new MyEntry() { eventID = EventTriggerType.EndDrag, callback = callback, methodName = methodName } });
    }
    public void RemoveAllTrggerEventListener()
    {
        triggerDic.Clear();
    }
    public void RemoveTrggerEventListener(EventTriggerType eventID, string methodName = null)
    {
        if (!triggerDic.ContainsKey(eventID.ToString())) return;
        if (methodName == null) { triggerDic.Remove(eventID.ToString()); return; };
        for (int i = triggerDic[eventID.ToString()].Count; i > 0; --i)
        {
            if (triggerDic[eventID.ToString()][i].methodName == methodName) triggerDic[eventID.ToString()].RemoveAt(i);
        }
    }
}
