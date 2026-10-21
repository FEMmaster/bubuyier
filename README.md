# 一二和布布 · Optimized 独立工程

这是以 `FEMmaster/YiErBuBu` 的 WPF 程序结构为基础整理的独立 Windows 工程。

## 开始使用

1. 将本 ZIP 解压，**不要只上传 ZIP**，把工程内容放到一个新 GitHub 仓库根目录。
2. GitHub → Actions → `Build Optimized Windows EXE` → Run workflow。
3. 下载 Artifacts 中的 `YiErBuBu-Optimized-Windows-x64`，解压运行 `YiErBuBu.exe`。
4. 使用自包含 .NET 8 构建，不需要在自己的电脑上安装 .NET。

## 已实现

- 独立角色本体、眨眼与四个独立爪子；动态贝塞尔手臂、按键投影跟随与待机收回；保留鼠标垫几何中心驱动鼠标。
- 钩子回调异常隔离、错误日志和监听器先建后换。
- 缓存颜色 Brush、描边 Pen、静态四边形几何和按键文字，降低每帧分配。
- 右键调整大小、锁定、暂停、重启监听、开机自启、退出。
- 位置/比例/锁定/暂停保存于 `%LocalAppData%\YiErBuBu\settings.json`，错误日志在同目录。
- 自动检查 PNG 文件是否存在、格式是否为 PNG、body/blink 尺寸是否一致。

## 重要的素材来源限制

本工作环境无法从 GitHub 连接器获得当前仓库中的 PNG **二进制字节**，因此 ZIP 中的八张 PNG 使用会话中可用的**较早本地工程素材**填充，而非仓库 HEAD 上的最终八张素材。此 ZIP 是完整的可构建源码包，但**不能保证视觉效果与现有仓库完全一致**。

**正式使用前：**请从你现有 GitHub 仓库的 `assets/characters/` 下载全部八张最新 PNG，并直接替换 ZIP 同名文件。特别注意 blink/body 的像素匹配；尺寸相同不代表非眼睛区域像素一致。

## 验证范围

执行了结构、文件和 PNG 格式/尺寸检查。本环境没有 Windows .NET SDK，不能实际编译或运行 WPF；GitHub Actions 负责真正的 Windows 构建验证。

## 有意不修改

- 没有添加任务栏托盘图标。
- 没有要求安装本机 .NET Runtime。
- 没有自动更改用户当前 GitHub 仓库。
