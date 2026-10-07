# Clothesline — Windows 截图晾衣绳

（中文说明。English version below: [README.md](README.md)）

灵感来自 macOS 小工具 [tendedero](https://github.com/alejandrobujan/tendedero)（其代码为 MIT，但名称与图标不在授权内，本项目名称、图标、代码均为原创实现）。每张新截图自动"晾"在屏幕顶部的绳子上：鼠标碰到屏幕顶边绳子滑下来，移开就收回去。

![演示](docs/demo.png)

## 运行

- Windows 10 / 11，需要 [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)（构建需 .NET 10 SDK）
- `dotnet build -c Release` 或单文件发布：
  `dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true`

## 操作

| 操作 | 效果 |
| --- | --- |
| 鼠标碰到屏幕最顶边 | 晾衣绳滑下 |
| 鼠标移开（或点空白绳子） | 收回 |
| `Ctrl+Alt+T`，被占用则自动回退 `Ctrl+Alt+L` | 显示 / 收起 |
| 新截图到达 | 绳子"瞄一眼"（滑下约 2.8 秒再收回） |
| 单击照片 | 复制**原图**到剪贴板 |
| 双击 | 用默认看图软件打开 |
| 拖拽 | 以文件形式拖进聊天 / 编辑器 |
| 右键照片 | 打开 / 打开所在文件夹 / 取下 |
| 托盘图标 | 双击=显示/收起；右键=菜单（打开文件夹、退出） |

## 截图来源（两条路都听）

1. **文件夹监听**：`Win+PrintScreen` 落到 `图片\Screenshots`（跟随 OneDrive 重定向）。
2. **剪贴板监听**：`Win+Shift+S` 的结果自动存 PNG 到 `%LOCALAPPDATA%\Clothesline\captures`（拖拽/双击需要真实文件）。

6 秒内同像素内容去重（一次 Win+Shift+S 会同时进剪贴板和自动保存，只挂一张）。

## 已知限制

- 只在主屏展示。
- 全屏游戏 / 演示模式时新截图不自动弹出（仍会挂上，热键或顶边可唤出）。
- 界面文案为英文。
- 顶边热区 3px，比较精确；若被其他置顶窗口挡住顶边，用热键。

## 踩坑记录（给做类似东西的人）

- WPF `Window` 不支持 `RenderTransform`（直接抛异常）；滑动用 `Window.Top` 动画。
- **`Window` 主题默认样式填白底**：删掉 `Background` 属性 ≠ 透明，必须显式 `Background="Transparent"`。
- **分层窗口的鼠标命中按像素 alpha**：全透明窗口被系统整体穿透（`MouseEnter` 永不触发）。需要接鼠标的透明区域刷 **alpha=1**（`#01000000`）。
- **微信等程序复制的 DIB 没有 alpha 字节（全 0）**：直接存 PNG 会整张隐形；按字节抬满 alpha 修复，**不要**用 `FormatConvertedBitmap` 转 `Bgr32`（走预乘空间，A=0 的图会变全黑）。
- 一次 `Clipboard.SetImage` 会触发多次 `WM_CLIPBOARDUPDATE`，必须去重。

## 许可

MIT — 见 [LICENSE](LICENSE)。
