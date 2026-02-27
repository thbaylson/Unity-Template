using System.Runtime.InteropServices;
public static class WebGLSaveFileSync
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncWebGLFileSystem();

    [DllImport("__Internal")]
    private static extern void SyncWebGLFileSystemFromIndexedDb();
#endif

    public static void SyncToPersistentStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SyncWebGLFileSystem();
#endif
    }

    public static void SyncFromPersistentStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SyncWebGLFileSystemFromIndexedDb();
#endif
    }
}
