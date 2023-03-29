using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceItem : MonoBehaviour
{
    public int childIndex = 0;
    public int infoIndex = 0;
    private InitItem initItem = null;

    public void InitSelf(int _infoIndex,InitItem _call) 
    {
        initItem = _call;
        infoIndex = _infoIndex;
        initItem?.Invoke(infoIndex, transform);
    }
    public void RefreshSelf(int _infoIndex) 
    {
        infoIndex = _infoIndex;
        initItem?.Invoke(infoIndex, transform);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
