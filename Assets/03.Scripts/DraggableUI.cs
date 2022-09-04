using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform canvas;            // UI가 소속되어 있는 최상위 캔버스의 Transform
    private Transform previousParent;    // 해당 오브젝트가 직전에 소속되어있던 부모의 Transform
    private RectTransform rect;          // UI 위치 제어를 위한 RectTransform
    private CanvasGroup canvasGroup;     // UI의 알파값과 상호작용 제어를 위한 CanvasGroup

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 현재 오브젝트를 드래그하기 시작할 때 1회 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 직전에 소속되어 있던 부모 Transform 정보 버장
        previousParent = transform.parent;
        // 현재 드래그중인 UI가 화면의 최상단에 출력되도록 하기 위해
        transform.SetParent(canvas);  // 부모 오브젝트를 Canvas로 설정
        transform.SetAsLastSibling(); // 가장 앞에 보이도록 마지막 자식으로 설정 (Hierarchy 상에서 가장 뒤면 가장 앞)

        // 드래그 가능 오브젝트가 하나가 아닌 자식들을 가지고 있을 수 있기 때문에 CanvasGroup으로 통제
        // 알파값을 0.6으로 설정하여 반투명하게 보이게 하고 광선 충돌처리가 되지 않도록 처리
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    // 현재 오브젝트를 드래그 중일 때 매 프레임 호출
    public void OnDrag(PointerEventData eventData)
    {
        // 현재 스크린상의 마우스 위치를 UI 위치로 설정(UI가 마우스를 쫓아다니는 상태)
        rect.position = eventData.position;
    }

    // 현재 오브젝트의 드래그를 종료할 때 1회 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그를 시작하면 부모가 canvas로 설정되기 때문에
        // 드래그를 종료할 때 부모가 canvas이면 아이템 슬롯이 아닌 엉뚱한 곳에
        // 드롭을 했다는 뜻이기 때문에 드래그 직전에 소속되어 있던 곳으로 이동
        if(transform.parent == canvas)
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
