#ifndef DEBUG_H
#define DEBUG_H

#include <string>

extern void (*DebugFunc)(int n, std::string str, ...);
extern void DebugSilently(int n, std::string str, ...);

void Debug(int n, std::string str, ...);
void Debug(int n, const char* str, ...);

#endif