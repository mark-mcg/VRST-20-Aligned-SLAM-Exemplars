using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


//public class Results : Result
//{
//    public string description;
//    public List<Result> results = new List<Result>();
//}

public static class FileHelpers
{
    public static void WriteToFile(this List<Loggable> m, string folder, string filedescriptor, string jsondescriptor)
    {
        //results.description = jsondescriptor;
        //results.results = m;
        m.WriteToFile(folder, filedescriptor);
    }

    public static void WritePropertyNameAndValue(this JsonTextWriter m, string name, object obj)
    {
        m.WritePropertyName(name);
        m.WriteRawValue(JsonConvert.SerializeObject(obj, Formatting.Indented));
    }

    public static bool WriteToFile(this object obj, string folder, string filedescriptor)
    {
        string output = JsonConvert.SerializeObject(obj, Formatting.Indented);

        try
        {
            StreamWriter writer = GetStreamWriter(folder, filedescriptor);

            if (writer != null)
            {
                writer.WriteLine(output);
                writer.Close();
                return true;
            }
            new Exception("Error creating streamwriter for " + folder + " " + filedescriptor);
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing JSON file " + e);
        }
        return false;
    }

    /// <summary>
    /// Gets a StreamWriter for a file in the given folder within the Application dataPath//Log Output//. Note that the
    /// current DateTime will be appended to the fileDescriptor to avoid conflicts/overwriting
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="fileDescriptor"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static StreamWriter GetStreamWriter(string folder, string fileDescriptor, string extension = ".json")
    {
        string path = GetFileNameAndPath(folder, fileDescriptor, true, extension);
        Debug.Log("Recording to " + path);
        return GetStreamWriter(path);
    }

    public static string GetFileNameAndPath(string folder, string fileDescriptor, bool ensureUnique = true, string extension = ".json")
    {
        try
        {
            string datapath = Application.persistentDataPath;// Application.dataPath;
            if (datapath.Contains("Assets"))
                datapath = datapath.Remove(datapath.Length - "Assets".Length);

            if (datapath.Contains("base.apk"))
                datapath = datapath.Remove(datapath.Length - "base.apk".Length);

            string DirectoryPath = datapath + System.IO.Path.DirectorySeparatorChar + "ExternalProjectFiles";

            if (folder != null && folder.Length > 0)
                DirectoryPath += System.IO.Path.DirectorySeparatorChar + folder;

            DirectoryInfo folderInstance = Directory.CreateDirectory(DirectoryPath);

            string filename = fileDescriptor + (ensureUnique ?  "-" + DateTime.Now.ToString("yyyy_MM_dd_THH_mm_ss_Z") : "") + extension;
            string fullfilepath = folderInstance.FullName + System.IO.Path.DirectorySeparatorChar + filename;
            return fullfilepath;
        }
        catch (Exception e)
        {
            Debug.LogError("GetFileNameAndPath error " + e + e.StackTrace);
        }
        return null;
    }

    public static StreamWriter GetStreamWriter(string fullfilepath)
    {
        StreamWriter writer = null;

        try
        {
            FileStream file = File.Create(fullfilepath);
            writer = new StreamWriter(file);
        }
        catch (Exception e)
        {
            Debug.LogError("Can't get stream reader for file " + fullfilepath + " " + e);
        }
        return writer;
    }

    public static StreamReader GetStreamReader(string fileName) 
    {
        StreamReader reader = null;
        try
        {
            FileStream file = File.OpenRead(fileName);
            reader = new StreamReader(file);
        } catch (Exception e)
        {
            Debug.LogError("Can't get stream reader for file " + fileName + " " + e);
        }
        return reader;
    }
}