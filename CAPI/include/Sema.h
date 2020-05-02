#pragma once
#ifndef SEMA_H
#define SEMA_H
#endif // !SEMA_H

#include <mutex>
#include <condition_variable>

class Sema
{
private:
	std::mutex mu;
	std::condition_variable cv;

public:
	void notify();
	void wait();
};

