using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ProtoBuf;
using UnityEngine;


public class CustomSyncMsg
{
    public int player_id { get; set; }
    public int msg_type { get; set; }
    public virtual void printInfo() { }
}

public class RotateMessage : CustomSyncMsg
{
    public int id;
    public Vector2 delta;
    public RotateMessage(int player_id, Vector2 delta)
    {
        this.delta = delta;
        this.player_id = player_id;
        msg_type = (int)RequestType.ROTATE;
    }

    public float delta_x { get { return delta.x; } set { delta.x = value; } }

    public float delta_y { get { return delta.y; } set { delta.y = value; } }

    public override void printInfo()
    {
        Console.WriteLine("rotate msg:  player id--" + player_id + "delta_x,y --" + delta_x + "  " + delta_y);
    }
}

public class PositionMessage : CustomSyncMsg
{
    public int id;
    public Vector2 delta;
    public PositionMessage(int player_id, Vector2 delta)
    {
        this.delta = delta;
        this.player_id = player_id;
        msg_type = (int)RequestType.POSITION;
    }

    public float delta_x { get { return delta.x; } set { delta.x = value; } }
    public float delta_y { get { return delta.y; } set { delta.y = value; } }

    public override void printInfo()
    {
        Console.WriteLine("rotate msg:  player id--" + player_id + "delta_x,y --" + delta_x + "  " + delta_y);
    }
}

public class SYNMessage : CustomSyncMsg
{
 
    public SYNMessage(int player_id, Dictionary<int, Vector2> TPlayer)
    {
        this.player_id = player_id;
        msg_type = (int)RequestType.SYNPOS;
        Players = TPlayer;
    }

    public Dictionary<int, Vector2> Players;


    public override void printInfo()
    {
        Console.WriteLine("rotate msg:  player id--" + player_id + "delta_x,y --");
    }
}


public class SpawnMessage : CustomSyncMsg
{
    public int id;

    public Vector3 position;
    public SpawnMessage(int player_id, Vector3 pos)
    {
        this.position = pos;
        this.player_id = player_id;
        msg_type = (int)RequestType.SPAWN;
    }

    public float position_x { get { return position.x; } set { position.x = value; } }
    public float position_y { get { return position.y; } set { position.y = value; } }
    public float position_z { get { return position.z; } set { position.z = value; } }

    public override void printInfo()
    {
        Console.WriteLine("spawn msg:  player id--" + player_id + "posx,y,z --" + position_x + "  " + position_y + " " + position_z);
    }
}

public class InputMessage : CustomSyncMsg
{
    public int id;

    public float moving_x { get; set; }
    public float moving_y { get; set; }
    public float moving_z { get; set; }
    public Vector3 moving;
    public InputMessage(int player_id, Vector3 moving)
    {
        this.moving = moving;
        this.player_id = player_id;
        moving_x = moving.x;
        moving_y = moving.y;
        moving_z = moving.z;
        msg_type = (int)RequestType.INPUT;
    }

    public override void printInfo()
    {
        Console.WriteLine("input msg:  player id--" + player_id + "moving x,y,z --" + moving_x + "  " + moving_y + " " + moving_z);
    }
}

public class EnterAreaMessage : CustomSyncMsg
{
    public int id;
    public int health;

    public Vector2 rotation;
    public Vector3 direction;
    public Vector3 position;
    public EnterAreaMessage(int player_id, int health, Vector2 rotation, Vector3 direction, Vector3 position)
    {
        this.player_id = player_id;
        this.health = health;
        this.rotation = rotation;
        this.direction = direction;
        this.position = position;
        msg_type = (int)RequestType.ENTERAREA;
    }

    public float position_x { get { return position.x; } set { position.x = value; } }
    public float position_y { get { return position.y; } set { position.y = value; } }
    public float position_z { get { return position.z; } set { position.z = value; } }

    public float direction_x { get { return direction.x; } set { direction.x = value; } }
    public float direction_y { get { return direction.y; } set { direction.y = value; } }
    public float direction_z { get { return direction.z; } set { direction.z = value; } }

    public float rotation_x { get { return rotation.x; } set { rotation.x = value; } }
    public float rotation_y { get { return rotation.y; } set { rotation.y = value; } }

    public override void printInfo()
    {
        Console.WriteLine("Enter msg:  player id--" + player_id + "posx,y,z --" + position_x + "  " + position_y + " " + direction_z);
    }
}

public class LeaveAreaMessage : CustomSyncMsg
{
    public int id;
    public LeaveAreaMessage(int player_id)
    {
        this.player_id = player_id;
        msg_type = (int)RequestType.LEAVEAREA;
    }

    public override void printInfo()
    {
        Console.WriteLine("LeaveAreaMessage msg:  player id--" + player_id);
    }
}







