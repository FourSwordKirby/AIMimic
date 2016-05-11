using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;


public class KthNearestCollector {

    public static List<GameSnapshot> snapshots = new List<GameSnapshot>();

    public static void addSnapshot(GameSnapshot snapshot)
    {
        snapshots.Add(snapshot);
    }


    static string dir = @"C:\Users\Roger Liu\Desktop\Research\KthNearest\Assets\PlayerLogs\Log.txt";
    static string serializationFile = dir;

    public static void writeToLog()
    {
        //serialize
        using (Stream stream = File.Open(serializationFile, FileMode.Create))
        {
            var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            bformatter.Serialize(stream, snapshots);
        }
    }

    public static List<GameSnapshot> readFromLog()
    {
        //deserialize
        using (Stream stream = File.Open(serializationFile, FileMode.Open))
        {
            var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            List<GameSnapshot> mySnapshots = (List<GameSnapshot>)bformatter.Deserialize(stream);

            return mySnapshots;
        }
    }
}
