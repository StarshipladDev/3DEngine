# Code Review — DoomCloneV2 / DoomScrolls

Repo: `StarshipladDev/3DEngine`
Scope: full review of the C# source (~5,400 lines), including `Screens/Form1.cs` in full, the networking layer, entity/gun/cell logic, and the project file. This is a hobby/student project, so severity is calibrated to "things that will actually bite you," not production-grade nitpicking.

## Housekeeping: the duplicate `Cell.cs`

`Cell.cs` (repo root) and `Core/Cell.cs` are byte-identical. Only `Core/Cell.cs` is referenced in `DoomCloneV2.csproj`. The root copy is dead and unsafe to edit (changes there do nothing) — safe to delete.

## Bugs / Correctness

1. **`Core/Program.cs:32-34`** (high) — `Application.Run(new MapGenForm())` blocks until that form is closed, so the actual game (`Form1`) never launches until the user closes an unrelated map-maker debug form first. Looks like leftover test code rather than intended startup flow.
2. **`Core/CommandReader.cs:30-34`** (high) — `MovePlayer` calls `players[x].GetX()`/`.Gety()` *before* the `x > -1 && x < players.Count` bounds check runs. A bad/out-of-range player ID (e.g. from a malformed network command) throws before the guard ever executes — the check is dead code.
3. **`Logic/CursorObject.cs:82-85`** (medium) — `NewGoal()` sets `goaly = x + rand.Next(maxShift)` — uses `x` instead of `y`, a copy/paste bug that makes the aim-reticle sway track the cursor's X position instead of Y (visibly wrong crosshair drift).
4. **`Screens/Form1.cs` (`RunCommands`, the network opcode switch)** (high) — Opcodes like `SHE`, `SHW`, `CRP`, `SEE` index `units[...]`/`players[...]` using values parsed straight off the wire, with no bounds check against `.Count`, and `RunCommands()` isn't wrapped in try/catch anywhere. A malformed or stale network command throws an unhandled exception and crashes the app.
5. **`Core/Globals.cs:216-231, 262-277`** (medium) — In `FindFirstUnit`'s `DOWN`/`RIGHT` cases, the loop checks bounds *before* incrementing but then uses the post-increment index to access the array, so scanning to the last valid cell throws `IndexOutOfRangeException`. (`UP`/`LEFT` don't have this bug.) Triggerable by enemy direction-scan logic near the map edge.
6. **`Threading/Server.cs:83-93`** (high) — `Stop()` calls `listeners[i].Stop()`, but `listeners` is never populated (only the single `listener` field is used) — it's always `null`, so `Stop()` throws `NullReferenceException` every time it's called (e.g. on server shutdown). Also uses `Thread.Abort()`, deprecated/unsafe even on .NET Framework.
7. **`Threading/Server.cs:16-19, 33-49`** (medium) — `clients`/`thread`/`listeners` are fixed-size arrays of 10, and the connection `counter` has no bound check. An 11th client throws inside the accept loop, which the outer catch turns into a full `Stop()` — silently killing the session for every connected player.
8. **`Logic/Gun.cs:339`** (medium) — `relevantImgs[rand.Next(relevantImgs.Count())]` — if no image matches the generated part/code combination, `relevantImgs` is empty and indexing throws. Reachable via random gun generation on spawn and on the "refresh gun" (`Q`) key.
9. **`Entities/Unit.cs:127-140`** (low/medium) — Ambient sound in `Draw()` creates a `SoundPlayer`, calls the asynchronous `Play()`, then disposes it right away — since playback hasn't finished, this can cut audio short or throw on a disposed player mid-playback.
10. **`Screens/Form1.cs:1130-1131, 1258-1259`** (low, cosmetic) — Debug timing math is backwards twice: subtracts `DateTime.Now` in the wrong order (negative durations), and uses `.Milliseconds` (the 0-999 component) instead of `.TotalMilliseconds`, discarding accumulated time each pass. Only affects debug output.
11. **`Core/Program.cs:19`** (low) — `[STAThread]` sits on the unused `ThreadRunnerServer` helper instead of `Main()`, which is where WinForms actually requires it.

