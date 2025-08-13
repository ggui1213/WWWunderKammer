
# WWWunderkammer · VR 场景菜单（前方跟随 + 腕表快捷）

**短按 Menu**：前方跟随主菜单（3×2 网格）。  
**长按 Menu（>0.6s）**：腕表快捷菜单（2×2），松手关闭。

## 目录
- `Assets/WWWunderkammerMenu/Scripts`：脚本
- `Assets/WWWunderkammerMenu/UI/Sprites`：九宫格精灵
- `Assets/WWWunderkammerMenu/Resources/WWWK_Scenes.json`：示例场景清单

## 快速接入
1. 场景内有 `EventSystem`（`XR UI Input Module`）。
2. 建立 **主菜单 Canvas（World Space）**：加 `CanvasGroup`、`TrackedDeviceGraphicRaycaster`，底板用 `panel_bg_9s.png`，内部做 3×2 按钮（每个按钮包含 `Thumb` 和 `Title`）。
3. Canvas 上挂：`FollowFrontMenu`（绑 `menuAction` 与 `CanvasGroup`）、`SceneMenuController`（绑 `gridParent` 和 `sceneCardPrefab`，可填 `sceneListJson`）。
4. **腕表菜单**：复制一个小 Canvas 为 2×2，挂 `WristMenuAnchor`（右手 Transform + CanvasGroup）。
5. 任意空物体挂 `MenuInputRouter` + `MenuGlue`，把引用连好。
6. `Build Settings` 中加入各场景，并与 JSON/Inspector 名称一致。

> 如需信息台/传送门，参考 `ScenePortal.cs` 在世界中放交互门点。

（首次导入后，Unity 会自动生成 .meta 与 GUID）
