using System;
using System.Collections.Generic;
using System.IO;

using ProtoBuf;


public class Serialization
{
    public static object SerializationLock = new object();


   
    public static byte[] SerializeData(NetworkMsg networkMsg)
    {
        var p_AllMsg = new DS_protocol.p_AllMsg();

        p_AllMsg.player_id = networkMsg.player_id;
        p_AllMsg.type = networkMsg.type;


        if (networkMsg.type == (int)CmdType.REPLYJOIN)
        {
            var replyJoin = networkMsg as ReplyJoin;

            p_AllMsg.ReplyJoin = new DS_protocol.p_ReplyJoin();
            p_AllMsg.ReplyJoin.real_player_id = replyJoin.real_player_id;

        }
        else if (networkMsg.type == (int)CmdType.REPLYASKFRAME)
        {
            var replyAskFrame = networkMsg as ReplyAskFrame;
            p_AllMsg.ReplyAskFrame = new DS_protocol.p_ReplyAskFrame();
            /*UnityEngine.Profiling.Profiler.BeginSample("SerializeDatareplyAskFramelast");
            for (int nLoop = 0; nLoop < replyAskFrame.areas.Count; nLoop++)
            {
                DS_protocol.p_IdToFrames idtoframe = new DS_protocol.p_IdToFrames();

                idtoframe.areaid = replyAskFrame.areas[nLoop];

                List<SyncFrame> list = replyAskFrame.frames[nLoop];
                for (int i = 0; i < list.Count; i++)
                {
                    DS_protocol.p_SyncFrame p_cur = new DS_protocol.p_SyncFrame();
                    var cur = list[i];
                    //p_cur.area_id = cur.get_area_id();
                    p_cur.frame_count = cur.get_frame_count();
                    Buffer_SyncFrame_msg_list2(cur.get_msg(),p_cur.msg_list);
                    idtoframe.frames.Add(p_cur);
                }

                p_AllMsg.ReplyAskFrame.areas_to_frames.Add(idtoframe);
            }
            UnityEngine.Profiling.Profiler.EndSample();*/
            UnityEngine.Profiling.Profiler.BeginSample("SerializeDatareplyAskFrame");
            var buffers = SerializeDatareplyAskFrame(replyAskFrame);
            UnityEngine.Profiling.Profiler.EndSample();

            try
            {
//                 UnityEngine.Profiling.Profiler.BeginSample("SerializeDatareplyCompress");
//                 p_AllMsg.ReplyAskFrame.buffers = Snappy.Sharp.Snappy.Compress(buffers);
//                 UnityEngine.Profiling.Profiler.EndSample();

              //  UnityEngine.Debug.Log("Compress " +buffers.Length );//+ "  "+ p_AllMsg.ReplyAskFrame.buffers.Length );

                p_AllMsg.ReplyAskFrame.buffers = buffers;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("Compress SendBuffer Failure! ");
            }
        }
        else if (networkMsg.type == (int)CmdType.REPLYSTART)
        {
            var replyStart = networkMsg as ReplyStart;
            p_AllMsg.ReplyStart = new DS_protocol.p_ReplyStart();
            p_AllMsg.ReplyStart.start = replyStart.start;
        }

        return Serialize<DS_protocol.p_AllMsg>(p_AllMsg);
    }

    public static void SerializeData( BinaryWriter br, CustomSyncMsg msg )
    {
        br.Write((short)msg.player_id);
        br.Write((char)msg.msg_type);

        if (msg.msg_type == (int)RequestType.ENTERAREA)
        {
            EnterAreaMessage enterArea = msg as EnterAreaMessage;
           // br.Write(enterArea.id);
            br.Write(enterArea.health); ;
            br.Write(enterArea.position.x);
            br.Write(enterArea.position.y);
            br.Write(enterArea.position.z);
            br.Write(enterArea.direction.x);
            br.Write(enterArea.direction.y);
            br.Write(enterArea.direction.z);
            //br.Write(enterArea.rotation.x);
            //br.Write(enterArea.rotation.y);
        }
        else if (msg.msg_type == (int)RequestType.INPUT)
        {
            InputMessage input = msg as InputMessage;
           // br.Write(input.id);
            br.Write(input.moving.x);
           // br.Write(input.moving.y);
            br.Write(input.moving.z);
        }
        else if (msg.msg_type == (int)RequestType.LEAVEAREA)
        {
            LeaveAreaMessage leaveArea = msg as LeaveAreaMessage;
            //br.Write(leaveArea.id);
        }
        else if (msg.msg_type == (int)RequestType.ROTATE)
        {
            RotateMessage rotate = msg as RotateMessage;
           // br.Write(rotate.id);
            br.Write(rotate.delta.x);
            br.Write(rotate.delta.y);
        }
        else if (msg.msg_type == (int)RequestType.POSITION)
        {
            PositionMessage rotate = msg as PositionMessage;
            //br.Write(rotate.id);
            br.Write(rotate.delta.x);
            br.Write(rotate.delta.y);
        }
        else if (msg.msg_type == (int)RequestType.SPAWN)
        {
            SpawnMessage spawn = msg as SpawnMessage;
            //br.Write(spawn.id);
            br.Write(spawn.position.x);
            br.Write(spawn.position.y);
            br.Write(spawn.position.z);
        }
    }

