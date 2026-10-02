# Early Loading Example

This example shows an `ISFEarlyModule`. It is a plugin type that runs while GTA is still loading,
before SA-MP's CNetGame and `ISF` exist.

The plugin has no regular module. On load it subscribes to every later load stage. Each stage is
logged with its time since the plugin loaded and the raw `gGameState`. The stages are listed in
[docs/PLUGINS.md](../../docs/PLUGINS.md#early-loading).
