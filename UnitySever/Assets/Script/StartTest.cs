using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using System;
public class StartTest : MonoBehaviour {

    static System.Threading.Thread threadConsole;
    int areasCount = 3;
    int singleAreaSize = 10;


    GameServer gameServer = null;

    FileStream Ts = null;
	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 10;


        string[] arguments = Environment.GetCommandLineArgs();

        if (arguments.Length == 3)
        {
            UnityEngine.Debug.Log(arguments[0]);

            areasCount = int.Parse(arguments[1]);
            singleAreaSize = int.Parse(arguments[2]);
        }



        gameServer = new GameServer(areasCount, singleAreaSize);
        NetCommon.gameServer = gameServer;
        gameServer.Init();


      	string path="outName_" +System.DateTime.Now.Ticks.ToString() + ".txt" ;

        if(File.Exists(path))
        {
            File.Delete(path);
        }



        Ts = new FileStream(path, FileMode.CreateNew);

	}
    Queue<string> TS = new Queue<string>();
    string TCuurent;
    public Material lineMaterial;
	// Update is called once per frame
    int a = 0;
	void Update ()
    {
        var startTime = System.DateTime.Now;
        int aa = System.Environment.TickCount;
        gameServer.Update();
        float tt = (float)(System.DateTime.Now - startTime).TotalMilliseconds;

        gameServer.networkManager.steptimeTotal += tt;

        if (gameServer.GetRoom(0).gameMap.GetFrame() > 50)
        {
            if (gameServer.networkManager.MaxStep < tt)
            {
                gameServer.networkManager.MaxStep = tt;
            }
        }

//         TCuurent = "Frame " + System.String.Format("{0, 6}", gameServer.GetRoom(0).gameMap.GetFrame()) +
//              "  rec " + System.String.Format("{0, 6}", gameServer.networkManager.ReciveTotal) +
//              "  send " + System.String.Format("{0, 6}", gameServer.networkManager.SendTotal) +
//              "  time " + tt + "ms";
        a++;
        //if (a > 5)

        if (gameServer.GetRoom(0).bStart)
        {
            int FrameCount = gameServer.GetRoom(0).gameMap.GetFrame();
            if (FrameCount < 1)
                FrameCount = 1;
            TCuurent = "total player " + gameServer.GetRoom(1).gameMap.Players.Count +
                " Frame " + System.String.Format("{0, 6}", gameServer.GetRoom(0).gameMap.GetFrame()) + "\n" +
                " total rec " + ((float)gameServer.networkManager.ReciveTotal / 1000000).ToString("f3") + " MB " +
                " send " + ((float)gameServer.networkManager.SendTotal / 1000000).ToString("f3") + " MB " +
                " time " + gameServer.networkManager.steptimeTotal + " ms\n" +
                " aver  rec " + ((float)gameServer.networkManager.ReciveTotal / 1000 / FrameCount).ToString("f3") + " KB " +
                " send " + ((float)gameServer.networkManager.SendTotal / 1000 / FrameCount).ToString("f3") + " KB " +
                " time " + gameServer.networkManager.steptimeTotal / FrameCount + " ms\n" +
                " max  rec " + (gameServer.networkManager.MaxRecive / 1000).ToString("f3") + " KB " +
                " send " + (gameServer.networkManager.MaxSend / 1000).ToString("f3") + " KB " +
                " time " + gameServer.networkManager.MaxStep + " ms\n"+
                " curr  rec " + gameServer.networkManager.CurrentRecive.ToString().PadLeft(6, '_') +
                " send " + gameServer.networkManager.CurrentSend.ToString().PadLeft(6, '_') +
                " time " + tt + " ms";
#if UNITY_EDITOR
            UnityEngine.Debug.Log(TCuurent);
#endif

            var tbuffer = System.Text.Encoding.UTF8.GetBytes(TCuurent.Replace("\n", "") + "\n");
            Ts.Write(tbuffer,0, tbuffer.Length);
            Ts.Flush();
        }
        else
        {
            TCuurent = "connect Count " + gameServer.GetRoom(0).players.Count;
#if UNITY_EDITOR
           // UnityEngine.Debug.Log(TCuurent);
#endif
        }


        // TS.Enqueue(tts);

      //  if(0)
        
	}

    void OnGUI()
    {


        GUI.Label(new Rect(10, 10, 550, 200), TCuurent);

    }

    public void RenderBox(Vector3 Center, Vector3 Half, Color TColor)
    {
        GL.Color(TColor);

        Vector3 RHalf = new Vector3(Half.x, Half.y, -Half.z);
        GL.Vertex(Center - Half); GL.Vertex(Center - RHalf);
        GL.Vertex(Center - Half); GL.Vertex(Center + RHalf);
        GL.Vertex(Center + Half); GL.Vertex(Center - RHalf);
        GL.Vertex(Center + Half); GL.Vertex(Center + RHalf);

    }

    Color GetColor(int n)
    {
        switch (n%9)
        {
            case 0:
                return Color.yellow;
            case 1:
                return Color.red;
            case 2:
                return Color.grey;
            case 3:
                return Color.black;
            case 4:
                return Color.green;
            case 5:
                return Color.blue;
            case 6:
                return Color.cyan;
            case 7:
                return Color.magenta;
            case 8:
                return Color.white;
        }

        return Color.white; 
    }

    public void OnPostRender()
    {
        if (gameServer != null)
        {
            Area[] TAreas = null;
            Room Troom = gameServer.GetRoom(1);
            if (Troom != null)
            {
                TAreas = Troom.gameMap.areas;
            }

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin (GL.LINES);


            Dictionary<int, Area> Areas = new Dictionary<int, Area>();

            foreach (var item in Troom.gameMap.Players)
            {
                var Are = TAreas[item.Value.area_id];

                if( !Areas.ContainsKey(item.Value.area_id))
                {
                    Areas[item.Value.area_id] = Are;
                }

                Gizmos.color = new Color(0, 1, 1, 1);

                Vector3 TPlayerPos = new Vector3(item.Value.Pos.x, 0, item.Value.Pos.y);

                Vector3 Half = new Vector3(Troom.gameMap.singleAreaSize / 2 - 2, 0, Troom.gameMap.singleAreaSize/2 - 2);

               // 
              
                int nLoop = 10;
                if (Troom.gameMap.PlayerProgressid.ContainsKey(item.Key))
                {
                    nLoop = 0;
                    int Progressid = Troom.gameMap.PlayerProgressid[item.Key];
                    for (; nLoop < Troom.gameMap.Progressids.Count; ++nLoop)
                    {
                        if (Troom.gameMap.Progressids[nLoop] == Progressid)
                        {
                            break;
                        }
                    }
                }
                RenderBox(TPlayerPos, new Vector3(3, 3, 3), GetColor(nLoop));
                RenderBox(TPlayerPos, new Vector3(2, 0, 2), GetColor(nLoop));
                RenderBox(TPlayerPos, new Vector3(1, 0, 1), GetColor(nLoop));

                if (TPlayerPos.x > (Are.AreaCenter.x + Half.x) || TPlayerPos.x < (Are.AreaCenter.x - Half.x) ||
                    TPlayerPos.y > (Are.AreaCenter.y + Half.y) || TPlayerPos.y < (Are.AreaCenter.y - Half.y))
                {
                    GL.Vertex(TPlayerPos); GL.Vertex(Are.AreaCenter);
                }

            }


            {
                for (int nLoop = 0; nLoop < TAreas.Length; ++nLoop)
                {
                    var Are = TAreas[nLoop];

                    
                    if (Areas.ContainsKey(Are.AreaID))
                    {
                        RenderBox(Are.AreaCenter, new Vector3(Troom.gameMap.singleAreaSize / 2 - 2, 0, Troom.gameMap.singleAreaSize / 2 - 2), new Color(1, 1, 1, 0.6f));
                    }

                    RenderBox(Are.AreaCenter, new Vector3(Troom.gameMap.singleAreaSize, 0, Troom.gameMap.singleAreaSize) / 2, new Color(0.5f, 0.5f, 0.5f, 0.2f));
                }
            }

            
            GL.End ();
            GL.PopMatrix ();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (gameServer != null)
        {
            Area[] TAreas = null;
            Room Troom = gameServer.GetRoom(1);
            if (Troom != null)
            {
                TAreas = Troom.gameMap.areas;
            }


            foreach (var item in Troom.gameMap.Players)
            {
                var Are = TAreas[item.Value.area_id];

                Gizmos.color = new Color(0, 1, 1, 1);

                Vector3 TPlayerPos = new Vector3(item.Value.Pos.x, 0, item.Value.Pos.y);

                Gizmos.DrawWireCube(Are.AreaCenter, new Vector3(Troom.gameMap.singleAreaSize - 1, 0, Troom.gameMap.singleAreaSize - 1));

#if UNITY_EDITOR
                GUIStyle style = new GUIStyle();
                //UnityEditor.Handles.color = color;
                UnityEditor.Handles.Label(
                    TPlayerPos,
                    item.Key.ToString(),
                    style);

#endif

                Gizmos.DrawWireSphere(TPlayerPos, 3.1f);

                Vector3 Half = new Vector3(Troom.gameMap.singleAreaSize / 2 - 2, 0, Troom.gameMap.singleAreaSize / 2 - 2);

                if (TPlayerPos.x > (Are.AreaCenter.x + Half.x) || TPlayerPos.x < (Are.AreaCenter.x - Half.x) ||
                TPlayerPos.y > (Are.AreaCenter.y + Half.y) || TPlayerPos.y < (Are.AreaCenter.y - Half.y))
                {
                    GL.Vertex(TPlayerPos); GL.Vertex(Are.AreaCenter);
                }


            }


            int np = 0;
            foreach (var item in Troom.gameMap.PlayersSYN)
            {
                var TPlayers = item.Value;

                Gizmos.color = GetColor(np);

                foreach (var itemPlayer in TPlayers)
                {
                    Vector3 TPlayerPos = new Vector3(itemPlayer.Value.x, np * 6 + 10, itemPlayer.Value.y);

#if UNITY_EDITOR
                    GUIStyle style = new GUIStyle();
                    //UnityEditor.Handles.color = color;
                    UnityEditor.Handles.Label(
                        TPlayerPos,
                        itemPlayer.Key.ToString(),
                        style);

#endif

                    Gizmos.DrawWireSphere(TPlayerPos, 3.1f);

                }

                np++;
            }


           
            {
                for (int nLoop = 0; nLoop < TAreas.Length; ++nLoop)
                {
                    var Are = TAreas[nLoop];

                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
                    Gizmos.DrawWireCube(Are.AreaCenter, new Vector3(Troom.gameMap.singleAreaSize, 0, Troom.gameMap.singleAreaSize));

#if UNITY_EDITOR
                    GUIStyle style = new GUIStyle();
                    style.fontSize = 6;
                    //UnityEditor.Handles.color = color;
                    UnityEditor.Handles.Label(
                        Are.AreaCenter + new Vector3(-Troom.gameMap.singleAreaHalfSize, 0, Troom.gameMap.singleAreaHalfSize),
                        " " +Are.AreaID,// +"\n" + "(" + Are.Index_x + "," + Are.Index_y + ")",
                        style);
#endif

                }
            }
        }
    }
}
