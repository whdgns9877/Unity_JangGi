using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropableSiyeon : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    private Image image;
    private RectTransform rect;

    private void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 위치할 곳의 색상을 변경
        image.color = new Color(255, 0, 0, 0.3f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 색상을 투명하게 변경
        image.color = new Color(0, 0, 0, 0);
    }

    public void OnDrop(PointerEventData eventData)
    {
        // pointerDrag는 현재 드래그하고 있는 대상
        if (eventData.pointerDrag != null)
        {
            // 드래그하고 있는 대상의 부모를 현재 오브젝트로 설정하고, 위치를 현재 오브젝트 위치와 동일하게 설정
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
        }
    }
}
