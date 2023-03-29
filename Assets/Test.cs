using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Transform item = null;
    public int showNum = 3;
    public int itemNum = 5;
    private ScrollRect scrollRect = null;
    public Color[] colors;
    int centerIndex = 0;
    float maxRatio = 0;
    // Start is called before the first frame update
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        float itemW = item.GetComponent<RectTransform>().sizeDelta.x;
        Transform content = scrollRect.content;
        for (int i = 0; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);

        int colorIndex = 0;
        for (int i = 0; i < itemNum; i++)
        {
            colorIndex = colorIndex == colors.Length ? colorIndex = 0 : colorIndex;
            Transform newItem = Instantiate(item);
            newItem.SetParent(content);
            newItem.localPosition = Vector3.zero;
            newItem.localScale = Vector3.one * (1 - i * 0.1f);
            newItem.GetComponent<Image>().color = colors[colorIndex];
            colorIndex++;
            newItem.name = "item_" + i;
            newItem.GetComponent<CanvasGroup>().alpha = i < showNum ? 1 : 0;
        }
        content.GetComponent<HorizontalLayoutGroup>().spacing = -0.5f * itemW;
        List<float> targetX = new List<float>();
        float offX = (1000 / (itemNum - 1)) * 0.001f;
        for (int i = 0; i < itemNum; i++) { targetX.Add(i == itemNum - 1 ? 1 : offX * i); };
        scrollRect.onValueChanged.AddListener((vec2) =>
        {
            centerIndex = 0;
            maxRatio = 0;
            for (int i = 0; i < itemNum; i++)
            {
                Transform child = content.GetChild(i);
                float distance = Mathf.Abs(targetX[i] - vec2.x) * (itemNum - 1);
                float ratio = 1 - 0.1f * distance;
                child.localScale = ratio * Vector3.one;
                child.GetComponent<Canvas>().sortingOrder = (int)(100 * ratio);
                if (ratio >= maxRatio) { maxRatio = ratio; centerIndex = i; }
            }
            for (int i = 0; i < itemNum; i++)
            {
                Transform child = content.GetChild(i);
                child.GetComponent<CanvasGroup>().alpha = centerIndex - Mathf.FloorToInt(showNum / 2) <= i && i <= centerIndex + Mathf.FloorToInt(showNum / 2) ? 1 : 0;
            }
        });
        ScrollToTarget(new Vector2(0, 0));
        EventTriggerExpand eventTrigger = GetComponent<EventTriggerExpand>();
        eventTrigger.RemoveAllTrggerEventListener();
        eventTrigger.AddTrggerEventListener(EventTriggerTypeExpand.PointerUpExpand, (eventPara) => { ScrollToTarget(new Vector2() { x = targetX[centerIndex] }); });
    }

    private void ScrollToTarget(Vector2 vec2)
    {
        scrollRect.normalizedPosition = vec2;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
