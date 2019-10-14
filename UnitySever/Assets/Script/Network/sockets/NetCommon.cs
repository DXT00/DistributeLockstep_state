using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using UnityEngine;

public class NetCommon
{
    public static int PortNum = 5000;
    public static string IP = "192.168.137.1";
    public static GameServer gameServer;
    public static List<double> SendTime = new List<double>();
    public static List<double> ReceiveTime = new List<double>();
    public static List<double> TickTime = new List<double>();
    public static bool start;

    public static void PrintLog()
    {
        string info = "";
        double time= 0f;
        double averageTime;
        double avgSendRecvTickTime = 0;
        foreach(double ms in SendTime)
        {
            //info += "ms: " + ms.ToString() + "\r\n";
            time += ms;
        }
        averageTime = time / SendTime.Count;
        info += "average send time: " + averageTime.ToString();
        avgSendRecvTickTime += averageTime;
        WriteMessage(info);
        time = 0;
        info = "";
        foreach(double ms in ReceiveTime)
        {
            //info += "ms: " + ms.ToString() + "\r\n";
            time += ms;
        }
        if (ReceiveTime.Count != 0)
        {
            averageTime = time / ReceiveTime.Count;
            info += "average receive time: " + averageTime.ToString();
        }
        avgSendRecvTickTime += averageTime;
        WriteMessage(info);
 

        time = 0;
        info = "";
        foreach (double ms in TickTime)
        {
            time += ms;
        }
        averageTime = time / TickTime.Count;
        info += "average tick time: " + averageTime.ToString();
        WriteMessage(info);

        avgSendRecvTickTime += averageTime;
        info = "";
        info += "average Send+Recv+Tick time: " + avgSendRecvTickTime.ToString();
        WriteMessage(info);

        info = "";
        //info += "total transaction bytes: " + DebugTool.GetTotalBytes().ToString();
        WriteMessage(info);

        info = "-----------------------";
        WriteMessage(info);

    }
    public static void RecordSendTime(double ms)
    {
        if(start)
        {
            SendTime.Add(ms);
        }
    }
    public static void RecordReceiveTime(double ms)
    {
        if(start)
        {
            ReceiveTime.Add(ms);
        }
    }
    public static void RecordTickTime(double ms)
    {
        if (start)
        {
            TickTime.Add(ms);
        }
    }
    public static void WriteMessage(string msg)
    {
        using (FileStream fs = new FileStream(@"C:\Users\Devlinzhou\Desktop\ServerTick.txt", FileMode.OpenOrCreate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine("{0}\n", msg, DateTime.Now);
                sw.Flush();
            }
        }
    }

