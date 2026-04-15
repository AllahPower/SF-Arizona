#pragma once

#include <wchar.h>

namespace sf::log
{
	void Open(const wchar_t* baseDir);
	void Info(const char* message);
	void Error(const char* message);
	void InfoW(const char* prefix, const wchar_t* value);
	void ErrorW(const char* prefix, const wchar_t* value);
	void ErrorCode(const char* prefix, int code);
}
