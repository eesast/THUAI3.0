#include "Sema.h"
#include <mutex>
#include <condition_variable>

void Sema::notify()
{
	std::unique_lock<std::mutex> locker(mu);
	cv.notify_one();
}
void Sema::wait()
{
	std::unique_lock<std::mutex> locker(mu);
	cv.wait(locker);
}
