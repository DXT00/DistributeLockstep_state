using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Area
{
    public LogicGameManager logic_mgr;
    public List<SyncFrame> TotallFrames;//隔几帧存一下
    public int CurrentFrame;//该区域的最大帧数

    int area_id;
    int index_x;
    int index_y;
    Vector3 center;

    public Area(int x, int y)
    {
        this.index_x = x;
        this.index_y = y;
        TotallFrames = new List<SyncFrame>();
        CurrentFrame = 0;
    }

    public void init(int id, Vector3 center)
    {
        this.area_id = id;
        this.center = center;
    }
    public void regist_logic_mgr(LogicGameManager logic_mgr)
    {
        this.logic_mgr = logic_mgr;
    }

    // called by every frame if this area is active
    public void Tick()
    {
        //for ( ; CurrentFrame < TotallFrames.Count; ++CurrentFrame)
        //{
        //    logic_mgr.excute_frame(TotallFrames[CurrentFrame]);
        //    logic_mgr.Tick();
        //}
        Console.Write("TotallFrames.Count " + TotallFrames.Count+"\n");

        foreach (SyncFrame syncframe in TotallFrames)
        {
            logic_mgr.excute_frame(syncframe); // 这里只拿到位置信息
            logic_mgr.Tick(); //计算新的位置
        }
        //执行完就清空
        TotallFrames.Clear();
    }
    public void record_frame(SyncFrame syncFrame)
    {

        if (area_id == syncFrame.area_id )
        {
            if (TotallFrames.Count == 0)
            {
                TotallFrames.Add(syncFrame);
                CurrentFrame = syncFrame.frame_index;
            }
            else
            {
                if( syncFrame.frame_index >= TotallFrames[TotallFrames.Count-1].frame_index )
                {
                    TotallFrames.Add(syncFrame);
                    CurrentFrame = syncFrame.frame_index;

                }
                else
                {
                    Console.Write("record_frame " + syncFrame.frame_index + " CurrentMaxFrame " + CurrentFrame + "\n");
                }
            }
        }
        else
        {
            Console.Write("area_uid != syncFrame.area_id  area_uid " + area_id + " syncFrame.area_id " + syncFrame.area_id + "\n");
        }

        Console.Write("area_id " + area_id + " record_frame " + syncFrame.frame_index + " CurrentMaxFrame " + CurrentFrame + "\n");


    }

    //public SyncFrame get_frame(int frameindex) //没用到
    //{
    //    if(0 <= frameindex && frameindex < TotallFrames.Count )
    //    {
    //        return TotallFrames[frameindex];
    //    }

    //    return null;
    //}

    //public bool has_frame(int frameindex)//没用到
    //{
    //    if (0 <= frameindex && frameindex < TotallFrames.Count)
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    public int get_area_id()
    {
        return area_id;
    }
    public int get_area_x()
    {
        return index_x;
    }
    public int get_area_y()
    {
        return index_y;
    }
    public Vector3 get_center()
    {
        return this.center;
    }

    public int get_frame_count()
    {
        
      return CurrentFrame;
    }
    public LogicGameManager get_logic_mgr()
    {
        return this.logic_mgr;
    }
}
