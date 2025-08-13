
WWWK UI Sprites v1 (matching deep purple + gold rounded style)
================================================================

**Import建议（Unity）**
1. Texture Type: Sprite (2D and UI)
2. Mesh Type: Full Rect
3. 9-slice Border 像素：
   - panel_bg_round_512.png:      56,56,56,56
   - card_bg_round_384x256.png:   40,40,40,40
   - thumb_slot_round_320x180.png:20,20,20,20
   - header_bar_round_1024x192.png:28,28,28,28
   - tooltip_bubble_512x160.png:  24,24,24,24

**使用建议**
- 大面板：用 panel_bg_round 做父 Image（Sliced），内边距 60px 左右。
- 场景按钮卡片：card_bg_round（Sliced），上方放 thumb_slot_round（保持 16:9/约 420×220）；下方用 TMP 放标题。
- 悬停高亮：叠一层 card_outline_only（Image Type = Simple），或改 Color = 金色 + 乘法混合（URP 可用渲染特效）。
- 阴影：在同级下放置 shadow_xxx 精灵，稍微放大一点并在背后。
- 居中标题条/腕表头：header_bar_round。
- 提示气泡：tooltip_bubble（文字左右各留 12–16px）。

颜色参考见 palette_preview.png。

