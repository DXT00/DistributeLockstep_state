using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Timers;
using System.Collections;
using System;
using System.Collections.Concurrent;

class ConnTurn
{
    public int ConnID;
    public bool value;
    public ConnTurn(int connID, bool value)
    {
        this.ConnID = connID;
        this.value = value;
    }

    public void Reset()
    {
        this.value = false;
    }
}

public class Room
{
    int roomID;
    public List<Player> players;
    List<int> ConnectionID;
    List<ConnTurn> ConnIDTurn;
    Dictionary<int, int> ConnIDToAreaID;
    Dictionary<int, int> PlayerIDToConnID;

    public static object TickLock = new object();//modified


    public GameMap gameMap;
    NetworkManager networkManager;

    int prestartTime = 0;

    public int frameCount = 0;
    double interval = 15;
   // System.Timers.Timer timer;
   // System.Timers.Timer preTimer;

    int playerID = 1;
    int maxPlayer = 2000;

    int printCount = 10;
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    System.TimeSpan timeSpan;

    public bool bStart;
    public Room(int areasCount, int singleAreaSize)
    {
        this.roomID = 0;
        players = new List<Player>();
        ConnectionID = new List<int>();
        PlayerIDToConnID = new Dictionary<int, int>();
        ConnIDToAreaID = new Dictionary<int, int>();
        ConnIDTurn = new List<ConnTurn>();

        gameMap = new GameMap(areasCount, singleAreaSize);
        gameMap.Init(this);

        bStart = false;

    }

    public void registerNetMgr(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    public void Start()
    {
        NetworkMsg msg = new ReplyStart(true);
        gameMap.Start(players);
        roomBroadCast(msg);
        bStart = true;
      //  preTimer.Start();
    }

    public void Tick( )
    {
        if (!bStart)
            return;

        gameMap.Tick();
    }
    public int JoinRoom(int clientID)
    {
        if (!ConnectionID.Contains(clientID) && players.Count() < maxPlayer)
        {
            Player player = new Player(playerID);
            this.players.Add(player);
            ConnectionID.Add(clientID);
            ConnIDTurn.Add(new ConnTurn(clientID, false));

            PlayerIDToConnID.Add(playerID, clientID);
            playerID++;

            return playerID - 1;
        }
        else
            return -1;
    }
    public void initConnID(int player_id, int area_id)
    {
        if (PlayerIDToConnID.ContainsKey(player_id))
        {
            int ConnID = (int)PlayerIDToConnID[player_id];
            RecordConnIDwithAreaID(ConnID, area_id);
        }
    }
    public void RecordConnIDwithAreaID(int connID, int areaId)
    {
        lock (ConnIDToAreaID)
        {

            if (!ConnIDToAreaID.ContainsKey(connID))
            {
                ConnIDToAreaID.Add(connID, areaId);
            }
            else
            {
                ConnIDToAreaID[connID] = areaId;
            }

        }
    }
    public void OnGetEnd(int connID)
    {
        foreach (ConnTurn ct in ConnIDTurn)
        {
            if (ct.ConnID == connID)
            {
                ct.value = true;
                return;
            }
        }
    }
    List<int> GetNearbyAreaID(int areaID)
    {
        return gameMap.findNearbyArea(areaID);
    }

    void pushNearbyFrame()
    {
        NetworkMsg msg;
        List<int> areasID;
        List<SyncFrame> syncFrames = new List<SyncFrame>();
        Dictionary<int, List<SyncFrame>> areasToFrames = new Dictionary<int, List<SyncFrame>>();


        Dictionary<int, int> cur;
        lock (ConnIDToAreaID)
        {
            cur = new Dictionary<int, int>(ConnIDToAreaID);
        }
        foreach (int clientID in cur.Keys)
        {
            areasToFrames.Clear();
            int areaID = cur[clientID];// ConnIDToAreaID[clientID];
            areasID = gameMap.findNearbyArea(areaID);
            foreach (int area_id in areasID)
            {
                syncFrames = new List<SyncFrame>();

                //    Console.WriteLine("GetFrame " + (frameCount - 1));//MY
                syncFrames.Add(GetArea(area_id).GetFrame(frameCount - 1));
                if (GetArea(area_id).GetFrame(frameCount - 1) == null)
                {
                    Console.WriteLine("A NULL frame!!________________________________");
                }
                areasToFrames.Add(area_id, syncFrames);

            }
           // msg = new ReplyAskFrame( areasToFrames);
           // msg.uid = roomID;
            //this.networkManager.SendDataTo(clientID, msg);
        }


    }
    public Area GetArea(int areaID)
    {
        return gameMap.GetArea(areaID);
    }
    public int GetRoomID()
    {
        return this.roomID;
    }
    void ResetEndList()
    {
        printCount = 10;
        foreach (ConnTurn ct in ConnIDTurn)
        {
            ct.Reset();
        }
    }
    bool CheckClientEnd()
    {
        foreach (ConnTurn ct in ConnIDTurn)
        {
            if (!ct.value)
            {
                return false;
            }
        }
        return true;
    }

    public void roomBroadCast(NetworkMsg msg)
    {
        foreach (int connID in ConnectionID)
        {
            networkManager.SendDataTo(connID, msg);
        }
    }
    public List<string> GetIDList()
    {
        List<string> ids = new List<string>();
        foreach (Player player in players)
        {
            ids.Add(player.playerID.ToString());
        }
        return ids;
    }
    public void SendDataTo(int playerID, NetworkMsg msg)
    {
        if (PlayerIDToConnID.ContainsKey(playerID))
        {
            int connID = PlayerIDToConnID[playerID];
            networkManager.SendDataTo(connID, msg);
        }
    }
}