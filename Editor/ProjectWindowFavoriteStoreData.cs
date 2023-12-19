using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectWindowHistory 
{
    [System.Serializable]
    public class ProjectWindowFavoriteStoreData
    {
        public List<ProjectWindowFavoriteRecord> StoredFavoriteRecordList;

        public ProjectWindowFavoriteStoreData(List<ProjectWindowFavoriteRecord> records)
        {
            StoredFavoriteRecordList = records;
        }
    }
}