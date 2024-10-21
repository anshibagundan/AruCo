using System;

[System.Serializable]
public class QuizTF
{
    public int id;
    public bool cor;
    public int quiz;

    public bool getCor()
    {
        return cor;
    }
    public int getId()
    {
        return id;
    }
}

