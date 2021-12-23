using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;


public class FileSync : MonoBehaviour{

    public UnityMainThreadDispatcher dispatcher;
    public LoginManager loginManager;


    public IEnumerator SyncFilesWithServer(){
        var datadir = Environment.CurrentDirectory + @"\TTData";
        Debug.Log("data directory is = " + datadir);
        var ftm = new FileTreeManager(datadir);
        Debug.Log(ftm.ToString());
        
        // dispatcher.Enqueue(loginManager.LoadMainMenu());
        yield return null;
    }
}

public class FileTreeManager{
    private DirectoryInfo mDirectoryInfo;
    public FileTreeManager(string aEntryPoint)
    {
        SetEntryPoint(aEntryPoint);
    }

    public void SetEntryPoint(string aEntryPoint)
    {
        this.mDirectoryInfo = new DirectoryInfo(aEntryPoint);
    }

    public Directory GetDirInfo(){
        Directory res = new Directory(mDirectoryInfo.Name);
        foreach (DirectoryInfo dir in this.mDirectoryInfo.EnumerateDirectories())
        {
            foreach (var file in dir.EnumerateFiles())
            {
                var fileInfo = file.Name + " - " + file.Length.ToString() + " - " + file.LastWriteTimeUtc.ToString();
                result.Append("--|-- " + fileInfo);
            }
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        foreach (DirectoryInfo dir in this.mDirectoryInfo.EnumerateDirectories())
        {
            result.Append("|-- " + dir.Name + Environment.NewLine);
            foreach (var file in dir.EnumerateFiles())
            {
                var fileInfo = file.Name + " - " + file.Length.ToString() + " - " + file.LastWriteTimeUtc.ToString();
                result.Append("--|-- " + fileInfo);
            }
        }
        return result.ToString();
    }
}

public class Directory{
    public string mName;
    public List<Directory> mSubdirectories;
    public List<File> mFiles;
    public Directory(string name){
        mSubdirectories = new List<Directory>();
        mFiles = new List<File>();
        mName = name;
    }

}
public class File{
    public string name;
    public long size;
    public DateTime lastModified;
    public File(string name, long size, DateTime lastModified){

    }
}
