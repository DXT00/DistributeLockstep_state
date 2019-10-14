using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;



public class ReplyJoin : NetworkMsg
{
    public int real_player_id;
    public List<string> player_names;
    public ReplyJoin(int id, List<string> player_names)
    {
        real_player_id = id;
        this.player_names = player_names;
        this.type = (int)CmdType.REPLYJOIN;
    }
}

public class ReplyAskFrame : NetworkMsg
{
    public List<int> areas;
    public List<List<SyncFrame>> frames;

    public ReplyAskFrame(List<int> Tareas, List<List<SyncFrame>> Tframes)
    {
        areas = Tareas;
        frames = Tframes;
        this.type = (int)CmdType.REPLYASKFRAME;
    }
}


public class ReplyStart : NetworkMsg
{
    public bool start;
    public ReplyStart(bool start)
    {
        this.start = start;
        this.type = (int)CmdType.REPLYSTART;
    }
}
