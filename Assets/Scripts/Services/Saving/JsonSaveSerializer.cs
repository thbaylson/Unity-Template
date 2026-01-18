using System.Text;
using UnityEngine;

public interface ISaveSerializer
{
    byte[] Serialize<T>(T obj);
    T Deserialize<T>(byte[] bytes);
}

public class JsonSaveSerializer : ISaveSerializer
{
    public byte[] Serialize<T>(T obj)
    {
        var json = JsonUtility.ToJson(obj, prettyPrint: false);
        return Encoding.UTF8.GetBytes(json);
    }

    public T Deserialize<T>(byte[] bytes)
    {
        var json = Encoding.UTF8.GetString(bytes);
        return JsonUtility.FromJson<T>(json);
    }
}
