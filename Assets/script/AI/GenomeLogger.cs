using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GenomeLogger
{
    const string LogFileName = "generation_log.json";

    public static string GetLogFilePath()
    {
        return Path.Combine(Application.persistentDataPath, LogFileName);
    }

    public static List<GenerationLog> LoadLogs()
    {
        string path = GetLogFilePath();
        if (!File.Exists(path))
            return new List<GenerationLog>();

        string json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
            return new List<GenerationLog>();

        GenerationLogCollection wrapper = JsonUtility.FromJson<GenerationLogCollection>(json);
        return wrapper != null && wrapper.logs != null ? wrapper.logs : new List<GenerationLog>();
    }

    public static void AppendLog(GenerationLog log)
    {
        if (log == null) return;

        GenerationLogCollection wrapper = new GenerationLogCollection
        {
            logs = LoadLogs()
        };
        wrapper.logs.Add(log);

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(GetLogFilePath(), json);
    }
}