    public static byte[] SerializeDatareplyAskFrame(ReplyAskFrame replyAskFrame)
    {
        var ms = new MemoryStream();
        var br = new BinaryWriter(ms);

        var areas = replyAskFrame.areas;
        br.Write((char)areas.Count);
        for (int nLoop = 0; nLoop < areas.Count; nLoop++)
        {
            br.Write((char)areas[nLoop]);
            List<SyncFrame> list = replyAskFrame.frames[nLoop];
            br.Write((short)list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                SyncFrame cur = list[i];

                if (i == 0)
                {
                    br.Write((short)cur.frame_count);
                }
                
                br.Write((char)cur.msg_list.Count);

                for (int msgLoop = 0; msgLoop< cur.msg_list.Count; msgLoop++)
                {
                    SerializeData(br, cur.msg_list[msgLoop]);
                }
            }
        }

        var t = ms.ToArray();
        br.Close();
        ms.Close();
        return t;
    }



    public static void Buffer_SyncFrame_msg_list2(List<CustomSyncMsg> msg_list, List<DS_protocol.p_CustomSyncMsg> p_msg_list)
    {
        foreach (CustomSyncMsg msg in msg_list)
        {
            if (msg.msg_type == (int)RequestType.ENTERAREA)
            {
                EnterAreaMessage enterArea = msg as EnterAreaMessage;
                DS_protocol.p_EnterAreaMessage p_msg = new DS_protocol.p_EnterAreaMessage();
                p_msg.id = enterArea.id;
                p_msg.health = enterArea.health; ;
                p_msg.position_x = enterArea.position.x;
                p_msg.position_y = enterArea.position.y;
                p_msg.position_z = enterArea.position.z;

                p_msg.direction_x = enterArea.direction.x;
                p_msg.direction_y = enterArea.direction.y;
                p_msg.direction_z = enterArea.direction.z;

                p_msg.rotation_x = enterArea.rotation.x;
                p_msg.rotation_y = enterArea.rotation.y;


                //p_msg.area_id = enterArea.area_id;

                DS_protocol.p_CustomSyncMsg p_basemsg = new DS_protocol.p_CustomSyncMsg();
                p_basemsg.msg_type = enterArea.msg_type;
                p_basemsg.player_id = enterArea.player_id;
                p_basemsg.EnterArea = p_msg;
                p_msg_list.Add(p_basemsg);
            }
            if (msg.msg_type == (int)RequestType.INPUT)
            {
                InputMessage input = msg as InputMessage;
                DS_protocol.p_InputMessage p_msg = new DS_protocol.p_InputMessage();

                p_msg.moving_x = input.moving.x;
                p_msg.moving_y = input.moving.y;
                p_msg.moving_z = input.moving.z;

                //p_msg.area_id = input.area_id;

                DS_protocol.p_CustomSyncMsg p_basemsg = new DS_protocol.p_CustomSyncMsg();
                p_basemsg.msg_type = input.msg_type;
                p_basemsg.player_id = input.player_id;
                p_basemsg.Input = p_msg;
                p_msg_list.Add(p_basemsg);

            }
            if (msg.msg_type == (int)RequestType.LEAVEAREA)
            {
                LeaveAreaMessage leaveArea = msg as LeaveAreaMessage;
                DS_protocol.p_LeaveAreaMessage p_msg = new DS_protocol.p_LeaveAreaMessage();

                p_msg.id = leaveArea.id;

                //p_msg.area_id = leaveArea.area_id;
                DS_protocol.p_CustomSyncMsg p_basemsg = new DS_protocol.p_CustomSyncMsg();
                p_basemsg.msg_type = leaveArea.msg_type;
                p_basemsg.player_id = leaveArea.player_id;
                p_basemsg.LeaveArea = p_msg;
                p_msg_list.Add(p_basemsg);

            }
            if (msg.msg_type == (int)RequestType.ROTATE)
            {
                RotateMessage rotate = msg as RotateMessage;
                DS_protocol.p_RotateMessage p_msg = new DS_protocol.p_RotateMessage();


                p_msg.delta_x = rotate.delta.x;
                p_msg.delta_y = rotate.delta.y;

                //p_msg.area_id = rotate.area_id;

                DS_protocol.p_CustomSyncMsg p_basemsg = new DS_protocol.p_CustomSyncMsg();
                p_basemsg.msg_type = rotate.msg_type;
                p_basemsg.player_id = rotate.player_id;
                p_basemsg.Rotate = p_msg;
                p_msg_list.Add(p_basemsg);

            }
            if (msg.msg_type == (int)RequestType.POSITION)
            {
                PositionMessage rotate = msg as PositionMessage;
                DS_protocol.p_PositionMessage p_msg = new DS_protocol.p_PositionMessage();

                p_msg.delta_x = rotate.delta.x;
                p_msg.delta_y = rotate.delta.y;

                // p_msg.area_id = rotate.area_id;

                DS_protocol.p_CustomSyncMsg p_basemsg = new DS_protocol.p_CustomSyncMsg();
                p_basemsg.msg_type = rotate.msg_type;
                p_basemsg.player_id = rotate.player_id;
                p_basemsg.Position = p_msg;
                p_msg_list.Add(p_basemsg);

            }
            if (msg.msg_type == (int)RequestType.SPAWN)
            {
                SpawnMessage spawn = msg as SpawnMessage;
                DS_protocol.p_SpawnMessage p_msg = new DS_protocol.p_SpawnMessage();

                p_msg.id = spawn.id;
                p_msg.position_x = spawn.position.x;
                p_msg.position_y = spawn.position.y;
                p_msg.position_z = spawn.position.z;


                //p_msg.area_id = spawn.area_id;

                DS_protocol.p_CustomSyncMsg p_basemsg = new DS_protocol.p_CustomSyncMsg();
                p_basemsg.msg_type = spawn.msg_type;
                p_basemsg.player_id = spawn.player_id;
                p_basemsg.Spawn = p_msg;
                p_msg_list.Add(p_basemsg);

            }
        }
    }


 

    public static Byte[] Serialize<T>(T obj)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            Serializer.Serialize(memory, obj);
            return memory.ToArray();
        }
    }
}