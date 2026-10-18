# PR 必读

这份文件是这个仓库的代码风格与提交要求。它描述的是**现有代码的做法**，不是理想化的规范 —— 提交前请对照检查。

改动前请先跑通：

```
dotnet build D:\Tune\Tune.slnx
dotnet test --project Tune.Tests/Tune.Tests.csproj
```

要求 **0 警告、0 失败**。目前基线是 331 通过 / 1 跳过。

---

## 1. 命名

| 对象 | 规则 | 例子 |
|---|---|---|
| 私有字段 | `_camelCase`，**必须带下划线** | `_lyricData`、`_history`、`_leftPeaks` |
| 公开成员 | `PascalCase` | `UpdateLyricsBySecond`、`AttachEffect` |
| 参数 / 局部变量 | `camelCase` | `lyrics`、`seconds` |
| 接口 | `I` 前缀 | `IAudioLevelSource` |
| 测试方法 | `被测方法_场景_期望` | `AttachEffect_AfterAttaching_IsBuiltWhenTheProcessorIsCreated` |
| 事件 | 过去式 / 状态 | `LevelReported`、`EditingChanged` |

仓库里私有字段 242 处带下划线、0 处不带 —— 这条没有例外。

## 2. 格式

- **Allman 花括号**：块的花括号独占一行。对象/集合初始化器的花括号跟随表达式即可。
- **块作用域命名空间**，不用 file-scoped 的 `namespace X;`。
- 缩进 4 空格，不使用 Tab。
- 文件从 `using` 或 `namespace` 开始都行（现有代码两种都有）。换行保持文件现状，不要整文件重排 —— CRLF/LF 混排是历史遗留，重排会让 diff 变得不可读。

```csharp
namespace Tune.Engine.Audio
{
    public sealed class LevelWindow
    {
        public LevelWindowResult? Add(double peakLeft, double peakRight, double seconds)
        {
            ...
        }
    }
}
```

## 3. 注释与文档

- **公开成员写 XML 文档**，用完整英文句子，`/// <summary>` 起头。仓库里已有 725 处。
- 实现接口时优先用 `/// <inheritdoc />`。
- **新代码的注释写英文**。仓库里仍有 454 处中文 `//` 注释和 236 处中文 XML 注释，那是历史遗留 —— 不要模仿，也不要为了"统一"去批量翻译。
- 注释解释**为什么**，不复述代码。例子：

```csharp
// Attack is immediate, then the gain holds before it starts coming back. Without the
// hold the gain would rise between beats and be pushed down again, which is heard as
// pumping.
```

- 需要强调时用 `**NOTE**:`，与现有代码一致。
- 测试区块用 `#region ==== 标题 ====` 分隔，测试文件里尤其常见。

## 4. 分层（提交 PR 最常被退回的地方）

```
Engine       ← 音频、效果链、算法。不认识 WPF。
Client       ← Controllers / Presenters。把引擎状态变成界面能读的值。
ViewModel    ← 绑定逻辑。不认识视图类型。
Appearance   ← XAML 与自绘控件。可以引用上面三层。
```

硬性要求：

- **`ViewModel`、`Models`、`Client` 里不得出现 `using Tune.Appearance`，也不得出现 `typeof(某页面)`**。
- 导航只传页面身份，不传类型：

```csharp
WeakReferenceMessenger.Default.Send(new NavigationMessage(PageKind.Main));
```

视图类型的解析**只在 `MainWindow.Resolve` 里做**。新增页面的步骤：加 `PageKind` 值 → 在 `Resolve` 里加一行。

- 用户层音频只通过 `AttachEffect` 扩展，不要自己往管道里插节点：

```csharp
void AttachEffect(Func<ISampleProvider, ISampleProvider> attach);
```

- 系统层（安全限幅、电平测量）**不可被用户代码改动**，它不在用户链条里。

## 5. WPF 与自绘控件

- 依赖属性统一 `DependencyProperty.Register(nameof(X), ...)`；界面尺寸变化要重算几何时加 `AffectsRender`。
- **路由事件的 OwnerType 必须是声明它的类型**：

