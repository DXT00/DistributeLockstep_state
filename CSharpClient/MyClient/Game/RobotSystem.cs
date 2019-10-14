using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

//main idea, 单线程,时间分割(收-逻辑处理-发)，sendMsg打包
public class RobotSystem
{
    Robot[] robots;

    public bool bConnect = false;
    public RobotSystem()
    {

    }
    public int robotsNumber()
    {
        return robots.Length;
    }
    public void GerateRobots(int num, string ip, int aren,int size)
    {
        robots = new Robot[num];
        for(int i =0; i<num; ++i)
        {
            robots[i] = new Robot(i, ip, aren,size,false);
        }
    }
    public void Connect()
    {
        foreach (Robot robot in robots)
        {
            robot.Connect();
            Console.WriteLine("Robot connect successfully");
        }

        bConnect = true;
    }
    public void CheckGame()
    {
        if( bConnect )
        {
            CheckReceiveData();//所有robot接收数据
            CheckTick();
            CheckSendData();
        }
    }
    public void CheckReceiveData() //获取socket信息，逐一处理，把处理结果放入SendBuffer
    {
        foreach(Robot robot in robots)
        {
            robot.ReadData();
        }
    }

    public void CheckTick() // 逐一发送SendBuffer
    {
        foreach (Robot robot in robots)
        {
            robot.Tick();
        }
    }

    public void CheckSendData() // 逐一发送SendBuffer
    {
        foreach (Robot robot in robots)
        {
            robot.SendGameData();
        }
    }

    public void JoinRoom(int roomID)
    {
        foreach (Robot robot in robots)
        {
            robot.JoinRoom(roomID);
        }
    }

    public void Start()
    {
        NetworkMsg msg = new StartGame(0);
        robots[0].SendData(msg);
    }
}