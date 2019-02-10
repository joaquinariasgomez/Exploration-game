using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileWriter {

    private string path;
    StreamWriter writer;


    public FileWriter(string path)
    {
        this.path = path;
        //Create txt in path
        writer = new StreamWriter(path, true);
    }

    public void Write(float number)
    {
        writer.Write(number + " ");
    }

    public void End()
    {
        writer.Close();
    }
}
