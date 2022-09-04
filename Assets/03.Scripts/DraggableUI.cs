using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform canvas;            // UI�� �ҼӵǾ� �ִ� �ֻ��� ĵ������ Transform
    private Transform previousParent;    // �ش� ������Ʈ�� ������ �ҼӵǾ��ִ� �θ��� Transform
    private RectTransform rect;          // UI ��ġ ��� ���� RectTransform
    private CanvasGroup canvasGroup;     // UI�� ���İ��� ��ȣ�ۿ� ��� ���� CanvasGroup

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // ���� ������Ʈ�� �巡���ϱ� ������ �� 1ȸ ȣ��
    public void OnBeginDrag(PointerEventData eventData)
    {
        // �巡�� ������ �ҼӵǾ� �ִ� �θ� Transform ���� ����
        previousParent = transform.parent;
        // ���� �巡������ UI�� ȭ���� �ֻ�ܿ� ��µǵ��� �ϱ� ����
        transform.SetParent(canvas);  // �θ� ������Ʈ�� Canvas�� ����
        transform.SetAsLastSibling(); // ���� �տ� ���̵��� ������ �ڽ����� ���� (Hierarchy �󿡼� ���� �ڸ� ���� ��)

        // �巡�� ���� ������Ʈ�� �ϳ��� �ƴ� �ڽĵ��� ������ ���� �� �ֱ� ������ CanvasGroup���� ����
        // ���İ��� 0.6���� �����Ͽ� �������ϰ� ���̰� �ϰ� ���� �浹ó���� ���� �ʵ��� ó��
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    // ���� ������Ʈ�� �巡�� ���� �� �� ������ ȣ��
    public void OnDrag(PointerEventData eventData)
    {
        // ���� ��ũ������ ���콺 ��ġ�� UI ��ġ�� ����(UI�� ���콺�� �Ѿƴٴϴ� ����)
        rect.position = eventData.position;
    }

    // ���� ������Ʈ�� �巡�׸� ������ �� 1ȸ ȣ��
    public void OnEndDrag(PointerEventData eventData)
    {
        // �巡�׸� �����ϸ� �θ� canvas�� �����Ǳ� ������
        // �巡�׸� ������ �� �θ� canvas�̸� ������ ������ �ƴ� ������ ����
        // ����� �ߴٴ� ���̱� ������ �巡�� ������ �ҼӵǾ� �ִ� ������ �̵�
        if(transform.parent == canvas)
        {
            // ������ �ҼӵǾ��� previous�� �ڽ����� �����ϰ�, �ش� ��ġ�� ����
            transform.SetParent(previousParent);
            rect.position = previousParent.GetComponent<RectTransform>().position;
        }

        // �Ϸ�Ǹ� ���İ��� 1�� �ǵ��� �������� Ǯ���ְ� ���� ó���� �ٽ� �����ش�
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
