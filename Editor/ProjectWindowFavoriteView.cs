using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectWindowHistory
{
    public class ProjectWindowFavoriteView
    {
        private readonly EditorWindow _projectWindow;
        private readonly ProjectWindowFavoriteModel _model;
        private const float uiLeftBase = 100f; // History < > (56 + 20 + 4 + 20) の後ろ

        private bool _isOneColumnViewMode;

        private Button _storeButton;
        private Button _loadButton;
        private TextField _aliasTextField;

        private int[] _selectingFolderInstanceIds;

        private string _aliasText = "";
        private const float MIN_WINDOW_WIDTH_TO_SHOW_UI = 730f;

        public ProjectWindowFavoriteView(EditorWindow projectWindow, ProjectWindowFavoriteModel model) {
            _projectWindow = projectWindow;
            _model = model;

            CreateButton();
            RefreshButtons();
        }

        /// <summary>
        /// Undo/Redoボタンを作成する
        /// </summary>
        private void CreateButton()
        {
            const float buttonWidth = 20f;
            const float fieldWidth = 120f;
            float currentLeft = uiLeftBase;

            // 1. エイリアス TextField
            _aliasTextField = new TextField()
            {
                value = "",
                focusable = true,
                style =
                {
                    width = fieldWidth,
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = currentLeft
                },
            };
            _aliasTextField.RegisterValueChangedCallback(evt => OnAliasChanged(evt.newValue));
            currentLeft += fieldWidth + 4f;

            // 2. ♥ ボタン
            _storeButton = new Button(Store)
            {
                text = "♥",
                focusable = false,
                style =
                {
                    width = buttonWidth,
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = currentLeft
                }
            };
            currentLeft += buttonWidth + 4f;

            // 3. Load ボタン
            const float loadButtonWidth = 40f;
            _loadButton = new Button(Load)
            {
                text = "Load",
                focusable = false,
                style =
                {
                    width = loadButtonWidth,
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = currentLeft
                }
            };

            _projectWindow.rootVisualElement.Add(_aliasTextField);
            _projectWindow.rootVisualElement.Add(_storeButton);
            _projectWindow.rootVisualElement.Add(_loadButton);
        }

        public void OnUpdate()
        {
            // 1カラムビューかを取得
            _isOneColumnViewMode = ProjectWindowReflectionUtility.IsOneColumnViewMode(_projectWindow);
            if (_isOneColumnViewMode)
            {
                // 現状1カラムビューは未対応なので、ボタン状態の更新だけして終了
                RefreshButtons();
                return;
            }

            RefreshButtons();
            UpdateSelectedFolder();
        }

        /// <summary>
        /// Undo/Redoボタンの状態を更新する
        /// </summary>
        private void RefreshButtons()
        {
            bool isTooNarrow = _projectWindow.position.width < MIN_WINDOW_WIDTH_TO_SHOW_UI;
            bool hide = _isOneColumnViewMode || isTooNarrow;

            _aliasTextField.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;
            _storeButton.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;
            _loadButton.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;

            bool isStoreEnabled = !_model.HasRecord(_selectingFolderInstanceIds);
            _storeButton.SetEnabled(!_isOneColumnViewMode && isStoreEnabled);
        }

        private void Store()
        {
            var selectedFolderInstanceIds = ProjectWindowReflectionUtility.GetLastFolderInstanceIds(_projectWindow);
            if (selectedFolderInstanceIds == null || !selectedFolderInstanceIds.Any())
            {
                return;
            }

            var record = new ProjectWindowFavoriteRecord(selectedFolderInstanceIds, _aliasText, FavoriteStoreType.USER_LOCAL);
            _model.ApplyAndSave(record);
        }

        private void Load()
        {
            ShowFavoriteRecordListMenu();
        }

        private void ApplyFavoriteRecord(ProjectWindowFavoriteRecord record)
        {
            ProjectWindowReflectionUtility.SetFolderSelection(_projectWindow, record.SelectedFolderInstanceIDs);

            RefreshButtons();
        }

        private void UpdateSelectedFolder()
        {
            var selectedFolderInstanceIds = ProjectWindowReflectionUtility.GetLastFolderInstanceIds(_projectWindow);
            var isFolderSelected = selectedFolderInstanceIds != null && selectedFolderInstanceIds.Any();

            // ツリービューでフォルダが選択されていなければ何もしない
            if (!isFolderSelected)
            {
                return;
            }

            _selectingFolderInstanceIds = selectedFolderInstanceIds
                .Where(instanceId => AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(instanceId)))
                .ToArray();

            RefreshButtons();
        }
        private void ShowFavoriteRecordListMenu()
        {
            var menu = new GenericMenu();
            var recordList = _model.GetAllFavoriteRecords().ToList();
            foreach (var record in recordList)
            {
                menu.AddItem(new GUIContent(record.ToLabelText()), false, () =>
                {
                    ApplyFavoriteRecord(record);
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("設定変更"), false, () =>
            {
                ProjectWindowFavoriteEditorWindow.Open(_model);
            });
            menu.AddItem(new GUIContent("Open UserLocal Memorized Json"), false, () =>
            {
                var path = _model.StoreDataPath(_model.UserLocalStoreDataDirectory);
                System.Diagnostics.Process.Start(path);
            });
            menu.AddItem(new GUIContent("Reload Json"), false, () =>
            {
                _model.Reload();
            });

            menu.ShowAsContext();
        }

        private void OnAliasChanged(string alias)
        {
            _aliasText = alias;
        }

        public void Destroy()
        {
            _storeButton?.RemoveFromHierarchy();
            _loadButton?.RemoveFromHierarchy();
            _aliasTextField?.RemoveFromHierarchy();
        }
    }
}
