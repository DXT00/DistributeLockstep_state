using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

public class ClientSocket
{
    public Socket socket;
    int portNum = NetCommon.PortNum;
    IPEndPoint endPoint;

    public ClientSocket( string tip )
    {
        IPAddress ip = IPAddress.Parse(tip);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        endPoint = new IPEndPoint(ip, portNum);
    }
    public void Connet()
    {
        try
        {
            socket.Connect(endPoint);
            Console.WriteLine("Connet " + ":" + portNum);
        }
        catch(Exception ee)
        {

        }
    }

    public void SendData( byte[] buffer, int nLength)
    {
        int nResult = socket.Send(buffer, nLength, SocketFlags.None);
        if (nResult == 0)
        {
            Console.WriteLine("Send zero Date! ");
        }
        else
        {
             //Console.WriteLine("Send  Date " + nResult);
        }
    }

    public int ReceiveData( byte[] s_net_buff)
    {
        Socket m_Socket = socket;

        if(socket.Available > 0 )
        {

            int ret = m_Socket.Receive(s_net_buff, sizeof(uint), SocketFlags.None);

            if (ret == 4)
            {
                int response_len = BitConverter.ToInt32(s_net_buff, 0);

                //Console.Write("response_len " + response_len + "\n");

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


                   // Console.Write("ReceiveData " + response_len + "\n");

                    return (int)response_len;
                }
            }

        }


        return 0;
    }


}