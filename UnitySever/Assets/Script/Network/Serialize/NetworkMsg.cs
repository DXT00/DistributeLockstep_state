using System.Collections;
using System.Collections.Generic;
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
    public Dictionary<int, int> area_to_frame;
    public AskFrame(int player_id, Dictionary<int, int> area_to_frame)
    {
        this.type = (int)CmdType.ASKFRAME;
        this.player_id = player_id;
        this.area_to_frame = area_to_frame;
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









