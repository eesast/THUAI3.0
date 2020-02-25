#ifndef CAPI_H
#define CAPI_H

#define _CRT_SECURE_NO_WARNINGS
#include "Message.h"
#include "constants.h"

using namespace std;

class CAPI;
class CListenerImpl : public CTcpClientListener
{
private:
	CAPI* pthis;
public:
	CListenerImpl(CAPI* p) :pthis(p) {}
	virtual EnHandleResult OnPrepareConnect(ITcpClient* pSender, CONNID dwConnID, SOCKET socket);
	virtual EnHandleResult OnConnect(ITcpClient* pSender, CONNID dwConnID);
	virtual EnHandleResult OnHandShake(ITcpClient* pSender, CONNID dwConnID);
	virtual EnHandleResult OnReceive(ITcpClient* pSender, CONNID dwConnID, int iLength);
	virtual EnHandleResult OnSend(ITcpClient* pSender, CONNID dwConnID, const BYTE* pData, int iLength);
	virtual EnHandleResult OnClose(ITcpClient* pSender, CONNID dwConnID, EnSocketOperation enOperation, int iErrorCode);
	virtual EnHandleResult OnReceive(ITcpClient* pSender, CONNID dwConnID, const BYTE* pData, int iLength);
	virtual EnHandleResult OnPrepareListen(ITcpServer* pSender, SOCKET soListen);
};

class CAPI
{
public:
	string ip;
	int port;
	float Ping;
	int PlayerId;
	int AgentId;
	int AgentCount;
	string buffer;
	Player player;
	bool Closed;
	bool PauseUpdate;
private:
	CListenerImpl listener;
	CTcpPackClientPtr pclient;
public:
	CAPI();
	void Initialize();
	bool ConnectServer(const char* address, USHORT port);
	bool IsConnected();
	void Refresh();
	bool Send(const byte* pBuffer, int iLength, int iOffset = 0);
	void Disconnect();
	bool PrintBuffer();
	void Send(Message* mes);
	void OnReceive(IMessage* message);
	void Quit();
	void SendChatMessage(string message);
	void UpdateInfo(Protobuf::MessageToClient* message);
	Player GetInfo();
};


#endif 