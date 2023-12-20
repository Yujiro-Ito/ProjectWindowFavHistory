using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace ProjectWindowHistory 
{
    public class ProjectWindowFavoriteModel 
    {
        public string StoreDataDirectory => Path.Combine(Application.persistentDataPath, Application.dataPath.Split('/').Reverse().Skip(1).FirstOrDefault());
        public string StoreDataPath() => Path.Combine(StoreDataDirectory, "FavoriteRecord.json");

        private List<ProjectWindowFavoriteRecord> _records = new();

        private const int MaxRecordCount = 20;

        public ProjectWindowFavoriteModel()
        {
            if (IsStored)
            {
                LoadFromJson();
                RemoveInvalidRecords();
            }
        }

        public void Reload()
        {
            LoadFromJson();
            RemoveInvalidRecords();
        }

        /// <summary>
        /// Invalidなレコードを削除する
        /// </summary>
        private void RemoveInvalidRecords()
        {
            if (_records == null)
            {
                return;
            } 
            for (var i = _records.Count - 1; i >= 0; i--)
            {
                var record = _records[i];
                if (record == null || record.IsValid())
                {
                    continue;
                }

                _records.RemoveAt(i);
            }
        }

        public void ApplyAndSave(ProjectWindowFavoriteRecord record)
        {
            RemoveInvalidRecords();
            if (_records == null || _records.Count > MaxRecordCount)
            {
                return;
            }
            bool hasAlready = _records.Contains(record);
            if (hasAlready)
            {
                return;
            }

            _records.Add(record);
            StoreToJson();
        }

        public IEnumerable<ProjectWindowFavoriteRecord> GetAllFavoriteRecords()
        {
            return _records;
        }

        public bool HasRecord(int[] folderInstancedIds)
        {
            if (folderInstancedIds == null || !folderInstancedIds.Any()) return false;
            return _records.Any(x => x?.IsSequenceEqual(folderInstancedIds) ?? false);
        }

        private bool IsStored => File.Exists(StoreDataPath());

        private void LoadFromJson()
        {
            var json = File.ReadAllText(StoreDataPath());
            Debug.Log($"Load {json}");
            _records = JsonUtility.FromJson<ProjectWindowFavoriteStoreData>(json)?.FavoriteRecordList;
        }

        private void StoreToJson()
        {
            Debug.Log(_records);
            if (!Directory.Exists(StoreDataDirectory))
            {
                Directory.CreateDirectory(StoreDataDirectory);
            }
            var storeData = new ProjectWindowFavoriteStoreData(_records);
            var json = JsonUtility.ToJson(storeData);
            Debug.Log(json + " " + StoreDataPath());
            File.WriteAllText(StoreDataPath(), json);
        }
    }
}