#ifndef CAPI_H
#define CAPI_H
#ifndef DEVELOPER_ONLY
#error This file is only included for developers
#endif

#define _CRT_SECURE_NO_WARNINGS
#include "Message.h"
#include "Constant.h"
#include <MessageToServer.pb.h>
#include <MessageToClient.pb.h>
#include "Sema.h"

using namespace std;

class CAPI;
class CListenerImpl : public CTcpClientListener
{
private:
	CAPI *pthis;

public:
	CListenerImpl(CAPI *p) : pthis(p) {}
	virtual EnHandleResult OnPrepareConnect(ITcpClient *pSender, CONNID dwConnID, SOCKET socket);
	virtual EnHandleResult OnConnect(ITcpClient *pSender, CONNID dwConnID);
	virtual EnHandleResult OnHandShake(ITcpClient *pSender, CONNID dwConnID);
	virtual EnHandleResult OnReceive(ITcpClient *pSender, CONNID dwConnID, int iLength);
	virtual EnHandleResult OnSend(ITcpClient *pSender, CONNID dwConnID, const BYTE *pData, int iLength);
	virtual EnHandleResult OnClose(ITcpClient *pSender, CONNID dwConnID, EnSocketOperation enOperation, int iErrorCode);
	virtual EnHandleResult OnReceive(ITcpClient *pSender, CONNID dwConnID, const BYTE *pData, int iLength);
	virtual EnHandleResult OnPrepareListen(ITcpServer *pSender, SOCKET soListen);
};

class Obj;
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
	Constant::Player player;
	bool Closed;
	bool PauseUpdate;
	Sema sema;
	Sema start_game_sema;

private:
	CListenerImpl listener;
	CTcpPackClientPtr pclient;
	void CreateObj(int64_t id, Protobuf::MessageToClient* message);
	void MoveObj(int64_t id, Protobuf::MessageToClient* message, std::unordered_map<int64_t, std::shared_ptr<Obj>>& objectsToDelete);

public:
	CAPI();
	void Initialize();
	bool ConnectServer(const char *address, USHORT port);
	bool IsConnected();
	void Refresh();
	bool Send(const byte *pBuffer, int iLength, int iOffset = 0);
	void Disconnect();
	bool PrintBuffer();
	void Send(Message *mes);
	void Send(shared_ptr<Message> mes);
	void OnReceive(IMessage *message);
	void OnReceive(shared_ptr<Message> message);
	void Quit();
	void SendChatMessage(string message);
	bool SendCommandMessage(Protobuf::MessageToServer message);
	void UpdateInfo(Protobuf::MessageToClient *message);
	Constant::Player GetInfo();
};

#endif //CAPI_H