using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using TMPro;

public class FileSync : MonoBehaviour{

    public UnityMainThreadDispatcher dispatcher;
    public LoginManager loginManager;
    public ConnectionManager connManager;

    // PROGRESS BAR
    public GameObject progress_bar;
    public int max , curr = 0;
    public Image mask;
    public TMP_Text overBarInfo, barInfo;

    // FILE SYNC FLAGS
    public int folderCreateTotal = 99999, folderDeleteTotal = 99999, fileCreateTotal = 99999, fileOvewriteTotal = 99999, fileDeleteTotal = 99999;
    public int folderCreateCurrent = 0, folderDeleteCurrent = 0, fileCreateCurrent = 0, fileOvewriteCurrent = 0, fileDeleteCurrent = 0;
    public int actionIndex = -1;
    
    public List<int> fileids = new List<int>();


    public IEnumerator SendFileInformationToServer(){
        var datadir = Environment.CurrentDirectory + @"\TTData";
        var ftm = new DirectoryManager(datadir);

        var toSend = JsonConvert.SerializeObject(ftm.ToJson().Replace("\n",""), Formatting.None);
        connManager.SendMessageToServer("005:"+toSend);

        progress_bar.SetActive(true);
        overBarInfo.text = "downloading files from server, please be patient...";
        
        yield return null;
    }

    public IEnumerator SetInfo(string infostring){
        var infoparts = infostring.Split(',');
        folderCreateTotal = int.Parse(infoparts[0]);
        folderDeleteTotal = int.Parse(infoparts[1]);
        fileCreateTotal = int.Parse(infoparts[2]);
        fileOvewriteTotal = int.Parse(infoparts[3]);
        fileDeleteTotal = int.Parse(infoparts[4]);

        // This should be the number of files to delete/update/create etc...
        max = folderCreateTotal + folderDeleteTotal + fileCreateTotal + fileOvewriteTotal + fileDeleteTotal; 

        StartCoroutine(FileOrderSynchronizer());
        yield return null;
    }

    public IEnumerator FileOrderSynchronizer(){
        while(max > fileids.Count){
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("RECIEVED ALL FILES!!!");
        fileids.Sort();

        actionIndex = fileids[0];
        var i = fileids[0]+fileids.Count;
        
        curr = 0;
        overBarInfo.text = "Writing files.";

        while(actionIndex != i){
            Debug.Log("Number of sinchronizations: " + actionIndex.ToString()+"/"+i.ToString());
            yield return null;
        }

        if(folderCreateTotal==folderCreateCurrent &&
        folderDeleteTotal==folderDeleteCurrent &&
        fileCreateTotal==fileCreateCurrent &&
        fileOvewriteTotal==fileOvewriteCurrent &&
        fileDeleteTotal==fileDeleteCurrent){ // when we've finished syncing files.
            Debug.Log("synced all files");
            
            dispatcher.Enqueue(loginManager.LoadMainMenu());
        }
    }

    public IEnumerator FileRecieve(int msgID, string protocol, string type, string path, DateTime date, byte[] bytemsg, string trail){
        //Progress bar
        if(!fileids.Contains(msgID)){
            // Debug.Log("increased progress bar because of msg: "+ msgID.ToString());
            curr++; // whith each successfull action increase the bar.
            fileids.Add(msgID);
        }
        UpdateProgressBar();

        while (actionIndex != msgID){yield return null;}

        curr ++;
        UpdateProgressBar();
        

        switch (protocol)
        {
            case "update": // goes first cuz its a file only thing.
                File.WriteAllBytes(@"./"+path, bytemsg);
                File.SetLastWriteTime(@"./"+path, date);
                fileOvewriteCurrent++;
            break;

            case "create":
                if(type == "file"){
                    // File.Create(@"./"+path);
                    File.WriteAllBytes(@"./"+path, bytemsg);
                    File.SetLastWriteTime(@"./"+path, date);
                    fileCreateCurrent++;
                }

                if(type=="folder"){
                    Directory.CreateDirectory(path);
                    folderCreateCurrent++;
                }
            break;

            case "delete":
                if(type=="file"){
                    if (File.Exists(path))
                        File.Delete(path);  
                    fileDeleteCurrent++;
                }

                if(type=="folder"){
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    folderDeleteCurrent++;
                }

            break;
            
            default:
            Debug.Log("wrong protocol selected, aborting...");
            //TODO: abort operation (reset to starting state)
            break;
        }

        actionIndex++;
        yield return null;
    }

    void UpdateProgressBar(){
        float fillAmount = (float)curr/(float)max;
        mask.fillAmount = fillAmount;
        barInfo.text = curr.ToString()+"/"+max.ToString();
    }
}

public class DirectoryManager{    
    _Directory mDir;

    public DirectoryManager(string mEntryPath)
    {
        Debug.Log("parent dir path: " + mEntryPath);
        mDir = GetSubdirectoriesAndFilesRecursive(mEntryPath);
    }

    public _Directory GetSubdirectoriesAndFilesRecursive(string entrypath){
        var thisdir = new DirectoryInfo(entrypath);
        _Directory res = new _Directory(thisdir.FullName, thisdir.Name);

        foreach (var file in thisdir.EnumerateFiles()){
            _File f = new _File(file.Name, file.FullName, file.Length, file.LastWriteTimeUtc);
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
        res += JsonRecursive(mDir, true);
        res += "}";

        return res;
    }

    string JsonRecursive(_Directory dir, bool isLastSubdir){
        string res = "";
        
        res += @$"""folder:{dir.mName}"":{{
""files"":";

        string files = "[";
        int cont = 1;
        foreach (var file in dir.mFiles){
            files += @$"{{""name"":""{file.path}"",""size"":{file.size},""lastModified"":""{file.lastModified}""}}";
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

        var dircount = 1;
        foreach (var subdir in dir.mSubdirectories){
            res += JsonRecursive(subdir, dircount == dir.mSubdirectories.Count);
            dircount ++;
        }
        if(isLastSubdir)
            res +="}\n";
        else
            res +="},\n";
        return res;
    }
}

public class _Directory{
    public string mPath, mName;
    public List<_Directory> mSubdirectories;
    public List<_File> mFiles;
    public _Directory(string path, string name){
        mSubdirectories = new List<_Directory>();
        mFiles = new List<_File>();
        mPath = path;
        mName = name;
    }
}

public class _File{
    public string name, path;
    public long size;
    public DateTime lastModified;
    public _File(string name, string path, long size, DateTime lastModified){
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
