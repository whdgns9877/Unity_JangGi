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

    // ���콺 �����Ͱ� ���� �������� �� �� 1ȸ ȣ��
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ���콺 ��ġ�� ���� ������ ����
        image.color = new Color(0, 255, 0, 0.3f);
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
        if(CanDrop == true)
        {
            // pointerDrag�� ���� �巡���ϰ� �ִ� ���
            if (eventData.pointerDrag != null)
            {
                // �巡���ϰ� �ִ� ����� �θ� ���� ������Ʈ�� �����ϰ�, ��ġ�� ���� ������Ʈ ��ġ�� �����ϰ� ����
                eventData.pointerDrag.transform.SetParent(transform);
                eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
            }
            CanDrop = false;
        }
    }
}
