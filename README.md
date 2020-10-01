# savetray

Simple WIN+D replacement to access your programs directly from the notification area.

*Keep your windows maximized.*

## installation
The software is assumed to run with default permissions. </br>

1. add a shortcut in the `shell:startup` directory
2. make icon visible in notification area `shell:::{05d7b0f4-2121-4eff-bf6b-ed3f69b894d9}`
3. **optional** add a custom icon `Resources\favicon.ico` (16x16)

## settings
**syntax** `<menu>\<item>; <path>; <arguments>`
```yaml
# double-click action
$; explorer; %userprofile%

# top-level action
about; explorer; https://github.com/nick3ds/savetray

# nested action
dev\bash; "C:\Program Files\Git\git-bash.exe"; "--cd-to-home"
```
```yaml
# single commands
ip-tools\ping (google); "powershell"; -NoExit ping google.nl

# multiple commands
sketch; "cmd"; /c "start mspaint & start notepad"
browse; "powershell"; @('explorer \"C:\Program Files\"', 'taskmgr') | cmd
```
