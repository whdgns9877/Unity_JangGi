using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;      // ���� ����� ����
using Photon.Realtime; //      ||
using UnityEngine.UI;  // UI ����� ���� 


public class GameManager : MonoBehaviourPunCallbacks
{
    #region �̱���
    static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }
    }
    #endregion

    #region ���� UI�鿡 ������ ������
    public Text ConnectionStatus;      // ���� ��Ʈ��ũ�� ���¸� ��Ÿ���� �ؽ�Ʈ
    public Text IDtext;                // �÷��̾ �Է��Ͽ� ���� �г��� �ؽ�Ʈ
    public Text Warningtext;           // �г��� ���Ŀ��� ����� ��� ��� �ؽ�Ʈ
    public Text RoomName;              // �� Ÿ��Ʋ�� ��Ÿ�� �ؽ�Ʈ
    public Text[] playersNickNameInRoom = new Text[2]; // �濡 �����ִ� �÷��̾���� �г����� ���� �ؽ�Ʈ �迭

    public InputField RoomInputFieldText; // ������ ��ǲ�ʵ�
    public InputField IDInputField;       // ID ��ǲ�ʵ�

    public Button connectButton;       // ���� ��ư�� ���� ������ ���� ���� ��ư
    public GameObject RoomPanel;       // �濡 �����ϸ� ����� �ǳ�
    public GameObject LoginPanel;      // �α��ΰ� �� �������� ���������� ���� �ǳ�
    public GameObject Lobby;           // �κ� ���� ������ ���� �ǳ�
    public GameObject CreateRoomPanel; // ���� �����ϴµ� ���� �ǳ�
    public GameObject CreateRoomPanelButton;    // ����� �ǳ��� ����� ��ư
    public GameObject CreateButton;    // ���� �����ϴ� ��ư
    public GameObject WarningPanel;    // ��� �ؽ�Ʈ�� ���� �ǳ�
    public GameObject StartButton;

    public GameObject room;            // �� ������ ����� ������
    public Transform GridTr;           // �� ������ ��ġ�� ������ Transform
    public string gameVersion = "1.0"; // ���� ���� ������ ���� string
    #endregion

    #region ���� ������ �ʿ��� ���������
    // �� ������ ���� ����Ʈ
    List<RoomInfo> _roomList = new List<RoomInfo>();

    // ���� �κ� �ִ� �÷��̾���� �г����� ���� ����Ʈ
    List<string> _LobbyUserNickName = new List<string>();

    // �� Ÿ��Ʋ�� ���� string
    private string roomNameText;

    // ��� �÷��̾ �غ�Ǿ��ִ����� ��Ÿ�� bool ����
    private bool allplayerEnter = false;

    // ���� �ڽ��� �����ִ� ���� �����͸� ���� RoomData �� ����
    RoomData curRoomData = null;
    #endregion

    void Awake()
    {
        // ���� ���۽� ��ũ�� ����� ������ 16 : 9 ������
        Screen.SetResolution(800, 450, false);
        // ���� ���� ���� �̰� �ٸ������̸� ���� ���� ���Ѵ�
        PhotonNetwork.GameVersion = gameVersion;
        // ������(ȣ��Ʈ)�� ���� �̵��ϸ� Ŭ���̾�Ʈ �÷��̾���� �ڵ������� ��ũ�ȴ�
        PhotonNetwork.AutomaticallySyncScene = true;
        // �濡�� ���Ӿ����� �ε��ɶ� �濡�� ������ �޼����� ���� �ʴ´�
        PhotonNetwork.IsMessageQueueRunning = false;
        // ���� �Ŵ����� ���� �̵��ص� �ı����� �ʰ� ����
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Game")
        {
            // ���� ������ ���¸� �ؽ�Ʈ�� �����
            ConnectionStatus.text = "���� ���� ���� : " + PhotonNetwork.NetworkClientState.ToString();

            // �κ� �ǳ��� Ȱ��ȭ �Ǿ�������
            if (Lobby.activeInHierarchy)
            {
                // ID�Է��ϴ� ��ǲ�ʵ�� ���� ��ư�� ��Ȱ��ȭ�Ѵ�.
                IDInputField.interactable = false;
                connectButton.GetComponent<Button>().interactable = false;
            }
            // �ݴ� ��쿡��
            else
            {
                // ��ǲ�ʵ带 Ȱ��ȭ�ϰ�
                IDInputField.interactable = true;

                // ��ǲ�ʵ尡 ����������� �����ư ��Ȱ��ȭ 
                if (IDtext.text.Length == 0)
                    connectButton.GetComponent<Button>().interactable = false;
                // ��ǲ�ʵ忡 ������ ������ �����ư Ȱ��ȭ
                else
                    connectButton.GetComponent<Button>().interactable = true;
            }

            // �� �����Ҷ� ���� ������ �������� ������ư�� Ȱ��ȭ
            if (RoomInputFieldText.text.Length > 0) CreateButton.GetComponent<Button>().interactable = true;
            else CreateButton.GetComponent<Button>().interactable = false;

            // ������ ��쿡�� ���۹�ư�� Ȱ��ȭ
            if (PhotonNetwork.IsMasterClient)
                StartButton.SetActive(true);
            else
                StartButton.SetActive(false);
        }
        else
        {
            // ���� �� �ȿ����� GameManager ó��
            // ���Ӿ� �ε� �Ϸ� �� �濡�� ������ �޼����� �޴´�
            PhotonNetwork.IsMessageQueueRunning = true;
        }
    }

    #region �ݹ� �Լ�
    // ������ ��������� �ڵ����� ȣ��Ǵ� �Լ�
    public override void OnConnectedToMaster()
    {
        // �α����Ͽ� �����ͼ����� ����� �Ŀ� 
        // ���� �Է��� ���̵� �ٸ� ������� ��ġ���� ���Ѵ�
        // �켱 �α׾ƿ����¿��� �α����� �ϴ� �濡�� ������ ���� �����Ŀ� �ٽ� �κ�� ���ƿ��°��
        // ����Ʈ�� �ִ� �г��� ������ �� �����ش�
        PhotonNetwork.LocalPlayer.NickName = IDtext.text;
        _LobbyUserNickName.Clear();

        //// ������ ������ �����ϴ� ������ ������ ��� �������� �г����� ������ ����Ʈ�� �־��ش�
        //foreach (�÷��̾� in ������ ������ �ִ� ���� ������ ��� �÷��̾��)
        //{
        //    Debug.Log("Ȯ�ο�");
        //    _LobbyUserNickName.Add(�ش� �÷��̾� �г���);
        //}

        // ����Ʈ�� �����ϴ� ī��Ʈ�� 0 ���� ũ�ٴ°� �̹� ������
        // �����ͼ����� �����Ͽ� �г����� ������ �ִٴ� ���̱⶧���� �˻��Ѵ�
        if (_LobbyUserNickName.Count > 0)
        {
            for (int i = 0; i < _LobbyUserNickName.Count; i++)
            {
                if (_LobbyUserNickName.Contains(IDtext.text))
                {
                    WarningPanel.SetActive(true);
                    DisConnect(); // �ߺ��� �г��� �̹Ƿ� �г����� �ٽ� �Է¹ޱ����� ������ ������Ų��
                }
                else
                {
                    // �г��� �Է� ��ǲ�ʵ忡�� ���� �ؽ�Ʈ�� �г������� ����
                    PhotonNetwork.LocalPlayer.NickName = IDtext.text;
                    _LobbyUserNickName.Add(PhotonNetwork.LocalPlayer.NickName);
                }
            }
        }
        // ����Ʈ�� ī��Ʈ�� 0���� �۰ų� ���ٴ°��� �ƹ��� ���ٴ� �ǹ��̱⶧����
        // �ٷ� �г����� ����Ʈ�� �߰����ش�
        else
        {
            // �г��� �Է� ��ǲ�ʵ忡�� ���� �ؽ�Ʈ�� �г������� ����
            PhotonNetwork.LocalPlayer.NickName = IDtext.text;
            _LobbyUserNickName.Add(PhotonNetwork.LocalPlayer.NickName);
        }

        // ��� Ȯ���� ������
        // �ٷ� �κ�� ���� �Ѵ� (ū �Ը��� ������ �ƴϱ� ������)
        // �κ�� �ϳ��� �ѰŶ� �ٷ� �����ϰ� ��
        PhotonNetwork.JoinLobby();
    }

    // ������ ���������� �ڵ����� ȣ��Ǵ� �Լ�
    public override void OnDisconnected(DisconnectCause cause)
    {
        _LobbyUserNickName.Clear();
        // �α׾ƿ� �����̹Ƿ� �� ������ ����ִ� �ǳ��� ��Ȱ��ȭ
        Lobby.SetActive(false);
    }

    // �κ� ������ �ڵ����� ȣ��Ǵ� �Լ�
    public override void OnJoinedLobby()
    {
        // ���������� ���������� �� ������ ����� �ǳ��� Ȱ��ȭ
        Lobby.SetActive(true);
        // ó�� �����Ҷ� �� ������ �ѹ� �����
        _roomList.Clear();
    }

    // �� ������ �ٲ� �ڵ����� ȣ��Ǵ� �Լ�
    // �κ� ���� ��
    // ���ο� ���� ������� ���
    // ���� �����Ǵ� ���
    // ���� IsOpen ���� ��ȭ�� ���
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ���� ������ �ٲ�� �� �ݹ��Լ��� ����Ǹ�
        // ���� �ִ� ����� ���� �����ְ�
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ROOM"))
        {
            Destroy(obj);
        }

        // roomList ����Ʈ�� �������� Ȯ���Ѵ�
        foreach (RoomInfo roomInfo in roomList)
        {
            // �� ������ isVisible�� false �̰ų� ����Ʈ���� ���ŵ� ����(�÷��̾ �ƹ��� ���) ���
            if (!roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                // �� ��Ͽ��� �����Ѵ�
                if (_roomList.IndexOf(roomInfo) != -1)
                    _roomList.RemoveAt(_roomList.IndexOf(roomInfo));
            }
            else
            {
                // ���� ��Ȳ�� �ƴϸ� �渮��Ʈ�� ���� �־��ش�
                if (!_roomList.Contains(roomInfo)) _roomList.Add(roomInfo);
                else _roomList[_roomList.IndexOf(roomInfo)] = roomInfo;
            }
        }

        // ���� ������ ���� ����Ʈ�� �ִ� ����� ������ش�
        foreach (RoomInfo roomInfo in _roomList)
        {
            GameObject _room = Instantiate(room, GridTr);
            RoomData roomData = _room.GetComponent<RoomData>();
            roomData.roomName = roomInfo.Name;
            roomData.maxPlayer = roomInfo.MaxPlayers;
            roomData.playerCount = roomInfo.PlayerCount;
            roomData.isOpen = roomInfo.IsOpen;
            roomData.UpdateInfo();

            // �ش� ���� �ο��� ���������� ��ưŬ���� ���� �����Ҽ� �����Ѵ�
            if (roomData.playerCount == roomData.maxPlayer)
                roomData.GetComponent<Button>().interactable = false;

            // ���� ���������� �������� ��ư Ŭ���� ����
            if (roomData.isOpen == false)
                _room.GetComponent<Button>().interactable = false;
            else
            {
                // ���� ����������
                // �� �༮�� ������ ���� ����� ��ư�ε�  delegate�� ������ �����Ͽ� Ŭ�������� ����ǵ��� �Ѵ�
                roomData.GetComponent<Button>().onClick.AddListener
                (
                    delegate
                    {
                        curRoomData = roomData;
                        roomNameText = roomData.roomName;
                        // �� �κ��� ������ �濡 �����ϴ� �κ�
                        PhotonNetwork.JoinRoom(roomData.roomName, null);
                    }
                );
            }
        }
    }

    // �濡 �����ϸ� �ڵ������� ȣ��Ǵ� �ݹ��Լ�
    public override void OnJoinedRoom()
    {
        RoomName.text = "�� : " + roomNameText;

        if (PhotonNetwork.IsMasterClient)
            playersNickNameInRoom[0].text = "���� : " + PhotonNetwork.LocalPlayer.NickName;
        else
        {
            playersNickNameInRoom[0].text = "���� : " + PhotonNetwork.MasterClient.NickName;
            playersNickNameInRoom[1].text = "������ : " + PhotonNetwork.LocalPlayer.NickName;
        }
        LoginPanel.SetActive(false);
        RoomPanel.SetActive(true);
    }

    // �� �÷��̾ �濡 ���������� ȣ��Ǵ� �ݹ��Լ�
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playersNickNameInRoom[1].text = "������ : " + newPlayer.NickName;
        clientPlayer = newPlayer;
    }

    // �ٸ� �÷��̾ ���� �������� ȣ��Ǵ� �ݹ��Լ�
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playersNickNameInRoom[0].text = "���� : " + PhotonNetwork.LocalPlayer.NickName;
        playersNickNameInRoom[1].text = null;
    }

    Player clientPlayer;

    // �濡�� ������ ����Ǵ� �ݹ��Լ�
    public override void OnLeftRoom()
    {
        // ���� �ǳ��� ��Ȱ��ȭ�ϰ�
        RoomPanel.SetActive(false);
        // �α��� �ǳ��� Ȱ��ȭ�Ѵ�
        LoginPanel.SetActive(true);
        // �����Ͱ� ���� ������ �ݹ��Լ��� ����Ǹ�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Main");
        }
    }

    #endregion

    #region UI �̺�Ʈ �Լ�
    // ���� ��ư�� �������� �������� �Լ�
    public void Connect()
    {
        // �Է� �ؽ�Ʈ�� 3���� �̸��϶� ��� �ؽ�Ʈ�� �ְ� �ǳ��� ����� �Ŀ� ��ȯ
        if (IDtext.text.Length < 3)
        {
            Warningtext.text = "�г����� 3���� �̻� �̾�� �մϴ�";
            WarningPanel.SetActive(true);
            return;
        }

        // �Է� �ؽ�Ʈ�� 9���� �̻��϶� ��� �ؽ�Ʈ�� �ְ� �ǳ��� ����� �Ŀ� ��ȯ
        if (IDtext.text.Length > 8)
        {
            Warningtext.text = "�г����� 8���� ���� ���� �մϴ�";
            WarningPanel.SetActive(true);
            return;
        }

        // �� �λ�Ȳ�� �ɸ��� �ʾ� �ùٸ��� �Է��ߴٸ� ����
        PhotonNetwork.ConnectUsingSettings();
    }

    // �α׾ƿ� ��ư���� ���� ��ư�� ���� �Լ�
    // ������ �����ִ� �Լ��� ȣ���Ѵ�
    public void DisConnect() => PhotonNetwork.Disconnect();

    // �� ���� ��ư�� �� �Լ�
    public void OnCreateButtonClicked()
    {
        // ���� �ؽ�Ʈ�� ��ǲ�ʵ�� ���� �ؽ�Ʈ�� �ְ�
        roomNameText = RoomInputFieldText.text;
        // ���� ��������� ������ư�� �ٽ� ����Ҽ� �ְ� Ŭ���� Ǯ���ְ�
        CreateRoomPanelButton.GetComponent<Button>().interactable = true;
        // ����� �ǳ��� ��Ȱ��ȭ ��Ų��
        CreateRoomPanel.SetActive(false);

        // ���� ������ ����ִ� Ŭ����
        RoomOptions ro = new RoomOptions();
        ro.IsVisible = true;                        // ���� ���̰�
        ro.IsOpen = true;                           // ���� ����
        ro.MaxPlayers = 2;                          // �ִ� �ο����� 2
        ro.CleanupCacheOnLeave = true;

        PhotonNetwork.CreateRoom(roomNameText, ro); // ������ ���� ����� �Լ�
        RoomInputFieldText.text = null;             // ������ �ٽ� ����� ������ ��ǲ�ʵ�� ������´�
    }

    // �� ���� ��ư�� �޾��� �Լ�
    public void OnCreateRoomInfoButtonClicked()
    {
        // �� ������ �Է¹��� �ǳ��� Ȱ��ȭ ��Ű��
        CreateRoomPanel.SetActive(true);
        // �ߺ��ؼ� ��� ��ư�� ������ �ʰ� ���� ��ư�� Ŭ���� ���´�
        CreateButton.GetComponent<Button>().interactable = false;
    }

    // �� ���� ��ҿ� �޾��� �Լ�
    public void OnCancleButtonClicked()
    {
        RoomInputFieldText.text = null;
        // �� ������ ����Ҷ� �ǳ��� ��Ȱ��ȭ ��Ų��
        CreateRoomPanel.SetActive(false);
        // �ٽ� ���� ������ �� �ְ� ��ư�� Ŭ���� Ǯ���ش� 
        CreateButton.GetComponent<Button>().interactable = true;
    }

    // ��� �ǳ��� �ݾ��� ��ư�� �� �Լ�
    public void OnQuitButtonClicked()
    {
        // ��� �ǳ��� �����ش�
        WarningPanel.SetActive(false);
    }

    // �濡�� ������ ��ư�� ������ ���� ������ �Լ��� �޾��ش�
    public void OnOutRoomButtonClicked() => PhotonNetwork.LeaveRoom();

    // ������ ���۹�ư�� �������� ������ üũ�Ͽ� ���Ӿ����� �̵����� ���� �÷��̾ 
    // ���� ������ �ʾҴٴ� ����ǳ��� ����� üũ�ϴ� �Լ�
    public void CheckReady()
    {
        Room room = PhotonNetwork.CurrentRoom;

        if (room.PlayerCount == 2)
            allplayerEnter = true;

        if (allplayerEnter == true)
        {
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            Warningtext.text = "���� �ٸ� �÷��̾ �������� �ʾҽ��ϴ�";
            WarningPanel.SetActive(true);
        }
    }
    #endregion
}