```csharp
// ✗ 复制粘贴会炸：CheckBox 的静态构造会因为重复注册而抛 ArgumentException
EventManager.RegisterRoutedEvent("Checked", ..., typeof(CheckBox));

// ✓
EventManager.RegisterRoutedEvent("Checked", ..., typeof(IlluminatedButton));
```

- **不要重复声明基类已有的依赖属性**（例如自己再写一个 `ItemsPanel`），会隐藏基类成员并产生 `CS0108`。
- 自绘控件（`PeakMeterPanel`、`CustomSlider`、`EqFader`）**按比例绘制，不用图片**；几何用统一的 dB→像素换算，不要各自算一套。
- 项目是固定 1600×792 画布 + `Viewbox`，改动布局时按像素对齐，别引入会随窗口变化的相对尺寸。

## 6. 音频

- 采样一律通过 `ISampleProvider`；NAudio 3 的接口要求实现 `Read(Span<float>)`，不要只写数组重载。
- 音频回调里**不要碰 UI、不要分配**。跨线程到界面的搬运只放在 `Presenters` 层。
- 限幅用**增益包络**，不要削波：

```csharp
double target = peak <= _ceiling ? 1.0 : _ceiling / peak;
_gain = target < _gain ? target : target + (_gain - target) * release;
```

- 电平条与均值/削波是**两个频率**：条每块更新，均值与削波每 2 秒（`LevelWindow.DefaultWindow`）更新一次。改采样段时长只改那一个常量。

## 7. 测试

- xUnit。`[Fact]`，参数化用 `[Theory]` + `[InlineData]`。
- 每个测试写一句 `/// <summary>` 说明它盯的是什么行为。
- **断言要贴近被测动作**：用刚拿到的返回值断言，不要"事后"从别处补齐数据再比。
- **不要依赖共享磁盘状态**。歌单、设置、缓存这类读写文件的测试，用一次性目录：

```csharp
string directory = Path.Combine(Path.GetTempPath(), "tune-history-tests", Guid.NewGuid().ToString("N"));
LoadableServicesManager.Register(new PlaylistService(new PlaylistProvider(directory)));
```

- 间歇失败视为缺陷，不要用重跑来"通过"。
- 测试里**关掉不需要的被测系统**，避免互相干扰（例如测 EQ 时 `IsOutputLimited = false`）。
- 断言浮点时给合理精度（音频幅度 3～4 位小数即可），不要卡到 6 位。

## 8. 本地化

- 面向用户的字符串放进 `LangStrings`（`LangStrings.resx` 英文 + `LangStrings.zh-CN.resx` 中文 + `LangStrings.Designer.cs` 访问器），三处同步。
- **中英两份 resx 的键必须完全一致**，测试会检查。
- **不要借用别的键**表达新语义 —— 新增一个键，哪怕只有一字之差。
- 大写设计标签（`LYRIC`、`ALBUM`、`LAST`/`NEXT`、对话框标题等）**不翻译**。

## 9. 提交

- 提交信息用**短英文祈使句**，一句话说清做了什么，不加句号：

```
Split the system output stage from the user effect chain
Hold the limiter gain so it stops pumping
Drop the unused meter from the user chain
```

- 常用动词：`Add` / `Drop` / `Remove` / `Move` / `Split` / `Bind` / `Rename` / `Fix`。
- 一次提交做一件事。清理与功能改动分开。
- **在 `feature` 分支上提交**，不要直接提交到 `master`。
- 提交前确认：`git branch --show-current`、工作区干净、构建与测试都绿。

## 10. 提交前自检清单

- [ ] `dotnet build` 0 警告 0 错误
- [ ] `dotnet test` 0 失败（允许那条标注了原因的跳过）
- [ ] 私有字段都带 `_` 前缀
- [ ] 公开成员有 XML 文档，新注释是英文
- [ ] `ViewModel`/`Models`/`Client` 没有引用 `Tune.Appearance`，没有 `typeof(页面)`
- [ ] 新增依赖属性/路由事件的 OwnerType 与 `nameof` 正确，没有隐藏基类成员
- [ ] 新增用户可见字符串三处同步，中英键一致
- [ ] 测试不依赖共享磁盘状态，断言贴近动作
- [ ] 提交信息短、英文、祈使句；分支是 `feature`
- [ ] 没有顺手提交临时目录、构建输出、个人配置
