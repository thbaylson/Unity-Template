/// <summary>
/// Defines read/write/delete operations for save payloads across platform-specific storage backends.
/// </summary>
namespace Template.Services.Saving
{
    public interface ISaveStorage
    {
        bool Exists(string fileName);
        byte[] ReadAllBytes(string fileName);
        void WriteAllBytes(string fileName, byte[] bytes);
        void Delete(string fileName);
    }
}
