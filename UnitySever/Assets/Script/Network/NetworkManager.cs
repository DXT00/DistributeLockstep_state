using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;

public class NetworkManager
{
    public ServerSocket server;

    public GameServer gameServer;

    public NetworkManager()
    {
        server = new ServerSocket();
    }

    public void StartListen()
    {
        server.StartListen();

    }

    const int MaxBuffer = 1024 * 1024 * 16;
    byte[] BigRecivebuffer = new byte[MaxBuffer]; // 4m
    int ReiveBufLength = 0;

    public long ReciveTotal = 0;
    public long SendTotal = 0;
    public float steptimeTotal = 0;

    public int MaxRecive = 0;
    public int MaxSend = 0;
    public float MaxStep = 0;

    public int CurrentRecive = 0;
    public int CurrentSend = 0;
    public float CurrentStep = 0;

    public List<DS_protocol.p_AllMsg> TempMsg = new List<DS_protocol.p_AllMsg>();
    public List<int> TempMsgConenct = new List<int>();
    public void Update()
    {
        CurrentRecive = 0;
        CurrentSend = 0;
        lock (server.clients)
        {

            TempMsg.Clear();
            TempMsgConenct.Clear();

            foreach (Client client in server.clients.Values)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Socket::RecordData");
                ReiveBufLength = server.ReceiveData(client.acceptSocket, BigRecivebuffer);
                UnityEngine.Profiling.Profiler.EndSample();
                if (ReiveBufLength != 0)
                {
                    CurrentRecive += ReiveBufLength + 4;
                    UnityEngine.Profiling.Profiler.BeginSample("RecordData");
                    client.RecordData(BigRecivebuffer, ReiveBufLength, TempMsg, TempMsgConenct);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }

            for (int nLoop = 0; nLoop < TempMsg.Count; ++nLoop)
            {
                UnityEngine.Profiling.Profiler.BeginSample("RecordData::HandleData");
                NetCommon.HandleData(TempMsgConenct[nLoop], TempMsg[nLoop]);
                UnityEngine.Profiling.Profiler.EndSample();
            }


            UnityEngine.Profiling.Profiler.BeginSample("client.SendData");

            foreach (Client client in server.clients.Values)
            {
                CurrentSend += client.SendData();
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        if (gameServer.GetRoom(0).gameMap.GetFrame() > 50)
        {
            if (MaxRecive < CurrentRecive)
            {
                MaxRecive = CurrentRecive;
            }

            if (MaxSend < CurrentSend)
            {
                MaxSend = CurrentSend;
            }
        }

        ReciveTotal += CurrentRecive;
        SendTotal += CurrentSend;

    }
    public void Init()
    {

    }

    public void SendDataTo(int clientID, NetworkMsg msg)
    {
        if(msg != null)
        {
            lock(Serialization.SerializationLock)
            {
                //NetCommon.WriteMessage("roomId: " +msg.uid + "sends msg to client: " + clientID);
               // UnityEngine.Debug.Log("send Date Size " );
                Client client = server.clients[clientID];
                client.BeginSend(msg);
            }
        }
    }
    public void Stop()
    {

        NetCommon.PrintLog();
    }
    public void RegisterGameServer(GameServer gameServer)
    {
        this.gameServer = gameServer;
    }
}