## Resource Management (GDI+ leaks)

12. **`Screens/Form1.cs:1140-1273` (`FormUpdate`)** (high) — Every frame (5×/sec) allocates `Bitmap n = new Bitmap(Width, Height)`. `Graphics g` gets disposed, but **`n` never does.** This is the single biggest leak in the app — unbounded GDI+ handle/memory growth for the life of any play session, eventually exhausting GDI handles and freezing or crashing the process.
13. **`Screens/Form1.cs:984-1009` (`DrawMenu`)** (medium) — Loads `Image.FromFile(...)` fresh from disk on every paint call while the menu is showing, and never disposes the previous image — a leak plus needless disk I/O every frame.
14. **Pervasive: `Core/Cell.cs` `Draw`, `Entities/Entity.cs` `Draw`, `Overlays/Hud.cs` `DrawHud`, `Overlays/TextDisplay.cs` `GetImage`, `Screens/MapDraw.cs` `PaintDrawing`** (medium, systemic) — All allocate `new SolidBrush(...)` / `new Font(...)` / `new Pen(...)` inline inside per-frame draw code, never disposed. Cheap individually, but multiplied across every cell/unit/entity drawn every frame this is a large accumulating leak — the classic WinForms GDI+ mistake, repeated throughout the render path.
15. **`Logic/Gun.cs:99-154, 340-400`** (medium) — Intermediate `Bitmap`s (the per-iteration `picture`, and bitmaps immediately overwritten by a `ReColorImage(new Bitmap(...))` call two lines later) are discarded without disposal. A new `Gun` is built on every spawn and every `Q` refresh, compounding the leak.
16. **`Entities/Entity.cs:35-61`** (low) — `baseFile` is cropped 8 times via `Globals.CropImage` when `animated` is true, but the source bitmap itself is never disposed afterward.
17. **`Core/Globals.cs:120-139` (`ReColorImage`)** (low/medium) — Uses `GetPixel`/`SetPixel` in a nested per-pixel loop (one of the slowest ways to touch a `Bitmap` in GDI+; `LockBits` would be far faster), and also writes the result back to disk on every call — from `Player.CreatePowerup` and `Gun.setComponent`, so every gun/powerup creation does slow pixel-by-pixel work plus a redundant disk write.

## Architecture / Structure

18. **`Screens/Form1.cs` (1,980 lines)** (high) — A God Object: game state (players/units/turns), the entire render pipeline (`DrawCells`, `FormUpdate`, `DrawMenu`), input handling (`OnKeyUp`, `MouseClicker`, `MouseMover`), the network protocol interpreter (`RunCommands`, ~500 lines of switch over 3-letter opcodes), and menu/UI logic all live in one partial class. This is the main structural debt item — splitting into a session/game-state manager, a renderer, an input controller, and a network dispatcher would pay off the most as the project grows (`CommandReader` is already a partial start in that direction, but the big switch is still in `Form1`).
19. **Ad-hoc network protocol** (medium) — Fixed-width string commands (`"MVP" + "{0:00}" + ...`) parsed via `Substring`/`Int32.Parse`, scattered across `Form1` and `CommandReader`, with no shared schema, versioning, or central validation. Any length/format mismatch throws. A small parsing/validation helper (or just a try/catch per command) would harden this a lot.
20. **`Core/Program.cs`** (low) — Contains a second, unused copy of `ThreadRunnerServer` (duplicating `ThreadFunctions.ThreadRunnerServer`, with a hardcoded IP `192.168.200.40:8006`), plus ~12 lines of commented-out dead startup code.
21. **`Screens/TitleScreen.cs`** (low) — `MenuScreen` class is empty and unused; dead file.
22. **`Threading/ThreadFunctions.cs:23-33` (`ThreadRunner`)** (low) — Unreferenced anywhere in the codebase; dead code that also unconditionally creates/overwrites `output.txt` in the working directory with no try/finally around the `FileStream`.

