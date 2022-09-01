using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // UI 사용을 위해 
using Photon.Realtime; // 포톤 사용을 위해
using Photon.Pun;      //       ||
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region 싱글턴
    static GameManager instance = null;

    public static GameManager Instance 
    {
        get
        {
            if(instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }
    }
    #endregion
    public string gameVersion = "1.0";

    public Text ConnectionStatus;      // 현재 네트워크의 상태를 나타내는 텍스트
    public Text IDtext;                // 플레이어가 입력하여 넣을 닉네임 텍스트
    public Text Warningtext;           // 닉네임 형식에서 벗어날때 띄울 경고 텍스트

    public InputField RoomInputFieldText; // 방정보 인풋필드
    public InputField IDInputField;       // ID 인풋필드

    public Button connectButton;       // 입장 버튼을 눌러 서버에 들어갈때 쓰일 버튼
    public GameObject RoomPanel;       // 방에 입장하면 띄워줄 판넬
    public GameObject LoginPanel;      // 로그인과 방 생성등을 종합적으로 담을 판넬
    public GameObject Lobby;           // 로비에 관한 모든것을 담을 판넬
    public GameObject CreateRoomPanel; // 방을 생성하는데 쓰일 판넬
    public GameObject CreateRoomPanelButton;    // 방생성 판넬을 띄워줄 버튼
    public GameObject CreateButton;    // 방을 생성하는 버튼
    public GameObject WarningPanel;    // 경고 텍스트를 담을 판넬

    public GameObject room;            // 방 생성시 사용할 프리팹
    public Transform GridTr;           // 방 생성시 위치를 정해줄 Transform

    // 방 정보를 담을 리스트
    List<RoomInfo> _roomList = new List<RoomInfo>();

    List<string> _LobbyUserNickName = new List<string>();

    private string roomNameText;

    void Awake()
    {
        // 게임 시작시 스크린 사이즈를 맞춰줌 16 : 9 사이즈
        Screen.SetResolution(800, 450, false);
        PhotonNetwork.GameVersion = gameVersion;
    }

    //private void Start()
    //{
    //    _LobbyUserNickName.Add("Default");
    //}

    // Update is called once per frame
    void Update()
    {
        // 현재 서버의 상태를 텍스트로 띄워줌
        ConnectionStatus.text = "현재 서버 상태 : " + PhotonNetwork.NetworkClientState.ToString();

        if(Lobby.activeInHierarchy)
        {
            IDInputField.interactable = false;
            connectButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            IDInputField.interactable = true;

            if (IDtext.text.Length == 0)
            {
                connectButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                connectButton.GetComponent<Button>().interactable = true;
            }
        }

        if (RoomInputFieldText.text.Length > 0) CreateButton.GetComponent<Button>().interactable = true;
        else CreateButton.GetComponent<Button>().interactable = false;
    }

    // 입장 버튼을 눌렀을때 연결해줄 함수
    public void Connect()
    {
        // 입력 텍스트가 3글자 미만일때 경고 텍스트를 넣고 판넬을 띄워준 후에 반환
        if (IDtext.text.Length < 3)
        {
            Warningtext.text = "닉네임은 3글자 이상 이어야 합니다";
            WarningPanel.SetActive(true);
            return;
        }

        // 입력 텍스트가 9글자 이상일때 경고 텍스트를 넣고 판넬을 띄워준 후에 반환
        if (IDtext.text.Length > 8)
        {
            Warningtext.text = "닉네임은 8글자 이하 여야 합니다";
            WarningPanel.SetActive(true);
            return;
        }

        // 위 두상황에 걸리지 않아 올바르게 입력했다면 연결
        PhotonNetwork.ConnectUsingSettings();
    }

    // 로그아웃 버튼으로 쓰일 버튼에 쓰일 함수
    // 연결을 끊어주는 함수를 호출한다
    public void DisConnect() => PhotonNetwork.Disconnect();

    // 서버에 연결됐을때 자동으로 호출되는 함수
    public override void OnConnectedToMaster()
    {
        // 로그인하여 마스터서버에 연결된 후에 
        // 내가 입력한 아이디가 다른 유저들과 겹치는지 비교한다
        // 리스트에 존재하는 카운트가 0 보다 크다는건 이미 누군가
        // 마스터서버에 존재하여 닉네임을 가지고 있다는 것이기때문에 검사한다
        
        // 마스터 서버에 존재하는 모든 유저들의 닉네임을 가져와 리스트에 넣어줌

        if(_LobbyUserNickName.Count > 0)
        {
            for (int i = 0; i < _LobbyUserNickName.Count; i++)
            {
                if (_LobbyUserNickName.Contains(IDtext.text))
                {
                    Warningtext.text = "이미 존재하는 닉네임 입니다.";
                    WarningPanel.SetActive(true);
                    DisConnect();
                }
                else
                {
                    Debug.Log("내 닉네임 추가 완료");
                    // 닉네임 입력 인풋필드에서 받은 텍스트를 닉네임으로 설정
                    PhotonNetwork.LocalPlayer.NickName = IDtext.text;
                    _LobbyUserNickName.Add(PhotonNetwork.LocalPlayer.NickName);
                }
            }
        }
        // 리스트의 카운트가 0보다 작거나 같다는것은 아무도 없다는 의미이기때문에
        // 바로 닉네임을 리스트에 추가해준다
        else
        {
            Debug.Log("내 닉네임 추가 완료");
            // 닉네임 입력 인풋필드에서 받은 텍스트를 닉네임으로 설정
            PhotonNetwork.LocalPlayer.NickName = IDtext.text;
            _LobbyUserNickName.Add(PhotonNetwork.LocalPlayer.NickName);
        }

        // 모든 확인이 끝나면
        // 바로 로비로 들어가게 한다 (큰 규모의 게임이 아니기 때문에)
        // 로비는 하나만 둘거라서 바로 접속하게 함
        PhotonNetwork.JoinLobby();
    }

    // 연결이 끊어졌을때 자동으로 호출되는 함수
    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LocalPlayer.NickName = null;
        // 로그아웃 상태이므로 방 정보를 띄워주는 판넬을 비활성화
        Lobby.SetActive(false);
    }

    // 방 생성 버튼에 달 함수
    public void OnCreateButtonClicked()
    {
        // 방의 텍스트는 인풋필드로 받은 텍스트를 넣고
        roomNameText = RoomInputFieldText.text;
        // 방을 만들었으니 생성버튼을 다시 사용할수 있게 클릭을 풀어주고
        CreateRoomPanelButton.GetComponent<Button>().interactable = true;
        // 띄웠던 판넬은 비활성화 시킨다
        CreateRoomPanel.SetActive(false);

        // 방의 정보를 담고있는 클래스
        RoomOptions ro = new RoomOptions();
        ro.IsVisible = true;                        // 방이 보이게
        ro.IsOpen = true;                           // 방을 열고
        ro.MaxPlayers = 2;                          // 최대 인원수는 2
        ro.CleanupCacheOnLeave = true;

        PhotonNetwork.CreateRoom(roomNameText, ro); // 실제로 방을 만드는 함수
        RoomInputFieldText.text = null;             // 다음에 다시 만들수 있으니 인풋필드는 비워놓는다
    }

    // 로비에 들어갔을때 자동으로 호출되는 함수
    public override void OnJoinedLobby()
    {
        // 정상적으로 접속했을때 방 정보를 띄워줄 판넬을 활성화
        Lobby.SetActive(true);
        // 처음 접속할때 방 정보를 한번 비워줌
        _roomList.Clear();
    }

    // 방 정보가 바뀔때 자동으로 호출되는 함수
    // 로비에 접속 시
    // 새로운 룸이 만들어질 경우
    // 룸이 삭제되는 경우
    // 룸의 IsOpen 값이 변화할 경우
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 방의 정보가 바뀌어 이 콜백함수가 실행되면
        // 원래 있던 방들을 전부 없애주고
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ROOM"))
        {
            Destroy(obj);
        }

        // roomList 리스트의 정보들을 확인한다
        foreach (RoomInfo roomInfo in roomList)
        {
            // 방 정보의 isVisible이 false 이거나 리스트에서 제거된 정보(플레이어가 아무도 없어서) 라면
            if(!roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                // 방 목록에서 제거한다
                if(_roomList.IndexOf(roomInfo) != -1)
                    _roomList.RemoveAt(_roomList.IndexOf(roomInfo));
            }
            else
            {
                // 위의 상황이 아니면 방리스트에 새로 넣어준다
                if (!_roomList.Contains(roomInfo)) _roomList.Add(roomInfo);
                else _roomList[_roomList.IndexOf(roomInfo)] = roomInfo;
            }
        }

        // 위의 조건을 돌고 리스트에 있는 방들을 만들어준다
        foreach (RoomInfo roomInfo in _roomList)
        {
            GameObject _room = Instantiate(room, GridTr);
            RoomData roomData = _room.GetComponent<RoomData>();
            roomData.roomName = roomInfo.Name;
            roomData.maxPlayer = roomInfo.MaxPlayers;
            roomData.playerCount = roomInfo.PlayerCount;
            roomData.isOpen = roomInfo.IsOpen;
            roomData.UpdateInfo();

            // 해당 방의 인원이 꽉차있으면 버튼클릭을 막아 접속할수 없게한다
            if (roomData.playerCount == roomData.maxPlayer)
                roomData.GetComponent<Button>().interactable = false;

            // 방이 닫혀있으면 못들어오게 버튼 클릭을 막고
            if (roomData.isOpen == false)
                _room.GetComponent<Button>().interactable = false;
            else
            {
                // 방이 열려있으면
                // 이 녀석이 실제로 방을 만드는 버튼인데  delegate로 내용을 참조하여 클릭했을때 실행되도록 한다
                roomData.GetComponent<Button>().onClick.AddListener
                (
                    delegate
                    {
                        roomNameText = roomData.roomName;
                        // 이 부분이 실제로 방에 참가하는 부분
                        PhotonNetwork.JoinRoom(roomData.roomName, null);
                    }
                );
            }
        }
    }

    // 방 생성 버튼에 달아줄 함수
    public void OnCreateRoomInfoButtonClicked()
    {
        // 방 정보를 입력받을 판넬을 활성화 시키고
        CreateRoomPanel.SetActive(true);
        // 중복해서 계속 버튼이 눌리지 않게 생성 버튼은 클릭을 막는다
        CreateButton.GetComponent<Button>().interactable = false;
    }

    public void OnCancleButtonClicked()
    {
        RoomInputFieldText.text = null;
        // 방 생성을 취소할때 판넬을 비활성화 시킨다
        CreateRoomPanel.SetActive(false);
        // 다시 방을 생성할 수 있게 버튼의 클릭을 풀어준다 
        CreateButton.GetComponent<Button>().interactable = true;
    }

    // 경고 판넬을 닫아줄 버튼에 달 함수
    public void OnQuitButtonClicked()
    {
        // 경고 판넬을 내려준다
        WarningPanel.SetActive(false);
    }

    // 방에 참가하면 자동적으로 호출되는 콜백함수
    public override void OnJoinedRoom()
    {
        RoomPanel.SetActive(true);
        LoginPanel.SetActive(false);
    }

    // 방에서 나가기 버튼을 누르면 방을 떠나는 함수를 달아준다
    public void OnOutRoomButtonClicked() => PhotonNetwork.LeaveRoom();

    // 방에서 나가고 실행되는 콜백함수
    public override void OnLeftRoom()
    {
        // 방의 판넬을 비활성화하고
        RoomPanel.SetActive(false);
        // 로그인 판넬은 활성화한다
        LoginPanel.SetActive(true);
    }
}
