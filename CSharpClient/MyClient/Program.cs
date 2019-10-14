using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
class Program
{

    static string cmd_info = "connect: connect to the server\n"
    + "init 'palyer num': init with player num of player instance\n"
    + "list rooms: list active rooms\n"
    + "join 'room_id' : join room with room_id\n"
    + "exit/help/connect/close";
    static bool initFlag = false;
    public static RobotSystem robotSystem = new RobotSystem();

    static bool bStart = false;
    static bool bJoin  = false;
    static void Main(string[] args)
    {
        Console.WriteLine("hello  try to use 'help' to get cmds.. " + args.Length.ToString());

        for( int n = 0; n < args.Length; ++n )
        {
            Console.WriteLine("args " + n + " "+args[n]);
        }

        if(args.Length == 6 && (args[0] == "init" || args[0] == "initstart"))
        {
            Console.WriteLine("it's in init ");

            try
            {
                int num = int.Parse(args[1]);
                // NetCommon.IP = args[2];
                ParameterCommon.moveSpeed = (float)int.Parse(args[4]);
                initPlayers(num, args[5], int.Parse(args[2]), int.Parse(args[3]));

                robotSystem.Connect();
                Console.WriteLine("connect successfully ");
                joinRoom(0);

                Console.WriteLine("joinRoom successfully ");


                if (args[0] == "initstart")
                {
                    robotSystem.Start();
                }

                bStart = true;

            }
            catch (Exception e)
            {
                Console.WriteLine("wrong num, please input a right number of players " + e.ToString());
            }

        }
        else
        {
            Console.WriteLine("it's not in init  ");
        }

        while (!bStart)
        {
            string str = Console.ReadLine();

            string[] subStr = str.Split(' ');
            string header = subStr[0];

            if (header == "init" || header == "initstart" )
            {
                Console.WriteLine("it's in init ");
                if (subStr.Length > 1)
                {
                    try
                    {
                        int num = int.Parse(subStr[1]);

                        ParameterCommon.moveSpeed = (float)int.Parse(subStr[4]);

                        initPlayers(num, subStr[5],int.Parse(subStr[2]), int.Parse(subStr[3]));

                        robotSystem.Connect();
                        Console.WriteLine("connect successfully ");
                        joinRoom(0);

                        Console.WriteLine("joinRoom successfully ");


                        if (header == "initstart")
                        {
                            robotSystem.Start();
                        }

                        bStart = true;

                    }
                    catch( Exception e )
                    {
                        Console.WriteLine("wrong num, please input a right number of players " + e.ToString());
                    }
                }
                else
                    Console.WriteLine("please input number! ");
            }
            else if (header == "join")
            {
                Console.WriteLine("it's in join ");
                if (subStr.Length > 1)
                {
                    try
                    {
                        int room_id = int.Parse(subStr[1]);
                        joinRoom(room_id);
                        bJoin = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("join wrong id" + ex.ToString());
                    }
                }
                else
                    Console.WriteLine("please input room_id! ");
            }
            else if (header == "exit")
            {
                Console.WriteLine("it's in exit ");
                System.Environment.Exit(0);
            }
            else if (header == "start")
            {
                Console.WriteLine("it's in start ");
                //NetworkMsg msg = new StartGame(0);
                //NetworkManager.SendData(msg);
                robotSystem.Start();
                bStart = true;
            }

            robotSystem.CheckGame();
        }




        while(true)
        {
            var startTime = System.DateTime.Now;


            int aa = System.Environment.TickCount;
            robotSystem.CheckGame();

            int UseTime = System.Environment.TickCount - aa;
            string tt = (System.DateTime.Now - startTime).TotalMilliseconds.ToString();

            if ( UseTime < 100 )
            {
                Thread.Sleep(100 - UseTime);
            }

          // Console.WriteLine(" step time " + tt + " ms"); //单位毫秒 
           
        }


    }


    static void printCmd()
    {
        Console.WriteLine(cmd_info);
    }

    static void initPlayers(int num, string ip, int aren, int size)
    {
        robotSystem.GerateRobots(num, ip, aren,  size);
        initFlag = true;
    }

    static void joinRoom(int room_id)
    {
        Console.WriteLine("progressid: " + System.Diagnostics.Process.GetCurrentProcess().Id);

      //  Process.GetCurrentProcess().Id;
        robotSystem.JoinRoom(System.Diagnostics.Process.GetCurrentProcess().Id);
    }
}
