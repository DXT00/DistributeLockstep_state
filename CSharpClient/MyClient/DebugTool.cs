using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class DebugTool
{
    static System.Diagnostics.Stopwatch sw1;
    static System.Diagnostics.Stopwatch sw2;
    static System.Diagnostics.Stopwatch sw3;
    static System.TimeSpan timeSpan;

    static public double totalTime = 0;
    static public double totalTime2 = 0;
    static public int tickCount = 0;
    static public int tickCount2 = 0;
    //static public int tickCount3 = 0;
    public static void TimeWactherStart(int id )
    {
        switch (id)
        {
            case 1:
                sw1 = new System.Diagnostics.Stopwatch();
                sw1.Start();
                tickCount++;
                break;
            case 2:
                sw2 = new System.Diagnostics.Stopwatch();
                sw2.Start();
                tickCount2++;

                break;
            case 3:
                sw3 = new System.Diagnostics.Stopwatch();
                sw3.Start();
                tickCount++;

                break;
        }
    }

    

    public static void TimeWactherEnd(int id  , string msg = " ")
    {
        double ms;
        switch (id)
        {
            case 1:
                sw1.Stop();
                timeSpan = sw1.Elapsed;
                ms = timeSpan.TotalMilliseconds;
                totalTime += ms;
                WriteMessage(msg + " time watch, it uese " + ms.ToString() + "ms",id);
                break;
            case 2:
                sw2.Stop();
                timeSpan = sw2.Elapsed;
                ms = timeSpan.TotalMilliseconds;
                totalTime2 += ms;
                WriteMessage(msg + " time watch, it uese " + ms.ToString() + "ms",id);
                break;
            case 3:
                sw3.Stop();
                timeSpan = sw3.Elapsed;
                ms = timeSpan.TotalMilliseconds;
                totalTime += ms;
                WriteMessage(msg + " time watch, it uese " + ms.ToString() + "ms",id);
                break;
        }
    }

    public static void PrintAvgTime(int TotalFrameCount, int id,string msg = " ")
    {
        double avgTime = (double)totalTime / TotalFrameCount;
        WriteMessage(msg + " time watch, it uese avg" + avgTime.ToString() + "ms",id);
    }

    public static void PrintAvgWhileTime( int id, string msg = " ")
    {

        double avgTime = (double)totalTime /tickCount ;
        //double FPS = 1000.0/avgTime
        WriteMessage(msg + " time watch, it uese avg" + avgTime.ToString() + "ms", id);
        //WriteMessage(msg + " FPS" + FPS.ToString() + "ms", id);

    }
    public static void PrintFPS(int TickCnt, string msg = " ")
    {

        double avgTime = (double)totalTime2 / (149*TickCnt);
        double FPS = 1000.0 / avgTime;
        WriteMessage(msg + " FPS = " + FPS.ToString() , 2);
        //WriteMessage(msg + " FPS" + FPS.ToString() + "ms", id);

    }
    public static void WriteMessage(string msg,int id = 1)
    {
      
        if (id == 1)
        {
            using (FileStream fs = new FileStream(@"C:\Users\Devlinzhou\Desktop\testfile\LogicTick.txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.BaseStream.Seek(0, SeekOrigin.End);
                    sw.WriteLine("{0}\n", msg, DateTime.Now);
                    sw.Flush();
                }
            }

        }
        else if(id == 2)
        {
            using (FileStream fs = new FileStream(@"C:\Users\Devlinzhou\Desktop\testfile\FPS.txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.BaseStream.Seek(0, SeekOrigin.End);
                    sw.WriteLine("{0}\n", msg, DateTime.Now);
                    sw.Flush();
                }
            }
        }
       
    }

    public static void PrintBytesInfo(byte[] bytes, string type, int clientID)
    {
        WriteMessage("in clientID: " + clientID + "the type of bytes is:" + type + "bytes size: " + bytes.Length);
    }
    public static void PrintLog()
    {

    }
    public static void PrintSendMsgType(NetworkMsg msg,int clientID){

        int type = msg.type;
        switch (type)
        {
            case 0:
                Console.WriteLine("Server sending Msg type:NULL " + clientID);
                break;
            case 1:
                Console.WriteLine("Server sending Msg type:SPAWNINFO " + clientID);
                break;
            case 2:
                Console.WriteLine("Server sending Msg type:GETROOMS " + clientID);
                break;
            case 3:
                Console.WriteLine("Server sending Msg type:JOIN " + clientID);
                break;
            case 4:
                Console.WriteLine("Server sending Msg type:ASKFRAME" + clientID);
                break;
            case 5:
                Console.WriteLine("Server sending Msg type:ASKCHASEFRAME " + clientID);
                break;
            case 6:
                Console.WriteLine("Server sending Msg type:START " + clientID);
                break;
            case 7:
                Console.WriteLine("Server sending Msg type:FRAME " + clientID);
                break;
            case 8:
                Console.WriteLine("Server sending Msg type:GETFRAME " + clientID);
                break;
            case 9:
                Console.WriteLine("Server sending Msg type:ENTERAREA" + clientID);
                break;
            case 10:
                Console.WriteLine("Server sending Msg type: LEAVEAREA " + clientID);
                break;
            case 11:
                Console.WriteLine("Server sending Msg type:SYNC_FRAME " + clientID);
                break;
            case 12:
                Console.WriteLine("Server sending Msg type:REPLYGETROOMS" + clientID);
                break;
            case 13:
                Console.WriteLine("Server sending Msg type:REPLYJOIN " + clientID);
                break;
            case 14:
                Console.WriteLine("Server sending Msg type:REPLYASKFRAME " + clientID);
                break;
            case 15:
                Console.WriteLine("Server sending Msg type:REPLYASKCHASEFRAME " + clientID);
                break;
            case 16:
                Console.WriteLine("Server sending Msg type:REPLYSTART " + clientID);
                break;
            case 17:
                Console.WriteLine("Server sending Msg type:TICK " + clientID);
                break;
            case 18:
                Console.WriteLine("Server sending Msg type:END " + clientID);
                break;
            case 19:
                Console.WriteLine("Server sending Msg type:HEART " + clientID);
                break;

        }
            
               
    	
    }
}
