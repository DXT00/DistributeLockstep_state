using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;



public class NetworkMsg //: BaseProtocol
{

    public int player_id;
    public int type;

}



public class Join : NetworkMsg
{
    //  public CmdType type;
    public int room_id;
    public Join(int player_id, int room_id)
    {
        this.player_id = player_id;
        this.room_id = room_id;
        this.type = (int)CmdType.JOIN;
    }
}
public class AskFrame : NetworkMsg
{
    // public CmdType type;
    public List<int> areas;
    public List<int> frames;

    public AskFrame(int player_id, List<int> area, List<int> frame )
    {
        this.type = (int)CmdType.ASKFRAME;
        this.player_id = player_id;
        this.areas = area;
        this.frames = frame;

    }
}


public class StartGame : NetworkMsg
{
    //public CmdType type;
    public StartGame(int player_id)
    {
        this.player_id = player_id;
        this.type = (int)CmdType.START;

    }
}

public class Frame : NetworkMsg
{
    // public CmdType type;
    public SyncFrame syncFrame;
    public Frame(int player_id, SyncFrame syncFrame)
    {
        this.player_id = player_id;
        this.syncFrame = syncFrame;
        this.type = (int)CmdType.FRAME;
    }
}










