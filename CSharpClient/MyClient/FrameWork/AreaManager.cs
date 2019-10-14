using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/*
 * ara:(index:x,index:y)
 * 00	10	20	30	
 * 01	11	21	31
 * 02	12	22	32
 * 03	13	23	33
 * 
 * 0	1	2	3
 * 4	5	6	7
 * 8	9	10	11
 * 12	13	14	15
 * 
 * are_id = areas_count * y + x
 * area_index = area_id (in current design)
 * 
 * center_area_id = (areas_count ^ 2 - 1 )/ 2 should be at pos(0, half_single_size, 0)
*/

public class AreaManager
{

    public List<Area> areas; // all areas
    public List<Area> nearby_areas; // current active areas
    List<Area> lagged_areas; // lagged areas
                             // need some syncFrame info

    public float MapSize = 0;

    int areas_count;
    int total_area_number;
    int single_area_size;
    int single_area_half_size;
    int center_area_index;



    RobotGame robotGame; 

    public AreaManager(int areas_count, int single_size)
    {
        this.areas_count = areas_count;
        this.single_area_size = single_size;
        this.single_area_half_size = single_size / 2;
        this.total_area_number = areas_count * areas_count;
        this.center_area_index = (areas_count * areas_count - 1) / 2;

        MapSize = (float)(areas_count * single_size);

        areas = new List<Area>();
        nearby_areas = new List<Area>();
        lagged_areas = new List<Area>();

        for (int posY = 0; posY < areas_count; posY++)
        {
            for (int posX = 0; posX < areas_count; posX++)
            {
                Area a = new Area(posX, posY);
                int id = id_map(posX, posY);
                a.init(id, index_to_pos(id));
                LogicGameManager logicMgr = new LogicGameManager(id, a);
                a.regist_logic_mgr(logicMgr);
                areas.Add(a);
            }
        }
        //find_nearby_areas(0);
    }
    public void Init(RobotGame robotGame)
    {
        this.robotGame = robotGame;
        foreach (Area area in areas)
        {
            area.logic_mgr.bindRobotGame(robotGame);
        }
    }
    //client fc is 1 bigger than server

    public void chase_up()
    {


    }

    private int id_map(int x, int y)
    {
        return areas_count * y + x;
    }
    public int get_area_count()
    {
        return this.areas_count;
    }
    public int get_total_count()
    {
        return this.total_area_number;
    }
    public int get_single_size()
    {
        return this.single_area_size;
    }

    public Vector3 index_to_pos(int index)
    {
        //1. get the pos of index: 0
        int row = this.center_area_index / areas_count;
        int x = -row * single_area_size;
        int y = row * single_area_size;
        int index_y = y - (index / areas_count) * single_area_size;
        int index_x = x + (index % areas_count) * single_area_size;
        Vector3 T =  new Vector3(index_x, 0, index_y);





        return T;
    }

    public int get_id_by_pos(Vector3 pos)
    {
        float x = pos.x;
        float y = pos.z;

        int row = this.center_area_index / areas_count;
        int zero_x = -row * single_area_size - single_area_half_size;
        int zero_y = row * single_area_size + single_area_half_size;

        float diff_x = x - zero_x;
        float diff_y = y - zero_y;

        int model_x = Mathf.Abs((int)diff_x / single_area_size);
        int model_y = Mathf.Abs((int)diff_y / single_area_size);

        int index = model_y * areas_count + model_x;



        return index;
    }

//     bool is_area_nearby(Area a)
//     {
//         if (a == get_current_area())
//             return true;
//         else
//             // not finished
//             return false;
//     }

    bool is_area_upto_date(Area a)
    {
        if( a.TotallFrames.Count == 0 )
        {
            return true;
        }
        else
        {

            return a.TotallFrames.Count == a.CurrentFrame;
        }

    }

    public Area get_area_by_id(int id)
    {
        foreach (Area a in areas)
        {
            if (a.get_area_id() == id)
                return a;
        }
        return null;
    }

    public Area safe_get_area_at(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < areas_count && y < areas_count)
        {
            return areas[id_map(x, y)];
        }
        else return null;

    }

    public void find_nearby_areas(List<Area> Result, int Tcurrent_area_index)
    {
        //1. find current area
        int index_y = Tcurrent_area_index / areas_count;
        int index_x = Tcurrent_area_index - index_y * areas_count;
        //2. decide the nearby area
        Result.Clear();
        Result.Add(get_areas()[Tcurrent_area_index]);

        // left
        Area temp = safe_get_area_at(index_x - 1, index_y);
        if (temp != null)
            Result.Add(temp);
        // right
        temp = safe_get_area_at(index_x + 1, index_y);
        if (temp != null)
            Result.Add(temp);
        // top
        temp = safe_get_area_at(index_x, index_y + 1);
        if (temp != null)
            Result.Add(temp);
        // bottom
        temp = safe_get_area_at(index_x, index_y - 1);
        if (temp != null)
            Result.Add(temp);
        // top right
        temp = safe_get_area_at(index_x + 1, index_y + 1);
        if (temp != null)
            Result.Add(temp);
        // top left
        temp = safe_get_area_at(index_x - 1, index_y + 1);
        if (temp != null)
            Result.Add(temp);
        // bottom right
        temp = safe_get_area_at(index_x + 1, index_y - 1);
        if (temp != null)
            Result.Add(temp);
        // bottom left
        temp = safe_get_area_at(index_x - 1, index_y - 1);
        if (temp != null)
            Result.Add(temp);

        //3. check the lagged area
       // lagged_areas.Clear();
//         foreach (Area a in nearby_areas)
//         {
//             if (a.timer.get_frame_count() < target_frame_count)
//             {
//                 lagged_areas.Add(a);
//             }
//         }
    }
    public void received_sync_frame( int areaid, SyncFrame syncFrame)
    {
        Area area = get_area_by_id(areaid);
        area.record_frame(syncFrame);
    }

    public int get_center_area_id()
    {
        return this.center_area_index;
    }
    public List<Area> get_areas()
    {
        return this.areas;
    }
    public List<Area> get_nearby_areas()
    {
        return this.nearby_areas;
    }
    public bool area_exist(Area target, List<Area> list)
    {
        foreach (Area area in list)
        {
            if (target.get_area_id() == area.get_area_id())
            {
                return true;
            }
        }
        return false;
    }

}
