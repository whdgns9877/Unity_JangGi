using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragableSiyeon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform canvas;                // UI가 소속되어 있는 최상위 캔버스의 Transform
    private Transform previousParent;        // 해당 오브젝트가 직전에 소속되어있던 부모의 Transform
    private RectTransform rect;              // UI 위치 제어를 위한 RectTransform
    private CanvasGroup canvasGroup;         // UI의 알파값과 상호작용 제어를 위한 CanvasGroup

    private void Awake()
    {
        // 각 변수들에 해당하는 타입을 넣는다
        canvas = FindObjectOfType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent;
        // 현재 드래그중인 UI가 화면의 최상단에 출력되도록 하기 위해
        transform.SetParent(canvas);  // 부모 오브젝트를 Canvas로 설정
        transform.SetAsLastSibling(); // 가장 앞에 보이도록 마지막 자식으로 설정 (Hierarchy 상에서 가장 뒤면 가장 앞)

        // 드래그 가능 오브젝트가 하나가 아닌 자식들을 가지고 있을 수 있기 때문에 CanvasGroup으로 통제
        // 알파값을 0.6으로 설정하여 반투명하게 보이게 하고 광선 충돌처리가 되지 않도록 처리
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 현재 스크린상의 마우스 위치를 UI 위치로 설정(UI가 마우스를 쫓아다니는 상태)
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (transform.parent == canvas)
        {
            // 마지막 소속되었던 previous의 자식으로 설정하고, 해당 위치로 설정
            transform.SetParent(previousParent);
            rect.position = previousParent.GetComponent<RectTransform>().position;
        }

        // 완료되면 알파값을 1로 되돌려 반투명을 풀어주고 광선 처리도 다시 돌려준다
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
