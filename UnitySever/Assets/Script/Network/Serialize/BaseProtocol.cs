using System.Collections;
using System.Collections.Generic;

public class BaseProtocol
{
    public int seq;
    public int cmd;
    public int uid;
    public int ack;
    public int flag;

    public int get_seq()
    {
        return this.seq;
    }
    public int get_cmd()
    {
        return this.cmd;
    }
    public int get_uid()
    {
        return this.uid;
    }
    public int get_ack()
    {
        return this.ack;
    }
    public int get_flag()
    {
        return this.flag;
    }
}