    public static CustomSyncMsg DSerializeData(BinaryReader br, int areaid)
    {

        int player_id = br.ReadInt16();
        int msg_type = br.ReadChar();

        if (msg_type == (int)RequestType.ENTERAREA)
        {
            //int id = br.ReadInt32();
            int health = br.ReadInt32();
            float positionx = br.ReadSingle();
            float positiony = br.ReadSingle();
            float positionz = br.ReadSingle();
            float directionx = br.ReadSingle();
            float directiony = br.ReadSingle();
            float directionz = br.ReadSingle();
            // float rotationx = br.ReadSingle();
            // float rotationy = br.ReadSingle();

            var msg = new EnterAreaMessage(player_id, health,
                new Vector2(0, 0),
                new Vector3(directionx, directiony, directionz),
                new Vector3(positionx, positiony, positionz));

            // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.INPUT)
        {
            // int id = br.ReadInt32();
            float x = br.ReadSingle();
            // float y = br.ReadSingle();
            float z = br.ReadSingle();
            var msg = new InputMessage(player_id, new Vector3(x, 0, z));
            //  msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.LEAVEAREA)
        {
            //int id = br.ReadInt32();
            var msg = new LeaveAreaMessage(player_id);
            // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.ROTATE)
        {
            //int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();

            var msg = new RotateMessage(player_id, new Vector2(x, y));
            // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.POSITION)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            var msg = new PositionMessage(player_id, new Vector2(x, y));
            // msg.id = id;
            gameServer.SetPlayerPosition(player_id, areaid, new UnityEngine.Vector2(x, y));
            return msg;
        }
        else if (msg_type == (int)RequestType.SPAWN)
        {
            //int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            var msg = new SpawnMessage(player_id, new Vector3(x, y, z));

            // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.SYNPOS)
        {
            int Count = br.ReadInt32();

            for (int n = 0; n < Count; ++n)
            {
                int TPlayerid = br.ReadInt32();
                float x = br.ReadSingle();
                float y = br.ReadSingle();

                gameServer.SetSYNPlayerPosition(player_id, TPlayerid, new UnityEngine.Vector2(x, y));
            }


            // msg.id = id;
            return null;
        }
        else
        {
            return null;
        }


    }

    public static int DSerializeData(byte[] buffers, List<CustomSyncMsg> customSyncMsgs )
    {
        var ms = new MemoryStream(buffers);
        var br = new BinaryReader(ms);

        List<DS_protocol.p_IdToFrames> idtofreames = new List<DS_protocol.p_IdToFrames>();


        int nArea_id = br.ReadChar();
       // int Frameindex = br.ReadInt16();
        int msgCount = br.ReadInt16();

        for (int nLoop = 0; nLoop < msgCount; nLoop++)
        {
            DS_protocol.p_IdToFrames TIdToFrame = new DS_protocol.p_IdToFrames();

            var tms = DSerializeData(br, nArea_id);
            if( tms != null )
                customSyncMsgs.Add(tms);
        }

        br.Close();
        ms.Close();

        return nArea_id;
    }



    public static void HandleData(int clientID, DS_protocol.p_AllMsg p_allmsg)
    {
        //p_AllMsg ReceiveMsg = new p_AllMsg();
        int msg_type = p_allmsg.type;
        //int seq = p_allmsg.BaseProtocol.seq;
        //gameServer.hanldeSeq(client_id, seq);

        //Console.WriteLine("get client: " + client_id + "seq pkg : " + seq);
        if (msg_type == (int)CmdType.START)
        {
            Console.Write("get start info\n");
            
            gameServer.PreGameStart(clientID);
        }
        else if (msg_type == (int)CmdType.FRAME)
        {
            //Console.WriteLine("get frame from client :" + client_id);
            //int frame_count = p_allmsg.Frame.syncFrame.frame_count;
            List<CustomSyncMsg> customSyncMsgs = new List<CustomSyncMsg>();
            int area_id = DSerializeData( p_allmsg.Frame.buffers, customSyncMsgs);
            
             //   extract_msg(p_allmsg.Frame.syncFrame.msg_list, area_id);


            // if (customSyncMsgs.Count != 3)
            //  UnityEngine.Debug.Log("clientID: " + clientID + " area_id- " + area_id + "customSyncMsgs Count " + customSyncMsgs.Count);
            gameServer.OnGetFrame(clientID, area_id, customSyncMsgs);
        }
        else if (msg_type == (int)CmdType.JOIN)
        {
             int room_id = p_allmsg.Join.room_id;
            //int player_id = allMsg.Join.player_id;
            Console.WriteLine("get join info, it is from :" + clientID + " Progress id" + room_id);
            gameServer.OnPlayerJoin(clientID, room_id);
        }
        else if (msg_type == (int)CmdType.ASKFRAME)
        {
            //Console.WriteLine("get asked chase frame request");
            UnityEngine.Profiling.Profiler.BeginSample("HandleData::ASKFRAME");
            gameServer.OnAskedFrame(clientID, p_allmsg.AskFrame.areas, p_allmsg.AskFrame.frames);
            UnityEngine.Profiling.Profiler.EndSample();
        }

    }

    public static List<CustomSyncMsg> extract_msg(List<DS_protocol.p_CustomSyncMsg> p_msg_list, int areaid)
    {
        List<CustomSyncMsg> msg_list = new List<CustomSyncMsg>();
        CustomSyncMsg msg;
        if (p_msg_list != null)
        {
            foreach (DS_protocol.p_CustomSyncMsg p_msg in p_msg_list)
            {
                if (p_msg.msg_type == (int)RequestType.INPUT)
                {
                    DS_protocol.p_InputMessage i_msg = p_msg.Input;
                    int player_id = p_msg.player_id;
                    float moving_x = i_msg.moving_x;
                    float moving_y = i_msg.moving_y;
                    float moving_z = i_msg.moving_z;
                    //Console.WriteLine("get input info: " + player_id +" " + moving_x+" " + moving_y + " " + moving_z);
                    msg = new InputMessage(player_id, new Vector3(moving_x, moving_y, moving_z));
                    msg_list.Add(msg);
                }
                else if (p_msg.msg_type == (int)RequestType.ROTATE)
                {
                    DS_protocol.p_RotateMessage r_msg = p_msg.Rotate;
                    int player_id = p_msg.player_id;
                    float rotation_x = r_msg.delta_x;
                    float rotation_y = r_msg.delta_y;
                    //Console.WriteLine("get rotate info: " + player_id + "rotation_x-- " + rotation_x + "rotation_y-- " + rotation_y);
                    msg = new RotateMessage(player_id, new Vector2(rotation_x, rotation_y));
                    msg_list.Add(msg);
                }
                else if (p_msg.msg_type == (int)RequestType.POSITION)
                {
                    DS_protocol.p_PositionMessage r_msg = p_msg.Position;
                    int player_id = p_msg.player_id;
                    float rotation_x = r_msg.delta_x;
                    float rotation_y = r_msg.delta_y;

                    gameServer.SetPlayerPosition(player_id, areaid, new UnityEngine.Vector2(rotation_x, rotation_y));

                    //UnityEngine.Debug.Log("get rotate info: " + player_id + "position_x-- " + rotation_x + "position_y-- " + rotation_y);
                    //Console.WriteLine("get rotate info: " + player_id + "rotation_x-- " + rotation_x + "rotation_y-- " + rotation_y);
                    // msg = new PositionMessage(player_id, new Vector2(rotation_x, rotation_y));
                    // msg_list.Add(msg);
                }
                else if (p_msg.msg_type == (int)RequestType.SPAWN)
                {
                    DS_protocol.p_SpawnMessage s_msg = p_msg.Spawn;
                    int player_id = p_msg.player_id;
                    float position_x = s_msg.position_x;
                    float position_y = s_msg.position_y;
                    float position_z = s_msg.position_z;
                    //Console.WriteLine("get spawn info: " + player_id + "position_x-- " + position_x + "position_y-- " + position_y + "position_z--" + position_z);
                    msg = new SpawnMessage(player_id, new Vector3(position_x, position_y, position_z));
                    msg_list.Add(msg);
                }
                else if (p_msg.msg_type == (int)RequestType.ENTERAREA)
                {
                    DS_protocol.p_EnterAreaMessage e_msg = p_msg.EnterArea;
                    int player_id = p_msg.player_id;
                    int health = e_msg.health;
                    float position_x = e_msg.position_x;
                    float position_y = e_msg.position_y;
                    float position_z = e_msg.position_z;

                    float direction_x = e_msg.direction_x;
                    float direction_y = e_msg.direction_y;
                    float direction_z = e_msg.direction_z;

                    float rotation_x = e_msg.rotation_x;
                    float rotation_y = e_msg.rotation_y;
                    //Console.WriteLine("get enterArea info: " + player_id + "position_x-- " + position_x + "position_y-- " + position_y + "position_z--" + position_z);
                    msg = new EnterAreaMessage(player_id, health, new Vector2(rotation_x, rotation_y), new Vector3(direction_x, direction_y, direction_z),
                        new Vector3(position_x, position_y, position_z));
                    msg_list.Add(msg);

                }
                else if (p_msg.msg_type == (int)RequestType.LEAVEAREA)
                {
                    int player_id = p_msg.player_id;
                    msg = new LeaveAreaMessage(player_id);
                    msg_list.Add(msg);
                }
            }
        }
        return msg_list;
    }
}

