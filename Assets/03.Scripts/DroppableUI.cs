using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DroppableUI : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    public int myColIdx;
    public int myRowIdx;
    private Image image;
    private RectTransform rect;

    public bool CanDrop { get; set; }

    private void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        CanDrop = false;
    }

    // 마우스 포인터가 현재 영역으로 들어갈 때 1회 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 위치할 곳의 색상을 변경
        image.color = new Color(0, 255, 0, 0.3f);
    }

    // 마우스 포인터가 현재 영역을 빠져나갈 때 1회 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        // 색상을 하얀색으로 변경
        image.color = new Color(0, 0, 0, 0);
    }

    // 해당 영역에 드롭 하였을때 1회 호출
    public void OnDrop(PointerEventData eventData)
    {
        if(CanDrop == true)
        {
            // pointerDrag는 현재 드래그하고 있는 대상
            if (eventData.pointerDrag != null)
            {
                // 드래그하고 있는 대상의 부모를 현재 오브젝트로 설정하고, 위치를 현재 오브젝트 위치와 동일하게 설정
                eventData.pointerDrag.transform.SetParent(transform);
                eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
            }
            CanDrop = false;
        }
    }
}
