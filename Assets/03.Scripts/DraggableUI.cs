using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // �����ε������� �ִ� �ּ� ���� const ��������� ����
    const int BOARD_MINVALUE_ROW_AND_COL = 0;
    const int BOARD_MAXVALUE_ROW = 8;
    const int BOARD_MAXVALUE_COL = 9;

    private Transform canvas;                // UI�� �ҼӵǾ� �ִ� �ֻ��� ĵ������ Transform
    private Transform previousParent;        // �ش� ������Ʈ�� ������ �ҼӵǾ��ִ� �θ��� Transform
    private RectTransform rect;              // UI ��ġ ��� ���� RectTransform
    private CanvasGroup canvasGroup;         // UI�� ���İ��� ��ȣ�ۿ� ��� ���� CanvasGroup
    private DroppableUI[] boards;            // �⹰���� ��ӵɼ� �ִ� ������� ��� �迭
    private Transform[,] boardPos = new Transform[10, 9]; // �������� ��ġ�� ���� 2���� �迭

    private List<Transform> canMoveBoardList = new List<Transform>(); // �̵� ���� ������ Transform �� ���� ����Ʈ
    private List<GameObject> showObj = new List<GameObject>();        // �̵� ���� ���� ���� �ð������� ������ GameObject�� ��� ����Ʈ

    private string pickedSoldier = null; // �÷��̾ �巡�׸� �����Ͽ����� ��� �⹰������ ������ string
    private int curPosColInBoard = 0;    // ���� �ڱ��ڽ�(�⹰) �� ���� �������� ������ ����
    private int curPosRowInBoard = 0;    // ���� �ڱ��ڽ�(�⹰) �� ���� ����� ������ ����

    private void Awake()
    {
        // �� �����鿡 �ش��ϴ� Ÿ���� �ִ´�
        canvas = FindObjectOfType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // �⹰���� ��ӵɼ� �ִ� ������� ��� �迭�� ���ӿ�����Ʈ�� �̸���
        // Panel_Board�� ���� ã�� DroppableUIŸ���� ���� �� ������� boards �迭�� �ִ´�
        boards = GameObject.Find("Panel_Board").GetComponentsInChildren<DroppableUI>();

        // ���ڰ��� ���� ����
        int index = 0;

        // �������� ���ǰ��� 10
        for (int col = 0; col < boardPos.GetLength(0); col++)
        {
            // �������� ���ǰ��� 9
            for (int row = 0; row < boardPos.GetLength(1); row++)
            {
                // �ش� ��ġ�� Droppable��ũ��Ʈ�� �پ��ִ� ������� ��ġ����
                // �������� �־��ش�
                boardPos[col, row] = boards[index].transform;
                // 1�����迭�� ���� 2���� �迭�� �ִ°��̱� ������
                // ���ڰ� ������ ���� count�� �÷��ش�
                index++;
            }
        }

        // ���� �ڽ��� ��� ���� �ڽ��� �����ִ� ������ Idx ���� �޾ƿ´�
        curPosColInBoard = GetComponentInParent<DroppableUI>().myColIdx;
        curPosRowInBoard = GetComponentInParent<DroppableUI>().myRowIdx;
    }

    // ���� ������Ʈ�� �巡���ϱ� ������ �� 1ȸ ȣ��
    public void OnBeginDrag(PointerEventData eventData)
    {
        ShowCanMoveBoards();
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
        if (transform.parent == canvas)
        {
            // ������ �ҼӵǾ��� previous�� �ڽ����� �����ϰ�, �ش� ��ġ�� ����
            transform.SetParent(previousParent);
            rect.position = previousParent.GetComponent<RectTransform>().position;
        }

        // �Ϸ�Ǹ� ���İ��� 1�� �ǵ��� �������� Ǯ���ְ� ���� ó���� �ٽ� �����ش�
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // �̵� �Ҷ� ���ߴ� �������� �� �������ش�
        ClearInfos();

        // �׸��� ���� �ڽ��� ��ġ�� �ڽ��� ���� �θ��� ������ ��ġ�� �����Ѵ�
        curPosColInBoard = GetComponentInParent<DroppableUI>().myColIdx;
        curPosRowInBoard = GetComponentInParent<DroppableUI>().myRowIdx;
    }

    // ������ ����� ����
    private void ClearInfos()
    {
        // ���� �����̷��� �⹰�� string�� ����
        pickedSoldier = null;
        // �ش� �⹰���� �� ���������� �̵����ɺ����� ����Ʈ�� ����ŭ
        // �ݺ����� ���Ƽ�
        for (int i = 0; i < canMoveBoardList.Count; i++)
        {
            // �ش� ������ ���ӿ�����Ʈ�� �޷��ִ� DroppableUI�� CanDrop ������ false�� �ٲ��ش�
            canMoveBoardList[i].gameObject.GetComponent<DroppableUI>().CanDrop = false;
        }
        // ������ �ٲ����Ŀ� ����Ʈ�� ����
        canMoveBoardList.Clear();

        // �ش� �⹰�� ���� �ִ� ��ġ�� �ӽ������� �����ؼ� ����־��� ������Ʈ��
        for (int i = 0; i < showObj.Count; i++)
        {
            // �����Ѵ�
            Destroy(showObj[i].gameObject);
        }
        // ���� �Ŀ� �ش� ����Ʈ�� ����
        showObj.Clear();
    }

    // �� �⹰���� �̸����� ���� �ش� �⹰����
    // �̵������� �����ϰ� �ش� �⹰�� �����ϼ� �ִ� ��ġ��
    // ������ �̹����� �����ϴ� �Լ��� �����Ѵ�
    private void ShowCanMoveBoards()
    {
        pickedSoldier = gameObject.name;

        switch (pickedSoldier)
        {
            case "Han_King":
                KingAndSaLojic();
                break;

            case "Han_Cha":
                break;

            case "Han_Po":
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
        // ������ �̹����� �����ϴ� �Լ�
        CreateCanMovePosImage();
    }

    // ������ �̹����� �����ϴ� �Լ�
    private void CreateCanMovePosImage()
    {
        // ������ ���� ���� canMoveBoardList�� ����ŭ
        // �ݺ����� ����
        for (int i = 0; i < canMoveBoardList.Count; i++)
        {
            // �ش� ��ġ ���ο� �ڱ��ڽ��� gameObject�� �����ϰ�
            showObj.Add(Instantiate(gameObject, canMoveBoardList[i].transform));
            // ������ Ŭ�е��� ���İ��� 0.3���� �Ͽ� �������ϰ� �����
            showObj[i].GetComponent<Image>().color = new Color(255, 255, 255, 0.3f);
        }
    }

    // �� �� �� �� ������ ����
    private void KingAndSaLojic()
    {
        // �ڽ��� ��ġ�� ������ 8������ ������� �˻��Ѵ�
        for (int dirX = -1; dirX <= 1; dirX++)
        {
            for (int dirY = -1; dirY <= 1; dirY++)
            {
                // ���� ���� �ִ� ��ġ�� �ִ���� 8�̱� ������ 
                // ���ڰ��� 8�̸� 9��°�� ���Ϸ��� ���̹Ƿ�
                // ���ڰ��� 8�϶� �̵����ɺ��� ���ϴ°��� �����
                // �ڱ� �ڽ� ����
                if (dirX == 0 && dirY == 0) continue;
                int col = curPosColInBoard + dirX;
                int row = curPosRowInBoard + dirY;

                // �� ��ġ���� ���Ϸ��� ���� ���� ���� �����϶�
                if (col >= BOARD_MINVALUE_ROW_AND_COL
                    && col <= BOARD_MAXVALUE_COL
                    && row >= BOARD_MINVALUE_ROW_AND_COL
                    && row <= BOARD_MAXVALUE_ROW)
                {
                    // ����鿡�� �ش� ���尡 KingPalace �±׶��
                    // �ش� ����� ���� �̵� ������ �����̹Ƿ� canMoveBoard�� �־��ش�
                    if (boardPos[col, row].gameObject.CompareTag("KingPalace")
                        && CheckBoardHasNoChild(col, row))
                    {
                        canMoveBoardList.Add(boardPos[col, row]);
                        boardPos[col, row].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                    }

                    // �� ��ġ�� �밢������ ���� ���ϱ⶧���� �̵� ���ɹ������� ���ܽ�Ų��
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

    // �� �� ������ ����
    private void ZzolLogic()
    {
        // �ڱ� �ڽ��� ��ġ�������� ��ĭ �������� ���� �� �̸�
        if (++curPosRowInBoard <= BOARD_MAXVALUE_ROW)
        {
            // �ش� ���忡 �⹰�� �ִ��� Ȯ���ϰ�
            if (CheckBoardHasNoChild(curPosColInBoard, curPosRowInBoard))
            {
                // ���°� Ȯ�� ������ canMoveBoardList�� �ش� ���带 �߰��ϰ�
                canMoveBoardList.Add(boardPos[curPosColInBoard, curPosRowInBoard]);
                // �ش� ������ CanDrop������ true�� �����Ѵ�
                boardPos[curPosColInBoard, curPosRowInBoard].gameObject.GetComponent<DroppableUI>().CanDrop = true;
            }
            // �⹰�� �����Ͽ� ���� if���� ������� ������ ���� �˻縦 ���� ���� �ٽ� ������
            --curPosRowInBoard;
        } // �˻� ������ �ƿ� �Ѿ �����̸� ���� �˻縦 ���� ���� ������
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

    private void MaLogic()
    {
        // ���� ��ġ ���� ������ üũ
        if (curPosColInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard))
            {
                // ���� �� �⹰�� ��ġ���� ��ĭ �� ������ ��ġ�� �⹰�� �������
                if (curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                {
                    // �ش� ��ġ ���ؿ��� ������
                    if (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW)
                    {
                        // ���� ���� ���� ��ġ���� ���� ��ĭ ���������� ��ĭ ��ġ�� �⹰�� �ִ��� �˻�
                        if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard + 1))
                        {
                            // �⹰�� ���� ����� ������ �̵����� ����Ʈ�� �ְ� �ش� ������ CanDrop ������ true��
                            canMoveBoardList.Add(boardPos[curPosColInBoard - 2, curPosRowInBoard + 1]);
                            boardPos[curPosColInBoard - 2, curPosRowInBoard + 1].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        } 
                    }

                    // �ش� ��ġ ���ؿ��� ����
                    if (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
                    {
                        if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard - 1))
                        {
                            // �⹰�� ���� ����� ������ �̵����� ����Ʈ�� �ְ� �ش� ������ CanDrop ������ true��
                            canMoveBoardList.Add(boardPos[curPosColInBoard - 2, curPosRowInBoard - 1]);
                            boardPos[curPosColInBoard - 2, curPosRowInBoard - 1].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                        } 
                    }
                }
            }
        }

        // ���� ��ġ ���� �Ʒ����� üũ
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

        // ���� ��ġ ���� ���ʹ��� üũ
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

        // ���� ��ġ ���� �����ʹ��� üũ
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

    private void SangLogic()
    {
        // ���� ��ġ ���� ������ üũ
        if (curPosColInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL)
        {
            if (CheckBoardHasNoChild(curPosColInBoard - 1, curPosRowInBoard))
            {
                // ���� �� �⹰�� ��ġ�� ��ĭ ������ ������ ���带 �˻��Ѵ� (�� ���� �� �⹰�� ��ġ���� �������� ��ĭ ���� ��ĭ)
                if ((curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                    && (curPosRowInBoard - 1 >= BOARD_MINVALUE_ROW_AND_COL))
                {
                    // �ش� ��ġ�� ���忡 �⹰�� ���ٸ�
                    if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard - 1))
                    {
                        // �ش� ��ġ���� �ѹ��� �������� (���� �� �⹰�� ��ġ���� �������� ��ĭ ���� ��ĭ ��ġ �˻�)
                        if ((curPosColInBoard - 3 >= BOARD_MINVALUE_ROW_AND_COL)
                            && (curPosRowInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL))
                        {
                            // �ش� ��ġ�� ���忡 �⹰�� ���ٸ�
                            if (CheckBoardHasNoChild(curPosColInBoard - 3, curPosRowInBoard - 2))
                            {
                                // �̵����ɺ��� ����Ʈ�� �߰��ϰ� �ش� ��ġ�� ���忡 �̵����� ������ true�� ����
                                canMoveBoardList.Add(boardPos[curPosColInBoard - 3, curPosRowInBoard - 2]);
                                boardPos[curPosColInBoard - 3, curPosRowInBoard - 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }

                // ���� �� �⹰�� ��ġ�� ��ĭ ������ ������ �� ���带 �˻��Ѵ�
                if ((curPosColInBoard - 2 >= BOARD_MINVALUE_ROW_AND_COL)
                    && (curPosRowInBoard + 1 <= BOARD_MAXVALUE_ROW))
                {
                    // �ش� ��ġ�� ���忡 �⹰�� ���ٸ�
                    if (CheckBoardHasNoChild(curPosColInBoard - 2, curPosRowInBoard + 1))
                    {
                        // �ش� ��ġ���� �ѹ��� �������� (���� �� �⹰�� ��ġ���� ���������� ��ĭ ���� ��ĭ ��ġ �˻�)
                        if ((curPosColInBoard - 3 >= BOARD_MINVALUE_ROW_AND_COL)
                            && (curPosRowInBoard + 2 <= BOARD_MAXVALUE_ROW))
                        {
                            // �ش� ��ġ�� ���忡 �⹰�� ���ٸ�
                            if (CheckBoardHasNoChild(curPosColInBoard - 3, curPosRowInBoard + 2))
                            {
                                // �̵����ɺ��� ����Ʈ�� �߰��ϰ� �ش� ��ġ�� ���忡 �̵����� ������ true�� ����
                                canMoveBoardList.Add(boardPos[curPosColInBoard - 3, curPosRowInBoard + 2]);
                                boardPos[curPosColInBoard - 3, curPosRowInBoard + 2].gameObject.GetComponent<DroppableUI>().CanDrop = true;
                            }
                        }
                    }
                }
            }
        }

        // ���� ������ ���� �Ʒ����� üũ
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

        // ���� ������ ���� ���� ���� üũ
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

        // ���� ������ ���� ������ ���� üũ
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

    // �Ű������� �� �� ���� �޾�
    private bool CheckBoardHasNoChild(int col, int row)
    {
        // �ش� ��ġ�� ���忡 �ڽ��� �ִ����� Ȯ���Ͽ� null�̸� �ڽĿ�����Ʈ�� ���°��̹Ƿ� true
        if (boardPos[col, row].gameObject.GetComponentInChildren<DraggableUI>() == null)
            return true;
        // �ݴ� ��쿣 DroppableUI Ÿ���� ���� �ڽ� �� �⹰�� �����ϴ� ���̹Ƿ� false ��ȯ
        else
            return false;
    }
}
