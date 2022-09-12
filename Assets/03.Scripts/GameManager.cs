using System.Collections;
using System.Collections.Generic;
using Photon.Pun;      // 포톤 사용을 위해
using Photon.Realtime; //      ||
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) Destroy(gameObject);
        PV = photonView;
        PhotonNetwork.IsMessageQueueRunning = true;
    }
}
