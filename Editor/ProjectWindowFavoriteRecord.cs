using System.IO;
using System.Linq;
using UnityEditor;
using System;

namespace ProjectWindowHistory
{
    public class ProjectWindowFavoriteRecord : IEquatable<ProjectWindowFavoriteRecord>
    {
        private string _aliasText;
        private int[] _selectedFolderInstanceIds;

        public int[] SelectedFolderInstanceIDs => _selectedFolderInstanceIds;

        public ProjectWindowFavoriteRecord(int[] selectedFolderInstanceIds, string aliasText)
        {
            _selectedFolderInstanceIds = selectedFolderInstanceIds;
            _aliasText = aliasText;
        }

        public bool IsValid()
        {
            // フォルダが何かしら削除されていた場合は無効にしておく
            return (_selectedFolderInstanceIds?.Any() ?? false)
                   && _selectedFolderInstanceIds.All(instanceId => EditorUtility.InstanceIDToObject(instanceId) != null);
        }

        public string ToLabelText()
        {
            return string.IsNullOrEmpty(_aliasText) ? SelectedFolderToLabelText() : _aliasText;

            string SelectedFolderToLabelText()
            {
                const int displayFolderCountMax = 3; // 表示は最大3件
                var targetFolderNames = _selectedFolderInstanceIds
                    .Take(displayFolderCountMax)
                    .Select(id =>
                    {
                        var path = AssetDatabase.GetAssetPath(id);
                        return Path.GetFileName(path);
                    });

                var suffix = _selectedFolderInstanceIds.Length > displayFolderCountMax ? "+" : string.Empty;
                return string.Join(",", targetFolderNames) + suffix;
            }
        }
        public override int GetHashCode()
        {
            return (_selectedFolderInstanceIds != null ? _selectedFolderInstanceIds.GetHashCode() : 0);
        }

        public bool Equals(ProjectWindowFavoriteRecord other)
        {
            return _selectedFolderInstanceIds.SequenceEqual(other._selectedFolderInstanceIds);
        }

        public bool IsSequenceEqual(int[] folderInstancedIds)
        {
            if (folderInstancedIds == null || !folderInstancedIds.Any()) return false;
            if (_selectedFolderInstanceIds == null) return false;
            return _selectedFolderInstanceIds.SequenceEqual(folderInstancedIds);
        }

        public FavoriteRecordStoreData ToStoreData()
        {
            var assetPathArray = new string[_selectedFolderInstanceIds.Length];
            for (var i = 0; i < _selectedFolderInstanceIds.Length; i++)
            {
                var instanceId = _selectedFolderInstanceIds[i];
                var assetPath = AssetDatabase.GetAssetPath(instanceId);
                assetPathArray[i] = assetPath;
            }

            return new FavoriteRecordStoreData(assetPathArray, _aliasText);
        }
    }
}