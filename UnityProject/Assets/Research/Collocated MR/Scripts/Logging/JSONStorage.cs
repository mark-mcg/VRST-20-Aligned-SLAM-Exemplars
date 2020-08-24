using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSONStorage 
{
    public static void StoreObject<T>(T instance, string filepath, bool generateFilepath = false)
    {
        Debug.Log("Trying to store object of type " + typeof(T) + " at " + filepath);

        if (generateFilepath)
        {
            filepath = GetGenericFileStorePath(filepath);
        }

        // Then write the config back out (if it didn't already exist) so we can manually edit it later after first run
        if (instance != null)
        {
            StreamWriter streamWriter = FileHelpers.GetStreamWriter(filepath);

            if (streamWriter != null)
            {
                JsonSerializer serializer = new JsonSerializer();
                JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter);
                serializer.Serialize(jsonWriter, instance);
                jsonWriter.Close();
                streamWriter.Close();
            }
        }

        Debug.Log("Wrote JSON file to " + filepath);
    }

    public static T RetrieveObject<T>(string filepath, bool generateFilePath = false)
    {
        if (generateFilePath)
        {
            filepath = GetGenericFileStorePath(filepath);
        }

        JsonSerializer serializer = new JsonSerializer();
        T storedObj = default(T);

        Debug.Log("Trying to load object of type " + typeof(T) + " from " + filepath);
        StreamReader streamReader = FileHelpers.GetStreamReader(filepath);

        if (streamReader != null)
        {
            JsonTextReader jsonReader = new JsonTextReader(streamReader);
            storedObj = (T)serializer.Deserialize(jsonReader, typeof(T));
            jsonReader.Close();
            streamReader.Close();
        }

        return storedObj;
    }

    private static string GetGenericFileStorePath(string name)
    {
        return FileHelpers.GetFileNameAndPath("JSONObjectStore", name, false, ".json");
    }
}
