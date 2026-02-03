# ChangeLog

## [1.1.0] - 2026-02-03
### Added
- Favorite 設定変更 EditorWindow を追加（Load メニュー内「設定変更」から開く）
  - エイリアス名の編集
  - StoreType（USER_LOCAL / PJ_GLOBAL）の切り替え
  - レコードの削除
  - ドラッグ＆ドロップによる並び替え
  - 保存して閉じる / ウィンドウを閉じると自動保存
- Project ウィンドウが一定幅より狭い場合、カスタム UI を自動で非表示にする機能を追加

### Changed
- カスタム UI の配置を右端基準（right）から左端基準（left）に変更し、検索フィールドとの重なりを解消
- UI の並び順を変更: `< >` → エイリアス入力 → ♥ → Load

### Fixed
- StoreToLocalJson / StoreToGlobalJson で引数の records ではなく _records を使っていたバグを修正

## [1.0.1] - 2024-xx-xx
### Added
- Favorite（ブックマーク）機能を追加
  - Project ウィンドウのフォルダ選択状態を ♥ ボタンで保存
  - Load ボタンからお気に入り一覧を表示し、選択でフォルダ状態を復元
  - エイリアス名の入力に対応
  - USER_LOCAL / PJ_GLOBAL の2種類の保存先に対応（ローカル JSON / StreamingAssets）
- 右クリックで Undo / Redo 履歴一覧を表示する機能を追加
- 履歴の一括クリア機能を追加

## [1.0.0] - 2023-12-02
### first release
