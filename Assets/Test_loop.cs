using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Test_loop : MonoBehaviour
{
    public Transform item;
    public float spacing = 0;
    public int showNum = 3;
    public int itemNum = 5;
    public float speed = 5f;
    public Color[] colors;
    public int centerIndex = 0;
    private float maxRatio = 0;
    private Vector2 deltaPos = Vector2.zero;
    void Start()
    {
        float itemW = item.GetComponent<RectTransform>().sizeDelta.x;
        int colorIndex = 0;
        centerIndex = (showNum - 1) / 2;
        Transform parent = transform;
        spacing = -0.5f * itemW;
        List<float> targetX = new List<float>();
        List<float> targetS = new List<float>();
        Dictionary<int, float> posXDic = new Dictionary<int, float>();
        targetX.Clear();
        targetS.Clear();
        for (int i = 0; i < itemNum; i++)
        {
            targetX.Add((itemW + spacing) * (i - centerIndex));
            targetS.Add(1 - 0.1f * (centerIndex - i) * (i > centerIndex ? -1 : 1));
        }

        for (int i = 0; i < itemNum; i++)
        {
            Transform newItem = Instantiate(item);
            newItem.SetParent(parent);
            newItem.localScale = Vector3.one * targetS[i];
            newItem.localPosition = new Vector3() { x = targetX[i] };
            colorIndex = colorIndex == colors.Length ? colorIndex = 0 : colorIndex;
            newItem.GetChild(0).GetComponent<Image>().color = colors[colorIndex];
            newItem.name = "item_" + i;
            newItem.GetComponent<CanvasGroup>().alpha = Mathf.Abs(i - centerIndex) < showNum - 1 ? 1 : 0;
            newItem.GetComponentInChildren<Canvas>().sortingOrder = 99 - Mathf.Abs(i - centerIndex);
            EventTriggerExpand eventTrigger = newItem.GetComponent<EventTriggerExpand>();
            int index = i;
            colorIndex++;
            posXDic.Add(i, targetX[i]);
            eventTrigger.AddTrggerEventListener(EventTriggerType.Drag, (data) =>
            {
                deltaPos = new Vector2(Input.GetAxis("Mouse X"), 0);
                newItem.localPosition += deltaPos.x * Time.deltaTime * 100 * speed * Vector3.right;
                maxRatio = 0;
                List<float> targetVecXList = new List<float>();
                targetVecXList.Clear();
                for (int j = 0; j < itemNum; j++)
                {
                    Transform child = parent.GetChild(j);
                    float distance = Mathf.Abs(posXDic[j] - targetX[index] + newItem.localPosition.x);
                    float ratio = 1 - 0.1f * distance / (itemW + spacing);
                    if (ratio >= maxRatio) { maxRatio = ratio; centerIndex = j; }
                    float targetVecX = posXDic[j] - targetX[index] + newItem.localPosition.x;
                    targetVecXList.Add(targetVecX);
                }
                for (int j = 0; j < itemNum; j++)
                {
                    Transform child = parent.GetChild(j);
                    bool hideItem = Mathf.Abs(j - centerIndex) < showNum - 1;
                    float targetA = hideItem ? 1 : 0;
                    child.GetComponent<CanvasGroup>().alpha = targetA;
                    float distance = Mathf.Abs(posXDic[j] - targetX[index] + newItem.localPosition.x);
                    float ratio = 1 - 0.1f * distance / (itemW + spacing);
                    child.localScale = ratio * Vector3.one;
                    child.GetComponentInChildren<Canvas>().sortingOrder = (int)(100 * ratio);
                    child.localPosition = new Vector3() { x = targetVecXList[j] };

                }
                //for (int j = 0; j < itemNum; j++)
                //{
                //    Transform child = parent.GetChild(j);
                //    bool hideItem = Mathf.Abs(j - centerIndex) < showNum - 1;
                //    if (hideItem)
                //    {
                //        child 
                //    }
                //}
            });
        } 
    }
    // Update is called once per frame
    void Update()
    {

    }
}
