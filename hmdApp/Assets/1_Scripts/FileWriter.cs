using System.IO;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
#if WINDOWS_UWP
using Windows.Storage;
#endif

public class FileWriter
{
    private StreamWriter writer;
    private const string sessionFolderRoot = "LogFiles";
    private string sessionPath;
    private string filePath;

    // The constructor starts the asynchronous process of creating the file.
    //public FileWriter(string participantID, string fileName, string header)
    //{
    //    //Task.Run(() => Initialize(participantID, fileName, header));
    //    Initialize(participantID, fileName, header);
    //    //Initialize(folderName, fileName, header).Wait();
    //}
    private FileWriter() { }

    public static async Task<FileWriter> CreateAsync(string participantID, string fileName, string header)
    {
        var fileWriter = new FileWriter();
        await fileWriter.Initialize(participantID, fileName, header);
        return fileWriter;
    }
    private async Task Initialize(string participantID, string fileName, string header)
    {
        string rootPath = "";
        sessionPath = Path.Combine(sessionFolderRoot, participantID); //  folder name
#if WINDOWS_UWP
        StorageFolder sessionParentFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(sessionPath, CreationCollisionOption.OpenIfExists);
        sessionPath = sessionParentFolder.Path;
#else
        rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), sessionFolderRoot);
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
        sessionPath = Path.Combine(rootPath, participantID); //  folder name
        Directory.CreateDirectory(sessionPath);
#endif

        filePath = Path.Combine(sessionPath, fileName);
        writer = new StreamWriter(filePath, append: true); // 'append: false' will overwrite old files with the same name.
        writer.WriteLine(header);
        writer.AutoFlush = true;
    }

    public void WriteLine(string line)
    {
        writer?.WriteLine(line);
    }

    public void Close()
    {
        writer?.Flush();
        writer?.Close();
        writer?.Dispose();
        writer = null;
    }
}