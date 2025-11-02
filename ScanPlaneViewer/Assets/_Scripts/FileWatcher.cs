using UnityEngine;
using System;
using System.IO;
using UnityEngine.Events;
using System.Collections.Generic;

public class FileWatcher : MonoBehaviour
{
    public string folderPath = "";
    FileSystemWatcher fileSystemWatcher;

    public UnityEvent<string> fileChanged;

    public bool loadFileAtStart;

    object filesChanged_locker = new object();
    List<string> filesChanged = new List<string>();


    void Start()
    {
        if (fileChanged == null)
            fileChanged = new UnityEvent<string>();

        // Initialise le FileSystemWatcher
        fileSystemWatcher = new FileSystemWatcher();
        fileSystemWatcher.Path = folderPath;

        // Ajoute un filtre pour tous les fichiers
        fileSystemWatcher.Filter = "*.*";

        // Ajoute un événement pour la création de fichiers
        fileSystemWatcher.Created += OnFileCreated;

        // Commence à surveiller le dossier
        fileSystemWatcher.EnableRaisingEvents = true;

        Debug.Log("Surveillance du dossier : " + folderPath);

        if (loadFileAtStart)
            LoadFileAtStart();
    }

    void LoadFileAtStart()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        FileInfo[] fileInfos = dirInfo.GetFiles();
        lock (filesChanged_locker)
            foreach (FileInfo file in fileInfos)
                filesChanged.Add(file.FullName);

    }

    void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        // Attends un peu pour s'assurer que le fichier est complètement écrit
        System.Threading.Thread.Sleep(100);

        // Vérifie si le fichier existe et est accessible
        if (File.Exists(e.FullPath))
        {
            try
            {
                lock (filesChanged_locker)
                    filesChanged.Add(e.FullPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur : " + ex.Message);
            }
        }
    }

    void Update()
    {
        if (filesChanged.Count > 0)
        {
            lock (filesChanged_locker)
                while (filesChanged.Count > 0)
                {
                    string file = filesChanged[0];
                    fileChanged.Invoke(file);
                    filesChanged.RemoveAt(0);
                }
        }
    }

    void OnDestroy()
    {
        if (fileSystemWatcher != null)
            fileSystemWatcher.Dispose();
    }
}
