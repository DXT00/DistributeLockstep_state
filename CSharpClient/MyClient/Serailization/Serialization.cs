using System;
using System.IO;
using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Serialization
{
    public static byte[] Serialize(NetworkMsg networkMsg)//在这可以继续添加要发送的Msg
    {
        var p_TheAllMsg = new DS_protocol.p_AllMsg();

        p_TheAllMsg.player_id = networkMsg.player_id;
        p_TheAllMsg.type = networkMsg.type;

        if (networkMsg.type == (int)CmdType.ASKFRAME)
        {
            var askFrame = networkMsg as AskFrame;
            p_TheAllMsg.AskFrame = new DS_protocol.p_AskFrame();

            askFrame.areas.ForEach(i => p_TheAllMsg.AskFrame.areas.Add(i));
            askFrame.frames.ForEach(i => p_TheAllMsg.AskFrame.frames.Add(i));

        }
        else if (networkMsg.type == (int)CmdType.START)
        {
            var startGame = networkMsg as StartGame;

            p_TheAllMsg.StartGame = new DS_protocol.p_StartGame();
            p_TheAllMsg.StartGame.test = 0;
        }
        else if (networkMsg.type == (int)CmdType.FRAME)
        {
            var frame = networkMsg as Frame;

            p_TheAllMsg.Frame = new DS_protocol.p_Frame();
            p_TheAllMsg.Frame.buffers = SerializeFrame(frame);
           // p_TheAllMsg.Frame.syncFrame = new DS_protocol.p_SyncFrame();
           // p_TheAllMsg.Frame.area_id = frame.syncFrame.get_area_id();
           // p_TheAllMsg.Frame.syncFrame.frame_count = frame.syncFrame.get_frame_count();

           // Buffer_SyncFrame_msg_list(frame.syncFrame.get_msg(), p_TheAllMsg.Frame.syncFrame.msg_list);
        }
        else if (networkMsg.type == (int)CmdType.JOIN)
        {
            var join = networkMsg as Join;
            p_TheAllMsg.Join = new DS_protocol.p_Join();
            p_TheAllMsg.Join.room_id = join.room_id;
        }

        return Serialize<DS_protocol.p_AllMsg>(p_TheAllMsg);
    }

    public static void SerializeData(BinaryWriter br, CustomSyncMsg msg)
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
            //br.Write(rotate.id);
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
           // br.Write(spawn.id);
            br.Write(spawn.position.x);
            br.Write(spawn.position.y);
            br.Write(spawn.position.z);
        }
        else if (msg.msg_type == (int)RequestType.SYNPOS)
        {
            SYNMessage spawn = msg as SYNMessage;
            br.Write(spawn.Players.Count);

            foreach( var item in spawn.Players)
            {
                br.Write(item.Key);
                br.Write(item.Value.x);
                br.Write(item.Value.y);
            }
        }
    }

    public static byte[] SerializeFrame(Frame replyAskFrame)
    {
        var ms = new MemoryStream();
        var br = new BinaryWriter(ms);

        br.Write((char) replyAskFrame.syncFrame.area_id);
        //br.Write((short)replyAskFrame.syncFrame.frame_index);
        br.Write((short)replyAskFrame.syncFrame.msg_list.Count);

        for (int nLoop = 0; nLoop < replyAskFrame.syncFrame.msg_list.Count; nLoop++)
        {
            SerializeData(br, replyAskFrame.syncFrame.msg_list[nLoop]);
        }

        var t = ms.ToArray();
        br.Close();
        ms.Close();
        return t;
    }



    public static void Buffer_SyncFrame_msg_list(List<CustomSyncMsg> msg_list, List<DS_protocol.p_CustomSyncMsg> p_msg_list )
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



