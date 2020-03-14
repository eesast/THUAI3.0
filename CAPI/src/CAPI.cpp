#define _CRT_SECURE_NO_WARNINGS
#ifndef NOMINMAX                      
#define NOMINMAX
#endif
#include<cstdio>
#include<HPSocket.h>
#include<HPSocket-SSL.h>
#include<HPTypeDef.h>
#include<SocketInterface.h>
#include<mutex>
#include<sstream>
#include<typeinfo>
#include<ctime>
#include<iostream>
#include"CAPI.h"
#include<thread>
#pragma comment(lib, "HPSocket.lib" ) 

#include <sys/timeb.h>

using namespace std;
mutex io_mutex;
mutex inforw_mutex;

EnHandleResult CListenerImpl::OnPrepareConnect(ITcpClient* pSender, CONNID dwConnID, SOCKET socket) 
{ 
	return HR_OK;
}

EnHandleResult CListenerImpl::OnConnect(ITcpClient* pSender, CONNID dwConnID)
{
	byte mes[maxl];
	byte* tmp = mes;
	if (pthis->PlayerId == -1)
	{
		tmp = WriteInt32((int)PacketType::IdRequest, tmp);
		Debug(1,"ClientSide: Request ID\n");
	}
	else
	{
		tmp=WriteInt32((int)PacketType::IdAllocate,tmp);
		tmp=WriteInt32(pthis->PlayerId,tmp);
		Debug(1,"ClientSide: Using Pre-Allocated ID # " + to_string(pthis->PlayerId));
	}
	tmp[0] = 0;
	pthis->Send(mes,tmp-mes,0);
	return HR_OK; 
}
EnHandleResult CListenerImpl::OnHandShake(ITcpClient* pSender, CONNID dwConnID) {  return HR_OK; }
EnHandleResult CListenerImpl::OnReceive(ITcpClient* pSender, CONNID dwConnID, int iLength) {return HR_OK;}
EnHandleResult CListenerImpl::OnReceive(ITcpClient* pSender, CONNID dwConnID, const BYTE* pData, int iLength)
{
	if (pthis->PauseUpdate)return HR_IGNORE;
	const byte* p = pData;
	INT32 type = *(INT32*)p;
	p += 4;
	Message* message = new Message();
	switch (type)
	{
	case (INT32)PacketType::IdAllocate:
		pthis->PlayerId = *(INT32*)p;
		Debug(1, "ClientSide: Allocated ID " + to_string(pthis->PlayerId));
		p = NULL;
		break;
	case (INT32)PacketType::Disconnected:
		Debug(1, "ClientSide: Disconnect Message Received.");
		pthis->Disconnect();
		break;
	case (INT32)PacketType::ProtoPacket:
		Debug(1, "ClientSide: ProtoPacket Message Received.");
		message->ParseFromArray(p, iLength - 4);
		pthis->OnReceive(message);
		break;
	default:
		throw new domain_error("unknown Packet Type ID");
		break;
	}
	return HR_OK;
}
EnHandleResult CListenerImpl::OnPrepareListen(ITcpServer* pSender, SOCKET soListen) {
	return HR_IGNORE;
};
EnHandleResult CListenerImpl::OnSend(ITcpClient* pSender, CONNID dwConnID, const BYTE* pData, int iLength)
{ 
	return HR_OK;
}
EnHandleResult CListenerImpl::OnClose(ITcpClient* pSender, CONNID dwConnID, EnSocketOperation enOperation, int iErrorCode)
{
	if (!pthis->Closed) //��������
		while (!pSender->IsConnected())
		{
			printf("ClientSide: Connecting to server %s:%d\n", pthis->ip.c_str(), pthis->port);
			pSender->Start((LPCTSTR)(pthis->ip.c_str()), pthis->port, false);
			Sleep(1000);
		}
	return HR_OK; 
}

