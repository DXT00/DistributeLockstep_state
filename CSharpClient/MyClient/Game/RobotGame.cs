using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using System.IO;

public class RobotGame
{
    int areasCount;// = ParameterCommon.areasCount;
    int singleAreaSize;// = ParameterCommon.singleAreaSize;
    int areaLength; //= areasCount * areasCount;

    int gameID;

    System.Random random;
    public AreaManager areaManager;
    Player currentPlayer;

    public int player_id = -1;
    public List<string> playerList;

    int currentInitAreaID = -1;
    int cornerValue;

    bool switchingArea;
    int targetAreaID = -1;

    int ticktimes =0;


    bool bRecordFrame;
    bool bstart;
    public bool bSYN = false;

    Queue<NetworkMsg> sendBuffer;

    public RobotGame(int gameID, int TareasCount, int TsingleAreaSize)
    {
        areasCount = TareasCount;
        singleAreaSize = TsingleAreaSize;
        areaLength = areasCount * areasCount;

        random = new System.Random(gameID);
        ticktimes = random.Next(0, 5);
        n= random.Next(0, 10);
        sendBuffer = new Queue<NetworkMsg>();
        this.gameID = gameID;
        bstart = false;
        bRecordFrame = true;
    }

    public void Start(bool flag)
    {
        GameInit();
        cornerValue = areaLength / 2;
        bstart = true;
    }

    void GameInit()
    {
        areaManager = new AreaManager(areasCount, singleAreaSize);
        areaManager.Init(this);
    }


    public void OnGetFrames(List<DS_protocol.p_IdToFrames> areas_to_frames)
    {
        RecordFrames(areas_to_frames);
    }

    public void OnGetFrames(byte[] buffers)
    {
        RecordFrames(buffers);
    }
    public void OnInitMsg(int areaID)
    {
        currentInitAreaID = areaID;
    }
    public void BindPlayer(Player player)
    {
        currentPlayer = player;
    }
    public void GenUsers(int playerID, List<string> names)
    {
        currentPlayer = new Player(playerID, "robots: " + playerID.ToString());
        player_id = playerID;
        playerList = names;
    }
    public void UpdatePlayerList(List<string> names)
    {
        playerList = names;
    }
    public void LogicTick()
    {
        if (!bstart)
            return;
        //DebugTool.TimeWactherStart(1);

        if( currentPlayer != null && !bSYN )
        {
            areaManager.find_nearby_areas(areaManager.nearby_areas, currentPlayer.get_area_id());
        }
        else
        {
            areaManager.nearby_areas.Clear();
            areaManager.areas.ForEach(i => areaManager.nearby_areas.Add(i));
        }


        List<int> Tareas = new List<int>();
        List<int> Tframs = new List<int>();

        foreach (Area area in areaManager.nearby_areas)
        {
            // if (!area_exist(area, lagged_areas))
            {
                area.Tick();
                Tareas.Add(area.get_area_id());
                Tframs.Add(area.CurrentFrame);
            }
        }
       // Console.WriteLine("AskFrame " + areaManager.nearby_areas[0].CurrentFrame );
        currentPlayer = null;

        for ( int n = 0; n < areaManager.areas.Count; ++n )
        {
            Area TArea = areaManager.areas[n];

            if( TArea != null )
            {
                Console.WriteLine("TArea.logic_mgr.player_list.Count " + TArea.logic_mgr.player_list.Count+"\n"); 

                for (int i = 0; i < TArea.logic_mgr.player_list.Count; ++i)
                {
                    Player TPlayer = TArea.logic_mgr.player_list[i];
                    if (TPlayer.get_id() == player_id)
                    {
                        if( currentPlayer == null )
                        {
                            currentPlayer = TPlayer;
                        }
                        else
                        {
                            Console.WriteLine("Player " + player_id + " areaid " + currentPlayer.get_area_id() + " in other areas " + TArea.get_area_id() );
                        }
                    }

                    if(TPlayer.get_area_id() != TArea.get_area_id() )
                    {
                        Console.WriteLine("Player id " + player_id + "!= areaid " + TArea.get_area_id() );
                    }
                }
            }
        }

        if(bRecordFrame )
        {
            if (currentPlayer != null)
            {
                AreaCheck(currentPlayer);//跨区检测
                ticktimes++;
                if (ticktimes > 5 )
                {
                    SyncFrame localFrame = LocalFrameRecord(currentPlayer);
                    NetworkMsg msg = new Frame(player_id, localFrame);
                    sendBuffer.Enqueue(msg);
                    ticktimes = 0;
                }
            }
            else
            {
                Console.WriteLine(" currentPlayer " + player_id + " is not in any areas ");
            }

            AskFrame TAskFrame = new AskFrame(player_id, Tareas, Tframs);
            sendBuffer.Enqueue(TAskFrame);

            bRecordFrame = false;
        }


       // DebugTool.TimeWactherEnd(1,msg: "currentID"+player_id.ToString());
        //DebugTool.PrintAvgWhileTime(1, msg: player_id.ToString());

    }
    void ChaseTick()
    {
       // areaManager.ChaseTick();
    }
    void AreaCheck( Player TPlayer )
    {
        //Console.WriteLine("in gameID: " + gameID + "curplayer pos is: " + currentPlayer.get_position());
        if(switchingArea)
        {
            if(TPlayer.get_area_id() == targetAreaID)
            {
                switchingArea = false;
            }
        }
        else
        {
            int newAreaID = areaManager.get_id_by_pos(TPlayer.get_position());

            int currentAreaID = TPlayer.get_area_id();

            if (newAreaID != currentAreaID)
            {
              //  Console.WriteLine("area switch, newAreaID != currentAreaID: newAreaID " + newAreaID + " currentAreaID " + currentAreaID);
                if (newAreaID >= areaLength)
                {
                    return;
                }
                switchingArea = true;

                var customSyncMsg = new LeaveAreaMessage(currentPlayer.get_id());
                var syncFrame = new SyncFrame(0, currentAreaID);
                syncFrame.dump_actions(customSyncMsg);

                NetworkMsg networkMsg = new Frame(0, syncFrame);
                sendBuffer.Enqueue(networkMsg);

                targetAreaID = newAreaID;

                //Debug.Log ("area changed!");

                var EnterAreaSyncMsg = new EnterAreaMessage(currentPlayer.get_id(),
                    100,
                     currentPlayer.get_axis(),
                     currentPlayer.get_direction(),
                     currentPlayer.get_position());

                syncFrame = new SyncFrame(0, newAreaID);
                syncFrame.dump_actions(EnterAreaSyncMsg);

                networkMsg = new Frame(0, syncFrame);
                sendBuffer.Enqueue(networkMsg);
            }
            else
            {

            }
        }
    }
    public void EnqueueMsg(NetworkMsg msg)
    {
        sendBuffer.Enqueue(msg);
    }

