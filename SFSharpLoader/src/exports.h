#pragma once

#include "plugin.h"
#include <game_api.h>

struct CSharpExports;

typedef void(__stdcall* CSharpLoopCallback)();
typedef int(__stdcall* CSharpInitFunction)(CSharpExports*);

struct CSharpExports {

	void(__stdcall* registerDialogCallback)(void(__stdcall*)(int, int, int, const char*)) = [](void(__stdcall* dialogCallback)(int, int, int, const char*)) {
		SF->getSAMP()->registerDialogCallback(dialogCallback);
		};

	bool(__stdcall* isKeyDown)(char) = [](char key) {
		return SF->getGame()->isKeyDown(key);
		};

	bool(__stdcall* isKeyPressed)(char) = [](char key) {
		return SF->getGame()->isKeyPressed(key);
		};

	void(__stdcall* registerChatCommand)(char*, void(__cdecl*) (char*)) = [](char* commandName, void(__cdecl* cmdProc) (char*)) {
		SF->getSAMP()->getInput()->AddClientCommand(commandName, (uint)cmdProc);
		};

	void(__stdcall* unregisterChatCommand)(char*) = [](char* commandName) {
		SF->getSAMP()->getInput()->UnregisterClientCommand(commandName);
		};

	void(__stdcall* takeScreenshot)() = []() {
		SF->getSAMP()->takeScreenShot();
		};

	bool(__stdcall* isDialogActive)(uint) = [](uint dialogId) {
		return (!SF->getSAMP()->getDialog()->serverSide) && SF->getSAMP()->getDialog()->dialogID == dialogId;
		};

	void(__stdcall* setScoreboardVisibility)() = []() {
		SF->getSAMP()->getInfo()->UpdateScoreAndPing();
		};
};

