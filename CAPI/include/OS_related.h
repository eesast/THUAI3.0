#ifndef OS_RELATED_H
#define OS_RELATED_H

#ifdef WIN32

#endif

#ifdef __linux__

#include <dlfcn.h>
#include <sys/utsname.h>
#include <unistd.h>
#include <sys/timeb.h>

void Sleep(int x);

#endif

#endif
