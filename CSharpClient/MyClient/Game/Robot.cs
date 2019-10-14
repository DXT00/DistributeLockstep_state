using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ProtoBuf;
public class Robot
{
    public ClientSocket clientSocket;
    public RobotGame    robotGame;
    public int          robotID;
    public bool         bCheckSync;

    const int MaxRecive = 1024 * 1024*4;
    byte[] BigRecivebuffer = new byte[MaxRecive]; // 4m

    public Robot(int robotID, string ip, int aren, int size, bool bSYNC = false)
    {
        this.robotID = robotID;
        clientSocket = new ClientSocket(ip);
        robotGame = new RobotGame(robotID, aren, size);
        bCheckSync = bSYNC;

        robotGame.bSYN = bSYNC;
    }

    public void Connect()
    {
        clientSocket.Connet();
    }
    public void SendData(NetworkMsg msg)
    {
        int currentindx = 4;
        var tbuffer = NetCommon.Encode(Serialization.Serialize( msg));
        Array.Copy(tbuffer, 0, BigRecivebuffer, currentindx, tbuffer.Length);
        currentindx += tbuffer.Length;


        if (currentindx > 4)
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(currentindx - 4), 0, BigRecivebuffer, 0, 4);

            clientSocket.SendData(BigRecivebuffer, currentindx);
        }
    }
    public void SendGameData()
    {
        Queue<NetworkMsg> sendBuffer = robotGame.GetSendBuffer();
        int currentindx = 4;
        while(sendBuffer.Count > 0)
        {
            NetworkMsg msg = sendBuffer.Dequeue();

            var tbuffer = NetCommon.Encode(Serialization.Serialize( msg));
            Array.Copy(tbuffer,0, BigRecivebuffer, currentindx, tbuffer.Length );
            currentindx += tbuffer.Length;
        }
        if( currentindx > 4 )
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(currentindx-4), 0, BigRecivebuffer, 0, 4);

            clientSocket.SendData(BigRecivebuffer, currentindx );
        }
    }
    public void ReadData()
    {
       // lock(Serialization.lockObject)

        int ReiveBufLength = clientSocket.ReceiveData( BigRecivebuffer);

        int msgcount = 0;
        if (ReiveBufLength > 4)
        {
            MemoryStream ms = new MemoryStream(BigRecivebuffer, 0, ReiveBufLength);
            BinaryReader br = new BinaryReader(ms);

            int currentIndex = 0;

            while (currentIndex + 4 < ReiveBufLength)
            {
                int len = br.ReadInt32();

                byte[] result = br.ReadBytes(len);

                MemoryStream Tms = new MemoryStream(result);

                DS_protocol.p_AllMsg p_allMsg = Serializer.Deserialize<DS_protocol.p_AllMsg>(Tms);

                NetCommon.HandleData(this, p_allMsg);
                msgcount++;
                currentIndex += 4 + len;
            }
        }

       // Console.WriteLine("recive , ReiveBufLength " + (ReiveBufLength + 4) + " Count " + msgcount);
    }

    public void Tick()
    {

        robotGame.LogicTick();
    }

    public void JoinRoom(int roomID)
    {
        Console.WriteLine("in robot, the join room id is: " + roomID);
        NetworkMsg msg = new Join(robotID, roomID);
       // clientSocket.sendm(msg);

        SendData(msg);
    }
    public void OnJoinReply(int id, List<string> names)
    {
        if(id != -1)
        {
            robotID = id;
            robotGame.GenUsers(id, names);
            Console.WriteLine("join succ, my id is: " + id);
        }
        else
        {
            robotGame.UpdatePlayerList(names);
        }
    }
    public void Start(bool flag)
    {
        if(flag)
        {
            Console.WriteLine("Robot id: " + robotID + "start! ");
            robotGame.Start(true);
        }
    }

    public void OnGetFrames(List<DS_protocol.p_IdToFrames> areas_to_frames)
    {
        robotGame.OnGetFrames(areas_to_frames);
    }
    public void OnGetFrames( byte[] buffers)
    {
        robotGame.OnGetFrames(buffers);
    }
    public void OnInitMsg(int areaID)
    {
        robotGame.OnInitMsg(areaID);
    }
}