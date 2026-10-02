# Examples

External sample modules for `SF` that build against `SF.Abstractions`. Every plugin ships a
`manifest.json`; see [docs/PLUGINS.md](../docs/PLUGINS.md) for its format and dependency versions.

- `HelloWorld` shows the smallest external module that writes to chat and to the log.
- `CommandEcho` shows the direct `ISFModule` contract, chat commands, and clean shutdown on unload.
- `ConfigCounter` shows module config storage, lifecycle hooks, and persistent state between starts.
