# Plugins

A plugin is a folder in `<GTA>/SF/modules/<folder>/` with a `manifest.json` and the plugin assembly.
The loader scans every folder on startup; `/sfs plugin-load <folder>` loads one later.

Plugins compile against the built `SF.Abstractions.dll`, never against SF-Arizona sources, and must not
ship their own copy of it: the host shares `SF.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`
and `System.Text.Json`, plus every library installed directly in `SF/`.

```xml
<Reference Include="SF.Abstractions" HintPath="$(SFAbstractionsPath)" Private="false" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.5" ExcludeAssets="runtime" />
<None Include="manifest.json" CopyToOutputDirectory="PreserveNewest" />
```

Plugins that consume parsed traffic, such as `context.SF.Events.OnIncomingRpc<SetPlayerPosRpc>(...)` or the
sync, Arizona 220/221 and AZVoice models, also reference the built `SF.Protocol.dll` the same way (namespaces
`SFSharp.Protocol.*`). The host shares the instance it uses itself, so model types match on both sides:

```xml
<Reference Include="SF.Protocol" HintPath="$(SFProtocolPath)" Private="false" />
```

## manifest.json

```json
{
  "id": "arizona-chat",
  "displayName": "Arizona Chat",
  "version": "1.0.0",
  "description": "Optional text.",
  "author": "Optional",
  "website": "Optional",
  "assembly": "ArizonaChat.dll",
  "enabledOnStart": true,
  "dependencies": {
    "sf":    { "min": "4.0.0", "max": "4.x", "target": "4.0.0" },
    "sf.ui": { "min": "0.3.0-alpha.2", "max": "0.3.x" }
  }
}
```

| Field | Required | Meaning |
|---|---|---|
| `id` | yes | `[A-Za-z0-9][A-Za-z0-9._-]{0,127}`. `sf` is reserved for the host. |
| `version` | yes | SemVer 2.0 version of the plugin, checked by plugins that depend on it. |
| `assembly` | yes | Relative path to the plugin assembly inside the plugin folder. |
| `enabledOnStart` | no | Overrides `[SFModule(DefaultEnabled)]` for every module of the plugin. |
| `dependencies` | no | Supported versions of the host (`sf`) and of other plugins, keyed by plugin id. |

## Dependency versions

Each dependency accepts three optional fields:

- `min`: lowest supported version, inclusive.
- `max`: highest supported version, inclusive. Either an exact SemVer version or a wildcard:
  `4.x` accepts every 4.\* release and prerelease, `4.1.x` every 4.1.\*. Wildcards compare numeric
  components only, so `4.x` never accepts `5.0.0-alpha`.
- `target`: the version the plugin was built and tested against.

The manifest is rejected when `min` is above `max`, or `target` is outside `[min, max]`.

When the dependency is outside `[min, max]` or a required plugin is not installed, the plugin is not
loaded and the reason is logged and shown by `/sfs`. Plugins that depend on a rejected plugin are rejected
too. When the dependency is supported but differs from `target`, the plugin loads with a warning in the
log, `/sfs` and `/sfd`.

### The host: `sf`

The host version is the SF-Arizona release (`Version.props`), which is also the `SF.Abstractions` version.
Preview builds such as `4.0.0-preview.43.1234567` are compared as their core version `4.0.0`; their tag
marks the build channel, not a different API.

Bounds missing for `sf` are filled from the `SF.Abstractions` version the plugin assembly references:
`min` and `target` become that version and `max` becomes its major (`4.x`). A plugin with no `sf` entry
built against 4.0.0 therefore loads on 4.0.0 up to any 4.\*, warns on a newer 4.\* and is rejected by 5.0.0.
Plugins built against 3.x are rejected by 4.0.0 and must be rebuilt.
Declare `sf` explicitly when the plugin needs a wider or narrower range.

### Other plugins

Dependencies on other plugins use full SemVer precedence (`0.3.0-alpha.2 < 0.3.0`). The loader loads
dependencies before dependents; plugins without dependencies keep discovery order. Cycles are rejected.
A plugin cannot be unloaded or reloaded while plugins that depend on it are loaded.

Plugin dependencies order whole plugins. Ordering between individual modules still comes from
`[SFModule(Dependencies = [...])]`, which takes module ids.
