#ifndef MESSAGE_H
#define MESSAGE_H

#include "OS_related.h"
#include <string>
#include <sys/timeb.h>
#include <iostream>
#include <HPSocket.h>
#include <HPSocket-SSL.h>
#include <HPTypeDef.h>
#include <SocketInterface.h>
#include "ChatMessage.pb.h"
#include "PingPacket.pb.h"
#include "AgentId.pb.h"
#include "MessageToClient.pb.h"
#include "MessageToServer.pb.h"
#include "debug.h"
#include <pthread.h>

using namespace std;
#pragma comment(lib, "HPSocket.lib")
#pragma comment(lib, "libprotobuf.lib")
#pragma comment(lib, "libprotoc.lib")
#define IMessage google::protobuf::Message
typedef std::uint64_t hash_t;
typedef BYTE byte;
constexpr hash_t prime = 0x100000001B3ull;
constexpr hash_t basis = 0xCBF29CE484222325ull;
const int maxl = 1000;
using namespace Protobuf;

long long currentTimeMillisec();

hash_t hash2(string str);

constexpr hash_t hash_compile_time(char const *str, hash_t last_value = basis)
{
	return *str ? hash_compile_time(str + 1, (*str ^ last_value) * prime) : last_value;
}

string get_type(string s);

string get_string(stringstream *ss);

namespace PacketType
{
enum PacketType
{
	ProtoPacket = 0, //内容包(S2C,C2S)
	IdRequest = 1,   //Client请求ID(C2S)
	IdAllocate = 2,  //Client请求分配ID(C2S)，Server给Client分配ID(S2C)
	Disconnected = 3 //Server主动断开(S2C)
};
}

string ReadString(const byte *bytes);
class Message : public IMessage
{
public:
	int Address; //发送者/接收者，因环境而定
	IMessage *content;
	Message();
	Message(int addr, IMessage *p);
	Message *New() const override;
	int GetCachedSize() const;
	const byte *ParseFromArray(const byte *bytes, int size);
	byte *SerializeToArray(byte *bytes, int size);
	~Message();

protected:
	virtual google::protobuf::Metadata GetMetadata() const;
};

byte *WriteInt32(INT64 x, byte *bytes);

byte *WriteString(string str, byte *bytes);

int ReadMessageInt32(const byte *bytes);

byte *WriteMessageInt32(INT64 x, byte *bytes);

#endif
