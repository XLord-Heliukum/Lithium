# Lithium

> A modern Wine wrapper for Anywhere — 用 C# / .NET / Avalonia 重写的跨平台 Wine 管理器。

Lithium 是 [Whisky](https://github.com/Whisky-App/Whisky)（原 macOS 专属的 SwiftUI Wine 包装器）的**派生产品与跨平台重写版**。
原项目只支持 macOS，Lithium 改用 **C# + .NET 10 + Avalonia 12** 实现，可在 **macOS / Linux / Windows** 上运行，
沿用 Whisky 的核心概念（Bottle = Wine 前缀、Program、Engine），并把平台相关的 Wine 工具链抽象成可替换的「后端」。

---

## ⚠️ 许可与署名

Lithium 基于 Whisky 重写，属于其**衍生作品（derivative work）**，因此：

- **许可证：GPL-3.0**（与 Whisky 一致，强 copyleft）。
- 上游来源：[`Whisky-App/Whisky`](https://github.com/Whisky-App/Whisky)（GPL-3.0）。
- 若你分发 Lithium 的二进制或修改后版本，**必须**以 GPL-3.0 开源，并保留本署名。

---

## 它和 Whisky 的关系

| 维度 | Whisky（上游） | Lithium（本仓库） |
| --- | --- | --- |
| 语言 / 框架 | Swift / SwiftUI | C# / .NET 10 / Avalonia 12 |
| 平台 | 仅 macOS | macOS / Linux / Windows |
| Wine 工具链 | 内置 GPTK / Wine（macOS） | 可插拔后端（见下） |
| 形态 | GUI App | GUI（Avalonia）+ CLI（`lithium`） |

> 代码结构为全新编写，未复制上游 Swift 源码，仅继承其架构理念与 GPL-3.0 许可义务。

---

## 架构

```
Lithium.slnx
├── src/LithiumKit/       领域模型 + 跨平台服务（无 UI 依赖，可被任意宿主复用）
│   ├── Models/           Bottle / BottleConfig / BottleProgram / WineEngine / BottleState
│   ├── Services/         BottleManager、WineRunner、EngineManager、SettingsManager、
│   │                     DownloadManager、IPlatformBackend + macOS/Linux/Windows 实现
│   └── Constants/        LithiumConstants（项目名、上游署名、仓库地址）
├── src/Lithium.Desktop/  Avalonia 跨平台 GUI（Fluent 主题 + MVVM）
│   ├── Views/            MainWindow / NewBottleDialog / SettingsWindow / AboutWindow / AddProgramDialog
│   └── ViewModels/       基于 CommunityToolkit.Mvvm
└── src/Lithium.CLI/      命令行工具 `lithium`（对应 WhiskyCmd 的跨平台版）
```

**核心抽象 —— 平台后端（IPlatformBackend）**

| 后端 | 平台 | 发现到的 Wine 工具链 |
| --- | --- | --- |
| `MacOSBackend` | macOS | 系统 `wine`、Apple Game Porting Toolkit、CrossOver |
| `LinuxBackend` | Linux | 系统 `wine`、Proton、Steam Proton |
| `WindowsBackend` | Windows | PATH 中的 `wine`、本地 Engines 目录 |

后端负责：定位 `wine`/`wine64` 可执行文件、构造运行环境（`WINEPREFIX`、`PATH`、Esync、HiDPI 等）、发现可用引擎。

---

## 构建

需要 **.NET 10 SDK**（已在 `global.json` 中固定为 `10.0.400`）。

```bash
dotnet restore Lithium.slnx
dotnet build  Lithium.slnx -c Debug      # 或 -c Release
```

> 提示：在 Apple Silicon 上使用 CodeNet 版 .NET 构建大型解决方案可能不稳定，建议用官方 x64 SDK：
> `export PATH="$HOME/dotnet11x64:$PATH"`

### 运行

GUI（桌面）：

```bash
dotnet run --project src/Lithium.Desktop
```

命令行（跨平台，无需 GUI）：

```bash
dotnet run --project src/Lithium.CLI -- about
dotnet run --project src/Lithium.CLI -- list-engines
dotnet run --project src/Lithium.CLI -- create "My Game"
dotnet run --project src/Lithium.CLI -- list
dotnet run --project src/Lithium.CLI -- run "My Game" "C:\\Program Files\\App\\game.exe"
dotnet run --project src/Lithium.CLI -- delete "My Game"
```

瓶子默认存放在：

- macOS：`~/Library/Application Support/Lithium/Bottles`
- Linux：`~/.local/share/Lithium/Bottles`
- Windows：`%LOCALAPPDATA%/Lithium/Bottles`

---

## 当前状态

Lithium 目前是一个**功能完整的跨平台骨架 / 重写基础**：

- ✅ 跨平台工程结构（Kit / Desktop / CLI）
- ✅ Bottle 的创建、持久化（JSON 配置）、列举、删除
- ✅ 程序（exe）的登记、移除、启动
- ✅ 可插拔的平台后端与引擎发现
- ✅ Avalonia MVVM 界面（瓶子列表 / 详情 / 新建 / 设置 / 关于）
- 🚧 真实的 GPTK / Wine 下载与安装流程（由 `DownloadManager` 预留接口，待各平台接具体分发源）

---

## 许可证

[GPL-3.0](./LICENSE) © Whisky-App/Whisky 与 XLord-Heliukum/Lithium 的贡献者。
Lithium 是 Whisky 的衍生作品。
