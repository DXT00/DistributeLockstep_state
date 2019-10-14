using System.Collections;
using System.Collections.Generic;

public class SyncFrame
{
    // 帧号的基本信息
    public int frame_index;
    public int area_id;
    public List<CustomSyncMsg> msg_list;

    //帧信息初始化
    public SyncFrame(int frame_count, int area_id)
    {
        this.frame_index = frame_count;
        this.area_id = area_id;
        msg_list = new List<CustomSyncMsg>();
    }

    //帧内transaction记录
    // 这应该是一个统一msg的类list变量，Area通过enum一些cmd handler去做相应操作
    public void dump_actions(CustomSyncMsg msg)
    {
        msg_list.Add(msg);
    }
    public List<CustomSyncMsg> get_msg()
    {
        return this.msg_list;
    }

    public void conbine_msg(List<CustomSyncMsg> msgs)
    {
        if (msgs != null)
        {
            foreach (CustomSyncMsg msg in msgs)
            {
                this.msg_list.Add(msg);
            }
        }
    }

    public int get_area_id()
    {
        return this.area_id;
    }
    public int get_frame_count()
    {
        return this.frame_index;
    }

    public void printFrameInfo()
    {
        foreach (CustomSyncMsg msg in this.msg_list)
        {
            msg.printInfo();
        }
    }
}
