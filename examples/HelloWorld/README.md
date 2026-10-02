# Hello World Example

This example shows the smallest external module for `SF`.

It writes `Hello world!` to chat and to the SF log, reads the local player's position and health
through `ISF.Entities` (see `docs/ENTITIES.md`), and then exits. The module runs on the main thread,
which entity access requires.
