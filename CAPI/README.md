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

## 文件结构

- include文件存放公共头文件，其中CAPI.h定义了通信接口，API.h 定义了选手接口，Constant.h为常量表，部分通过python生成，Structures.h定义了所有数据结构

- windows_only和linux_only分别windows和linux平台下的包含文件和库文件 （目前linux上大部分依赖在构建docker时直接下载）

- proto存放protobuf文件，cmake时生成相应.h和.cc文件

- src存放源文件

## Windows上生成分发

- 目前仅支持使用VS2019  (VS2017缺少相应工具集)

- 项目生成：通过win_make.bat创建build目录并在其下生成解决方案AI.sln

- 项目构建：平台使用win32 Release，项目属性——C/C++——Code Generation中runtime library设为Multi-threaded, 项目属性——linker——command line中若出现/machine:x64字样，将x64改成x86。右键项目名选择构建即可生成AI.exe文件

- 运行程序: 需事先将windows_only中dll目录下文件放入C:/Windows/SysWOW64目录下或build/release目录下。命令行读入2个参数Agent_ip和Agent_port

- 项目分发：命令行运行``` python modify.py ```将项目引用的相应绝对路径改为相对路径。**此时解决方案部分路径并未成功加载，原因暂时未知**，需在项目包含目录中添加 ```..\windows_only\include;..\include;..\windows_only\proto_files;%(AdditionalIncludeDirectories) ```
  在库目录中添加```../windows_only/lib;../windows_only/lib/$(Configuration);%(AdditionalLibraryDirectories)```

  将附加库改为```gmock.lib;gmock_main.lib;libprotobuf.lib;libprotobuf-lite.lib;libprotoc.lib;pthreadVSE2.lib;HPSocket.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;comdlg32.lib;advapi32.lib```

## Linux上构建

- Linux平台目前提供了2个docker镜像。编译镜像环境包括编译工具，protobuf和hpsocket。运行run.sh后将选手程序送进第一个容器编译，将编译信息和可执行文件返回给主机，若编译成功则在第二个容器中运行。
