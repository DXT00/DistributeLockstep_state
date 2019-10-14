using System;
using System.Collections;
using System.Collections.Generic;


public struct PlayerPos
{
    public int area_id;
    public UnityEngine.Vector2 Pos;
}

public class GameMap
{
    public Area[] areas;

    public Dictionary<int, PlayerPos> Players;
    public Dictionary<int, Dictionary<int, UnityEngine.Vector2>> PlayersSYN;
    public Dictionary<int, int> PlayerProgressid;
    public List<int> Progressids;
    public List<int> PlayerCouts;

    int total_area_number;
    int areasCount;
    public int singleAreaSize;
    public int singleAreaHalfSize;
    int centerAreaIndex;

    int FrameCount;

    public int GetFrame()
    {
        return FrameCount;
    }

    Room room;
    public GameMap(int AreasCount, int SingleAreaSize)
    {
        PlayerCouts = new List<int>();
        PlayersSYN = new Dictionary<int, Dictionary<int, UnityEngine.Vector2>>();
        Players = new Dictionary<int, PlayerPos>();
        PlayerProgressid = new Dictionary<int, int>();
        Progressids = new List<int>();

        this.areasCount = AreasCount;
        this.singleAreaSize = SingleAreaSize;
        this.singleAreaHalfSize = SingleAreaSize / 2;
        this.centerAreaIndex = (AreasCount * AreasCount - 1) / 2;

        this.total_area_number = AreasCount * AreasCount;
        FrameCount = 0;
        areas = new Area[total_area_number];

        for (int posY = 0; posY < AreasCount; posY++)
        {
            for (int posX = 0; posX < AreasCount; posX++)
            {
                Area a = new Area(posX, posY);
                a.AreaID = id_map(posX, posY);
                a.SetCenter(indexToCenter(a.AreaID));
                areas[a.AreaID] = a;

            }
        }
    }

    public int id_map(int x, int y)
    {
        return areasCount * y + x;
    }

    public void Init(Room room)
    {
        this.room = room;
    }

    public Area GetArea(int AreaID)
    {
        return areas[AreaID];
    }
    public void Tick( )
    { 
        
        for (int index = 0; index < total_area_number; ++index)
        {
            Area area = areas[index];
            area.OnTick(FrameCount);
        }
        FrameCount++;
    }


    public List<int> findNearbyArea(int areaId)
    {
        List<int> nearby_areas = new List<int>();
        //1. find current area
        int index_y = areaId / areasCount;
        int index_x = areaId - index_y * areasCount;
        //2. decide the nearby area
        nearby_areas.Clear();
        nearby_areas.Add(areaId);
        // left
        int temp = safeGetAreaIdAt(index_x - 1, index_y);
        if (temp != -1)
            nearby_areas.Add(temp);
        // right
        temp = safeGetAreaIdAt(index_x + 1, index_y);
        if (temp != -1)
            nearby_areas.Add(temp);
        // top
        temp = safeGetAreaIdAt(index_x, index_y + 1);
        if (temp != -1)
            nearby_areas.Add(temp);
        // bottom
        temp = safeGetAreaIdAt(index_x, index_y - 1);
        if (temp != -1)
            nearby_areas.Add(temp);
        // top right
        temp = safeGetAreaIdAt(index_x + 1, index_y + 1);
        if (temp != -1)
            nearby_areas.Add(temp);
        // top left
        temp = safeGetAreaIdAt(index_x - 1, index_y + 1);
        if (temp != -1)
            nearby_areas.Add(temp);
        // bottom right
        temp = safeGetAreaIdAt(index_x + 1, index_y - 1);
        if (temp != -1)
            nearby_areas.Add(temp);
        // bottom left
        temp = safeGetAreaIdAt(index_x - 1, index_y - 1);
        if (temp != -1)
            nearby_areas.Add(temp);
        return nearby_areas;
    }

    public int safeGetAreaIdAt(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < areasCount && y < areasCount)
        {
            return IDMap(x, y);
        }
        else return -1;
    }
    private int IDMap(int x, int y)
    {
        return areasCount * y + x;
    }

    public void Start(List<Player> playerList)
    {
        Tick();
        if (playerList != null)
        {
            Random random = new Random();

            Area area;
            CustomSyncMsg msg;
            NetworkMsg n_msg;

            foreach (Player player in playerList)
            {
                int index = random.Next(0, total_area_number - 1);
                int playerID = player.playerID;
                area = areas[index];

                UnityEngine.Vector3 TPos = UnityEngine.Random.onUnitSphere * singleAreaHalfSize;
                TPos.y = 0;

                msg = new SpawnMessage(playerID, area.AreaCenter + TPos);

                area.RecordCustomSyncMsg(msg);

                n_msg = new NetworkMsg();
                n_msg.type = (int)CmdType.SPAWNINFO;
                n_msg.player_id = index;

                room.initConnID(playerID, index);
                room.SendDataTo(playerID, n_msg);
            }
        }
    
    }
    public UnityEngine.Vector3 indexToCenter(int index)
    {
        //1. get the pos of index: 0
        int row = this.centerAreaIndex / areasCount;
        int x = -row * singleAreaSize;
        int y = row * singleAreaSize;
        int index_y = y - (index / areasCount) * singleAreaSize;
        int index_x = x + (index % areasCount) * singleAreaSize;
        return new UnityEngine.Vector3(index_x, 0, index_y);
    }
}
