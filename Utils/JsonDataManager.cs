﻿using Newtonsoft.Json;
using System.IO;
using System;
using ServerManager.Models;

namespace ServerManager.Utils
{
    /// <summary>
    /// A generic class for managing JSON data files.
    /// </summary>
    /// <typeparam name="T">The type of the data to be managed.</typeparam>
    public class JsonDataManager<T> where T : class
    {
        private readonly string _directoryPath;
        private readonly string _filePath;

        public JsonDataManager(string basePath, string fileName)
        {
            string modDir = Path.GetDirectoryName(basePath) ?? "";

            _directoryPath = Path.Combine(modDir,"Data");
            _filePath = Path.Combine(_directoryPath, fileName);

            EnsureDataDirectory();
        }

        private void EnsureDataDirectory()
        {
            Console.WriteLine("CHECKING DIRECTORY");
            Console.WriteLine("Directory: " + _directoryPath);  
            if (!Directory.Exists(_directoryPath))
            {
                Console.WriteLine("CREATING DIRECTORY");

                Directory.CreateDirectory(_directoryPath);
            }
        }

        public T Load()
        {
            if (!File.Exists(_filePath))
            {
                return default;
            }

            string json = File.ReadAllText(_filePath);
            var obj = JsonConvert.DeserializeObject<T>(json);

            return obj ?? default;
        }

        public void Save(T data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Console.WriteLine($"SAVING JSON IN: {_filePath}");
            File.WriteAllText(_filePath, json);
        }

        public void Update(Action<T> updateAction)
        {
            T data = Load() ?? ServerData.Instance as T;
            updateAction(data);
            Save(data);
        }

        public bool DeleteFile()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                return true;
            }
            return false;
        }
    }
}
