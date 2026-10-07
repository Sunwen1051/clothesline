# Clothesline

English | [简体中文](README.zh-CN.md)

A screenshot clothesline for Windows. Every new screenshot gets pegged to a line at the top of your screen; hover the very top edge and the line slides down, move away and it retracts.

![Clothesline](docs/demo.png)

Inspired by [tendedero](https://github.com/alejandrobujan/tendedero) (macOS, Swift). This is an independent Windows implementation — per tendedero's license note, the name and icon here are original.

## Requirements

- Windows 10 / 11
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) to run, or the .NET 10 SDK to build

## Usage

| Action | Effect |
| --- | --- |
| Move the mouse to the very top edge of the screen | The line slides down |
| Move the mouse away (or click empty rope) | The line retracts |
| `Ctrl+Alt+T` (falls back to `Ctrl+Alt+L` if taken) | Toggle the line |
| A new screenshot arrives | The line peeks down for a moment |
| Click a photo | Copy the full-resolution original to the clipboard |
| Double-click | Open in the default image viewer |
| Drag | Drag the file into a chat / editor |
| Right-click a photo | Open / reveal in folder / take down |
| Tray icon | Double-click toggles; right-click for menu (open folders, exit) |

## Where screenshots come from

Two sources are watched, covering the usual Windows capture flows:

1. **Folder watch** — `Win+PrintScreen` saves to `Pictures\Screenshots` (follows OneDrive redirection).
2. **Clipboard watch** — `Win+Shift+S` (Snipping Tool) puts the snip on the clipboard; it is saved as a PNG under `%LOCALAPPDATA%\Clothesline\captures` so it can be copied/dragged as a file.

Identical content within 6 seconds is deduplicated (one `Win+Shift+S` typically reaches both the clipboard and the auto-save folder; only one peg is created).

## Known limitations

- Primary screen only; screenshots taken anywhere hang on the primary screen's line.
- While a fullscreen game or presentation is in the foreground, new captures don't auto-peek (they are still pegged; use the hotkey or the top edge).
- The top hotzone is 3 px tall: precise, but it can be blocked by other always-on-top windows covering the screen edge (use the hotkey then).

## Building

```text
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

No NuGet dependencies — tray icon, clipboard listener, global hotkey and fullscreen detection are all hand-rolled Win32.

## Engineering notes (the fun part)

Things that bit during development, in case you build something similar:

- A WPF `Window` does not support `RenderTransform` — it throws. Slide the window itself by animating `Window.Top` instead.
- A `Window`'s theme style fills a **white background** by default: removing the `Background` attribute in XAML is *not* transparency. Set `Background="Transparent"` explicitly.
- **Layered windows hit-test per pixel alpha**: a fully transparent window is mouse-through for the whole system — `MouseEnter` never fires and `WindowFromPoint` lands on the desktop. Paint hit-test areas with **alpha = 1** (`#01000000`): invisible, but hittable.
- Images copied by WeChat (and other apps) arrive as DIBs with **all-zero alpha bytes**. Saving them as-is produces a fully transparent PNG — the image renders invisible. Fix the alpha bytes directly; do **not** use `FormatConvertedBitmap` to convert to `Bgr32`, because conversion passes through premultiplied alpha and an A=0 image turns pitch black.
- One `Clipboard.SetImage` raises several `WM_CLIPBOARDUPDATE` messages — deduplicate.

## License

MIT — see [LICENSE](LICENSE).
