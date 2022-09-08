using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region 컴포넌트와 멤버변수
    // 보드인덱스들의 최대 최소 값을 const 상수값으로 지정
    const int BOARD_MINVALUE_ROW_AND_COL = 0;
    const int BOARD_MAXVALUE_ROW = 8;
    const int BOARD_MAXVALUE_COL = 9;

    private Transform canvas;                // UI가 소속되어 있는 최상위 캔버스의 Transform
    private Transform previousParent;        // 해당 오브젝트가 직전에 소속되어있던 부모의 Transform
    private RectTransform rect;              // UI 위치 제어를 위한 RectTransform
    private CanvasGroup canvasGroup;         // UI의 알파값과 상호작용 제어를 위한 CanvasGroup
    private DroppableUI[] boards;            // 기물들이 드롭될수 있는 보드들을 담는 배열
    private Transform[,] boardPos = new Transform[10, 9]; // 보드판의 위치를 담을 2차원 배열

    private List<Transform> canMoveBoardList = new List<Transform>(); // 이동 가능 보드의 Transform 을 담을 리스트
    private List<GameObject> showObj = new List<GameObject>();        // 이동 가능 보드 위에 시각적으로 보여줄 GameObject를 담는 리스트

    private string pickedSoldier = null; // 플레이어가 드래그를 시작하였을때 어느 기물인지를 저장할 string
    private int curPosColInBoard = 0;    // 현재 자기자신(기물) 의 행이 몇행인지 저장할 변수
    private int curPosRowInBoard = 0;    // 현재 자기자신(기물) 의 열이 몇열인지 저장할 변수
    #endregion

    // Awake 함수를 통해 해당 기물 정보 초기화
    private void Awake()
    {
        // 각 변수들에 해당하는 타입을 넣는다
        canvas = FindObjectOfType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 기물들이 드롭될수 있는 보드들을 담는 배열에 게임오브젝트의 이름이
        // Panel_Board인 것을 찾아 DroppableUI타입을 갖는 즉 보드들을 boards 배열에 넣는다
        boards = GameObject.Find("Panel_Board").GetComponentsInChildren<DroppableUI>();

        // 인자값을 위한 변수
        int index = 0;

        // 보드판의 행의개수 10
        for (int col = 0; col < boardPos.GetLength(0); col++)
        {
            // 보드판의 열의개수 9
            for (int row = 0; row < boardPos.GetLength(1); row++)
            {
                // 해당 위치에 Droppable스크립트가 붙어있는 보드들의 위치들을
                // 차곡차곡 넣어준다
                boardPos[col, row] = boards[index].transform;
                // 1차원배열의 값을 2차원 배열에 넣는것이기 때문에
                // 인자값 증가를 위한 count를 올려준다
                index++;
            }
        }

        // 현재 자신의 행과 열을 자신이 속해있는 보드의 Idx 값을 받아온다
        curPosColInBoard = GetComponentInParent<DroppableUI>().myColIdx;
        curPosRowInBoard = GetComponentInParent<DroppableUI>().myRowIdx;
    }

    // 현재 오브젝트를 드래그하기 시작할 때 1회 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        ShowCanMoveBoards();
        // 드래그 직전에 소속되어 있던 부모 Transform 정보 저장
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
        if (transform.parent == canvas)
        {
            // 마지막 소속되었던 previous의 자식으로 설정하고, 해당 위치로 설정
            transform.SetParent(previousParent);
            rect.position = previousParent.GetComponent<RectTransform>().position;
        }

        // 완료되면 알파값을 1로 되돌려 반투명을 풀어주고 광선 처리도 다시 돌려준다
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 이동 할때 구했던 정보들을 싹 정리해준다
        ClearInfos();

        // 그리고 현재 자신의 위치를 자신이 속한 부모인 보드의 위치로 저장한다
        curPosColInBoard = GetComponentInParent<DroppableUI>().myColIdx;
        curPosRowInBoard = GetComponentInParent<DroppableUI>().myRowIdx;
    }

    // 정보를 지우는 로직
    private void ClearInfos()
    {
        // 내가 움직이려는 기물의 string을 비우고
        pickedSoldier = null;
        // 해당 기물들의 각 로직에따라 이동가능보드의 리스트의 수만큼
        // 반복문을 돌아서
        for (int i = 0; i < canMoveBoardList.Count; i++)
        {
            // 해당 보드의 게임오브젝트에 달려있는 DroppableUI의 CanDrop 변수를 false로 바꿔준다
            canMoveBoardList[i].gameObject.GetComponent<DroppableUI>().CanDrop = false;
        }
        // 변수를 바꿔준후에 리스트를 비운다
        canMoveBoardList.Clear();

        // 해당 기물이 갈수 있는 위치에 임시적으로 생성해서 띄워주었던 오브젝트를
        for (int i = 0; i < showObj.Count; i++)
        {
            // 삭제한다
            Destroy(showObj[i].gameObject);
        }
        // 삭제 후에 해당 리스트를 비운다
        showObj.Clear();
    }

    // 각 기물들의 이름값을 통해 해당 기물들의
    // 이동로직을 실행하고 해당 기물이 움직일수 있는 위치에
    // 반투명 이미지를 생성하는 함수를 실행한다
    private void ShowCanMoveBoards()
    {
        pickedSoldier = gameObject.name;

        switch (pickedSoldier)
        {
            case "Han_King":
                KingAndSaLojic();
                break;

            case "Han_Cha":
                ChaLogic();
                break;

            case "Han_Po":
                PoLogic();
                break;

            case "Han_Ma":
                MaLogic();
                break;

            case "Han_Sang":
                SangLogic();
                break;

            case "Han_Sa":
                KingAndSaLojic();
                break;

            case "Han_Zzol":
                ZzolLogic();
                break;
        }
        // 반투명 이미지를 생성하는 함수
        CreateCanMovePosImage();
    }

    // 반투명 이미지를 생성하는 함수
    private void CreateCanMovePosImage()
    {
        // 로직에 따라 들어온 canMoveBoardList의 수만큼
        // 반복문을 돌아
        for (int i = 0; i < canMoveBoardList.Count; i++)
        {
            // 해당 위치 전부에 자기자신의 gameObject를 생성하고
            showObj.Add(Instantiate(gameObject, canMoveBoardList[i].transform));
            // 생성한 클론들의 알파값을 0.3으로 하여 반투명하게 만든다
            showObj[i].GetComponent<Image>().color = new Color(255, 255, 255, 0.3f);
        }
    }

    // 왕 과 사 의 움직임 로직
    private void KingAndSaLojic()
    {
        // 자신의 위치를 제외한 8방향의 보드들을 검사한다
        for (int dirX = -1; dirX <= 1; dirX++)
        {
            for (int dirY = -1; dirY <= 1; dirY++)
            {
                // 왕이 갈수 있는 위치의 최대수는 8이기 때문에 
                // 인자값이 8이면 9번째를 구하려는 것이므로
                // 인자값이 8일때 이동가능보드 구하는것을 멈춘다
                // 자기 자신 제외
                if (dirX == 0 && dirY == 0) continue;
                int col = curPosColInBoard + dirX;
                int row = curPosRowInBoard + dirY;

                // 내 위치에서 구하려는 값이 보드 안의 범위일때
                if (col >= BOARD_MINVALUE_ROW_AND_COL
                    && col <= BOARD_MAXVALUE_COL
                    && row >= BOARD_MINVALUE_ROW_AND_COL
                    && row <= BOARD_MAXVALUE_ROW)
                {
                    // 보드들에서 해당 보드가 KingPalace 태그라면
                    // 해당 보드는 왕이 이동 가능한 보드이므로 canMoveBoard에 넣어준다
                    if (boardPos[col, row].gameObject.CompareTag("KingPalace")
                        && CheckBoardHasNoChild(col, row))
                    {
                        canMoveBoardList.Add(boardPos[col, row]);
                        boardPos[col, row].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                    }

                    // 각 위치면 대각선으로 가지 못하기때문에 이동 가능범위에서 제외시킨다
                    if (((curPosColInBoard == 7) && (curPosRowInBoard == 4))
                        || ((curPosColInBoard == 9) && (curPosRowInBoard == 4)))
                    {
                        canMoveBoardList.Remove(boardPos[8, 3]);
                        boardPos[8, 3].gameObject.GetComponent<DroppableUI>().CanDrop = false;
                        canMoveBoardList.Remove(boardPos[8, 5]);
                        boardPos[8, 5].gameObject.GetComponent<DroppableUI>().CanDrop = false;
                    }

                    if (((curPosColInBoard == 8) && (curPosRowInBoard == 3))
                        || ((curPosColInBoard == 8) && (curPosRowInBoard == 5)))
                    {
                        canMoveBoardList.Remove(boardPos[7, 4]);
                        boardPos[7, 4].gameObject.GetComponent<DroppableUI>().CanDrop = false;
                        canMoveBoardList.Remove(boardPos[9, 4]);
                        boardPos[9, 4].gameObject.GetComponent<DroppableUI>().CanDrop = false;
                    }
                }
            }
        }
    }

    // 졸 의 움직임 로직
    private void ZzolLogic()
    {
        // 자기 자신의 위치기준으로 한칸 오른쪽이 범위 내 이면
        if (++curPosRowInBoard <= BOARD_MAXVALUE_ROW)
        {
            // 해당 보드에 기물이 있는지 확인하고
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard))
            {
                // 없는게 확인 됐으면 canMoveBoardList에 해당 보드를 추가하고
                canMoveBoardList.Add(boardPos[curPosColInBoard, curPosRowInBoard]);
                // 해당 보드의 CanDrop변수를 true로 설정한다
                boardPos[curPosColInBoard, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
            // 기물이 존재하여 위의 if문이 실행되지 않으면 다음 검사를 위해 값을 다시 내린다
            --curPosRowInBoard;
        } // 검사 범위를 아예 넘어간 상태이면 다음 검사를 위해 값을 내린다
        else --curPosRowInBoard;

        if (--curPosRowInBoard >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard))
            {
                canMoveBoardList.Add(boardPos[curPosColInBoard, curPosRowInBoard]);
                boardPos[curPosColInBoard, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
            ++curPosRowInBoard;
        }
        else ++curPosRowInBoard;

        if (--curPosColInBoard >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard))
            {
                canMoveBoardList.Add(boardPos[curPosColInBoard, curPosRowInBoard]);
                boardPos[curPosColInBoard, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
            ++curPosColInBoard;
        }
        else ++curPosColInBoard;
    }

    // 마 의 움직임 로직
    private void MaLogic()
    {
        // 현재 위치 기준 윗방향 체크
        if (curPosColInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard))
            {
                // 현재 마 기물의 위치에서 한칸 위 보드의 위치에 기물이 없을경우
                if (curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                {
                    // 해당 위치 기준에서 오른쪽
                    if (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW)
                    {
                        // 현재 마의 기준 위치에서 위로 두칸 오른쪽으로 한칸 위치에 기물이 있는지 검사
                        if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard + 1))
                        {
                            // 기물이 없이 비워져 있으면 이동가능 리스트에 넣고 해당 보드의 CanDrop 변수를 true로
                            canMoveBoardList.Add(boardPos[curPosColInBoard - 2, curPosRowInBoard + 1]);
                            boardPos[curPosColInBoard - 2, curPosRowInBoard + 1].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        } 
                    }

                    // 해당 위치 기준에서 왼쪽
                    if (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard - 1))
                        {
                            // 기물이 없이 비워져 있으면 이동가능 리스트에 넣고 해당 보드의 CanDrop 변수를 true로
                            canMoveBoardList.Add(boardPos[curPosColInBoard - 2, curPosRowInBoard - 1]);
                            boardPos[curPosColInBoard - 2, curPosRowInBoard - 1].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        } 
                    }
                }
            }
        }

        // 현재 위치 기준 아랫방향 체크
        if (curPosColInBoard + 1 <= BOARD_MAXVALUE_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard + 1, curPosRowInBoard))
            {
                if (curPosColInBoard + 2 <= BOARD_MAXVALUE_COL)
                {
                    if (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard + 2, curPosRowInBoard + 1))
                        {
                            canMoveBoardList.Add(boardPos[curPosColInBoard + 2, curPosRowInBoard + 1]);
                            boardPos[curPosColInBoard + 2, curPosRowInBoard + 1].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        }
                    }

                    if (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard + 2, curPosRowInBoard - 1))
                        {
                            canMoveBoardList.Add(boardPos[curPosColInBoard + 2, curPosRowInBoard - 1]);
                            boardPos[curPosColInBoard + 2, curPosRowInBoard - 1].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        }
                    }
                }
            }
        }

        // 현재 위치 기준 왼쪽방향 체크
        if (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard - 1))
            {
                if (curPosRowInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                {
                    if (curPosColInBoard + 1 <= BOARD_MAXVALUE_COL)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard + 1, curPosRowInBoard - 2))
                        {
                            canMoveBoardList.Add(boardPos[curPosColInBoard + 1, curPosRowInBoard - 2]);
                            boardPos[curPosColInBoard + 1, curPosRowInBoard - 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        }
                    }

                    if (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard - 2))
                        {
                            canMoveBoardList.Add(boardPos[curPosColInBoard -1, curPosRowInBoard - 2]);
                            boardPos[curPosColInBoard - 1, curPosRowInBoard - 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        }
                    }
                }
            }
        }

        // 현재 위치 기준 오른쪽방향 체크
        if (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW)
        {
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard + 1))
            {
                if (curPosRowInBoard + 2 <= BOARD_MAXVALUE_ROW)
                {
                    if (curPosColInBoard + 1 <= BOARD_MAXVALUE_COL)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard + 1, curPosRowInBoard + 2))
                        {
                            canMoveBoardList.Add(boardPos[curPosColInBoard + 1, curPosRowInBoard + 2]);
                            boardPos[curPosColInBoard + 1, curPosRowInBoard + 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        }
                    }

                    if (curPosColInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard + 2))
                        {
                            canMoveBoardList.Add(boardPos[curPosColInBoard - 1, curPosRowInBoard + 2]);
                            boardPos[curPosColInBoard - 1, curPosRowInBoard + 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        }
                    }
                }
            }
        }
    }

    // 상 의 움직임 로직
    private void SangLogic()
    {
        // 현재 위치 기준 윗방향 체크
        if (curPosColInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard))
            {
                // 현재 상 기물의 위치의 한칸 위에서 왼쪽위 보드를 검사한다 (즉 현재 상 기물의 위치에서 왼쪽으로 한칸 위로 두칸)
                if ((curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                    && (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL))
                {
                    // 해당 위치의 보드에 기물이 없다면
                    if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard - 1))
                    {
                        // 해당 위치에서 한번더 로직실행 (현재 상 기물의 위치에서 왼쪽으로 두칸 위로 세칸 위치 검사)
                        if ((curPosColInBoard - 3 >= BOARD_MINVALUE_ROW_AND_COL)
                            && (curPosRowInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL))
                        {
                            // 해당 위치의 보드에 기물이 없다면
                            if (CheckBoardHasNoChild(curPosColInBoard - 3, curPosRowInBoard - 2))
                            {
                                // 이동가능보드 리스트에 추가하고 해당 위치의 보드에 이동가능 변수를 true로 변경
                                canMoveBoardList.Add(boardPos[curPosColInBoard - 3, curPosRowInBoard - 2]);
                                boardPos[curPosColInBoard - 3, curPosRowInBoard - 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }

                // 현재 상 기물의 위치의 한칸 위에서 오른쪽 위 보드를 검사한다
                if ((curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                    && (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW))
                {
                    // 해당 위치의 보드에 기물이 없다면
                    if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard + 1))
                    {
                        // 해당 위치에서 한번더 로직실행 (현재 상 기물의 위치에서 오른쪽으로 두칸 위로 세칸 위치 검사)
                        if ((curPosColInBoard - 3 >= BOARD_MINVALUE_ROW_AND_COL)
                            && (curPosRowInBoard + 2 <= BOARD_MAXVALUE_ROW))
                        {
                            // 해당 위치의 보드에 기물이 없다면
                            if (CheckBoardHasNoChild(curPosColInBoard - 3, curPosRowInBoard + 2))
                            {
                                // 이동가능보드 리스트에 추가하고 해당 위치의 보드에 이동가능 변수를 true로 변경
                                canMoveBoardList.Add(boardPos[curPosColInBoard - 3, curPosRowInBoard + 2]);
                                boardPos[curPosColInBoard - 3, curPosRowInBoard + 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }
            }
        }

        // 현재 포지션 기준 아랫방향 체크
        if (curPosColInBoard + 1 <= BOARD_MAXVALUE_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard + 1, curPosRowInBoard))
            {
                if ((curPosColInBoard + 2 <= BOARD_MAXVALUE_COL)
                    && (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL))
                {
                    if (CheckBoardHasNoChild(curPosColInBoard + 2, curPosRowInBoard - 1))
                    {
                        if ((curPosColInBoard + 3 <= BOARD_MAXVALUE_COL)
                            && (curPosRowInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL))
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard + 3, curPosRowInBoard - 2))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard + 3, curPosRowInBoard - 2]);
                                boardPos[curPosColInBoard + 3, curPosRowInBoard - 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }

                if ((curPosColInBoard + 2 <= BOARD_MAXVALUE_COL)
                    && (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW))
                {
                    if (CheckBoardHasNoChild(curPosColInBoard + 2, curPosRowInBoard + 1))
                    {
                        if ((curPosColInBoard + 3 <= BOARD_MAXVALUE_COL)
                            && (curPosRowInBoard + 2 <= BOARD_MAXVALUE_ROW))
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard + 3, curPosRowInBoard + 2))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard + 3, curPosRowInBoard + 2]);
                                boardPos[curPosColInBoard + 3, curPosRowInBoard + 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }
            }
        }

        // 현재 포지션 기준 왼쪽 방향 체크
        if (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard - 1))
            {
                if ((curPosColInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
                    && (curPosRowInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL))
                {
                    if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard - 2))
                    {
                        if ((curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                            && (curPosRowInBoard - 3 >= BOARD_MINVALUE_ROW_AND_COL))
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard - 3))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard - 2, curPosRowInBoard - 3]);
                                boardPos[curPosColInBoard - 2, curPosRowInBoard - 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }

                if ((curPosColInBoard + 1 <= BOARD_MAXVALUE_COL)
                    && (curPosRowInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL))
                {
                    if (CheckBoardHasNoChild(curPosColInBoard + 1, curPosRowInBoard - 2))
                    {
                        if ((curPosColInBoard + 2 <= BOARD_MAXVALUE_COL)
                            && (curPosRowInBoard - 3 >= BOARD_MINVALUE_ROW_AND_COL))
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard + 2, curPosRowInBoard - 3))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard + 2, curPosRowInBoard - 3]);
                                boardPos[curPosColInBoard + 2, curPosRowInBoard - 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }
            }
        }

        // 현재 포지션 기준 오른쪽 방향 체크
        if (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW)
        {
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard + 1))
            {
                if ((curPosColInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
                    && (curPosRowInBoard + 2 <= BOARD_MAXVALUE_ROW))
                {
                    if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard + 2))
                    {
                        if ((curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                            && (curPosRowInBoard + 3 <= BOARD_MAXVALUE_ROW))
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard + 3))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard - 2, curPosRowInBoard + 3]);
                                boardPos[curPosColInBoard - 2, curPosRowInBoard + 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }

                if ((curPosColInBoard + 1 <= BOARD_MAXVALUE_COL)
                    && (curPosRowInBoard + 2 <= BOARD_MAXVALUE_ROW))
                {
                    if (CheckBoardHasNoChild(curPosColInBoard + 1, curPosRowInBoard + 2))
                    {
                        if ((curPosColInBoard + 2 <= BOARD_MAXVALUE_COL)
                            && (curPosRowInBoard + 3 <= BOARD_MAXVALUE_ROW))
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard + 2, curPosRowInBoard + 3))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard + 2, curPosRowInBoard + 3]);
                                boardPos[curPosColInBoard + 2, curPosRowInBoard + 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }
            }
        }
    }
    
    // 차 의 움직임 로직
    private void ChaLogic()
    {
        // 차의 현재위치 기준으로 위쪽을 맨 위 보드까지 검사한다
        for(int i = 1; i <= curPosColInBoard; i++)
        {
            if(curPosColInBoard - i >= BOARD_MINVALUE_ROW_AND_COL)
            {
                // i값을 높여가며 한칸씩 확인해서 기물이 없으면 계속 그 위칸을 계산한다
                if (CheckBoardHasNoChild(curPosColInBoard - i, curPosRowInBoard))
                {
                    // 검사한 보드가 비어있으면 canMoveBoardList에 해당 보드를 넣어주고 해당 위치의 보드의 CanDrop변수를 true로 설정
                    canMoveBoardList.Add(boardPos[curPosColInBoard - i, curPosRowInBoard]);
                    boardPos[curPosColInBoard - i, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
                else
                    break;
            }
        }

        // 차의 현재위치 기준으로 아래쪽을 맨 아래 보드까지 검사한다
        for (int i = 1; i <= BOARD_MAXVALUE_COL - curPosColInBoard; i++)
        {
            if (curPosColInBoard + i <= BOARD_MAXVALUE_COL)
            {
                if (CheckBoardHasNoChild(curPosColInBoard + i, curPosRowInBoard))
                {
                    canMoveBoardList.Add(boardPos[curPosColInBoard + i, curPosRowInBoard]);
                    boardPos[curPosColInBoard + i, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
                else
                    break;
            }
        }

        // 차의 현재위치 기준으로 왼쪽을 맨 왼 보드까지 검사한다
        for (int i = 1; i <= curPosRowInBoard; i++)
        {
            if (curPosRowInBoard - i >= BOARD_MINVALUE_ROW_AND_COL)
            {
                // i값을 높여가며 한칸씩 확인해서 기물이 없으면 계속 그 위칸을 계산한다
                if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard - i))
                {
                    // 검사한 보드가 비어있으면 canMoveBoardList에 해당 보드를 넣어주고 해당 위치의 보드의 CanDrop변수를 true로 설정
                    canMoveBoardList.Add(boardPos[curPosColInBoard, curPosRowInBoard - i]);
                    boardPos[curPosColInBoard, curPosRowInBoard - i].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
                else
                    break;
            }
        }

        // 차의 현재위치 기준으로 오른쪽을 맨 오른 보드까지 검사한다
        for (int i = 1; i <= BOARD_MAXVALUE_ROW - curPosRowInBoard; i++)
        {
            if (curPosRowInBoard + i <= BOARD_MAXVALUE_COL)
            {
                // i값을 높여가며 한칸씩 확인해서 기물이 없으면 계속 그 위칸을 계산한다
                if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard + i))
                {
                    // 검사한 보드가 비어있으면 canMoveBoardList에 해당 보드를 넣어주고 해당 위치의 보드의 CanDrop변수를 true로 설정
                    canMoveBoardList.Add(boardPos[curPosColInBoard, curPosRowInBoard + i]);
                    boardPos[curPosColInBoard, curPosRowInBoard + i].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
                else
                    break;
            }
        }

        // 차의 현재위치가 궁 안일때의 로직
        // 궁 안에서는 모든 위치에서 중앙에 가능하므로 해당보드에
        // 기물이 있는지 확인 후 없으면 리스트들에 추가해준다
        if (gameObject.GetComponentInParent<DroppableUI>().CompareTag("KingPalace"))
        {
            if (CheckBoardHasNoChild(8,4) && curPosColInBoard != 8 && curPosRowInBoard != 4)
            {
                canMoveBoardList.Add(boardPos[8, 4]);
                boardPos[8, 4].gameObject.GetComponent<DroppableUI>().CanDrop = true; 
            }
        }

        // 현재 위치가 7,3 (궁에서의 왼쪽 위)
        if (curPosColInBoard == 7 && curPosRowInBoard == 3)
        {
            // 이동 가능 범위에 기물이 있는지 확인하고
            // 해당 보드가 비어있으면 canMoveBoardList와 boardPos에 해당 보드를 추가한다
            // 궁에서의 오른쪽 아래 위치
            if (CheckBoardHasNoChild(9, 5))
            {
                canMoveBoardList.Add(boardPos[9, 5]);
                boardPos[9, 5].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
        }

        // 현재 위치가 7,5 (궁에서의 오른쪽 위)
        if (curPosColInBoard == 7 && curPosRowInBoard == 5)
        {
            // 이동 가능 범위에 기물이 있는지 확인하고
            // 해당 보드가 비어있으면 canMoveBoardList와 boardPos에 해당 보드를 추가한다
            // 궁에서의 왼쪽 아래 위치
            if (CheckBoardHasNoChild(9, 3))
            {
                canMoveBoardList.Add(boardPos[9, 3]);
                boardPos[9, 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
        }

        // 현재 위치가 9,3 (궁에서의 왼쪽 아래)
        if (curPosColInBoard == 9 && curPosRowInBoard == 3)
        {
            // 이동 가능 범위에 기물이 있는지 확인하고
            // 해당 보드가 비어있으면 canMoveBoardList와 boardPos에 해당 보드를 추가한다
            // 궁에서의 오른쪽 위 위치
            if (CheckBoardHasNoChild(7, 5))
            {
                canMoveBoardList.Add(boardPos[7, 5]);
                boardPos[7, 5].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
        }

        // 현재 위치가 9,3 (궁에서의 오른쪽 아래)
        if (curPosColInBoard == 9 && curPosRowInBoard == 5)
        {
            // 이동 가능 범위에 기물이 있는지 확인하고
            // 해당 보드가 비어있으면 canMoveBoardList와 boardPos에 해당 보드를 추가한다
            // 궁에서의 왼쪽 위 위치
            if (CheckBoardHasNoChild(7, 3))
            {
                canMoveBoardList.Add(boardPos[7, 3]);
                boardPos[7, 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
        }

        // 현재 위치가 8,4 (궁의 중앙)
        if(curPosColInBoard == 8 && curPosRowInBoard == 4)
        {
            // 궁에서 왼쪽 위
            if (CheckBoardHasNoChild(7, 3))
            {
                canMoveBoardList.Add(boardPos[7, 3]);
                boardPos[7, 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }

            // 궁에서 오른쪽 위
            if (CheckBoardHasNoChild(7, 5))
            {
                canMoveBoardList.Add(boardPos[7, 5]);
                boardPos[7, 5].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }

            // 궁에서 왼쪽 아래
            if (CheckBoardHasNoChild(9, 3))
            {
                canMoveBoardList.Add(boardPos[9, 3]);
                boardPos[9, 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }

            // 궁에서 오른쪽 아래
            if (CheckBoardHasNoChild(9, 5))
            {
                canMoveBoardList.Add(boardPos[9, 5]);
                boardPos[9, 5].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
        }
    }

    // 포 의 움직임 로직
    private void PoLogic()
    {
        // 포의 현재 위치에서 위쪽을 맨 위 보드까지 검사
        for(int i = 1; i <= curPosColInBoard; i++)
        {
            if(curPosColInBoard - i >= BOARD_MINVALUE_ROW_AND_COL)
            {
                // i값을 높여가며 한칸씩 확인해서 기물이 없으면 계속 그 위칸을 계산한다
                // 검사한 보드에 자식이 있다면 (포는 기물 하나를 뛰어넘어 이동해야한다)
                // 그리고 해당 보드의 자식이 포가 아니라면 (포 끼리는 뛰어넘지 못한다)
                if (!CheckBoardHasNoChild(curPosColInBoard - i, curPosRowInBoard)
                    && !boardPos[curPosColInBoard - i, curPosRowInBoard].gameObject.GetComponentInChildren<DraggableUI>().gameObject.CompareTag("Po"))
                {
                    // 해당 위치에서부터 차로직의 위쪽부분을 적용한다
                    // 차의 현재위치 기준으로 위쪽을 맨 위 보드까지 검사한다
                    int pivotColBoard = curPosColInBoard - i;

                    for (int j = 1; j <= BOARD_MAXVALUE_COL; j++)
                    {
                        if (pivotColBoard - j >= BOARD_MINVALUE_ROW_AND_COL)
                        {
                            // i값을 높여가며 한칸씩 확인해서 기물이 없으면 계속 그 위칸을 계산한다
                            if (CheckBoardHasNoChild(pivotColBoard - j, curPosRowInBoard))
                            {
                                // 검사한 보드가 비어있으면 canMoveBoardList에 해당 보드를 넣어주고 해당 위치의 보드의 CanDrop변수를 true로 설정
                                canMoveBoardList.Add(boardPos[pivotColBoard - j, curPosRowInBoard]);
                                boardPos[pivotColBoard - j, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                            else
                                break;
                        }
                    }
                    // 기준 위치에서 차 로직을 실행했으면 여기서 이제 위쪽 검사는 끝내야한다
                    break;
                }
            }
        }

        // 포의 현재 위치에서 아래쪽을 맨 아래 보드까지 검사
        for (int i = 1; i <= BOARD_MAXVALUE_COL - curPosColInBoard; i++)
        {
            if (curPosColInBoard + i <= BOARD_MAXVALUE_COL)
            {
                if (!CheckBoardHasNoChild(curPosColInBoard + i, curPosRowInBoard)
                    && !boardPos[curPosColInBoard + i, curPosRowInBoard].gameObject.GetComponentInChildren<DraggableUI>().gameObject.CompareTag("Po"))
                {
                    int pivotColBoard = curPosColInBoard + i;

                    for (int j = 1; j <= BOARD_MAXVALUE_COL; j++)
                    {
                        if (pivotColBoard + j <= BOARD_MAXVALUE_COL)
                        {
                            if (CheckBoardHasNoChild(pivotColBoard + j, curPosRowInBoard))
                            {
                                canMoveBoardList.Add(boardPos[pivotColBoard + j, curPosRowInBoard]);
                                boardPos[pivotColBoard + j, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                            else
                                break;
                        }
                    }
                    break;
                }
            }
        }

        // 포의 현재 위치에서 왼쪽을 맨 왼 보드까지 검사
        for (int i = 1; i <= curPosRowInBoard; i++)
        {
            if (curPosRowInBoard - i >= BOARD_MINVALUE_ROW_AND_COL)
            {
                if (!CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard - i)
                    && !boardPos[curPosColInBoard, curPosRowInBoard - i].gameObject.GetComponentInChildren<DraggableUI>().gameObject.CompareTag("Po"))
                {
                    int pivotRowBoard = curPosRowInBoard - i;

                    for (int j = 1; j <= BOARD_MAXVALUE_ROW; j++)
                    {
                        if (pivotRowBoard - j >= BOARD_MINVALUE_ROW_AND_COL)
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard, pivotRowBoard - j))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard, pivotRowBoard - j]);
                                boardPos[curPosColInBoard, pivotRowBoard - j].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                            else
                                break;
                        }
                    }
                    break;
                }
            }
        }

        // 포의 현재 위치에서 오른쪽을 맨 오른 보드까지 검사
        for (int i = 1; i <= BOARD_MAXVALUE_ROW - curPosRowInBoard; i++)
        {
            if (curPosRowInBoard + i <= BOARD_MAXVALUE_ROW)
            {
                if (!CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard + i)
                    && !boardPos[curPosColInBoard, curPosRowInBoard + i].gameObject.GetComponentInChildren<DraggableUI>().gameObject.CompareTag("Po"))
                {
                    int pivotRowBoard = curPosRowInBoard + i;

                    for (int j = 1; j <= BOARD_MAXVALUE_ROW; j++)
                    {
                        if (pivotRowBoard + j <= BOARD_MAXVALUE_ROW)
                        {
                            if (CheckBoardHasNoChild(curPosColInBoard, pivotRowBoard + j))
                            {
                                canMoveBoardList.Add(boardPos[curPosColInBoard, pivotRowBoard + j]);
                                boardPos[curPosColInBoard, pivotRowBoard + j].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                            else
                                break;
                        }
                    }
                    break;
                }
            }
        }

        // 포의 궁 안에서의 처리
        if (gameObject.GetComponentInParent<DroppableUI>().CompareTag("KingPalace"))
        {
            // 현재 포의 위치가 궁에서의 왼쪽 위
            if(curPosColInBoard == 7 && curPosRowInBoard == 3)
            {
                // 궁에서 오른쪽 아래 보드에 기물이 없을때
                if (CheckBoardHasNoChild(9, 5))
                {
                    canMoveBoardList.Add(boardPos[9, 5]);
                    boardPos[9, 5].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
            }

            // 현재 포의 위치가 궁에서의 오른쪽 위
            if (curPosColInBoard == 7 && curPosRowInBoard == 5)
            {
                // 궁에서 왼쪽 아래 보드에 기물이 없을때
                if (CheckBoardHasNoChild(9, 3))
                {
                    canMoveBoardList.Add(boardPos[9, 3]);
                    boardPos[9, 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
            }

            // 현재 포의 위치가 궁에서의 왼쪽 아래
            if (curPosColInBoard == 9 && curPosRowInBoard == 3)
            {
                // 궁에서 오른쪽 위 보드에 기물이 없을때
                if (CheckBoardHasNoChild(7, 5))
                {
                    canMoveBoardList.Add(boardPos[7, 5]);
                    boardPos[7, 5].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
            }

            // 현재 포의 위치가 궁에서의 오른쪽 아래
            if (curPosColInBoard == 9 && curPosRowInBoard == 5)
            {
                // 궁에서 왼쪽 위 보드에 기물이 없을때
                if (CheckBoardHasNoChild(7, 3))
                {
                    canMoveBoardList.Add(boardPos[7, 3]);
                    boardPos[7, 3].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                }
            }
        }
    }

    // 매개변수로 행 과 열을 받아 해당보드에 자식이 없는지를 알려줄 함수
    private bool CheckBoardHasNoChild(int col, int row)
    {
        // 해당 위치의 보드에 자식이 있는지를 확인하여 null이면 자식오브젝트가 없는것이므로 true
        if (boardPos[col, row].gameObject.GetComponentInChildren<DraggableUI>() == null)
            return true;
        // 반대 경우엔 DroppableUI 타입을 가진 자식 즉 기물이 존재하는 것이므로 false 반환
        else
            return false;
    }
}
