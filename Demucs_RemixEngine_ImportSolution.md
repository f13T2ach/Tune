# Demucs 分轨模型导入方案

## 定义
方案提供了C#内嵌调用 Demucs 模型的方案，这类似内嵌一个 AI 运行环境并由 C# 层调用的方案，允许 Tune 在本地运行 Demucs 模型来实现分轨。

这是一个跨语言方案，所以需要新增文件。

### 什么是 Demucs
Demucs 是 Meta 开源的一个音乐源分离（Music Source Separation）模型。它能将一首混合的歌曲，通过人工智能技术，分离成独立的音轨，比如人声、鼓点、贝斯等。其名字是“音乐资源深度提取器”（Deep Extractor for Music Sources）的缩写

Demucs 将会成为 Tune 第一个支持的分轨模型。

### 为什么选用 demucs.onnx
demucs.onnx 是支持 Demucs v4 hybird transformer 架构的 ONNX 接口。

正如作者提到的：
```
demucs.onnx
ONNX inference for the Demucs v4 hybrid transformer model, a high-performance and high quality PyTorch neural network for music source separation.

Unlike my other project demucs.cpp, where I wrote all of the neural network layers by hand, in this repo I use ONNXRuntime, which is a universal high-performance neural network inference library with native support for diverse hardware platforms (CPU, GPU, desktop OS, web, smartphone OS, etc.).

This code should perform significantly faster than my other project, and you can also leverage the different ONNX execution providers e.g. CUDA, GPUs, WebGPU, WebNN.
```
demucs.onnx是纯手写的神经网络层，它不会引入过多中间依赖，而且性能应该较好。

demucs.onnx为我们提供了cli，这为包装创造了可能。

## 客户端新增文件
|文件名|大小|作用|
|---|---|---|
|demucs_wrapper.dll|400kb|Demucs 业务层|
|onnxruntime.dll|10mb|外部依赖ONNX推理运行时|
|onnxruntime_providers_cuda.dll*|300mb|Nvidia GPU硬件加速拓展|
|htdemucs*.onnx*|[256mb,1.26gb]|AI模型|

结尾 * 表示可选项。

如果识别到用户有 CUDA 核心，下载 onnxruntime_providers_cuda.dll 并改变 DllImprrt 时的参数。 

如果用户同意，则下载 AI 模型，避免 Tune 原始体积过大。

## 方案简述
1. 添加项目`Demucs`。
2. 修改`https://github.com/sevagh/demucs.onnx`项目的代码，主要是添加包装类`demucs_wrapper.h`，至少包含`void* demucs_create(const char* onnxModelPath, bool requestCuda)`，`void demucs_destroy(void* handle)`和下文的`demucs_process()`函数。
3. 抽提项目中的`src_cli`业务逻辑，包括处理为包装类中的`int demucs_process(...args)`，其中返回的处理状态是整形。
4. 编译为 dll，注意不能静态链接。
5. Dll 应该存在于 `demucs` 目录下，由 Tune 使用 DllImport 特性进行调用

## 细节
### 用户业务细节
以下内容可能最初由 AI 生成。

#### 模型下载
提示

> 分析器需要依靠 AI 模型和复杂的数学运算，因此你需要下载一个大小在2G以内的本地模型。

#### 硬件加速提示
只包含 demucs_wrapper.dll 和 onnxruntime.dll，因为这是内置功能。应当提示：

> 分析器使用 Demucs 模型，当前只能使用 CPU 运算，较慢。
>
> 没有检测到 CUDA 核心，不能进行硬件加速。

如果检测到可用 Nvidia 显示卡，则提示：

> 分析器使用 Demucs 模型，当前只能使用 CPU 运算，较慢。
>
> 在你的电脑检测到 Nvidia 显示卡，可点击“配置硬件加速”来下载适合该显示卡的硬件加速运行库，这会让分析非常快。

如果用户同意下载，从公开源下载可选项。

### 嵌入细节
> 注意：以下内容完全由 AI 生成。

根据你提供的 `README.md`，这个项目的 **C++ 部分实际上是由 CMake 驱动的**（`src_cli/CMakeLists.txt`）。顶层 `Makefile` 只是一个“调度器”，负责安装 Python 依赖、运行转换脚本、调用 `make cli`（实际执行 `cmake --build`）。因此，**将它迁移到 Visual Studio 的 `.sln` 是完全可行的，而且不必“转换” Makefile —— 直接使用 CMake 生成 VS 解决方案即可**。

下面我为你梳理出一套在 Windows 上操作的完整步骤，并指出几个关键坑位。

---

## 🧭 总体思路
- **核心构建工具**：`CMake` + `Visual Studio`（2019 或 2022）。
- **核心依赖**：`ONNX Runtime`（Windows 预编译包）、`Eigen`（头文件库）、`libnyquist`（源码，需编译）。
- **最终产物**：一个可执行文件 `demucs.exe`，用于运行 ONNX 模型进行音源分离。

---

## 📦 第一步：准备源码和子模块
```bash
git clone --recurse-submodules https://github.com/sevagh/demucs.onnx
```
确保 `vendor/` 下的 `onnxruntime`、`eigen`、`libnyquist` 都拉取完整。

---

