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
        var ftm = new DirectoryManager(datadir);
        Debug.Log(ftm.ToJson());
        
        // dispatcher.Enqueue(loginManager.LoadMainMenu());
        yield return null;
    }
}

public class DirectoryManager{    
    Directory mDir;

    public DirectoryManager(string mEntryPath)
    {
        Debug.Log("parent dir path: " + mEntryPath);
        mDir = GetSubdirectoriesAndFilesRecursive(mEntryPath);
    }

    public Directory GetSubdirectoriesAndFilesRecursive(string entrypath){
        var thisdir = new DirectoryInfo(entrypath);
        Directory res = new Directory(thisdir.FullName, thisdir.Name);

        foreach (var file in thisdir.EnumerateFiles()){
            File f = new File(file.Name, file.FullName, file.Length, file.LastWriteTimeUtc);
            res.mFiles.Add(f);
        }

        foreach (DirectoryInfo dir in thisdir.EnumerateDirectories()){
            var subdir = GetSubdirectoriesAndFilesRecursive(dir.FullName);
            res.mSubdirectories.Add(subdir);
        }

        return res;
    }

    public string ToJson(){
        var res = "{";
        res += JsonRecursive(mDir);
        res += "}";

        return res;
    }

    string JsonRecursive(Directory dir){
        // string res = @"
        // {
        //     ---START WRITE---
        //     ""directoryName"":{
        //         ""files"":[
        //             {""name"":""filename.txt"",""size"":123,""lastModified"":""12/03/24T23:45:16-GTM+03:00},
        //             {---}
        //         ]
        //         --- WRITE NEXT ---
        //         ""directoryName"":{
        //             ""files"":[...]
        //             ---WRITE NEXT---
        //             ""directoryName"":{
        //                 ""files"":[...]
        //             }--- FINISH WRITE ---
        //         }--- FINISH WRITE ---
        //     }--- FINISH WRITE ---
        // }";

        string res = "";
        
        res += @$"""{dir.mName}"":{{
""files"":";

        string files = "[";
        int cont = 1;
        foreach (var file in dir.mFiles){
            files += @$"{{""name"":""{file.path.Replace(@"\", @"\\")}"",""size"":{file.size},""lastModified"":""{file.lastModified}""}}";
            if(dir.mFiles.Count != cont){
                files += ",";
            }
            cont ++;
        }
        
        res+= files + "]";
        if(dir.mSubdirectories.Count == 0){
            res +="\n";
        }else{
            res +=",\n";
        }

        foreach (var subdir in dir.mSubdirectories){
            res += JsonRecursive(subdir);
        }
        res +="}\n";
        return res;
    }
}

public class Directory{
    public string mPath, mName;
    public List<Directory> mSubdirectories;
    public List<File> mFiles;
    public Directory(string path, string name){
        mSubdirectories = new List<Directory>();
        mFiles = new List<File>();
        mPath = path;
        mName = name;
    }
}

public class File{
    public string name, path;
    public long size;
    public DateTime lastModified;
    public File(string name, string path, long size, DateTime lastModified){
        this.name = name;
        this.path = path;
        this.size = size;
        this.lastModified = lastModified;
    }

    public override string ToString()
    {
        return path + " - " + size.ToString() + " - " + lastModified.ToString();
    }
}
