#include "debug.h"
#include <iostream>
#include <stdarg.h>

#define END -1

void (*DebugFunc)(int n, std::string str, ...);

void Debug(int n, std::string str, ...)
{
	va_list ap;
	va_start(ap, str);
	std::cout << str << ' ';
	std::string temp = "";
	for (int i = 1; i < n; i++)
	{
		temp = va_arg(ap, std::string);
		std::cout << temp << ' ';
	}
	va_end(ap);
	std::cout << std::endl;
}
void Debug(int n, const char* str, ...)
{
	va_list ap;
	va_start(ap, str);
	std::cout << str << ' ';
	const char* temp = "";
	for (int i = 1; i < n; i++)
	{
		temp = va_arg(ap, const char*);
		std::cout << temp << ' ';
	}
	va_end(ap);
	std::cout << std::endl;
}
void DebugSilently(int n, std::string str, ...)
{}

