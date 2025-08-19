# WWWunderkammer VR Menu Package (Unity)
- FollowFrontMenu（前方跟随主菜单，3×2 场景网格）
- WristMenuAnchor（腕表快捷菜单，2×2 常用/最近）
- MenuInputRouter（短按/长按/双击）
- SceneMenuController（自动/手动填充场景，点击跳转）
- CreateWWWKMenuWindow（Tools 菜单一键生成 Prefab 与装配）

## 使用
1) 将此文件夹置于 `Assets/WWWK_VR_Menu_Package/`。
2) Unity 顶部菜单：**Tools ▸ WWWunderkammer ▸ Create VR Menus**。
3) 在 `WWWK_MenuRig`：
   - `MenuInputRouter.menuAction` 绑定 `<XRController>{LeftHand}/menuButton`。
   - 在 `WWWK_WristMenu` 里的 **RightHand** 指向你的右手控制器 Transform。
4) 在 `SceneMenuController` 上 **从 Build Settings 自动填充**，或手动填写显示名/场景名/缩略图。
5) 运行：短按 Menu → 主菜单；长按 Menu → 腕表；扳机选择。