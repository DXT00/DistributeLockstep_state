using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area
{
    public List<SyncFrame> TotallFrames;
    public int AreaID;
    public int FrameIndex;

    public int Index_x;
    public int Index_y;


    public Vector3 AreaCenter;



    public Area( int x, int y )
    {
        Index_x = x;
        Index_y = y;

        TotallFrames = new List<SyncFrame>();

        FrameIndex = 0;
    }

    public void SetCenter(Vector3 Center)
    {
        this.AreaCenter = Center;
    }
    public bool HasFrame(int index)
    {
        if (0 <= index && index < TotallFrames.Count)
        {
            return true;
        }

        return false;
    }

    public void RecordCustomSyncMsg(CustomSyncMsg customSyncMsg)
    {
        if (TotallFrames.Count > 0)
        {
            SyncFrame CurrentSyncFrame = TotallFrames[TotallFrames.Count - 1];
            CurrentSyncFrame.conbine_msg(customSyncMsg);
        }
        else
        {
            UnityEngine.Debug.Log("RecordCustomSyncMsg  TotallFrames count" + 0);
        }
    }

    public void RecordCustomSyncMsg(List<CustomSyncMsg> customSyncMsgs)
    {
        if (TotallFrames.Count > 0)
        {
            SyncFrame CurrentSyncFrame = TotallFrames[TotallFrames.Count - 1];
            CurrentSyncFrame.conbine_msg(customSyncMsgs);
        }
        else
        {
            UnityEngine.Debug.Log("RecordCustomSyncMsg  TotallFrames count" + 0);
        }
    }

    public List<SyncFrame> GetNewFrames(int StartIndex)
    {

        List<SyncFrame> syncFrames = new List<SyncFrame>();

        for( int i = StartIndex; i< TotallFrames.Count; ++i )
        {
            syncFrames.Add( TotallFrames[i]);
        }

        return syncFrames;
    }

//     public List<SyncFrame> GetFrames(List<int> MissList)
//     {
//         List<SyncFrame> syncFrames = new List<SyncFrame>();
//         if(MissList != null)
//         {
//             foreach(int index in MissList)
//             {
//                 if(HasFrame(index))
//                 {
//                     syncFrames.Add(syncFrame_ht[index] as SyncFrame);
//                 }
//             }
//         }
//         return syncFrames;
//     }

    public SyncFrame GetFrame(int frameindex)
    {
        if(0 <= frameindex && frameindex < TotallFrames.Count)
        {
            return TotallFrames[frameindex];
        }
        return null;
    }
    public int GetAreaID()
    {
        return this.AreaID;
    }
    public void OnTick(int frame)
    {
        //Players.Clear();

        this.FrameIndex = frame;

        SyncFrame syncFrame = new SyncFrame(frame, this.AreaID);
        TotallFrames.Add(syncFrame);

        if (TotallFrames.Count == frame + 1)
        {

        }
        else
        {
            UnityEngine.Debug.Log("tick error frame " + frame + " TotallFrames Count " + TotallFrames.Count);
        }
    }
}