    int sysTime = 0;
    SyncFrame LocalFrameRecord( Player TPlayer )
    {
        var moving = ControlDirecton(TPlayer );

        var rotation = RotMove();

        var syncFrame = new SyncFrame(0, TPlayer.get_area_id());
        //var InputSyncMsg = new InputMessage(player_id, moving); //TODO :改为发送位置！

        // syncFrame.dump_actions(InputSyncMsg);

        //         var RotateSyncMsg = new RotateMessage(player_id, rotation);
        //         syncFrame.dump_actions(RotateSyncMsg);
        TPlayer.move(moving);//本地move,获取新的位置

        sysTime++;
        if (sysTime >3 )
        {
            var thePos = TPlayer.get_position();

            var Tmsg = new PositionMessage(player_id, new Vector2(thePos.x, thePos.z));
            Console.Write("LocalFrameRecord posioin" + thePos.x, "," + thePos.z);
            syncFrame.dump_actions(Tmsg);
            sysTime = 0;
        }


        if (bSYN )
        {
            Dictionary<int, Vector2> TPlayers = new Dictionary<int, Vector2>();

            for (int n = 0; n < areaManager.areas.Count; ++n)
            {
                Area TArea = areaManager.areas[n];

                for (int np = 0; np < TArea.logic_mgr.player_list.Count; ++np)
                {
                    var Tlogic_mgr = TArea.logic_mgr.player_list[np];

                    if(!TPlayers.ContainsKey(Tlogic_mgr.get_id()))
                    {
                        TPlayers[Tlogic_mgr.get_id()] = new Vector2(Tlogic_mgr.position.x, Tlogic_mgr.position.z);
                    }
                    else
                    {

                    }

                }
            }

            var Tms = new SYNMessage(player_id, TPlayers);

            syncFrame.dump_actions(Tms);
        }

        

        return syncFrame;
    }

    int n = 0;

    UnityEngine.Vector3 ControlDirecton( Player TPlayer )
    {
        UnityEngine.Vector3 TDir = TPlayer.get_move_direction();


        if( n >10 )
        {
            n = 0;
        }
        
       // if(TDir.magnitude < 0.1f )

        if( n == 0 )
        {
            for( int n = 0; n < player_id; ++n )
            {
                TDir.x = random.Next(-100, 100);
                TDir.z = random.Next(-100, 100);
            }

            TDir.y = 0;
            TDir.Normalize();
        }
        n++;

        return TDir;
    }

