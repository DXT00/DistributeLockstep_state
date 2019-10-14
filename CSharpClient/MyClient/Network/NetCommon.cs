using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

public class NetCommon
{
    public static int PortNum = 5000;
    public static int Tick_cnt = 0;
    public static int flag = 0;
    public static int player_nums= Program.robotSystem.robotsNumber();
    public static void WriteMessage(string msg)
    {
        using (FileStream fs = new FileStream(@".\log.txt", FileMode.OpenOrCreate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine("{0}\n", msg, DateTime.Now);
                sw.Flush();
            }
        }
    }


    public static CustomSyncMsg DSerializeData(MemoryStream ms, BinaryReader br)
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
            //int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            var msg = new PositionMessage(player_id, new Vector2(x, y));
            // msg.id = id;
            return msg;
        }
        else if (msg_type == (int)RequestType.SPAWN)
        {
           // int id = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            var msg = new SpawnMessage(player_id, new Vector3(x, y, z));

            // msg.id = id;
            return msg;
        }
        else
        {
            return null;
        }


    }

    public static List<DS_protocol.p_IdToFrames> DSerializeData(byte[] buffers)
    {
        var ms = new MemoryStream(buffers);
        var br = new BinaryReader(ms);

        List<DS_protocol.p_IdToFrames> idtofreames = new List<DS_protocol.p_IdToFrames>(); 

        int nAreaCount = br.ReadChar();
        for (int nLoop = 0; nLoop < nAreaCount; nLoop++)
        {
            DS_protocol.p_IdToFrames TIdToFrame = new DS_protocol.p_IdToFrames();


            TIdToFrame.areaid  = br.ReadChar();
            int nSyncFrameCount = br.ReadInt16();

            int nStartIndex = 0;
            for (int i = 0; i < nSyncFrameCount; i++)
            {
                DS_protocol.p_SyncFrame TFrame = new DS_protocol.p_SyncFrame();
                //TFrame.frame_count = br.ReadInt16();
      
                if(i == 0)
                {
                    nStartIndex = br.ReadInt16();
                }
                int nFrameMsgCount = br.ReadChar();
                TFrame.frame_count = nStartIndex++;
                SyncFrame cur = new SyncFrame(TFrame.frame_count, TIdToFrame.areaid);

                for (int msgLoop = 0; msgLoop < nFrameMsgCount; msgLoop++)
                {
                    var tt = DSerializeData(ms, br);
                    if (tt != null)
                    {
                        cur.msg_list.Add(tt);
                    }
                }

                Serialization.Buffer_SyncFrame_msg_list(cur.msg_list, TFrame.msg_list);
                TIdToFrame.frames.Add(TFrame);
            }

            idtofreames.Add(TIdToFrame);
        }

        br.Close();
        ms.Close();

        return idtofreames;
    }

    public static void HandleData(Robot robot, DS_protocol.p_AllMsg p_allmsg)
    {
        int msg_type = p_allmsg.type;

        if (msg_type == (int)CmdType.REPLYJOIN)
        {
            int id = p_allmsg.ReplyJoin.real_player_id;
            List<string> names = new List<string>();// p_allmsg.ReplyJoin.player_names;
            robot.OnJoinReply(id, names);
        }
        else if (msg_type == (int)CmdType.REPLYSTART)
        {
            bool flag = p_allmsg.ReplyStart.start;
            robot.Start(flag);
        }
        else if (msg_type == (int)CmdType.REPLYASKFRAME)
        {
            var Ttest = DSerializeData(p_allmsg.ReplyAskFrame.buffers);

            robot.OnGetFrames(Ttest);
          /*  if (Commpare(Ttest, p_allmsg.ReplyAskFrame.areas_to_frames))
            {
                robot.OnGetFrames(Ttest);
            }
            else
            {
                robot.OnGetFrames(p_allmsg.ReplyAskFrame.areas_to_frames);

                robot.OnGetFrames(Ttest);
            }*/

            
           // 
            //robot.OnGetFrames(p_allmsg.ReplyAskFrame.buffers);
        }
        else if (msg_type == (int)CmdType.SPAWNINFO)
        {
            int areaID = p_allmsg.player_id; //此处msg用playerID 表示了areaID
            robot.OnInitMsg(areaID);
        }
    }

    public static bool Commpare(DS_protocol.p_CustomSyncMsg T1Ms, DS_protocol.p_CustomSyncMsg T2Ms)
    {
        if( T1Ms.player_id != T2Ms.player_id )
        {
            return false;
        }

        if(T1Ms.msg_type != T2Ms.msg_type)
        {
            return false;
        }

        if (T1Ms.msg_type == (int)RequestType.ENTERAREA)
        {
            DS_protocol.p_EnterAreaMessage T1Msg = T1Ms.EnterArea;
            DS_protocol.p_EnterAreaMessage T2Msg = T2Ms.EnterArea;

            if (T1Msg.id         != T2Msg.id ||
                T1Msg.health != T2Msg.health ||
                T1Msg.position_x != T2Msg.position_x ||
                T1Msg.position_y != T2Msg.position_y ||
                T1Msg.position_z != T2Msg.position_z ||
                T1Msg.direction_x != T2Msg.direction_x ||
                T1Msg.direction_y != T2Msg.direction_y ||
                T1Msg.direction_z != T2Msg.direction_z ||
                T1Msg.rotation_x != T2Msg.rotation_x ||
                T1Msg.rotation_y != T2Msg.rotation_y)
            {
                return false;
            }
        }
        else if (T1Ms.msg_type == (int)RequestType.INPUT)
        {
            DS_protocol.p_InputMessage T1Msg = T1Ms.Input;
            DS_protocol.p_InputMessage T2Msg = T2Ms.Input;


            if (T1Msg.moving_x != T2Msg.moving_x ||
               T1Msg.moving_y != T2Msg.moving_y ||
               T1Msg.moving_z != T2Msg.moving_z )
            {
                return false;
            }
        }
        else if (T1Ms.msg_type == (int)RequestType.LEAVEAREA)
        {
            DS_protocol.p_LeaveAreaMessage T1Msg = T1Ms.LeaveArea;
            DS_protocol.p_LeaveAreaMessage T2Msg = T2Ms.LeaveArea;

            if (T1Msg.id != T2Msg.id  )
            {
                return false;
            }
        }
        else if (T1Ms.msg_type == (int)RequestType.ROTATE)
        {
            DS_protocol.p_RotateMessage T1Msg = T1Ms.Rotate;
            DS_protocol.p_RotateMessage T2Msg = T2Ms.Rotate;

            if (T1Msg.delta_x != T2Msg.delta_x ||
               T1Msg.delta_y != T2Msg.delta_y )
            {
                return false;
            }
        }
        else if (T1Ms.msg_type == (int)RequestType.POSITION)
        {
            DS_protocol.p_PositionMessage T1Msg = T1Ms.Position;
            DS_protocol.p_PositionMessage T2Msg = T2Ms.Position;

            if (T1Msg.delta_x != T2Msg.delta_x ||
               T1Msg.delta_y != T2Msg.delta_y)
            {
                return false;
            }
        }
        else if (T1Ms.msg_type == (int)RequestType.SPAWN)
        {
            DS_protocol.p_SpawnMessage T1Msg = T1Ms.Spawn;
            DS_protocol.p_SpawnMessage T2Msg = T2Ms.Spawn;

            if (T1Msg.id != T2Msg.id ||
                T1Msg.position_x != T2Msg.position_x ||
                T1Msg.position_y != T2Msg.position_y ||
                T1Msg.position_z != T2Msg.position_z )
            {
                return false;
            }
        }


        return true;
    }



    public static bool Commpare(List<DS_protocol.p_IdToFrames> T1, List<DS_protocol.p_IdToFrames> T2 )
    {
        if( T1.Count != T2.Count)
        {
            return false;
        }

        for (int nLoop = 0; nLoop < T1.Count; ++nLoop)
        {
            DS_protocol.p_IdToFrames T1area_to_Frames = T1[nLoop];
            DS_protocol.p_IdToFrames T2area_to_Frames = T2[nLoop];

            if (T1area_to_Frames.areaid != T2area_to_Frames.areaid)
            {
                return false;
            }

            for (int nLoop2 = 0; nLoop2 < T1area_to_Frames.frames.Count; ++nLoop2)
            {
                DS_protocol.p_SyncFrame T1p_syncFrame = T1area_to_Frames.frames[nLoop2];
                DS_protocol.p_SyncFrame T2p_syncFrame = T2area_to_Frames.frames[nLoop2];

                if (T1p_syncFrame.frame_count != T2p_syncFrame.frame_count)
                {
                    return false;
                }

                if (T1p_syncFrame.msg_list.Count != T2p_syncFrame.msg_list.Count)
                {
                    return false;
                }

                for (int nLoop3 = 0; nLoop3 < T1p_syncFrame.msg_list.Count; ++nLoop3)
                {
                    DS_protocol.p_CustomSyncMsg T1Msg = T1p_syncFrame.msg_list[nLoop3];
                    DS_protocol.p_CustomSyncMsg T2Msg = T2p_syncFrame.msg_list[nLoop3];

                    if (!Commpare( T1Msg, T2Msg))
                    {
                        return false;
                    }


                }

            }
        }


        return true;
    }
        

    public static byte[] Encode(byte[] data)
    {
        byte[] result = new byte[data.Length + 4];

        System.Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, result, 0, 4);
        System.Buffer.BlockCopy(data, 0, result, 4, (int)data.Length);
 
        return result;
    }







    public static List<CustomSyncMsg> extract_msg(List<DS_protocol.p_CustomSyncMsg> p_msg_list)
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

