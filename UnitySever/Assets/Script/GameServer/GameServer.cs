using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;

public class GameServer
{

    Room TheRoom;

    int areasTotalCount;
    int areasCount;
    int singleAreaSize;

    public NetworkManager networkManager;

    public GameServer(int areasCount, int singleAreaSize)
    {
        this.networkManager = new NetworkManager();
        this.areasTotalCount = areasCount * areasCount;
        this.areasCount = areasCount;
        this.singleAreaSize = singleAreaSize;

        CreatDefaultRoom();
    }

    public void Init()
    {
        networkManager.RegisterGameServer(this);
        networkManager.StartListen();
        networkManager.Init();
    }

    public void SetSYNPlayerPosition(int SendPlayer_Id, int Player_Id, UnityEngine.Vector2 Pos)
    {
        if (TheRoom != null)
        {
            Dictionary<int, UnityEngine.Vector2> TPLayers = null;

            if (TheRoom.gameMap.PlayersSYN.ContainsKey(SendPlayer_Id))
            {
                TPLayers = TheRoom.gameMap.PlayersSYN[SendPlayer_Id];
            }
            else
            {
                TPLayers = new Dictionary<int, UnityEngine.Vector2>();
                TheRoom.gameMap.PlayersSYN[SendPlayer_Id] = TPLayers;
            }

            if (TPLayers != null)
            {
                TPLayers[Player_Id] = Pos;
            }
        }
        else
        {
            UnityEngine.Debug.Log("tmpRoom == null");
        }
    }

    public void SetPlayerPosition(int Player_Id, int Areaid, UnityEngine.Vector2 Pos)
    {
        if (TheRoom != null)
        {
            PlayerPos TPlayers = new PlayerPos();
            TPlayers.area_id = Areaid;
            TPlayers.Pos = Pos;

            TheRoom.gameMap.Players[Player_Id] = TPlayers;

        }
        else
        {
            UnityEngine.Debug.Log("tmpRoom == null");
        }
    }

    public void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Update::Tick");
        if (TheRoom != null)
        {
            TheRoom.Tick();
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("Update::Update");
        networkManager.Update();
        UnityEngine.Profiling.Profiler.EndSample();
    }


    public void OnPlayerJoin(int Playerid, int roomID)
    {
        int flag = -1;
        if (TheRoom != null)
        {
            flag = TheRoom.JoinRoom(Playerid);

            TheRoom.gameMap.PlayerProgressid[Playerid] = roomID;

            if (TheRoom.gameMap.Progressids.IndexOf(roomID) == -1)
            {
                TheRoom.gameMap.Progressids.Add(roomID);
            }

            UnityEngine.Debug.Log("OnPlayerJoin progress id " + roomID);
        }
        if(flag != -1)
        {
            SendRoomInfo(Playerid, flag, TheRoom);
        }
    }
    public void PreGameStart(int clientID)
    {
        OnGameStart();
    }
    public void OnGameStart()
    {
        TheRoom.Start();
    }

    public void OnAskedFrame(int clientID, List<int> areas, List<int> frames)
    {
        if (TheRoom != null)
        {
            List<int> Tareas = new List<int>();
            List<List<SyncFrame>> Tframes = new List<List<SyncFrame>>();


            for (int nLoop = 0; nLoop < areas.Count; ++nLoop)
            {
                int areaID = areas[nLoop];
                int startFrame = frames[nLoop];
                List<SyncFrame> syncFrames = TheRoom.GetArea(areaID).GetNewFrames(startFrame);
 
                Tareas.Add(areaID);
                Tframes.Add(syncFrames);
            }

            NetworkMsg msg = new ReplyAskFrame(Tareas, Tframes);
            networkManager.SendDataTo(clientID, msg);
        }
        else
        {
            UnityEngine.Debug.Log("TheRoom != null");
        }
    }


    public void OnGetFrame(int clientID, int areaid, List<CustomSyncMsg> customSyncMsgs)
    {
        if (TheRoom != null)
        {
            Area TArea = TheRoom.GetArea(areaid);
            if (TArea != null)
            {
                TArea.RecordCustomSyncMsg(customSyncMsgs);
                TheRoom.RecordConnIDwithAreaID(clientID, areaid);
            }
            else
            {
                UnityEngine.Debug.Log("TArea == null");
            }
        }
        

    }
    public void OnGetEnd(int clientID)
    {
        if (TheRoom != null)
        {
            TheRoom.OnGetEnd(clientID);
        }
    }
    void SendRoomInfo(int clientID, int flag, Room room)
    {
        List<string> ids = room.GetIDList();
        NetworkMsg msg = new ReplyJoin(flag, ids);
        networkManager.SendDataTo(clientID, msg);

        msg = new ReplyJoin(-1, ids);
        room.roomBroadCast(msg);
    }

    public int GetRoomsCount()
    {
        return 1;
    }

    public Room GetRoom(int id)
    {
        return TheRoom;
    }

    public void CreatRoom()
    {
        TheRoom = new Room( areasCount, singleAreaSize);
        TheRoom.registerNetMgr(networkManager);
    }
    void CreatDefaultRoom()
    {
        CreatRoom();
    }
}