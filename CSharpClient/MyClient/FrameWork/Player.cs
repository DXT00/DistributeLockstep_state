using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player
{
    string name;
    float health_point;
    int id;
    int area_id;
    float axisX2 = 0;
    float axisY2 = 0;
    private float rot_speed = 3.0f;
    private float move_speed = ParameterCommon.moveSpeed;


    Vector3 move_direction;
    public Vector3 position;
    Vector3 delta_distance;
    RobotGame robotGame;

    LogicGameManager game_manager;

    public Player(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
    public Player()
    {
        move_direction = new Vector3(0, 0, 0);
    }
    public void Tick()
    {
        //this.delta_distance = move_direction * move_speed * Time.deltaTime;
        this.delta_distance = move_direction * move_speed * 0.1f;
        //Console.WriteLine("player: " + id + "tick! it's position is:" + position);

        Vector3 NewPos = position + delta_distance;

        bool bRecaclue = false;

        float fsize = robotGame.areaManager.MapSize / 2 - 1;

        if (NewPos.x < -fsize || fsize < NewPos.x)
        {
            move_direction.x = -move_direction.x;
            bRecaclue = true;
        }

        if (NewPos.z < -fsize || fsize < NewPos.z)
        {
            move_direction.z = -move_direction.z;
            bRecaclue = true;
        }

        if (bRecaclue )
        {
            this.position += move_direction * move_speed * 0.20f;
        }
        else
        {
            this.position = NewPos;
            this.position.y = 0;
        }

        
    }

    public void init(LogicGameManager manager, int id, Vector3 spawnPos)
    {
        this.id = id;
        this.game_manager = manager;
        //this.trans.localPosition = spawnPos;
        this.position = spawnPos;
        if(id == robotGame.player_id)
        {
            robotGame.BindPlayer(this);
        }
        //obj = Instantiate(player_prefab, spawnPos,Quaternion.identity);
        // todo: figure out the area id by spawnPos
        //area_manager.get_id_by_pos(this.trans.localPosition)

    }

    public void bindRobotGame(RobotGame robotGame)
    {
        this.robotGame = robotGame;
    }
    public void set_area_id(int area_id)
    {
        this.area_id = area_id;
    }


    public int get_id()
    {
        return this.id;
    }
    public string get_name()
    {
        return this.name;
    }
    public void add_delta_rotation(Vector2 delta)
    {
        axisX2 = axisX2 + delta.x;
        axisY2 = axisY2 + delta.y;
        //this.rotation = Quaternion.Euler(-axisY2 * rot_speed, axisX2 * rot_speed, 0);
    }
    public void move(Vector3 direction)
    {
        this.move_direction = direction;
        this.delta_distance = move_direction * move_speed * Time.deltaTime;
        this.position += delta_distance;
    }

    public void set_fire_direction(Vector3 direction)
    {

    }
    public void set_move_direction(Vector3 direction)
    {
        this.move_direction = direction;
    }

    public Vector3 get_move_direction()
    {
        return move_direction;
    }
    public void set_health(float hp)
    {
        this.health_point = hp;
    }

    public Vector3 get_position()
    {
        return this.position;
    }

    public float get_move_speed()
    {
        return this.move_speed;
    }
    public float get_rot_speed()
    {
        return this.rot_speed;
    }
    public Vector3 get_delta_dis()
    {
        return this.delta_distance;
    }
    public Vector2 get_axis()
    {
        return new Vector2(axisX2, axisY2);
    }
    public Vector3 get_direction()
    {
        return this.move_direction;
    }
    public void set_position(Vector3 pos)
    {
        this.position = pos;
    }
    public int get_area_id()
    {
        return this.area_id;
    }
    public void destroy()
    {
        // call view player to destroy
        //由logicMgr invoke, 考虑使用析构函数取消instance，然后在gameMgr重新绑定new player
    }

    public object print_info()
    {
        return "player info id: " + this.id + " area_id: " + this.area_id + " name: " + this.name;
    }
}

