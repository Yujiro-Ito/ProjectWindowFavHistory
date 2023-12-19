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
        private const float uiMarginRight = 465f;

        private bool _isOneColumnViewMode;

        private Button _storeButton;
        private Button _loadButton;
        private TextField _aliasTextField;

        private int[] _selectingFolderInstanceIds;

        private string _aliasText = "";

        public ProjectWindowFavoriteView(EditorWindow projectWindow, ProjectWindowFavoriteModel model) {
            _projectWindow = projectWindow;
            _model = model;

            CreateButton();
            CreateInputField();
            RefreshButtons();
        }

        /// <summary>
        /// Undo/Redoボタンを作成する
        /// </summary>
        private void CreateButton()
        {

            const float loadButtonWidth = 40f;
            _loadButton = new Button(Load)
            {
                text = "Load",
                focusable = false,
                style = 
                {
                    width = loadButtonWidth,
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = uiMarginRight + loadButtonWidth / 2
                }
            };

            const float buttonWidth = 20f;
            _storeButton = new Button(Store)
            {
                text = "♥",
                focusable = false,
                style = 
                {
                    width = buttonWidth,
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = uiMarginRight + buttonWidth + loadButtonWidth
                }
            };

            _projectWindow.rootVisualElement.Add(_storeButton);
            _projectWindow.rootVisualElement.Add(_loadButton);
        }

        private void CreateInputField()
        {
            const float fieldWidth = 120;
            _aliasTextField = new TextField()
            {
                value = "",
                focusable = true,
                style = 
                {
                    width = fieldWidth,
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = uiMarginRight + fieldWidth * 0.75f
                },
            };
            _aliasTextField.RegisterValueChangedCallback(evt => OnAliasChanged(evt.newValue));

            _projectWindow.rootVisualElement.Add(_aliasTextField);
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

            UpdateSelectedFolder();
        }

        /// <summary>
        /// Undo/Redoボタンの状態を更新する
        /// </summary>
        private void RefreshButtons()
        {
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

            var record = new ProjectWindowFavoriteRecord(selectedFolderInstanceIds, _aliasText);
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
            menu.AddItem(new GUIContent("Open Memorized Json"), false, () =>
            {
                var path = _model.StoreDataPath();
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
