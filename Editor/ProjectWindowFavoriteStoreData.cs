using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace ProjectWindowHistory 
{
    [Serializable]
    public class FavoriteRecordStoreData
    {
        [SerializeField]
        private string[] _assetPathArray;
        [SerializeField]
        private string _aliasText;

        public FavoriteRecordStoreData(string[] assetPathArray, string aliasText)
        {
            _assetPathArray = assetPathArray;
            _aliasText = aliasText;
        }

        public ProjectWindowFavoriteRecord ToRecordData()
        {
            var instanceIds = new int[_assetPathArray.Length];
            for (var i = 0; i < _assetPathArray.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_assetPathArray[i]);
                var instanceId = asset != null ? asset.GetInstanceID() : 0;
                instanceIds[i] = instanceId;
            }

            return new ProjectWindowFavoriteRecord(instanceIds, _aliasText);
        }
    }

    [Serializable]
    public class ProjectWindowFavoriteStoreData
    {
        [SerializeField]
        private List<FavoriteRecordStoreData> _favoriteStoreDataList;

        public List<ProjectWindowFavoriteRecord> FavoriteRecordList => _favoriteStoreDataList.Select(x => x.ToRecordData()).ToList();

        public ProjectWindowFavoriteStoreData(List<ProjectWindowFavoriteRecord> records)
        {
            _favoriteStoreDataList = records.Select(x => x.ToStoreData()).ToList();
        }
    }
}