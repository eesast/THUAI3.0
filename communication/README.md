# THUAI3.0 通信

THUAI3.0 原电子系第21届队式程序设计大赛

## 设计思路（参考）

目标框架：.Net Standard 2, .Net Core 2.1

Server设计：

- 采用HP-Socket库TcpPackServer（或其他设计合理的Socket框架）
- 基于事件驱动，将收到信息传输给逻辑部分进行实时处理
- 计算通信延时
- 断线重连
- （饼）支持向Agent的增量更新与校验

Agent设计：

- 采用...（待调研）
- 主动轮询更新信息
- 合理的分发（C++/Unity）
- 断线重连
- （饼）支持数据的增量更新与校验

Client设计：

- 尽量简单轻量

重点：

- Protobuf的使用
- 资源的互斥访问（与逻辑）
- 事件驱动

难点（饼）：

- 增量更新
- 断线重连机制

## 开发组成员

（自己加）

## 10.3 基本通信框架

分为Client/Agent/Server三个部分，Client部分用c#模拟CAPI来进行测试工作

### Client部分

- 实现ICAPI接口
- 在CAPI中进行通讯，初步和logic的client进行对接，并且分离通信部分到CAPI中

TODO:

- 使用c++进行实现
- 客户端断线重连
- 将资源数据内置于CAPI内
- 细分包类型

### Agent部分

- 编号并转发客户端包
- 服务端包解包并分发给客户端
- 在客户端人满后向Server请求开始游戏

TODO:

- 连接到Server后与同步总人数
- 轮询功能
- 客户端断线重连
- 转发到unity

### Server部分

- 拆分logic的server通信部分
- 目前用IGame接口与logic对接 （？）

TODO:

- 支持多Agent （？）
- 细分包类型
