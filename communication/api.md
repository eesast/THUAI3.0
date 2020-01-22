
# api接口（参考）

调用方式分为post和get两种，均可以使用，post时参数使用json传递，utf8编码

GET /login?username=admin&password=admin
POST /login {"username":"admin","password":"admin"}
效果相同

返回结果必定含有status, result
status = success 调用成功
status = failed 参数错误
status = error 内部错误（也有可能是参数错误）
结果均存储在result中

## /register

注册新用户，需要admin用户的token

### 参数

- username: 新用户名
- password: 新用户密码
- token: 必须是admin的token

### 调用成功时返回

无

## /login

用户登录

### 参数

- username: 用户名
- password: 用户密码

### 调用成功时返回

token

## /create

新建房间

### 参数

token

### 调用成功时返回

room ID

## /query

查询所有房间

### 参数

token

### 调用成功时返回

一个列表，里面包含{"id":roomid, "count": 人数}

## /upload

上传源码

### 参数

- token
- sharing: 是否允许他人挑战(bool)
- name: 文件名(相同文件名会覆盖)
- data: b64编码的文件内容(感觉有些奇怪?)

### 调用成功时返回

无

## /join

加入房间

### 参数

- token
- name: 上传时的文件名，如果name和token对应的用户不同且未开启运行他人挑战则会调用失败
- room: roomID

### 调用成功时返回

无

## /status

查询房间状态

### 参数

- token
- room: roomID

### 调用成功时返回

一个enum表示目前房间状态（仅测试）
