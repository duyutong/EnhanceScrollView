using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void ScrollComplete(int centerIndex);
public delegate void InitItem(int index, Transform item);
public class EnhanceScrollView : MonoBehaviour
{
    [Range(0, 5)]
    public float offsetX = 0.5f;
    public Transform item = null;
    public float smoothSpeed = 5f;
    public float spacing = 0;
    public bool isLoop = true;

    private ScrollComplete scrollComplete = null;
    private int showNum = 5;
    private int itemNum = 6;
    private float startMousePosX = 0;
    private int showCenterIndex = 0;
    private Transform content = null;
    private Dictionary<int, float> oriPosX = new Dictionary<int, float>();
    private Dictionary<int, float> oriLocalPosX = new Dictionary<int, float>();
    private bool resetAnimaFinish = true;
    private bool isSmooth = true;
    private float itemW = 0;
    public Camera UICamera = null;
    public int infoCenterIndex
    {
        get
        {
            return content.GetChild(showCenterIndex).GetComponent<EnhanceItem>().infoIndex;
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitInfo(int _itemNum, int _showNum, InitItem _initCall, ScrollComplete _completeCall)
    {
        itemW = item.GetComponent<RectTransform>().sizeDelta.x;
        itemNum = _itemNum;
        showNum = Mathf.Min(_showNum, _itemNum);
        scrollComplete = _completeCall;
        showCenterIndex = (showNum - 1) / 2;

        content = transform.Find("content");
        if (content == null)
        {
            GameObject go = new GameObject("content");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            content = go.transform;
        }

        RemovChild(content);
        oriPosX.Clear();
        oriLocalPosX.Clear();
        for (int i = 0; i < showNum; i++)
        {
            int index = i;
            float localPosX = (itemW + spacing) * (index - showCenterIndex);
            float localScale = 1 - 0.1f * (showCenterIndex - index) * (index > showCenterIndex ? -1 : 1);
            Transform newItem = Instantiate(item, content);
            newItem.localPosition = new Vector3() { x = localPosX };
            newItem.localScale = Vector3.one * localScale;
            newItem.GetComponentInChildren<Canvas>().sortingOrder = 99 - Mathf.Abs(index - showCenterIndex);
            oriPosX.Add(index, newItem.position.x);
            oriLocalPosX.Add(index, newItem.localPosition.x);
            EnhanceItem enhanceItem = newItem.GetComponent<EnhanceItem>();
            enhanceItem.InitSelf(index, _initCall);
            EventTriggerExpand eventTrigger = newItem.GetComponent<EventTriggerExpand>();
            eventTrigger.RemoveAllTrggerEventListener();
            eventTrigger.AddTrggerEventListener(EventTriggerType.BeginDrag, OnBeginDrag);
            eventTrigger.AddTrggerEventListener(EventTriggerType.Drag, OnDrag);
            eventTrigger.AddTrggerEventListener(EventTriggerType.EndDrag, OnEndDrag);
        }
        if (!isLoop && showNum > 2)
        {
            content.GetChild(0).gameObject.SetActive(infoCenterIndex > 1);
            content.GetChild(showNum - 1).gameObject.SetActive(infoCenterIndex < itemNum - 2);
        }
    }
    private void OnBeginDrag(BaseEventData eventData)
    {
        if (!resetAnimaFinish) return;
        Vector3 mousePosition = UICamera.ScreenToWorldPoint(Input.mousePosition);
        startMousePosX = mousePosition.x;
    }
    private void OnDrag(BaseEventData eventData)
    {
        if (!resetAnimaFinish) return;
        if (showNum == 1) return;
        if (showNum == 2)
        {
            showCenterIndex = 0;
            for (int i = 0; i < showNum; i++)
            {
                int index = i;
                Transform child = content.GetChild(index);
                child.localPosition += Input.GetAxis("Mouse X") * Vector3.right * smoothSpeed * 2;
                child.localScale = TargetScale(child.localPosition.x) * Vector3.one;
                child.GetComponent<CanvasGroup>().alpha = TargetAlph(child.localPosition.x);
            }
            Transform rightChild = content.GetChild(showNum - 1);
            Transform leftChild = content.GetChild(0);
            bool showLeft = Mathf.Abs(rightChild.localPosition.x) > Mathf.Abs(leftChild.localPosition.x);
            bool showRight = Mathf.Abs(rightChild.localPosition.x) < Mathf.Abs(leftChild.localPosition.x);
            rightChild.GetComponent<Canvas>().sortingOrder = showRight ? 100 : 90;
            leftChild.GetComponent<Canvas>().sortingOrder = showLeft ? 100 : 90;
        }
        else
        {
            Vector3 mousePosition = UICamera.ScreenToWorldPoint(Input.mousePosition);
            float currMousePosX = mousePosition.x;
            float deltaX = currMousePosX - startMousePosX;
            if (isSmooth)
            {
                for (int i = 0; i < showNum; i++)
                {
                    int index = i;
                    Transform child = content.GetChild(index);
                    float targetPosX = oriPosX[index] + deltaX;
                    child.position = new Vector3(targetPosX, child.position.y, child.position.z);
                    child.localScale = TargetScale(child.localPosition.x) * Vector3.one;
                    child.GetComponent<CanvasGroup>().alpha = TargetAlph(child.localPosition.x);
                }
            }
            if (!isSmooth)
            {
                for (int i = 0; i < showNum; i++)
                {
                    int index = i;
                    Transform child = content.GetChild(index);
                    child.localPosition += Input.GetAxis("Mouse X") * Vector3.right * smoothSpeed;
                    child.localScale = TargetScale(child.localPosition.x) * Vector3.one;
                    child.GetComponent<CanvasGroup>().alpha = TargetAlph(child.localPosition.x);
                }
            }
            Transform lastChild = content.GetChild(showNum - 1);
            Transform firstChild = content.GetChild(0);
            Transform centerChild = content.GetChild(showCenterIndex);
            EnhanceItem lastEnhanceItem = lastChild.GetComponent<EnhanceItem>();
            EnhanceItem firstEnhanceItem = firstChild.GetComponent<EnhanceItem>();
            bool isScrollLastItem = lastEnhanceItem.infoIndex == 0;
            bool isScrollFirstItem = firstEnhanceItem.infoIndex == itemNum - 1;
            bool showLast = !(!isLoop && isScrollFirstItem) && centerChild.position.x >= oriPosX[showCenterIndex + 1] + offsetX;
            bool showNext = !(!isLoop && isScrollLastItem) && centerChild.position.x <= oriPosX[showCenterIndex - 1] - offsetX;
            if (showLast)
            {
                isSmooth = false;
                lastChild.localPosition = firstChild.localPosition - (itemW + spacing) * Vector3.right;
                if (firstEnhanceItem.infoIndex > 0) lastEnhanceItem.RefreshSelf(firstEnhanceItem.infoIndex - 1);
                if (firstEnhanceItem.infoIndex == 0) lastEnhanceItem.RefreshSelf(itemNum - 1);
                lastChild.SetAsFirstSibling();
            }
            else if (showNext)
            {
                isSmooth = false;
                firstChild.localPosition = lastChild.localPosition + (itemW + spacing) * Vector3.right;
                if (lastEnhanceItem.infoIndex < itemNum - 1) firstEnhanceItem.RefreshSelf(lastEnhanceItem.infoIndex + 1);
                if (lastEnhanceItem.infoIndex == itemNum - 1) firstEnhanceItem.RefreshSelf(0);
                firstChild.SetAsLastSibling();
            }
            for (int i = 0; i < showNum; i++)
            {
                int index = i;
                Transform child = content.GetChild(index);
                child.gameObject.SetActive(true);
                child.GetComponentInChildren<Canvas>().sortingOrder = 99 - Mathf.Abs(index - showCenterIndex);
            }
            if (!isLoop && showNum > 2)
            {
                content.GetChild(0).gameObject.SetActive(infoCenterIndex >= 1);
                content.GetChild(showNum - 1).gameObject.SetActive(infoCenterIndex <= itemNum - 2);
            }
        }
    }
    private float TargetAlph(float currX)
    {
        float minLimitX = oriLocalPosX[0] - itemW - spacing;
        float maxLimitX = oriLocalPosX[showNum - 1] + itemW + spacing;
        float lx = oriLocalPosX[showNum - 1];

        float a = 1 * 100 / (lx - maxLimitX) * 0.01f;
        float b = 1 - a * lx;

        float result = 1;
        if (currX >= oriLocalPosX[0] && currX <= oriLocalPosX[showNum - 1]) return result;
        else if (currX < oriLocalPosX[0]) result = -1 * a * currX + b;
        else if (currX > oriLocalPosX[showNum - 1]) result = a * currX + b;

        return result;
    }
    private float TargetScale(float currX)
    {
        float checkIndex = showNum - 1;
        float lx = oriLocalPosX[showNum - 1];
        float ls = 1 - 0.1f * (showCenterIndex - checkIndex) * (checkIndex > showCenterIndex ? -1 : 1);
        float cx = oriLocalPosX[showCenterIndex];

        float a = (ls - 1) / lx;
        if (currX < cx) return -1 * a * currX + 1;
        if (currX > cx) return a * currX + 1;
        return 1;
    }
    private void OnEndDrag(BaseEventData eventData)
    {
        resetAnimaFinish = false;
        float resetTime = 0.2f;
        if (showNum > 2)
        {
            for (int i = 0; i < showNum; i++)
            {
                int index = i;
                Transform child = content.GetChild(index);

                float targetPosX = oriPosX[index];
                child.DOMoveX(targetPosX, resetTime);

                float localScale = 1 - 0.1f * (showCenterIndex - index) * (index > showCenterIndex ? -1 : 1);
                child.DOScale(localScale, 0.1f);

                child.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
            }
        }
        else
        {
            Transform rightChild = content.GetChild(showNum - 1);
            Transform leftChild = content.GetChild(0);
            bool showLeft = Mathf.Abs(rightChild.localPosition.x) > Mathf.Abs(leftChild.localPosition.x);
            bool showRight = Mathf.Abs(rightChild.localPosition.x) < Mathf.Abs(leftChild.localPosition.x);
            if (showLeft)
            {
                leftChild.DOLocalMoveX(oriLocalPosX[0], resetTime);
                rightChild.DOLocalMoveX(oriLocalPosX[1], resetTime);

                leftChild.DOScale(TargetScale(oriLocalPosX[0]), 0.1f);
                rightChild.DOScale(TargetScale(oriLocalPosX[1]), 0.1f);

                showCenterIndex = 0;
            }
            if (showRight)
            {
                leftChild.DOLocalMoveX(-1 * oriLocalPosX[1], resetTime);
                rightChild.DOLocalMoveX(oriLocalPosX[0], resetTime);

                leftChild.DOScale(TargetScale(oriLocalPosX[1]), 0.1f);
                rightChild.DOScale(TargetScale(oriLocalPosX[0]), 0.1f);

                showCenterIndex = 1;
            }
            leftChild.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
            rightChild.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
        }
        float timeCount = 0;
        DOTween.To(() => timeCount, a => timeCount = a, 1, resetTime).OnComplete(() => { resetAnimaFinish = true; isSmooth = true; });
        scrollComplete?.Invoke(infoCenterIndex);
    }
    public void ScrollToTarget(int targetIndex)
    {
        for (int i = 0; i < showNum; i++)
        {
            int index = i;
            Transform child = content.GetChild(index);
            int targetInfoIndex = targetIndex - showCenterIndex + index;
            if (targetInfoIndex < 0) targetInfoIndex += itemNum;
            else if (targetInfoIndex >= showNum) targetInfoIndex = 0;
            EnhanceItem enhanceItem = child.GetComponent<EnhanceItem>();
            enhanceItem.childIndex = index;
            enhanceItem.RefreshSelf(targetInfoIndex);
        }
    }
    public void ShowlLastItem()
    {
        if (showNum <= 1) return;
        if (showNum > 2)
        {
            showCenterIndex = 1;
            Transform lastChild = content.GetChild(showNum - 1);
            Transform firstChild = content.GetChild(0);
            EnhanceItem lastEnhanceItem = lastChild.GetComponent<EnhanceItem>();
            EnhanceItem firstEnhanceItem = firstChild.GetComponent<EnhanceItem>();

            firstChild.localPosition = lastChild.localPosition + (itemW + spacing) * Vector3.right;
            if (lastEnhanceItem.infoIndex < itemNum - 1) firstEnhanceItem.RefreshSelf(lastEnhanceItem.infoIndex + 1);
            if (lastEnhanceItem.infoIndex == itemNum - 1) firstEnhanceItem.RefreshSelf(0);
            firstChild.SetAsLastSibling();

            resetAnimaFinish = false;
            float resetTime = 0.2f;
            for (int i = 0; i < showNum; i++)
            {
                int index = i;
                Transform child = content.GetChild(index);
                float targetPosX = oriPosX[index];
                child.DOMoveX(targetPosX, resetTime);
            }
            float timeCount = 0;
            DOTween.To(() => timeCount, a => timeCount = a, 1, resetTime).OnComplete(() => { resetAnimaFinish = true; isSmooth = true; });

            for (int i = 0; i < showNum; i++)
            {
                int index = i;
                Transform child = content.GetChild(index);
                child.GetComponentInChildren<Canvas>().sortingOrder = 99 - Mathf.Abs(index - showCenterIndex);
                float localScale = 1 - 0.1f * (showCenterIndex - index) * (index > showCenterIndex ? -1 : 1);
                child.DOScale(localScale, 0.1f);
            }
        }
        else
        {
            Transform rightChild = content.GetChild(showNum - 1);
            Transform leftChild = content.GetChild(0);

            leftChild.DOLocalMoveX(oriLocalPosX[0], 0.2f);
            rightChild.DOLocalMoveX(oriLocalPosX[1], 0.2f);

            leftChild.DOScale(TargetScale(oriLocalPosX[0]), 0.1f);
            rightChild.DOScale(TargetScale(oriLocalPosX[1]), 0.1f);

            leftChild.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
            rightChild.GetComponent<CanvasGroup>().DOFade(1, 0.1f);

            rightChild.GetComponent<Canvas>().sortingOrder = 90;
            leftChild.GetComponent<Canvas>().sortingOrder = 100;
        }
        scrollComplete?.Invoke(infoCenterIndex);
    }
    public void ShowNextItem()
    {
        if (showNum <= 1) return;
        if (showNum > 2)
        {
            showCenterIndex = 1;
            Transform lastChild = content.GetChild(showNum - 1);
            Transform firstChild = content.GetChild(0);
            EnhanceItem lastEnhanceItem = lastChild.GetComponent<EnhanceItem>();
            EnhanceItem firstEnhanceItem = firstChild.GetComponent<EnhanceItem>();

            lastChild.localPosition = firstChild.localPosition - (itemW + spacing) * Vector3.right;
            if (firstEnhanceItem.infoIndex > 0) lastEnhanceItem.RefreshSelf(firstEnhanceItem.infoIndex - 1);
            if (firstEnhanceItem.infoIndex == 0) lastEnhanceItem.RefreshSelf(itemNum - 1);
            lastChild.SetAsFirstSibling();

            resetAnimaFinish = false;
            float resetTime = 0.2f;
            for (int i = 0; i < showNum; i++)
            {
                int index = i;
                Transform child = content.GetChild(index);
                float targetPosX = oriPosX[index];
                child.DOMoveX(targetPosX, resetTime);
            }
            float timeCount = 0;
            DOTween.To(() => timeCount, a => timeCount = a, 1, resetTime).OnComplete(() => { resetAnimaFinish = true; isSmooth = true; });

            for (int i = 0; i < showNum; i++)
            {
                int index = i;
                Transform child = content.GetChild(index);
                child.GetComponentInChildren<Canvas>().sortingOrder = 99 - Mathf.Abs(index - showCenterIndex);
                float localScale = 1 - 0.1f * (showCenterIndex - index) * (index > showCenterIndex ? -1 : 1);
                child.DOScale(localScale, 0.1f);
            }
        }
        else
        {
            Transform rightChild = content.GetChild(showNum - 1);
            Transform leftChild = content.GetChild(0);

            leftChild.DOLocalMoveX(-1 * oriLocalPosX[1], 0.2f);
            rightChild.DOLocalMoveX(oriLocalPosX[0], 0.2f);

            leftChild.DOScale(TargetScale(oriLocalPosX[1]), 0.1f);
            rightChild.DOScale(TargetScale(oriLocalPosX[0]), 0.1f);

            rightChild.GetComponent<Canvas>().sortingOrder = 100;
            leftChild.GetComponent<Canvas>().sortingOrder = 90;
        }

        scrollComplete?.Invoke(infoCenterIndex);
    }
    public void RemovChild(Transform parent)
    {
        if (parent == null)
        {
            return;
        }
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }
}
