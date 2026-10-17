using Clothesline;

int failed = 0;

void Check(bool cond, string name)
{
    if (cond)
    {
        Console.WriteLine("ok  " + name);
        return;
    }

    Console.Error.WriteLine("FAIL " + name);
    failed++;
}

var root = Path.Combine(Path.GetTempPath(), "clothesline-logic-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    var shots = Path.Combine(root, "Screenshots");
    Directory.CreateDirectory(shots);

    var created = AppSettings.Load(root, shots);
    Check(created.MaxPegs == AppSettings.DefaultMaxPegs, "default max pegs is 20");
    Check(created.ClipboardWatchEnabled, "clipboard watch defaults on");
    Check(created.HotkeyCtrl && created.HotkeyAlt && created.HotkeyKey == "T", "default hotkey Ctrl+Alt+T");
    Check(created.WatchFolders.Count == 1 && created.WatchFolders[0] == Path.GetFullPath(shots), "default watch folder");
    Check(File.Exists(AppSettings.FilePath(root)), "settings.json created");

    created.MaxPegs = 0;
    created.ClipboardMinWidth = -3;
    created.ClipboardMinHeight = -1;
    created.WatchFolders.Add(shots);
    created.WatchFolders.Add("  ");
    created.Normalize();
    Check(created.MaxPegs == 1, "max pegs clamps up to 1");
    Check(created.ClipboardMinWidth == 0 && created.ClipboardMinHeight == 0, "negative min size clamps to 0");
    Check(created.WatchFolders.Count == 1, "blank and duplicate watch folders drop");

    created.MaxPegs = 9999;
    created.Normalize();
    Check(created.MaxPegs == AppSettings.MaxPegsLimit, "max pegs clamps to limit");

    created.MaxPegs = 12;
    created.ClipboardWatchEnabled = false;
    created.ClipboardMinWidth = 80;
    created.ClipboardMinHeight = 40;
    created.HotkeyShift = true;
    created.HotkeyKey = "f5";
    created.WatchFolders = new List<string> { shots };
    created.Normalize();
    created.Save(root);

    var loaded = AppSettings.Load(root, shots);
    Check(loaded.MaxPegs == 12, "max pegs roundtrip");
    Check(!loaded.ClipboardWatchEnabled, "clipboard toggle roundtrip");
    Check(loaded.ClipboardMinWidth == 80 && loaded.ClipboardMinHeight == 40, "min size roundtrip");
    Check(loaded.HotkeyShift && loaded.HotkeyKey == "f5", "hotkey fields roundtrip");
    Check(!loaded.ClipboardAccepts(200, 200), "disabled clipboard rejects any size");

    loaded.ClipboardWatchEnabled = true;
    Check(!loaded.ClipboardAccepts(0, 10), "zero dimension rejected");
    Check(!loaded.ClipboardAccepts(79, 40), "width below min rejected");
    Check(!loaded.ClipboardAccepts(80, 39), "height below min rejected");
    Check(loaded.ClipboardAccepts(80, 40), "size at min accepted");
    Check(loaded.ClipboardAccepts(800, 600), "larger image accepted");

    var emptyDir = Path.Combine(root, "empty-watch");
    Directory.CreateDirectory(emptyDir);
    File.WriteAllText(AppSettings.FilePath(emptyDir), """
        {
          "maxPegs": 7,
          "clipboardWatchEnabled": true,
          "clipboardMinWidth": 0,
          "clipboardMinHeight": 0,
          "hotkeyCtrl": true,
          "hotkeyAlt": true,
          "hotkeyShift": false,
          "hotkeyWin": false,
          "hotkeyKey": "T",
          "watchFolders": []
        }
        """);
    var emptied = AppSettings.Load(emptyDir, shots);
    Check(emptied.WatchFolders.Count == 0, "saved empty watch list stays empty");
    Check(emptied.MaxPegs == 7, "camelCase settings load");

    File.WriteAllText(AppSettings.FilePath(emptyDir), "{");
    var corruptThrew = false;
    try { AppSettings.Load(emptyDir, shots); }
    catch (InvalidDataException) { corruptThrew = true; }
    Check(corruptThrew, "corrupt settings.json throws");
    Check(File.ReadAllText(AppSettings.FilePath(emptyDir)) == "{", "corrupt settings.json is not overwritten");

    var keep = Path.Combine(root, "keep.png");
    var gone = Path.Combine(root, "gone.png");
    File.WriteAllText(keep, "img");
    File.WriteAllText(gone, "img");
    PegStore.Save(root, new[]
    {
        new PegRecord { FilePath = keep, Angle = 1.5 },
        new PegRecord { FilePath = gone, Angle = -2 },
    });
    File.Delete(gone);
    var pegs = PegStore.Load(root);
    Check(pegs.Count == 1 && pegs[0].FilePath == keep && pegs[0].Angle == 1.5, "missing peg file dropped on load");

    var list = new List<string> { "a", "b", "c", "d" };
    PegStore.TrimOldest(list, 2);
    Check(list.Count == 2 && list[0] == "c" && list[1] == "d", "trim drops oldest");
    var trimThrew = false;
    try { PegStore.TrimOldest(list, 0); }
    catch (ArgumentOutOfRangeException) { trimThrew = true; }
    Check(trimThrew, "trim rejects non-positive capacity");

    File.WriteAllText(PegStore.FilePath(root), "{");
    var pegCorrupt = false;
    try { PegStore.Load(root); }
    catch (InvalidDataException) { pegCorrupt = true; }
    Check(pegCorrupt, "corrupt pegs.json throws");

    var preferred = new HotkeyChord(true, true, false, false, "t");
    Check(preferred.Label == "Ctrl+Alt+T", "default label");
    Check(preferred.VirtualKey == (uint)'T', "letter virtual key");
    Check(preferred.IsRegisterable, "ctrl+alt+t registerable");
    Check((preferred.ModifierFlags & HotkeyChord.ModNoRepeat) != 0, "norepeat flag set");

    var order = HotkeyChord.RegistrationOrder(preferred);
    Check(order.Count == 2 && order[0].Key == "T" && order[1].Key == "L", "fallback appends L without duplicating T");

    var custom = new HotkeyChord(true, false, true, false, "F5");
    var customOrder = HotkeyChord.RegistrationOrder(custom);
    Check(customOrder.Count == 3
        && customOrder[0].Label == "Ctrl+Shift+F5"
        && customOrder[1].Label == "Ctrl+Alt+L"
        && customOrder[2].Label == "Ctrl+Alt+T", "custom hotkey falls back to L then T");
    Check(custom.VirtualKey == 0x74, "F5 virtual key");

    var bare = new HotkeyChord(false, false, false, false, "A");
    Check(!bare.IsRegisterable, "modifier required");
    Check(HotkeyChord.TryVirtualKey("nope") == null, "unknown key rejected");
    Check(HotkeyChord.TryVirtualKey("Num3") == 0x63, "numpad virtual key");
    Check(new HotkeyChord(true, false, false, true, "space").Label == "Ctrl+Win+Space", "space label");

    var folder = Path.Combine(root, "drop-folder");
    Directory.CreateDirectory(folder);
    var folderUrl = new Uri(folder).AbsoluteUri;
    Check(DropLocation.FromShellUrl(folderUrl) == DropKind.Folder, "existing folder url");
    Check(DropLocation.FromShellUrl("::{645FF040-5081-101B-9F08-00AA002F954E}") == DropKind.RecycleBin, "recycle clsid url");
    Check(DropLocation.FromShellUrl("file:///C:/no/such/clothesline-drop") == DropKind.Other, "missing folder url is not a folder");
    Check(DropLocation.FromShellUrl("") == DropKind.Other, "empty shell url");
    Check(DropLocation.FromShellUrl("http://example.test") == DropKind.Other, "non-file shell url");
    var threw = false;
    try { _ = DropLocation.FromShellUrl("file:///%zz"); }
    catch { threw = true; }
    Check(!threw, "bad file url does not throw");
}
finally
{
    try { Directory.Delete(root, recursive: true); } catch { }
}

return failed == 0 ? 0 : 1;
