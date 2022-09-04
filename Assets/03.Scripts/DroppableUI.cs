using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DroppableUI : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    private Image image;
    private RectTransform rect;

    private void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    // ���콺 �����Ͱ� ���� �������� �� �� 1ȸ ȣ��
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ���콺 ��ġ�� ���� ������ ����
        image.color = Color.yellow;
    }

    // ���콺 �����Ͱ� ���� ������ �������� �� 1ȸ ȣ��
    public void OnPointerExit(PointerEventData eventData)
    {
        // ������ �Ͼ������ ����
        image.color = new Color(0, 0, 0, 0);
    }

    // �ش� ������ ��� �Ͽ����� 1ȸ ȣ��
    public void OnDrop(PointerEventData eventData)
    {
        // pointerDrag�� ���� �巡���ϰ� �ִ� ���
        if(eventData.pointerDrag != null)
        {
            // �巡���ϰ� �ִ� ����� �θ� ���� ������Ʈ�� �����ϰ�, ��ġ�� ���� ������Ʈ ��ġ�� �����ϰ� ����
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
        }
    }
}
