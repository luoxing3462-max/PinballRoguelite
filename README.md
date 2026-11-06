# PinballRoguelite

Unity 6.3 LTS 竖屏 2D 弹珠 Roguelite 可玩 Demo，参考 `F:\demo\ball\ball` 的现有 Godot 实现重新搭建。

## 运行入口

1. 用 Unity `6000.3.27f2` 打开本目录。
2. 打开 `Assets/Scenes/Main.unity`。
3. 点击 Play，从“开始冒险”进入完整流程。

设计分辨率为 780×1688；编辑器默认以 540×960 窗口调试。输入同时支持鼠标和触屏。

## 已实现

- 6～8 层随机向下爬塔地图，单入口、分裂/汇合路线和唯一 Boss。
- 普通、精英、事件、商店、休息、Boss 节点；非战斗节点为明确的基础占位流程。
- 右下固定发射口、右侧独立物理通道、顶部双轨弧形转向和中央碰撞区。
- 每次进入弹珠关卡都会生成新的安全随机碰撞柱排布：固定入场柱、22 个中央柱、3 个随机分裂柱，并保留柱间距及发射/倍率区域净空。
- 普通弹珠和只分裂一次的分裂弹珠；子弹珠统一结算。
- 碰撞去重计分、底部倍率、发射次数、卡死脱困、超时回收和失败发射返还。
- 底部 8 个结算槽每局打乱位置：2 个 ×1 原值槽、1 个 ×0 归零槽、5 个落入时随机抽取 ×2～×5 的倍率槽。
- 顶部战斗区复现 Godot 模式：左侧固定弹珠勇者、右侧关卡怪物、攻击弹道/伤害反馈，以及存活怪物按发射次数逐格逼近。
- 普通 140/5、精英 260/5、Boss 480/6 的目标和难度参数。
- 开始、地图、弹珠台、通关、失败重试和最终结算完整闭环。

## 暂未扩展

- 事件、商店、休息点保留基础占位逻辑。
- 未制作正式美术、遗物/技能树、联网或排行榜。

## WebGL 试玩构建

WebGL 构建与开发源码分离，输出位置固定为 `Builds/WebGL`。构建前需在 Unity Hub 为 Unity `6000.3.27f2` 安装 **WebGL Build Support** 模块；项目已经配置为 780×1688 竖屏、Brotli 压缩和浏览器端解压回退，适用于 GitHub Pages 静态托管。

1. 在 Unity 菜单选择 `Pinball Roguelite > Build WebGL For GitHub Pages`。
2. 构建完成后，用任意本地 HTTP 服务打开 `Builds/WebGL/index.html`；不要直接双击 HTML 文件。
3. 首次发布前建立 GitHub 仓库并启用 Pages 的 `GitHub Actions` 来源；仓库内的 `.github/workflows/deploy-pages.yml` 会发布 `Builds/WebGL`。
4. 更新试玩版时，先确认旧的 `Builds/WebGL` 可以被替换，再清空该发布目录、重新构建，并提交 `Builds/WebGL` 与相关源码改动。

`Builds/WebGL` 是唯一允许提交的构建目录；`.gitignore` 会排除 Unity 的 Library、Temp、Logs、Cache 等本机缓存。
