using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace ProjectWindowHistory
{
    public enum FavoriteStoreType
    {
        PJ_GLOBAL,
        USER_LOCAL
    }
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

        public ProjectWindowFavoriteRecord ToRecordData(FavoriteStoreType storeType = FavoriteStoreType.USER_LOCAL)
        {
            var instanceIds = new int[_assetPathArray.Length];
            for (var i = 0; i < _assetPathArray.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_assetPathArray[i]);
                var instanceId = asset != null ? asset.GetInstanceID() : 0;
                instanceIds[i] = instanceId;
            }

            return new ProjectWindowFavoriteRecord(instanceIds, _aliasText, storeType);
        }
    }

    [Serializable]
    public class ProjectWindowFavoriteStoreData
    {
        [SerializeField]
        private List<FavoriteRecordStoreData> _favoriteStoreDataList;

        public List<ProjectWindowFavoriteRecord> ToFavoriteRecordList(FavoriteStoreType storeType) => _favoriteStoreDataList.Select(x => x.ToRecordData(storeType)).ToList();

        public ProjectWindowFavoriteStoreData(List<ProjectWindowFavoriteRecord> records)
        {
            _favoriteStoreDataList = records.Select(x => x.ToStoreData()).ToList();
        }
    }
}
