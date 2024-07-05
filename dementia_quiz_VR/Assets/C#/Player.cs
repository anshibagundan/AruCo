using System;

[System.Serializable]
public class Player
{
    public float pos_x;
    public float pos_y;
    public float pos_z;
    public float rot_x;
    public float rot_y;  
    public float rot_z;
    public float getPosX()
    {
        return pos_x;
    }
    public float getPosY()
    {
        return pos_y;
    }
    public float getPosZ()
    {
        return pos_z;
    }
    public float getRotX()
    {
        return rot_x;
    }
    public float getRotY()
    {
        return rot_y;
    }
    public float getRotZ() 
    {
        return rot_z;
    }

}