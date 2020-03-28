#include "Message.h"
#include "Constant.h"
#include <string>
#include <cstdio>
#include <cstring>
#include <sstream>
#include <sys/timeb.h>
#include <iostream>
#pragma comment(lib, "libprotobuf.lib")
#pragma comment(lib, "libprotoc.lib")

using namespace std;

typedef std::uint64_t hash_t;

INT64 currentTimeMillisec()
{
	struct timeb tb;
	ftime(&tb);
	return (tb.time * 1000 + tb.millitm) * 10000 + 621355968000000000;
}

hash_t hash2(string str)
{
	hash_t ret = basis;
	for (unsigned int i = 0; i < str.length(); i++)
	{
		ret ^= str[i];
		ret *= prime;
	}
	return ret;
}

string get_type(string s)
{
	/* 获取真实type名称*/
	int i = 0;
	while (i < s.length() && isdigit(s[i]))
		i++;
	return s.substr(i);
}

string get_string(stringstream *ss)
{
	int length;
	*ss >> length;
	char c;
	string res = "";
	for (int i = 0; i < length; i++)
	{
		*ss >> c;
		res.push_back(c);
	}
	return res;
}

string ReadString(const byte *bytes)
{
	const byte *p = bytes;
	int len = p[0];
	p++;
	string str;
	for (int i = 0; i < len; i++)
		str.push_back(p[i]);
	return str;
}

Message::Message()
{
	content = NULL;
}
Message::Message(int addr, IMessage *p)
{
	Address = addr;
	content = p;
}
Message *Message::New() const
{
	Message *mes = new Message();
	return mes;
}

int Message::GetCachedSize() const
{
	// 尚未加入类型名字符串长.暂时用不到该函数
	return 4 + content->GetCachedSize();
}

const byte *Message::ParseFromArray(const byte *bytes, int size)
{
	const byte *p = bytes;
	Address = ReadMessageInt32(p);
	if (Address >= 0 && Address <= 10)
		p += 1;
	else
		p += (Address < 0 ? 10 : 4);
	if (bytes + size - p > 0)
	{
		string type = ReadString(p);
		p += type.length() + 1;
		switch (hash2(type))
		{
		case hash_compile_time("Communication.Proto.Message"):
			content = new Message();
			content->ParseFromArray(p, bytes + size - p);
			break;
		case hash_compile_time("Communication.Proto.PingPacket"):
			content = new Protobuf::PingPacket();
			content->ParseFromArray(p, bytes + size - p);
			break;
		case hash_compile_time("Communication.Proto.ChatMessage"):
			content = new Protobuf::ChatMessage();
			content->ParseFromArray(p, bytes + size - p);
			break;
		case hash_compile_time("Communication.Proto.AgentId"):
			content = new Protobuf::AgentId();
			content->ParseFromArray(p, bytes + size - p);
			break;
		case hash_compile_time("Communication.Proto.MessageToClient"):
			content = new Protobuf::MessageToClient();
			content->ParseFromArray(p, bytes + size - p);
			break;
		default:
			throw new domain_error("unknown protobuf packet type \n");
		}
	}
	else
		content = NULL;
}

byte *Message::SerializeToArray(byte *bytes, int size)
{
	byte *p = bytes;
	p = WriteMessageInt32(this->Address, p);
	if (this->content == NULL)
		return p;
	hash_t typehash = hash2(get_type(typeid(*(this->content)).name()));
	if (typehash == hash2(get_type(typeid(Message).name())))
	{
		p = WriteString("Communication.Proto.Message", p);
		p = ((Message *)content)->SerializeToArray(p, size - 5 - strlen("Communication.Proto.Message"));
	}
	else if (typehash == hash2(get_type(typeid(Protobuf::PingPacket).name())))
	{
		p = WriteString("Communication.Proto.PingPacket", p);
		((Protobuf::PingPacket *)content)->SerializeToArray(p, size - 5 - strlen("Communication.Proto.PingPacket"));
		p += ((Protobuf::PingPacket *)content)->ByteSize();
	}
	else if (typehash == hash2(get_type(typeid(Protobuf::ChatMessage).name())))
	{
		p = WriteString("Communication.Proto.ChatMessage", p);
		((Protobuf::ChatMessage *)content)->SerializeToArray(p, size - 5 - strlen("Communication.Proto.ChatMessage"));
		p += ((Protobuf::ChatMessage *)content)->ByteSize();
	}
	else if (typehash == hash2(get_type(typeid(Protobuf::MessageToServer).name())))
	{
		p = WriteString("Communication.Proto.MessageToServer", p);
		((Protobuf::MessageToServer *)content)->SerializeToArray(p, size - 5 - strlen("Communication.Proto.ChatMessage"));
		p += ((Protobuf::MessageToServer *)content)->ByteSize();
	}
	else
	{
		throw new domain_error("unknown protobuf packet type \n");
	}
	return p;
}

google::protobuf::Metadata Message::GetMetadata() const
{
	return google::protobuf::Metadata();
}

byte *WriteInt32(INT64 x, byte *bytes)
{
	for (int i = 0; i < 4; i++)
	{
		bytes[i] = x & 255;
		x = x >> 8;
	}
	return &bytes[4];
}

byte *WriteMessageInt32(INT64 x, byte *bytes)
{
	assert(x > -3);
	if (x == -1)
	{
		for (int i = 0; i < 9; i++)
		{
			bytes[i] = 255;
		}
		bytes[9] = 1;
		return &bytes[10];
	}
	else if (x == 0)
	{
		bytes[0] = 0;
		return bytes + 1;
	}
	else
	{
		bytes[0] = x;
		return bytes + 1;
	}
	//else for (int i = 0; i < 4; i++)
	//{
	//	bytes[i] = x & 255;
	//	x = x >> 8;
	//}
	return &bytes[4];
}

int ReadMessageInt32(const byte *bytes)
{
	if (bytes[0] >= 0 && bytes[0] <= 10)
	{
		return (int)bytes[0];
	}
	const byte *p = bytes;
	return *(INT32 *)p;
}

byte *WriteString(string str, byte *bytes)
{
	int len = str.length();
	byte *p = bytes;
	p[0] = (unsigned char)len;
	p++;
	for (int i = 0; i < len; i++)
	{
		p[i] = str[i];
	}
	return &p[len];
}

//const double Player::InitMoveSpeed = 5.0;
//const int Player::InitSightRange = 9;
