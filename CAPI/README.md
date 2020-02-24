# THUAI3.0 选手接口
THUAI3.0 原电子系第21届队式程序设计大赛

## 设计思路（参考）
目标框架：C++11及以上

多线程设计：
- 通信线程：监听代理Agent推送的信息更新与发送信息
- AI线程：死循环

重点：
- Protobuf的使用
- 资源的互斥访问（避免选手使用数据的同时通信线程修改它）

难点（饼）：
- 设计良好的debug接口

## 开发组成员
常灿，王冲

## 说明

- Windows使用时，需要将dll目录中文件放入C:\Windows\SysWOW64目录中。通过win_make.bat 生成项目，需要在VS中将项目平台修改为WIN32 Release，项目属性——C/C++——Code Generation中runtime library设为Multi-threaded, 项目属性——linker——command line中若出现/machine:x64字样，将x64改成x86.

- Linux下在build文件夹中通过命令cmake .. && make生成可执行文件AI(*需事先安装protobuf*), 已在docker中测试过基本通信功能。

- API.h文档中定义了所有的玩家接口。Player.cpp中的player函数为唯一的选手可编辑函数。

- 命令行读入2个参数Agent_ip和Agent_port

- 文件结构树形图
  ![1582509497220.png](https://i.loli.net/2020/02/24/OybS8fptQXLWgV3.png)

  ![2222]( https://i.loli.net/2020/02/24/w2hGtpYrF3TgBS7.png)

  其中，windows的依赖包括protobuf, hpsocket 和pthread (用于跨平台，模拟Linux的pthread) ；Linux的依赖包括hpsocket和其他平台相关依赖。

  打包下载: https://cloud.tsinghua.edu.cn/d/b4f6d32dfbeb4dfa8fe7/