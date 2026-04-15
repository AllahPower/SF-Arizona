#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include <string.h>
#include <wchar.h>

#include "sf_native/log.hpp"

namespace
{
	HANDLE g_logFile = INVALID_HANDLE_VALUE;
	CRITICAL_SECTION g_logLock;
	bool g_logInitialized = false;

	void WriteLine(const char* level, const char* line)
	{
		if (!g_logInitialized)
		{
			return;
		}

		SYSTEMTIME localTime;
		GetLocalTime(&localTime);

		char buffer[1024];
		int length = _snprintf_s(
			buffer,
			sizeof(buffer),
			_TRUNCATE,
			"[%04u-%02u-%02u %02u:%02u:%02u.%03u] [%s] %s\r\n",
			localTime.wYear,
			localTime.wMonth,
			localTime.wDay,
			localTime.wHour,
			localTime.wMinute,
			localTime.wSecond,
			localTime.wMilliseconds,
			level,
			line);
		if (length <= 0)
		{
			return;
		}

		EnterCriticalSection(&g_logLock);
		DWORD written = 0;
		WriteFile(g_logFile, buffer, static_cast<DWORD>(length), &written, nullptr);
		FlushFileBuffers(g_logFile);
		LeaveCriticalSection(&g_logLock);
	}

	void FormatWideMessage(const char* prefix, const wchar_t* value, char* outBuffer, size_t outBufferSize)
	{
		char narrowValue[1024];
		int converted = WideCharToMultiByte(CP_UTF8, 0, value, -1, narrowValue, static_cast<int>(sizeof(narrowValue)), nullptr, nullptr);
		if (converted <= 0)
		{
			strcpy_s(narrowValue, sizeof(narrowValue), "<conversion failed>");
		}

		_snprintf_s(outBuffer, outBufferSize, _TRUNCATE, "%s %s", prefix, narrowValue);
	}
}

namespace sf::log
{
	void Open(const wchar_t* baseDir)
	{
		wchar_t logPath[MAX_PATH];
		if (swprintf(logPath, MAX_PATH, L"%s\\sf_loader.log", baseDir) < 0)
		{
			return;
		}

		g_logFile = CreateFileW(logPath, GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
		if (g_logFile == INVALID_HANDLE_VALUE)
		{
			return;
		}

		InitializeCriticalSection(&g_logLock);
		g_logInitialized = true;
	}

	void Info(const char* message)
	{
		WriteLine("INFO", message);
	}

	void Error(const char* message)
	{
		WriteLine("ERROR", message);
	}

	void InfoW(const char* prefix, const wchar_t* value)
	{
		char line[1280];
		FormatWideMessage(prefix, value, line, sizeof(line));
		Info(line);
	}

	void ErrorW(const char* prefix, const wchar_t* value)
	{
		char line[1280];
		FormatWideMessage(prefix, value, line, sizeof(line));
		Error(line);
	}

	void ErrorCode(const char* prefix, int code)
	{
		char line[256];
		_snprintf_s(line, sizeof(line), _TRUNCATE, "%s hr=0x%08X", prefix, static_cast<unsigned>(code));
		Error(line);
	}
}
