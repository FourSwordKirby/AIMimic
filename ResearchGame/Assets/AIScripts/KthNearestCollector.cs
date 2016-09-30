using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;


public class KthNearestCollector {

    public static List<GameSnapshot> snapshots = new List<GameSnapshot>();
    private static XmlSerializer snapshotSerializer = new XmlSerializer(typeof(List<GameSnapshot>));


    public static void addSnapshot(GameSnapshot snapshot)
    {
        snapshots.Add(snapshot);
    }


    static string dir = Application.streamingAssetsPath + "/PlayerLogs/Log.txt";
    static string serializationFile = dir;

    public static void writeToLog()
    {
        Debug.Log("wrote to log");

        string datalog = "";
        StringWriter textWriter = new StringWriter();
        XmlWriter writer = XmlWriter.Create(textWriter);

        snapshotSerializer.Serialize(writer, snapshots);
        datalog = textWriter.ToString();
        File.WriteAllText(dir, datalog);
    }

    public static List<GameSnapshot> readFromLog()
    {
        //deserialize
        StreamReader reader = new StreamReader(dir);
        List<GameSnapshot> mySnapshots = (List<GameSnapshot>)snapshotSerializer.Deserialize(reader);

        return mySnapshots;
        
    }
}
