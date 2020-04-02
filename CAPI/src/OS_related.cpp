#include "OS_related.h"

#ifdef __linux__

#include <dlfcn.h>
#include <sys/utsname.h>
#include <unistd.h>
#include <sys/timeb.h>

void Sleep(int x)
{
	usleep(x * 1000);
}

#endif
