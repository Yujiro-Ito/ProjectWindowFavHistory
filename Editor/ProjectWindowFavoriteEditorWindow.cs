using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ProjectWindowHistory
{
    public class ProjectWindowFavoriteEditorWindow : EditorWindow
    {
        private ProjectWindowFavoriteModel _model;
        private List<ProjectWindowFavoriteRecord> _editingRecords;
        private ReorderableList _reorderableList;
        private Vector2 _scrollPos;

        private static readonly string[] StoreTypeNames = { "USER_LOCAL", "PJ_GLOBAL" };

        public static void Open(ProjectWindowFavoriteModel model)
        {
            var window = GetWindow<ProjectWindowFavoriteEditorWindow>("Favorite 設定");
            window._model = model;
            window.Initialize();
            window.Show();
        }

        private void Initialize()
        {
            _editingRecords = _model.GetAllFavoriteRecords().ToList();
            BuildReorderableList();
        }

        private void BuildReorderableList()
        {
            _reorderableList = new ReorderableList(_editingRecords, typeof(ProjectWindowFavoriteRecord), true, true, false, false);

            _reorderableList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Favorite Records");
            };

            _reorderableList.elementHeightCallback = index => EditorGUIUtility.singleLineHeight + 4f;

            _reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index < 0 || index >= _editingRecords.Count) return;

                var record = _editingRecords[index];
                rect.y += 2f;
                float lineHeight = EditorGUIUtility.singleLineHeight;

                float x = rect.x;
                float totalWidth = rect.width;

                // Alias TextField
                float aliasWidth = totalWidth * 0.25f;
                record.AliasText = EditorGUI.TextField(new Rect(x, rect.y, aliasWidth, lineHeight), record.AliasText);
                x += aliasWidth + 4f;

                // Folder path label (read-only)
                float pathWidth = totalWidth * 0.45f;
                EditorGUI.LabelField(new Rect(x, rect.y, pathWidth, lineHeight), record.FolderPathLabel());
                x += pathWidth + 4f;

                // StoreType popup
                float popupWidth = totalWidth * 0.15f;
                int currentIndex = record.StoreType == FavoriteStoreType.USER_LOCAL ? 0 : 1;
                int newIndex = EditorGUI.Popup(new Rect(x, rect.y, popupWidth, lineHeight), currentIndex, StoreTypeNames);
                record.StoreType = newIndex == 0 ? FavoriteStoreType.USER_LOCAL : FavoriteStoreType.PJ_GLOBAL;
                x += popupWidth + 4f;

                // Delete button (red, trash icon)
                float deleteWidth = 30f;
                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                var trashIcon = EditorGUIUtility.IconContent("TreeEditor.Trash");
                if (GUI.Button(new Rect(x, rect.y, deleteWidth, lineHeight), trashIcon))
                {
                    _editingRecords.RemoveAt(index);
                    BuildReorderableList();
                }
                GUI.backgroundColor = prevColor;
            };
        }

        private void OnGUI()
        {
            if (_model == null || _editingRecords == null)
            {
                EditorGUILayout.HelpBox("モデルが設定されていません。Load メニューから開いてください。", MessageType.Warning);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            _reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("保存して閉じる", GUILayout.Height(30)))
            {
                Save();
                Close();
            }
        }

        private void Save()
        {
            if (_model != null && _editingRecords != null)
            {
                _model.ReplaceAllAndSave(_editingRecords);
            }
        }

        private void OnDestroy()
        {
            Save();
        }
    }
}