void CAPI::OnReceive(IMessage* message)
{
	hash_t typehash = hash2(get_type(typeid(*message).name()));
	if (typehash == hash2(get_type(typeid(Message).name())))
	{
		if (((Message*)message)->content != NULL)OnReceive(((Message*)message)->content);
	}
	else if (typehash == hash2(get_type(typeid(Protobuf::PingPacket).name())))
	{
		Ping = (currentTimeMillisec() - ((Protobuf::PingPacket*)message)->ticks()) * 0.0001f;		
	}
	else if (typehash == hash2(get_type(typeid(Protobuf::AgentId).name())))
	{
		AgentId = ((Protobuf::AgentId*)message)->agent(); //AgentId��֪ͨPlayer��Ӧ��Agent
	}
	else if (typehash == hash2(get_type(typeid(Protobuf::ChatMessage).name())))
	{
		Debug(1, "ChatMessage Received\n");
		io_mutex.lock();
		buffer += ((Protobuf::ChatMessage*)message)->message();
		io_mutex.unlock();
	}
	else if (typehash == hash2(get_type(typeid(Protobuf::MessageToClient).name())))
	{
		UpdateInfo((Protobuf::MessageToClient*)message);
	}
	else
	{
		throw new domain_error("unknown protobuf packet type \n");
	}
}

void CAPI::Quit()
{
	byte bytes[4];
	WriteInt32((int)PacketType::IdRequest, bytes);
	pclient->Send(bytes, 4);
	Disconnect();
}

void CAPI::SendChatMessage(string message)
{
	Protobuf::ChatMessage* mes1 = new Protobuf::ChatMessage();
	mes1->set_message(message);
	Message* mes2 = new Message(PlayerId, mes1);
	Message* mes3 = new Message(-1, mes2);
	Message* mes = new Message(-1, mes3);
	Send(mes);
}

void CAPI::SendCommandMessage(MessageToServer* message)
{
	Message* mes2 = new Message(PlayerId, message);
	Message* mes3 = new Message(-1, mes2);
	Message* mes = new Message(-1, mes3);
	Send(mes);
}

void CAPI::UpdateInfo(Protobuf::MessageToClient* message)
{
	
}

Player CAPI::GetInfo()
{
	inforw_mutex.lock();
	Player p=player;
	inforw_mutex.unlock();
	return p;
}



CAPI::CAPI():listener(this), pclient(&listener)
{
	Closed = false;
	AgentId = 0;
	PlayerId = -1;
}

void CAPI::Initialize()
{
	buffer = "";
	Ping = -1;
	PauseUpdate = false;
}

bool CAPI::ConnectServer(const char* address, USHORT port)
{
	ip = address;
	while (!pclient->IsConnected())
	{
		Debug(1,"ClientSide: Connecting to server ");
		pclient->Start((LPCTSTR)address, port,false);
		Sleep(1000);
	}
	while (AgentId == -1 || PlayerId == -1);
	return true;
}

bool CAPI::IsConnected()
{
	return pclient->IsConnected();
}

void CAPI::Send(Message* mes) //����Message
{
	byte bytes[maxl];
	byte* p = bytes;
	p = WriteInt32((int)PacketType::ProtoPacket, p);
	p = mes->SerializeToArray(p, maxl - 4);
	pclient->Send(bytes, p-bytes);
	Debug(2, "ClientSide: Data sent ", get_type(typeid(*(mes->content)).name()).c_str());
}


void CAPI::Refresh()
{
	if (AgentId == -1 || PlayerId == -1) throw new logic_error("Can not refresh when not ready.");
	Protobuf::PingPacket* ping = new Protobuf::PingPacket();
	ping->set_ticks(currentTimeMillisec());
	Message* mes1 = new Message(PlayerId, ping);
	Message* mes2 = new Message(AgentId, mes1);
	Message* mes = new Message(-1, mes2);
	Send(mes);
}

bool CAPI::Send(const byte* pBuffer, int iLength, int iOffset )
{
	return pclient->Send(pBuffer, iLength, iOffset);
}

void CAPI::Disconnect()
{
	Debug(1, "ClientSide: Stopping");
	Closed = true;
	pclient->Stop();
}


bool CAPI::PrintBuffer()
{
	if (buffer == "")return false;
	io_mutex.lock();
	cout << buffer << endl;
	buffer = "";
	io_mutex.unlock();
	return true;
}





