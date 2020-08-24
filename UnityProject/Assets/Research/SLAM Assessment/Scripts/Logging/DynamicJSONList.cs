using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DynamicJsonList
{
    [JsonIgnore]
    private JsonSerializer serializer;

    [JsonIgnore]
    private JsonTextWriter periodicLogWriter;

    [JsonIgnore]
    private JsonSerializerSettings settings;

    public DynamicJsonList(string folder, string filedescriptor)
    {
        settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        serializer = new JsonSerializer();
        StreamWriter writer = FileHelpers.GetStreamWriter(folder, filedescriptor);
        periodicLogWriter = new JsonTextWriter(writer);
        periodicLogWriter.WriteStartObject();
        periodicLogWriter.WritePropertyName("List");
        periodicLogWriter.WriteStartArray();
    }

    public void CloseList()
    {
        if (periodicLogWriter != null)
        {
            periodicLogWriter.WriteEndArray();
            periodicLogWriter.WriteEndObject();
            periodicLogWriter.Flush();
            periodicLogWriter.CloseOutput = true;
            periodicLogWriter.Close();
        }
    }

    private bool firstLine = true;
    public void RecordRow(object row)
    {
        if (periodicLogWriter != null)
        {
            if (!firstLine)
                periodicLogWriter.WriteRaw(",");
            firstLine = false;

            periodicLogWriter.WriteRaw((JsonConvert.SerializeObject(row, Formatting.Indented, settings)));
        }
    }
}