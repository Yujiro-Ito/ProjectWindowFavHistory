using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace ProjectWindowHistory
{
    public class ProjectWindowFavoriteModel
    {
        public string UserLocalStoreDataDirectory => Path.Combine(Application.persistentDataPath, Application.dataPath.Split('/').Reverse().Skip(1).FirstOrDefault()); //Application.dataPathの末尾から2つ目がPJ名
        public string PJGlobalStoreDataDirectory => Application.streamingAssetsPath;
        public string StoreDataPath(string dirPath) => Path.Combine(dirPath, "FavoriteRecord.json");

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

        public void ReplaceAllAndSave(List<ProjectWindowFavoriteRecord> records)
        {
            _records = new List<ProjectWindowFavoriteRecord>(records);
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

        private bool IsStored => File.Exists(StoreDataPath(PJGlobalStoreDataDirectory)) || File.Exists(StoreDataPath(UserLocalStoreDataDirectory));

        private void LoadFromJson()
        {
            var records = new List<ProjectWindowFavoriteRecord>();

            var localPath = StoreDataPath(UserLocalStoreDataDirectory);
            if (File.Exists(localPath))
            {
                var localJson = File.ReadAllText(localPath);
                records.AddRange(JsonUtility.FromJson<ProjectWindowFavoriteStoreData>(localJson)?.ToFavoriteRecordList(FavoriteStoreType.USER_LOCAL) ?? new());
            }

            var globalPath = StoreDataPath(PJGlobalStoreDataDirectory);
            if (File.Exists(globalPath))
            {
                var globalJson = File.ReadAllText(globalPath);
                records.AddRange(JsonUtility.FromJson<ProjectWindowFavoriteStoreData>(globalJson)?.ToFavoriteRecordList(FavoriteStoreType.PJ_GLOBAL) ?? new());
            }

            _records = records;
        }

        private void StoreToJson()
        {
            StoreToLocalJson(_records.Where(x => !x.IsProjectGlobal));
            StoreToGlobalJson(_records.Where(x => x.IsProjectGlobal));
        }

        private void StoreToLocalJson(IEnumerable<ProjectWindowFavoriteRecord> records)
        {
            if (records == null) return;
            if (!Directory.Exists(UserLocalStoreDataDirectory))
            {
                Directory.CreateDirectory(UserLocalStoreDataDirectory);
            }
            var storeData = new ProjectWindowFavoriteStoreData(records.ToList());
            var json = JsonUtility.ToJson(storeData);
            File.WriteAllText(StoreDataPath(UserLocalStoreDataDirectory), json);
        }

        private void StoreToGlobalJson(IEnumerable<ProjectWindowFavoriteRecord> records)
        {
            if (records == null) return;
            if (!Directory.Exists(PJGlobalStoreDataDirectory))
            {
                Directory.CreateDirectory(PJGlobalStoreDataDirectory);
            }
            var storeData = new ProjectWindowFavoriteStoreData(records.ToList());
            var json = JsonUtility.ToJson(storeData);
            File.WriteAllText(StoreDataPath(PJGlobalStoreDataDirectory), json);
        }
    }
}
