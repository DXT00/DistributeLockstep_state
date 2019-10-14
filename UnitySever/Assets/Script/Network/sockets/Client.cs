using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.IO;
using ProtoBuf;
public class Client
{
    public Socket acceptSocket;
    public int connectionID;
    public IPAddress clientIP;
    public int clientPort;
    public UnityEngine.Vector2 Posion;

 
    public const int MaxSendBuffer = 2 * 1024 * 1024;
    public byte[] buffer = new byte[MaxSendBuffer];

    MemoryStream ms = null;// new MemoryStream();
    BinaryWriter br = null; // new BinaryWriter(ms);

    public Client()
    {
       
    }

    public void RecordData(byte[] data, int length, List<DS_protocol.p_AllMsg> TMsgs, List<int> TempMsgConenct)
    {

        if (data != null && length > 4)
        {
            MemoryStream ms = new MemoryStream(data,0, length);
            BinaryReader br = new BinaryReader(ms);

            int currentIndex = 0;
            
            while (currentIndex + 4 < length)
            {
                int len = br.ReadInt32();

                byte[] result = br.ReadBytes(len);

                MemoryStream Tms = new MemoryStream(result);

                DS_protocol.p_AllMsg p_allMsg = null;

                try
                {
                    UnityEngine.Profiling.Profiler.BeginSample("RecordData::Deserialize");
                    p_allMsg = Serializer.Deserialize<DS_protocol.p_AllMsg>(Tms);
                    UnityEngine.Profiling.Profiler.EndSample();

                }
                catch( Exception  )
                {
                    return;
                }

                if (p_allMsg.type != (int)CmdType.ASKFRAME)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("RecordData::HandleData");
                    NetCommon.HandleData(connectionID, p_allMsg);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                else
                {
                    TempMsgConenct.Add(connectionID);
                    TMsgs.Add(p_allMsg);
                }
         
                currentIndex += len+4;
            }
        }
    }

    public void RecordAction()
    {
        //for( int nLoop = 0)
    }

    public int SendData()
    {
        if (ms != null)
        {
            var bu = ms.ToArray();
            System.Buffer.BlockCopy(BitConverter.GetBytes(bu.Length - 4), 0, bu, 0, 4);
            int nresult = acceptSocket.Send(bu);
            //UnityEngine.Debug.Log("send Date Size " + result.Length + " nresult " + nresult);
            br.Close();
            ms.Close();
            br = null; 
            ms = null;

            return nresult;
        }

        return 0;
    }
    public void BeginSend(NetworkMsg msg)
    {
        if (ms == null || br == null)
        {
            ms = new MemoryStream();
            br = new BinaryWriter(ms);

            br.Write((int)(0));
        }

        if (ms != null && br != null)
        {
            byte[] data = Serialization.SerializeData( msg);
            if (data != null)
            {
                br.Write(data.Length);
                br.Write(data);
            }
        }
    }
}
