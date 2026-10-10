<div align="center">

# Tune

**像调音师或 DJ 一样播放音乐的专业桌面播放器**

![Platform](https://img.shields.io/badge/platform-Windows-0078D4?style=flat-square)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square)
![UI](https://img.shields.io/badge/UI-WPF-68217A?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
![Status](https://img.shields.io/badge/status-a1%20early%20preview-orange?style=flat-square)

**简体中文** · [English summary](#english-summary) · [English draft](Tune-en.md)

</div>

> Tune 是一个基于 **WPF / .NET 10** 的 Windows 桌面音乐播放器。它不满足于"能放歌"，
> 而是把播放器做成一台可以随手操作的调音台：虚拟旋钮、可拖拽时间轴、双语歌词、
> 歌单数据库，以及正在建设中的均衡器、效果链与声学分析工具。
>
> 项目目前处于 **a1 早期预览** 阶段：核心播放链路与歌单系统已经可用，
> 声效工具区完成界面骨架，逻辑仍在开发中。

---

## 目录

- [项目简介](#项目简介)
- [功能与进度](#功能与进度)
- [界面概览](#界面概览)
- [平台与运行要求](#平台与运行要求)
- [快速开始](#快速开始)
- [数据与文件位置](#数据与文件位置)
- [项目结构](#项目结构)
- [架构说明](#架构说明)
- [技术栈与第三方库](#技术栈与第三方库)
- [本地化](#本地化)
- [贡献指南](#贡献指南)
- [已知问题](#已知问题)
- [相关文档](#相关文档)
- [许可证](#许可证)
- [联系方式](#联系方式)
- [English summary](#english-summary)

---

## 项目简介

Tune 的目标是把"播放音乐"变成一件可以**调**的事：

- 用真实的旋钮控件（`AirKnob`）而不是滑块来调音量与效果参数；
- 在主界面同屏显示歌曲信息、采样率/码率、双行（原文 + 译文）歌词与时间轴；
- 以"歌单库（Tune Database）"的方式管理本地曲库，数据以可读的 JSON 落盘；
- 用可拆卸的"声效工具"页承载均衡器、效果器、声学分析与曲目详情。

项目全部使用 C# 与 XAML 编写，UI 采用固定 1600 × 792 设计画布 + `Viewbox` 等比缩放，
窗口宽高比锁定为 **1.945**，因此从 800 × 400 到 4K 都能保持一致的版式。

---

## 功能与进度

状态说明：✅ 已实现并可用 · 🚧 界面已就绪、逻辑开发中 · 📋 规划中

### 播放与曲目

| 功能 | 状态 | 说明 |
|---|---|---|
| 本地音频播放 | ✅ | `NAudio` `WaveOut` + `AudioFileReader` |
| 多格式支持 | ✅ | 打开对话框筛选 `*.mp3;*.wav;*.flac;*.m4a`；WAV 由 NAudio 内置解码，MP3 / FLAC / M4A(AAC) 等经 Windows Media Foundation 解码 |
| 播放 / 暂停 / 停止 | ✅ | 引擎层 `Play` / `Pause` / `Stop` / `Unload`，自然播放结束才触发"播放到结尾"事件 |
| 进度拖拽跳转 | ✅ | 拖动时暂停、释放后修正位置并续播；进度以 50 ms 定时刷新 |
| 音量控制 | ✅ | 旋钮 0–100 ↔ 引擎 0.0–1.0，拖动结束自动持久化 |
| 元数据读取 | ✅ | `TagLib#` 读标题/艺术家/专辑；缺失码率时用 NAudio 兜底，另取采样率与声道数 |
| 歌词显示 | ✅ | 自动加载同名 `.lrc`；按时间二分查找当前行 |
| 双语歌词 | ✅ | 同一时间戳含多行时自动识别原文与译文并分行显示 |

### 歌单与播放模式

| 功能 | 状态 | 说明 |
|---|---|---|
| 歌单创建 / 选择 | ✅ | `CreatePlaylist` / `SelectPlaylist` 对话框（含"无"选项） |
| 收藏当前曲目 | 🚧 | `FavoriteMusic` 对话框：改选"其他歌单"时会正确把曲目加入歌单，但直接确认已预选的歌单时漏调 `AddMuisc`，曲目不会入库（疑似缺陷） |
| 添加 / 删除曲目 | ✅ | `AddMusic` 对话框可从"正在播放"或本地文件选取 |
| 歌单持久化 | ✅ | 每个歌单一个 `playlists/<ID>.json`，`MusicList` 仅保存曲目路径 |
| 播放模式 | ✅ | 不循环 / 列表循环 / 单曲循环 / 随机播放，图标随模式切换 |
| 上一首 / 下一首 | ✅ | 基于播放历史栈，连续重复曲目不重复入栈 |
| 重命名 / 更新 / 删除歌单 | 🚧 | 歌单页（`CurrentPlaylist`）的「播放 / 重命名 / 更新 / 删除」按钮已排布，尚未接线 |
| 曲目条目操作 | 🚧 | 「下一首播放 / 加入歌单 / 更多信息」为 `TODO`（删除已实现） |
| 目录歌单（导入文件夹） | 📋 | `EPlaylistType.Directory` 已定义，导入按钮为 `TODO` |

### 声效工具

| 功能 | 状态 | 说明 |
|---|---|---|
| 工具页导航 | ✅ | 帮助 / 均衡器 / 效果器 / 分析 / 歌曲，带独立 ServiceProvider 与页面复用 |
| 均衡器、效果器、分析、歌曲、帮助页 | 🚧 | 当前为壁纸占位页 |
| 效果链（`EffectChain`） | 🚧 | 静态原型：左信号出入、右"未选择效果 + 参数面板" |
| 8 段图形均衡器与 EQ 导入导出 | 📋 | 见 [Tune.md](Tune.md) |
| 声学 / 乐理解析（BPM、调性、副歌） | 📋 | — |
| Demucs ONNX 分轨与重混 | 📋 | 方案见 [Demucs_RemixEngine_ImportSolution.md](Demucs_RemixEngine_ImportSolution.md) |
| 插件系统 | 📋 | — |

### 基础设施

| 功能 | 状态 | 说明 |
|---|---|---|
| 依赖注入 | ✅ | `Microsoft.Extensions.DependencyInjection`，服务/控制器/Presenter/页面统一注册 |
| 启动期异步服务加载 | ✅ | `LoadableServicesManager` 并行加载并统计进度，单个失败不影响其他服务 |
| 页面导航 | ✅ | `WeakReferenceMessenger` + `NavigationMessage`，弱引用避免内存泄漏 |
| 自定义对话框宿主 | ✅ | `DialogContentBase` / `DialogHost` / `DialogService`，支持 `OnShown` 生命周期 |
| 音频设备热切换 | ✅ | `MMDeviceEnumerator` 通知客户端，实时更新页脚设备名 |
| 日志 | ✅ | 每次运行一个 `logs/log_<时间戳>.txt`，保留 5 天、最多 100 个文件 |
| 全局异常兜底 | ✅ | 启动失败弹窗提示日志路径并安全退出 |
| 本地化 | ✅ | `.resx` + `WPFLocalizeExtension`，内置 English 与 zh-CN |
| 统一提示（Toast） | 🚧 | 目前仍使用 `MessageBox`，见 [Problems.md](Problems.md) |

---

## 界面概览

| 页面 | 类 | 说明 |
|---|---|---|
| 主界面 | `Appearance/Main` | 歌词与控制区、音量/EQ 旋钮、曲目信息、加载按钮、时间轴、循环/收藏/列表/声效/设置入口，页脚显示声卡名与时钟 |
| 当前歌单 | `Appearance/CurrentPlaylist` | 歌单名、描述、类型、曲目数与曲目列表（含悬停操作按钮） |
| 歌单库 | `Appearance/PlaylistsPage` | "Tune Database"卡片式歌单总览与播放 |
| 声效工具 | `Appearance/SoundEffect` | 顶栏工具导航 + 迷你时间轴 + 播放/暂停，内容区承载各工具页 |
| 对话框 | `Components/UI` | 添加音乐、收藏音乐、新建歌单、选择歌单 |

设计细节：全局字体为内置的 **Rajdhani**（`Assets/Rajdhani`），主色为黑白极简风格，
自定义控件包括 `AirKnob`（带阻尼、动画与设置持久化的扇形旋钮）、`Timeline`（可拖拽进度条）、
`Pause`、`SwitchMusic`、`RotatingBox`、`ImageButton`、`ExitButton` 等。

---

## 平台与运行要求

| 项目 | 要求 |
|---|---|
| 操作系统 | Windows（`net10.0-windows`，WPF 仅支持 Windows） |
| 运行时 / SDK | **.NET 10 SDK**（开发验证版本：`10.0.401`）；发布包为非自包含，目标机需安装 .NET 10 桌面运行时 |
| IDE（可选） | Visual Studio 2022 17.14+ 或 JetBrains Rider 2025.x（需支持 .NET 10 与 WPF） |
| 声卡 | 建议支持 WASAPI / DirectSound，部分功能（如设备热切换、未来的低延迟效果）依赖它 |
| 解码器 | 除 WAV 外的格式（MP3 / FLAC / M4A 等）依赖系统 **Media Foundation**；Windows 10/11 已内置 FLAC 与 AAC 解码，精简或服务器版系统可能缺失 |
| 显示 | 800 × 400 起，版式按 1600 × 792 设计并等比缩放 |

> **注意**：项目文件 `Tune/Tune.csproj` 的 `TargetFramework` 为 `net10.0-windows`，
> 使用 .NET 9 及更早的 SDK 无法构建。

---

## 快速开始

### 1. 获取源码

```bash
git clone https://github.com/f13T2ach/Tune.git
cd Tune
```

仓库根目录（即 `Tune.slnx` 所在目录）结构为 `Tune.slnx` + `Tune/`（主工程）+ 文档。

### 2. 还原与构建

```bash
dotnet restore Tune.slnx
dotnet build Tune.slnx -c Release
```

也可以直接构建工程：

```bash
dotnet build Tune/Tune.csproj -c Debug
```

### 3. 运行

```bash
dotnet run --project Tune/Tune.csproj
```

或者用 Visual Studio / Rider 打开 `Tune.slnx` 后直接 F5 运行。

### 4. 发布

```bash
dotnet publish Tune/Tune.csproj -c Release -p:PublishProfile=FolderProfile
```

输出目录为 `Tune/bin/Release/net10.0-windows/publish/`（`FolderProfile` 为非自包含发布，
目标机需要 .NET 10 桌面运行时）。

### 5. 第一次使用

1. 点击主界面 **LOAD** 按钮选择一个音频文件并开始播放；
2. 把音频同目录下同名的 `.lrc` 文件放到一起，即可自动显示（双语）歌词；
3. 点击右下角显示当前歌单名的按钮进入 **歌单库**，新建歌单；
   点击 **列表** 进入 **当前歌单** 页，用 `+` 添加曲目、用曲目行上的删除按钮移除曲目；
4. 拖动 `VOL` 旋钮调音量，拖动时间轴跳转，**循环** 按钮切换播放模式，**声效** 进入声效工具页。

---

## 数据与文件位置

Tune 不使用注册表或安装目录写入业务数据，而是在**进程的当前工作目录**下创建两个文件夹：

```
<工作目录>/
├── playlists/                 # 歌单数据，每个歌单一个 <ID>.json
│   └── <GUID>.json
└── logs/                      # 运行日志，每次启动一个文件
    └── log_2026-10-05_14-53-46.txt
```

- 工作目录由 `Directory.GetCurrentDirectory()` 决定，也就是**启动进程时所在的目录**：
  - 命令行 `dotnet run --project Tune/Tune.csproj`：为你执行命令时所在的目录
    （在仓库根目录执行，数据就落在仓库根目录下）；
  - 从 Visual Studio / Rider 调试启动：通常为输出目录
    `Tune/bin/<Configuration>/net10.0-windows/`；
  - 双击发布后的 `Tune.exe`：为 `Tune.exe` 所在目录。
- 因此换一种启动方式就会换一份歌单数据。请固定用一种方式启动，或把 `playlists/` 目录一并迁移。
- 歌单 JSON 由 `Newtonsoft.Json` 写入，字段包括 `ID`、`Name`、`Description`、`Type`、
  `IsDeleted`、`isEmptyOrNotInited`、`SourceFrom` 与 `MusicList`（仅存曲目路径，读取时重新解析元数据）。
- 日志保留策略：最多 5 天、最多 100 个文件，每天首次写入时清理一次。
- 用户设置（`PlayerVolume`，默认 `50`）通过 `Local/TuneSettings.settings` 写入用户级
  `user.config`，按 Windows 用户账户隔离，不会污染仓库。

> 歌词文件规则：与音频文件同目录、同文件名、扩展名为 `.lrc`（大小写不敏感）。

---

## 项目结构

```
Tune/                                     # 仓库根目录
├── Tune.slnx                             # 解决方案（.NET 新版解决方案格式）
├── LICENSE.txt                           # MIT 许可证
├── README.md                             # 本文件
├── Tune.md / Tune-en.md                  # 项目说明草稿（中文 / English）
├── Problems.md                           # 未解决问题记录
├── Prompts.md                            # AI 辅助模块的提示词存档
├── Demucs_RemixEngine_ImportSolution.md  # Demucs 分轨 / 重混方案
└── Tune/                                 # 主工程（Tune.csproj）
    ├── App.xaml(.cs)                     # 应用入口：环境初始化 → DI → 启动期服务加载
    ├── MainWindow.xaml(.cs)              # 壳窗口：ContentControl + 导航消息
    ├── Appearance/                       # 页面
    │   ├── Main / CurrentPlaylist / PlaylistsPage / SoundEffect
    │   ├── SoundEffectTools/             # 帮助、均衡器、效果器、分析、歌曲页
    │   └── EffectToolsUI/                # 效果链界面原型
    ├── Components/                       # 自定义控件
    │   ├── Slider/  Bar/  Button/  TabFrame/  Frame/  InputBox/
    │   ├── Content/                      # 曲目项、歌单项
    │   └── UI/                           # 对话框内容
    ├── Themes/                           # Generic.xaml、TabFrameStyles.xaml
    ├── Behaviors/                        # WindowBehavior（窗口宽高比 / 最小尺寸）
    ├── Engine/                           # IAudioPlayerEngine 与 NAudio 实现
    ├── Client/
    │   ├── Controllers/                  # PlayerController、ListController、MusicPlaybackController
    │   ├── Presenters/                   # MusicDisplayPresenter、MusicLyricPresenter
    │   └── Logger/                       # 文件日志
    ├── ViewModel/                        # 页面级 ViewModel
    ├── Models/                           # Music、MusicMetadata、歌词、歌单、结果类型、对话框契约
    ├── Persistence/                      # PlaylistProvider（IO）+ PlaylistService（缓存）
    ├── Services/                         # 时间、音频设备、文件对话框、可加载服务管理
    ├── Local/                            # TuneSettings 与本地设置读写
    ├── Helpers/                          # 值转换器、JSON 转换、路径/字符串/日志助手
    ├── Generic/                          # 对话框泛型基类
    ├── Utils/                            # 环境目录、文件 IO、元数据解析
    ├── Values/                           # 路径、字符串、枚举、配置常量
    ├── Assets/                           # 图片、Rajdhani 字体、本地化 .resx
    └── Properties/                       # 发布配置（FolderProfile.pubxml）
```

---

## 架构说明

Tune 采用**分层 + 依赖注入**的结构，View 与业务逻辑之间通过 ViewModel / Presenter 解耦：

```
    ┌─ View (XAML) ────────────────────────────────────────────────
    │   Main · CurrentPlaylist · PlaylistsPage · SoundEffect · Dialogs
    └───────────────┬───────────────────────────────────────────────
                    │ 数据绑定 / Command
    ┌───────────────▼───────────────────────────────────────────────
    │   ViewModel：MainViewModel · CurrentPlaylistViewModel · PlaylistsPageViewModel …
    │   Presenter：MusicDisplayPresenter · MusicLyricPresenter
    └───────────────┬───────────────────────────────────────────────
                    │
    ┌───────────────▼───────────────────────────────────────────────
    │   Controller
    │     PlayerController         引擎门面：播放 / 暂停 / 跳转 / 音量映射
    │     ListController           歌单与播放模式：顺序、历史、随机
    │     MusicPlaybackController  换曲总调度：加载文件 + 刷新展示 + 重置歌词
    └───────────────┬───────────────────────────────────────────────
                    │ IAudioPlayerEngine
    ┌───────────────▼───────────────────────────────────────────────
    │   Engine：NAudio（WaveOut + AudioFileReader）+ 50 ms 进度定时器
    └───────────────────────────────────────────────────────────────
```

几个关键约定：

1. **换曲只有一个入口**：所有需要切换曲目的地方都调用
   `MusicPlaybackController.PlayMusic(Music)`，它负责统一完成"加载引擎 → 更新元数据展示 →
   重置歌词 → 同步 `ListController`"，避免各页面各写一套。
2. **`CurrentMusic` 是单一事实来源**：由 `MusicPlaybackController` 持有，
   各 ViewModel 只订阅其变更，不各自保存副本。
3. **引擎事件只表达事实**：`PlaybackEnded` 仅在**自然播放到结尾**时触发，
   用户按停止不会触发，因此"自动下一首"不会误动作。
4. **导航与对话框都走统一通道**：导航用 `WeakReferenceMessenger` + `NavigationMessage`；
   对话框用 `DialogContentBase` 子类 + `DialogService.ShowDialog()` 扩展方法，
   业务代码只需 `dlg.ShowDialog()` 后读取 `dlg.Result`。
5. **持久化分两层**：`PlaylistProvider` 只管文件读写（带锁、可批量、逐项容错），
   `PlaylistService` 实现 `IAsyncLoadable`，维护内存缓存（`ConcurrentDictionary`）
   与 UI 可绑定的 `ObservableCollection`，并在 UI 线程上派发更新。

启动流程（`App.OnStartup`）：

```
初始化工作目录（playlists/、logs/）
  → 构建 DI 容器（服务、控制器、Presenter、页面、ViewModel）
  → 注册可加载服务（PlaylistService）
  → 并行 await LoadableServicesManager.LoadAllAsync()
  → 进入 MainWindow（默认导航到 Main 页面）
```

---

## 技术栈与第三方库

下表版本与许可证取自 `Tune.csproj` 及 NuGet 包元数据：

| 依赖 | 版本 | 用途 | 许可证 |
|---|---|---|---|
| [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm) | 8.4.2 | MVVM 源生成器：`ObservableObject`、`[ObservableProperty]`、`[RelayCommand]`、Messenger | MIT |
| [NAudio](https://www.nuget.org/packages/NAudio) | 3.0.1 | 音频播放、解码、设备枚举与通知 | MIT |
| [TagLibSharp](https://www.nuget.org/packages/TagLibSharp) | 2.3.0 | 音频文件标签（标题 / 艺术家 / 专辑）解析 | LGPL-2.1-only |
| [DevBase.Format](https://www.nuget.org/packages/DevBase.Format) | 1.7.3 | LRC 歌词解析（`FileParser<LrcParser, …>`） | MIT |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) | 13.0.4 | 歌单 JSON 序列化与反序列化 | MIT |
| [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) | 10.0.10 | 依赖注入容器 | MIT |
| [Microsoft.Xaml.Behaviors.Wpf](https://www.nuget.org/packages/Microsoft.Xaml.Behaviors.Wpf) | 1.1.142 | XAML 交互触发器（如时间轴拖动事件转命令） | MIT |
| [WPFLocalizeExtension](https://www.nuget.org/packages/WPFLocalizeExtension) | 3.10.0 | WPF 本地化（`.resx` 绑定） | Ms-PL |
| [XAMLMarkupExtensions](https://www.nuget.org/packages/XAMLMarkupExtensions) | 2.1.3（传递依赖） | `WPFLocalizeExtension` 的标记扩展基础库 | Ms-PL |

Tune 会严格遵守上述开源许可协议。若需分发，请注意 **TagLib# 为 LGPL-2.1-only**：
本项目以未修改的 NuGet 程序集形式引用它，请勿静态链接或修改其源码后闭源分发。

---

## 本地化

- 资源文件位于 `Tune/Assets/Localization/`：
  - `LangStrings.resx` —— 中性资源（English，默认回退）；
  - `LangStrings.zh-CN.resx` —— 简体中文；
  - `LangStrings.Designer.cs` —— 强类型访问器（`LangStrings.xxx`）。
- XAML 中通过 `xmlns:lang="clr-namespace:Tune.Assets.Localization"` 与
  `{x:Static lang:LangStrings.XXX}` 引用。
- **新增语言的步骤**：复制 `LangStrings.resx` 为 `LangStrings.<culture>.resx`，
  翻译其中的 `<data>` 条目，界面语言跟随系统区域设置生效。
- 当前 `App.OnStartup` 中手动切换 `CurrentUICulture` 的代码处于注释状态，
  界面语言跟随系统；同时仍有部分文案直接硬编码在 XAML 中（如主界面的
  "循环 / 收藏 / 列表 / 声效 / 设置"），迁移到资源文件是欢迎的贡献方向。

---

## 贡献指南

欢迎任何形式的贡献：报告问题、提出功能建议、完善文档、提交代码。

1. Fork 本仓库；
2. 创建功能分支：`git checkout -b feature/your-feature-name`；
3. 提交更改：`git commit -m 'your-commit-description'`（请遵循 Commit 规范，说明"做了什么/为什么"）；
4. 推送到分支：`git push origin feature/your-feature-name`；
5. 打开 Pull Request。

提交前请确认：

- 代码风格与现有代码保持一致（文件头 `#region`/`====` 分区注释、XML 文档注释、可空引用类型标注）；
- 不引入新的编译警告（当前 `Release` 构建为 0 错误、6 个已知警告）；
- 不破坏既有功能，并为新增逻辑补充必要的单元测试；
- 大范围重构请先在 Issue 中讨论。

### 常见的开发任务

| 任务 | 切入点 |
|---|---|
| 新增页面 | 在 `Appearance/` 下添加 `UserControl`，在 `App.OnStartup` 注册为服务，用 `WeakReferenceMessenger.Send(new NavigationMessage(typeof(YourPage)))` 导航 |
| 新增自定义控件 | 放到 `Components/<分类>/`，如需主题样式写入 `Themes/Generic.xaml` |
| 新增对话框 | 继承 `DialogContentBase`，实现 `OnShown()` 与结果属性，用 `this.ShowDialog()` 调用 |
| 接入新的持久化数据 | 参考 `Persistence/PlaylistStorage/`，实现 `IAsyncLoadable` 并注册到 `LoadableServicesManager` |
| 新增可绑定设置 | 在 `Local/TuneSettings.settings` 增加 `User` 作用域项，用 `LocalProperties` 或 `TuneSettings.Default` 读写 |

> 目前仓库中**尚无单元测试工程**，`Tune.slnx` 仅包含主工程。
> 搭建测试项目（如 xUnit + 针对 `ListController`、歌词解析、歌单序列化的测试）是很好的第一个贡献。

---

## 已知问题

- **声效工具页尚未实现**：均衡器、效果器、分析、歌曲页目前为壁纸占位，效果链为静态原型。
- **提示方式突兀**：多处仍使用 `MessageBox`，计划替换为应用内提示（见 [Problems.md](Problems.md) 的 `SHOW_TOAST`）。
- **`WindowFrame` 作用域内命名冲突**：在 `WindowFrame` 内嵌套多个具名元素会触发 XAML 编译错误（见 `NAME_REPEAT_PROBLEM`）。
- **数据目录跟随工作目录**：从不同目录启动会得到不同的歌单/日志目录，正式分发前建议改为固定的用户数据目录（如 `%APPDATA%/Tune`）。
- **待接线按钮**：主界面的"设置"，歌单页的"播放 / 重命名 / 更新 / 删除"，曲目项的"下一首播放 / 加入歌单 / 更多信息"。
- **收藏功能不完整**：`FavoriteMusic` 在"已预选歌单 + 直接确认"路径上漏调 `PlaylistBase.AddMuisc`，
  曲目不会被真正加入歌单；只有改选"其他歌单"的路径是完整的（见 `Components/UI/FavoriteMusic.xaml.cs`）。
- **许可证年份与作者未填写**：`LICENSE.txt` 中的 `[year] [fullname]` 仍为占位符。

---

## 相关文档

| 文档 | 内容 |
|---|---|
| [Tune.md](Tune.md) | 项目说明草稿（中文）：定位、功能、构建与贡献 |
| [Tune-en.md](Tune-en.md) | 项目说明草稿（English） |
| [Problems.md](Problems.md) | 开发过程中未解决的问题记录（含 TAG、位置、重要性与类别） |
| [Prompts.md](Prompts.md) | AI 辅助实现模块的提示词存档 |
| [Demucs_RemixEngine_ImportSolution.md](Demucs_RemixEngine_ImportSolution.md) | Demucs ONNX 分轨 / 重混引擎的导入方案 |

---

## 许可证

本项目采用 **MIT** 许可证，详情请参阅 [LICENSE.txt](LICENSE.txt)。

---

## 联系方式

| 渠道 | 地址 |
|---|---|
| 仓库 | <https://github.com/f13T2ach/Tune> |
| 问题反馈 | <https://github.com/f13T2ach/Tune/issues> |
| 邮箱 | f12fds@163.com |

---

## English summary

**Tune** is a professional desktop music player for Windows, built with **WPF on .NET 10**.
It aims to make listening feel like operating a mixing console: real knob controls, a draggable
seek bar, bilingual (original + translated) LRC lyrics, and a JSON-backed playlist database.

- **Stack** — C# / XAML, `net10.0-windows`; MVVM via CommunityToolkit.Mvvm, DI via
  Microsoft.Extensions.DependencyInjection, playback via NAudio, tags via TagLib#,
  lyrics via DevBase.Format, storage via Newtonsoft.Json, localization via WPFLocalizeExtension.
- **Architecture** — `View → ViewModel/Presenter → Controller → Engine`, plus services
  (time, audio device, file dialog, loadable-service manager) and a two-layer playlist
  persistence stack (`PlaylistProvider` for IO, `PlaylistService` for caching).
- **Working today** — playback with metadata and bilingual lyrics, seek/volume, playlist
  create/select/favorite/add/delete with persistence, four play modes with play history,
  device hot-swap display, file logging, and page navigation.
- **In progress** — the sound-effect tools (equalizer, effects, analysis, song info) are UI
  placeholders; several buttons are not wired up yet.
- **Build & run**

  ```bash
  git clone https://github.com/f13T2ach/Tune.git
  cd Tune
  dotnet restore Tune.slnx
  dotnet build Tune.slnx -c Release
  dotnet run --project Tune/Tune.csproj
  ```

- **Data** — `playlists/*.json` and `logs/log_*.txt` are created in the process working
  directory; user settings (volume) live in the per-user `user.config`.
- **License** — MIT. See [LICENSE.txt](LICENSE.txt).

For the full English draft, see [Tune-en.md](Tune-en.md).
