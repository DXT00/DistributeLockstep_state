using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class LogicGameManager
{

    public List<Player> player_list;
    RobotGame game;

    int area_id;

    Area mArea;

    public LogicGameManager(int area_id, Area a)
    {
        player_list = new List<Player>();
        mArea = a;
        //this.mgr_id = area.get_area_id();
        this.area_id = area_id;
    }
    public void Tick()
    {
        foreach (Player player in player_list)
        {
            player.Tick();
        }
    }
    public void init(int area_size, Vector3 area_center)
    {

    }
    public void bindRobotGame(RobotGame robotGame)
    {
        this.game = robotGame;
    }
    public void RemovePlayer(int id)
    {
        for( int n = player_list.Count-1; n >=0; n--)
        {
            if(player_list[n].get_id() == id )
            {
                player_list.RemoveAt(n);
            }
        }
    }

    public Player get_player(int id)
    {
        foreach (Player p in player_list)
        {
            if (p.get_id() == id)
            {
                return p;
            }
        }
        //Debug.Log("logic_mgr: " + mgr_id + " not find target player: " + id);
        return null;
    }

    public int get_player_count()
    {
        return player_list.Count;
    }

    public Player spawn_player(int id, Vector3 spawn_pos)
    {
        Player player = new Player();
        player.bindRobotGame(game);
        player.init(this, id, spawn_pos);
        //player.set_area_id(this.area.get_area_id());
        player.set_area_id(area_id);
        player_list.Add(player);
        return player;
    }

    // add existed player
    public void spawn_player(Player player)
    {
        //player.set_area_id(this.area.get_area_id());
        player.set_area_id(area_id);
        player_list.Add(player);
    }

    public void on_plyaer_rotate(int id, Vector2 delta)
    {
        Player p = get_player(id);
        if (p != null)
        {
            p.add_delta_rotation(delta);
        }
    }

    // this function is not used
    public void on_player_shoot(int id, Vector3 direction)
    {
        Player p = get_player(id);
        if (p != null)
        {
            p.set_fire_direction(direction);
        }
    }

    public void on_receive_player_input(int id, Vector3 direction)
    {
        Player p = get_player(id);
        if (p != null)
        {
            p.set_move_direction(direction);
        }
        else
        {
            Console.WriteLine(" on_receive_player_input " + id + " Player is not exsit " );
        }
    }
    public void on_player_position(int id,Vector2 delta)
    {
        Player p = get_player(id);
        if (p != null)
        {
            p.set_position(new Vector3(delta.x, 0, delta.y));
        }
        else
        {
            Console.WriteLine(" on_player_position " + id + " Player is not exsit ");

        }
    }
    public void on_player_enter_area(int id, int health, Vector2 rot, Vector3 dir, Vector3 pos)
    {
        Player p = get_player(id);
        if( p != null )
        {
            Console.WriteLine(" on_player_enter_area " + id + " has in this area " + area_id);
        }
        else
        {
            spawn_player(id, pos); //this function ensure the new player is the last one in list
            Player p2 = player_list[player_list.Count - 1];
            p2.add_delta_rotation(rot);
            p2.set_move_direction(dir);
            p2.set_health(health);
        }
    }

    public void on_player_leave_area(int playerid )
    {
        Player p = get_player(playerid);
        if (p == null)
        {
            Console.WriteLine(" on_player_leave_area " + playerid + "  not this area " + area_id);
        }

        RemovePlayer(playerid);
    }

    public void on_player_dead(int id)
    {
        Player p = get_player(id);
        player_list.Remove(p);
    }

    public void handle_msg(CustomSyncMsg msg)
    {
        if (msg.msg_type == (int)RequestType.SPAWN)
        {
           // Console.WriteLine(" SPAWN_msg " + msg.msg_type);
            SpawnMessage s_msg = msg as SpawnMessage;
            spawn_player(msg.player_id, s_msg.position);
        }
        else if (msg.msg_type == (int)RequestType.INPUT)
        {
           // Console.WriteLine(" INPUT_msg " + msg.msg_type);
            InputMessage i_msg = msg as InputMessage;
           // Console.WriteLine( "client " + game.player_id + " player_id: " + msg.player_id  +  " area_id: " + mArea.get_area_id() + "handle input: player_id:" + msg.player_id + "move direction" + i_msg.moving);
            on_receive_player_input(msg.player_id, i_msg.moving);
        }
        else if (msg.msg_type == (int)RequestType.ROTATE)
        {
            RotateMessage r_msg = msg as RotateMessage;
            on_plyaer_rotate(msg.player_id, r_msg.delta);
        }
        else if (msg.msg_type == (int)RequestType.ENTERAREA)
        {
            //Console.WriteLine("msg.player_id " + msg.player_id +" ENTER_AREA_msg " + mgr_id);
            EnterAreaMessage e_msg = msg as EnterAreaMessage;
            on_player_enter_area(msg.player_id,
                e_msg.health,
                e_msg.rotation,
                e_msg.direction,
                e_msg.position);
        }
        else if (msg.msg_type == (int)RequestType.LEAVEAREA)
        {
           // Console.WriteLine("msg.player_id " + msg.player_id + " LEAVE_AREA_msg " + mgr_id);
            on_player_leave_area( msg.player_id);
        }
        else if(msg.msg_type == (int)RequestType.POSITION)
        {
            PositionMessage p_msg = msg as PositionMessage;
            on_player_position(msg.player_id, p_msg.delta);
        }
    }
    public void excute_frame(SyncFrame syncFrame)
    {
        if (syncFrame == null)
        {
            return;
        }
        List<CustomSyncMsg> msg_list = syncFrame.get_msg();
        if (msg_list == null || msg_list.Count == 0)
        {
            return;
        }
        else
        {
            foreach (CustomSyncMsg msg in msg_list)
            {
                handle_msg(msg);
            }
        }
    }
    public void print_player_info()
    {
        foreach (Player player in player_list)
        {
            Debug.Log("logic mgr id:" + area_id + " center: " + player.print_info());
        }
    }

    public List<Player> GetPlayerList()
    {
        return this.player_list;
    }
}
