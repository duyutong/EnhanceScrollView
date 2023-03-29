using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Control : MonoBehaviour
{
    public EnhanceScrollView enhanceScrollView = null;
    public List<Color> colors = new List<Color>();
    // Start is called before the first frame update
    void Start()
    {
        enhanceScrollView.InitInfo(6, 3, InitItem, OnCompleteCall);
        enhanceScrollView.ScrollToTarget(0);
    }

    private void OnCompleteCall(int centerIndex)
    {
        //print("中间 item 的颜色 " + colors[centerIndex]);
    }

    private void InitItem(int index, Transform item)
    {
        Image image = item.GetComponent<Image>();
        image.color = colors[index];
        item.name = "item_" + index;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
