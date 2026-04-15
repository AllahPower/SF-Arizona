# HelloWorld

Минимальный внешний плагин для `SF`.

Что показывает пример:
- модуль ссылается только на `SF.Abstractions`;
- модуль реализует `ISFModule` напрямую;
- выводит `Hello world!` в игровой чат через `context.SF.Chat.Add(...)`;
- пишет строку в стандартный SF-лог через `context.Log`.

Сборка:

```powershell
dotnet build example/HelloWorld/HelloWorld.Example.csproj -c Release
```

Готовая папка плагина после сборки:

```text
example/HelloWorld/bin/Release/hello-world/
```

Её можно целиком скопировать в:

```text
<GAME_DIR>/SF/modules/hello-world/
```

Для твоего текущего окружения это будет:

```text
E:\games\ARIZONA GAMES\bin\Arizona\SF\modules\hello-world\
```
