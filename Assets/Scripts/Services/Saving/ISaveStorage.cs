/// <summary>
/// Defines read/write/delete operations for save payloads across platform-specific storage backends.
/// </summary>
public interface ISaveStorage
{
    bool Exists(string fileName);
    byte[] ReadAllBytes(string fileName);
    void WriteAllBytes(string fileName, byte[] bytes);
    void Delete(string fileName);
}
