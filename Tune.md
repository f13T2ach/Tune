# Tune

## 定义
Tune 是一个专业的音乐播放器，你能像调音师或者DJ一样播放音乐。调节EQ，在虚拟唱片轮盘上倒带，甚至将音乐解析重混......一切尽在掌握，你可以将你的工作录制或（和）保存，随时欣赏。

## 功能
- 多格式音频支持（MP3、FLAC、WAV、AAC 等）

- 图形化八段均衡器、EQ导入导出

- 自由搭配控件（轮盘、图标、封面等）

- 声学和乐理解析（BPM、调性、副歌片段猜测等）

- 播放列表管理（创建、导入、排序、收藏）

- 暂未支持的插件功能

## 平台
只能在支持 .Net 10 的 Windows 设备上运行

声卡应当支持 WASAPI / DirectSound 以启用某些功能

## 构建和运行
### 构建
```
git clone <.Git Address>
cd Tune
dotnet restore
dotnet build --configuration Release
```

### 运行
```
dotnet run --project Tune.csproj
```
你还可以直接用 Visual Studio 打开解决方案来构建运行。

## 结构
```
Tune/
├── Appearance/         # 页面
├── Assets/             # 资产
├── Client/             # 播放控制层和 Presenter 层
├── Components/         # 自定义控件
├── Engine/             # 播放器
├── Generic/            # 泛型类
├── Helpers/            # 辅助方法（转换器、扩展方法等）
├── Local               # 本地数据访问层
├── Models/             # 数据实体（音乐、歌单、歌词等）
├── Services/           # 服务
├── Utils/              # 工具类（解析和IO等）
├── Values/             # 常量
├── ViewModel/          # ViewModel
```

## 第三方库的使用
|类|用途|备注|许可证|
|---|---|
|CommunityToolkit.Mvvm|MVVM|-|MIT|
|DevBase.Format|解析歌词|这简直就是一坨|MIT|
|Microsoft.Extensions.DependencyInjection|DI|-|MIT|
|NAudio|音乐播放|-|MIT|
|NWave|音乐解析|-|MIT|
|Newtonsoft.Json|JSON解析|-|MIT|
|TagLibSharp|文件标签解析|-|LGPL-2.1-only|
|WPFLocalizeExtension|本地化|-|Ms-PL|

Tune 会严格按照开源许可协议工作

## 贡献指南
欢迎任何形式的贡献，包括但不限于报告问题、提交功能建议、完善文档或提供代码。

Tink 将在准备工作完成后开放本地化贡献，欢迎擅长翻译的人士！

1. Fork 本仓库

2. 创建您的功能分支 (git checkout -b feature/your-feature-name)

3. 提交更改 (git commit -m 'your-commit-description') **此处烦请严格遵循 Commit 标准要求**

4. 推送到分支 (git push origin feature/amazing-feature)

5. 打开 Pull Request

请至少确保代码风格与现有代码保持一致且不存在功能性异常，并添加必要的 UnitTest。

## 许可证
本项目采用 MIT 许可证。

详情请参阅 LICENSE 文件。

## 联系方式
项目主页：[...]

问题反馈：[...]

邮箱：f12fds@163.com