## Networking / Security

23. **No input validation or bounds checking on network-derived commands** (high) — Every opcode handler trusts `Substring`/`Int32.Parse` on peer-controlled data with only a `commands[i].Length > 2` check, and no exception boundary around `RunCommands()`. Practical impact: any peer (or a corrupted/truncated TCP read) can crash every connected client with one malformed message — a straightforward crash-DoS. No RCE vector was found (no reflection/deserialization present), so this is a stability/availability issue, not a code-execution one.
24. **No authentication or per-command authorization** (medium) — `Threading/Server.cs`'s `SendMessage` blindly rebroadcasts whatever any client sends, verbatim, to everyone else. Since the protocol includes state-mutating opcodes (`SHP`, `SHE`, `SEE`, `SEP`, `DEL`, ...), any connected client can forge them to damage other players, move/resurrect units, or reset world state — trivial cheating/griefing with zero server-side validation that a command actually came from a legitimate action.
25. **`Threading/Server.cs:60-82` (`SendMessage`)** (medium) — Writes to each client stream with no try/catch; one dead/disconnected client can throw uncaught on a background thread, which by default terminates the whole .NET Framework process.
26. **`Threading/ThreadFunctions.cs:56-60` (`Listen`)** (low) — Catches `Exception` and calls `Thread.CurrentThread.Abort()` — a deprecated/unsafe shutdown pattern that can leave cleanup incomplete.
27. **Message framing mismatch** (low/medium) — `Server.Listen` does one raw `Read()` per iteration and immediately rebroadcasts whatever bytes arrived, without the `'^'`-terminator reassembly that `Client.Read()` implements for the other direction. TCP doesn't guarantee message boundaries align with `Read()` calls, so a command split across two segments can be forwarded as two malformed halves.

## Code Quality / Duplication

28. **Root `/Cell.cs`** (medium) — dead duplicate of `Core/Cell.cs`, not compiled — delete it to avoid future edits landing in the wrong copy.
29. **`Core/CommandReader.cs:17-25`** (low) — Constructor does `this.i = i` against an uninitialized field (not a parameter) — a no-op self-assignment; `i` appears unused elsewhere. Leftover code.
30. **Naming inconsistency** (low) — Mixed casing/typos (`Gety()` vs `GetX()`, `palyer`, `RemoveProjecticle`, `SetUpProjecticle`, a field literally named `yeet` in `Server.cs`), plus heavy reliance on magic 3-char opcode strings instead of an enum/constants.
31. **Excessive `Debug.WriteLine`/commented-out code** (low) — Present in nearly every file (e.g. `CreateGun`'s commented-out original implementation, `Program.cs`'s commented startup code) — normal for a hobby project, but an easy cleanup pass.

---

## Suggested next steps, highest value first

1. **Fix the `FormUpdate` bitmap leak** and dispose the `Brush`/`Font`/`Pen` allocations in the hot per-frame draw paths — most likely to prevent real freezes/crashes during actual play.
2. **Add bounds checks (or one try/catch) around the `RunCommands` dispatch** — turns "malformed packet crashes the game" into "malformed packet gets logged and ignored," which matters a lot on the multiplayer path.
3. **Delete the dead files**: root `Cell.cs`, `ThreadFunctions.ThreadRunner`, `Screens/TitleScreen.cs`'s empty `MenuScreen`.
4. **Fix `Server.Stop()`'s `NullReferenceException`** and the fixed 10-client array — right now the server can't shut down cleanly and silently dies on an 11th connection.
5. **If the project keeps growing, split `Form1.cs`** into a renderer, an input controller, and a network dispatcher — not urgent, but it's what will save the most future debugging pain.