    UnityEngine.Vector2 RotMove()
    {
        float axisX = 0;
        float axisY = 0;
        UnityEngine.Vector2 delta = new UnityEngine.Vector2(axisX, axisY);
        return delta;
    }

    void PrintFrameInfo(SyncFrame syncFrame)
    {
        if (syncFrame.msg_list != null)
        {
            foreach (CustomSyncMsg msg in syncFrame.msg_list)
            {
                Console.WriteLine("the msg is :" + ((RequestType)msg.msg_type).ToString());
                msg.printInfo();
            }
        }
    }

    void RecordFrames(List<DS_protocol.p_IdToFrames> areas_to_frames)
    {
        bRecordFrame = true;
        for (int nLoop = 0; nLoop < areas_to_frames.Count; ++nLoop)
        {
            DS_protocol.p_IdToFrames area_to_Frames = areas_to_frames[nLoop];
            List<DS_protocol.p_SyncFrame> framesInfo = area_to_Frames.frames;
            if (framesInfo != null && framesInfo.Count > 0)
            {
                // Console.WriteLine(" recieve areaID " + area_to_Frames.areaid + " Frame start - to " + framesInfo[0].frame_count +"~" + framesInfo[framesInfo.Count-1].frame_count); //单位毫秒 
                foreach (DS_protocol.p_SyncFrame p_syncFrame in framesInfo)
                {
                    int frame_count = p_syncFrame.frame_count;
                    SyncFrame syncFrame = new SyncFrame(frame_count, area_to_Frames.areaid);
                    syncFrame.msg_list = NetCommon.extract_msg(p_syncFrame.msg_list);
                    areaManager.received_sync_frame(area_to_Frames.areaid, syncFrame);

                }
            }
        }
    }



    public static CustomSyncMsg DSerializeData(MemoryStream ms, BinaryReader br)
    {

        int player_id = br.ReadInt32();
        int msg_type = br.ReadInt32();

        if (msg_type == (int)RequestType.ENTERAREA)
        {
            int id = br.ReadInt32();
            int health = br.ReadInt32();
            float positionx = br.ReadSingle();
            float positiony   =br.ReadSingle();
            float positionz   =br.ReadSingle();
            float directionx  =br.ReadSingle();
            float directiony  =br.ReadSingle();
            float directionz  =br.ReadSingle();
            float rotationx   =br.ReadSingle();
            float rotationy   =br.ReadSingle();

            var msg = new EnterAreaMessage(player_id, health,
                new Vector2(rotationx, rotationy),
                new Vector3(directionx, directiony, directionz),
                new Vector3(positionx, positiony, positionx));

           // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.INPUT)
        {
            int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            var msg = new InputMessage(player_id, new Vector3(x, y, z));
          //  msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.LEAVEAREA)
        {
            int id = br.ReadInt32();
            var msg = new LeaveAreaMessage(player_id);
           // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.ROTATE)
        {
            int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();

            var msg = new RotateMessage(player_id, new Vector2(x,y));
           // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.POSITION)
        {
            int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            var msg = new PositionMessage(player_id, new Vector2(x, y));
           // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.SPAWN)
        {
            int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            var msg = new SpawnMessage(player_id, new Vector3(x, y,z));

           // msg.id = id;
            return msg;
        }
        else
        {
            return null;
        }

        
    }

    public void RecordFrames(byte[] buffers)
    {

        //var buffers = Snappy.Sharp.SnappyDecompressor.Decompress(Tbuffers,0, Tbuffers.Length);

        var ms = new MemoryStream(buffers);
        var br = new BinaryReader(ms);

        int nAreaCount = br.ReadChar();
        for (int nLoop = 0; nLoop < nAreaCount; nLoop++)
        {
            int nAreaid =  br.ReadChar();
            int nSyncFrameCount = br.ReadInt16();
            for (int i = 0; i < nSyncFrameCount; i++)
            {
                int nFrameIndex  = br.ReadInt16();
                int nFrameMsgCount = br.ReadChar();
                SyncFrame cur = new SyncFrame(nFrameIndex, nAreaid);

                for (int msgLoop = 0; msgLoop < nFrameMsgCount; msgLoop++)
                {
                   var tt = DSerializeData(ms, br);
                    if(tt!= null)
                    {
                        cur.msg_list.Add(tt);
                    }
                }

                areaManager.received_sync_frame(nAreaid, cur);
            }
        }

        br.Close();
        ms.Close();
    }


    public Queue<NetworkMsg> GetSendBuffer()
    {
        return this.sendBuffer;
    }
}
