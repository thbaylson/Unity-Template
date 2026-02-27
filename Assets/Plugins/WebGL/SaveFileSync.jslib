mergeInto(LibraryManager.library, {
  SyncWebGLFileSystem: function () {
    if (!FS || !FS.syncfs) {
      return;
    }

    FS.syncfs(false, function (error) {
      if (error) {
        console.error("WebGL save sync to IndexedDB failed.", error);
      }
    });
  },

  SyncWebGLFileSystemFromIndexedDb: function () {
    if (!FS || !FS.syncfs) {
      return;
    }

    FS.syncfs(true, function (error) {
      if (error) {
        console.error("WebGL save sync from IndexedDB failed.", error);
      }
    });
  }
});
