using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Area
{
    public LogicGameManager logic_mgr;
    public List<SyncFrame> TotallFrames;
    public int CurrentFrame;

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
        for ( ; CurrentFrame < TotallFrames.Count; ++CurrentFrame)
        {
            logic_mgr.excute_frame(TotallFrames[CurrentFrame]);
            logic_mgr.Tick();
        }
    }
    public void record_frame(SyncFrame syncFrame)
    {
        if(area_id == syncFrame.area_id )
        {
            if (TotallFrames.Count == 0)
            {
                TotallFrames.Add(syncFrame);
            }
            else
            {
                if( syncFrame.frame_index == TotallFrames.Count )
                {
                    TotallFrames.Add(syncFrame);
                }
                else
                {
                    Console.Write("record_frame " + syncFrame.frame_index + " CurrentMaxFrame " + (TotallFrames.Count -1 ) + "\n");
                }
            }
        }
        else
        {
            Console.Write("area_uid != syncFrame.area_id  area_uid " + area_id + " syncFrame.area_id " + syncFrame.area_id + "\n");
        }

    }

    public SyncFrame get_frame(int frameindex)
    {
        if(0 <= frameindex && frameindex < TotallFrames.Count )
        {
            return TotallFrames[frameindex];
        }

        return null;
    }

    public bool has_frame(int frameindex)
    {
        if (0 <= frameindex && frameindex < TotallFrames.Count)
        {
            return true;
        }

        return false;
    }

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