## 🖥️ 第二步：安装 Windows 必备工具
- **Visual Studio**（含 C++ 桌面开发工作负载）。
- **CMake**（≥ 3.15），并确保 `cmake` 在 PATH 中。
- （可选）**Python**（用于运行转换脚本，但如果你只需要编译 C++ 程序，且已有 `.ort` 模型文件，则不需要 Python）。

---

## 📥 第三步：获取 Windows 版 ONNX Runtime
原项目使用 `ort-builder` 在 Linux 下编译最小化库，但 Windows 下推荐直接下载官方预编译包：

1. 访问 [ONNX Runtime Releases](https://github.com/microsoft/onnxruntime/releases)，下载适合你架构的 CPU 版本（例如 `onnxruntime-win-x64-<version>.zip`）。
2. 解压到任意目录，例如 `C:/onnxruntime`，记住该路径（内含 `include/` 和 `lib/`）。

---

## 🛠️ 第四步：生成 Visual Studio 解决方案

有两种主流方式，任选其一：

### 方式 A：使用 CMake GUI（图形界面）
1. 打开 CMake GUI。
2. **源代码目录**：选择项目的 `src_cli` 文件夹（因为那里有 `CMakeLists.txt`）。
3. **构建目录**：在 `src_cli` 下新建 `build` 文件夹，并选中它。
4. 点击 **Configure**，选择生成器为 **Visual Studio 17 2022**（或你的 VS 版本），平台选 **x64**。
5. 首次配置会失败（因为找不到 ONNX Runtime），此时在列表中手动添加 CMake 变量：
   - 变量名：`ONNXRUNTIME_ROOT_DIR`，类型为 `PATH`，值设为你的 ONNX Runtime 解压根目录（例如 `C:/onnxruntime`）。
6. 再次点击 **Configure**，此时应能成功。
7. 点击 **Generate**，就会在 `build` 目录下生成 `demucs.sln`。

### 方式 B：使用命令行（更快捷）
在项目根目录打开 PowerShell（或 cmd），执行：
```powershell
cd src_cli
mkdir build
cd build
cmake .. -G "Visual Studio 17 2022" -A x64 -DONNXRUNTIME_ROOT_DIR="C:/onnxruntime"
```
执行后，`build` 目录下就会出现 `.sln` 文件。

---

## 🚧 第五步：处理可能出现的其他依赖问题

### Eigen
`Eigen` 是纯头文件库，通常 CMake 能通过 `find_package(Eigen3)` 自动找到 `vendor/eigen`，如果失败，可在 CMake 中手动设置 `Eigen3_DIR` 指向 `vendor/eigen/share/eigen3/cmake`（或直接添加 `-DEigen3_INCLUDE_DIR=<path>`）。

### libnyquist
`vendor/libnyquist` 是一个需要编译的 C++ 库。`src_cli/CMakeLists.txt` 中可能会通过 `add_subdirectory` 将它包含进来。确保它的 CMake 配置能在 Windows 下正常编译（通常是没问题的）。如果编译出错，可能需要调整其代码（极少见）。

### OpenBLAS
原 README 提到 OpenBLAS，但实际上 ONNX Runtime 自带 BLAS 实现，且项目代码中并未直接使用 OpenBLAS（仅用于 Python 部分）。如果 C++ 编译时链接报错找不到 BLAS，可以忽略，或检查 `CMakeLists.txt` 中是否有不必要的 `find_package(BLAS)`——大概率是作者忘记移除的冗余要求。

---

## 🏗️ 第六步：编译和运行
1. 双击打开 `demucs.sln`，在 VS 中选择 **Release x64** 配置，然后生成解决方案。
2. 生成的 `demucs.exe` 会在 `build/src_cli/Release/` 下。
3. 你需要准备好一个 `.ort` 模型文件（可以从项目发布页下载，或用 Python 脚本转换）。运行时：
   ```cmd
   demucs.exe path/to/model.ort input.wav output_dir
   ```

---

## ⚠️ 特别提醒
- **源码目录结构**：`src/` 中的 `.cpp` 和 `.hpp` 是被 `src_cli/CMakeLists.txt` 直接包含的（通过 `target_sources` 或 `add_library`），所以不需要额外操作。
- **模型转换**：如果你需要自己从 PyTorch 生成 ONNX/ORT，则必须配置 Python 环境（参照 README），但这一步可以在 WSL 或原生 Linux 下完成，然后将生成的 `.ort` 文件复制到 Windows 使用。
- **性能**：Windows 下 ONNX Runtime CPU 版性能良好，如想用 GPU，需下载 CUDA 版 ONNX Runtime，并设置 `-DONNXRUNTIME_ROOT_DIR` 指向对应目录。

---

## 🧩 如果实在不想折腾 CMake 配置
你也可以尝试 **Visual Studio 的“打开本地文件夹”功能**（需安装“用于 CMake 的 Visual C++ 工具”）。直接将 VS 的打开文件夹指向 `src_cli`，VS 会自动识别 `CMakeLists.txt` 并生成缓存，但你还是需要配置 `ONNXRUNTIME_ROOT_DIR`（在 `CMakeSettings.json` 中添加）。

---

总结：**这个项目不是“用 Makefile 构建”而是“用 CMake 构建”**，所以迁移到 VS 非常自然。关键点在于正确设置 ONNX Runtime 路径。如果你在操作中遇到具体错误（比如找不到头文件或库），欢迎贴出报错信息，我可以帮你进一步分析。