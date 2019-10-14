using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;


public class ServerSocket
{
    public Socket socket;
    public int portNum = NetCommon.PortNum;
    public string IP = NetCommon.IP;
    public Client client;

    object listenLock = new object();

    public Dictionary<int, Client> clients;

    public ServerSocket()
    {
        clients = new Dictionary<int, Client>();
        IPAddress ip = IPAddress.Any;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(ip, portNum));
    }

    public void StartListen()
    {
        try
        {
            Console.WriteLine("Listening started port:{portNum} protocol type: {ProtocolType.Tcp}");
            socket.Listen(128);//排队等待的数量
            socket.BeginAccept(AcceptCallback, socket);//when a client connects we can accept them by AcceptCallback
        }
        catch(Exception ex)
        {
            throw new Exception("listerning error" + ex);
        }
    }
   void AcceptCallback(IAsyncResult ar)
    {
        lock (clients)
        {
            try
            {
                Socket acceptSocket = socket.EndAccept(ar);
                socket.BeginAccept(AcceptCallback, socket);

                IPAddress clientIP = (acceptSocket.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (acceptSocket.RemoteEndPoint as IPEndPoint).Port;

                client = new Client();
                client.acceptSocket = acceptSocket;
                client.connectionID = clients.Count;
                client.clientIP = clientIP;
                client.clientPort = clientPort;

                UnityEngine.Debug.Log("client Id: " + clients.Count + "ip " + clientIP + " port " + clientPort + "connected");

                Console.WriteLine("client Id: " + clients.Count + "connected");

                clients.Add(clients.Count, client);
            }
            catch (Exception ex)
            {
                throw new Exception("Bace Accept error" + ex);
            }
        }

    }


//     public byte[] ReceiveData(Socket socket)
//     {
//         try
//         {
//             byte[] bytes = null;
//             int len = socket.Available;
//             if (len > 0)
//             {
//                 bytes = new byte[len];
//                 int receiveNumber = socket.Receive(bytes);
// 
//                // UnityEngine.Debug.Log("receive Number" + receiveNumber);
//             }
//             return bytes;
//         }
//         catch (Exception ex)
//         {
//             throw new Exception("msg sending error" + ex);
//         }
//     }

    public int ReceiveData(Socket m_Socket, byte[] s_net_buff)
    {
        try
        {
//             int len = m_Socket.Available;
//             if (len > 0)
//             {
//                 int receiveNumber = m_Socket.Receive(s_net_buff);
//                 
// 
//                // UnityEngine.Debug.Log("receive Number" + receiveNumber);
//                 return receiveNumber;
//             }

            if (m_Socket.Available > 0)
            {
                int ret = m_Socket.Receive(s_net_buff, sizeof(uint), SocketFlags.None);

                if (ret == 4)
                {
                    int response_len = BitConverter.ToInt32(s_net_buff, 0);

                    if (response_len > 0 || response_len <= s_net_buff.Length)
                    {
                        int nLeftSize = (int)response_len;
                        int nTotalSize = 0;

                        while (true)
                        {
                            int nCurrent = m_Socket.Receive(s_net_buff, nTotalSize, nLeftSize, SocketFlags.None);
                            nLeftSize -= nCurrent;
                            nTotalSize += nCurrent;
                            if (nCurrent == 0)
                            {
                                break;
                            }

                            if (nTotalSize == (int)response_len)
                            {
                                break;
                            }
                        }



                        //                     if (nTotalSize != (int)response_len)
                        //                     {
                        //                         CloseSocket();
                        //                         ConnectError("nTotalSize != (int)response_len : " + nTotalSize + " response_len " + response_len);
                        //                         return 0;
                        //                     }
                        //                     else
                        //                     {
                        //                         s_nReceiveSize = (int)response_len;
                        //                     }
                        //UnityEngine.Debug.Log("response_len" + response_len);
                        return (int)response_len;
                    }
                }
            }



            return 0;
        }
        catch (Exception ex)
        {
            throw new Exception("msg sending error" + ex);
        }
    }
}
