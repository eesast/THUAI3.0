@echo off
md ..\windows_only\proto_files
for /F %%a in ('dir/b^|findstr /i ".proto"') do (protoc --cpp_out=../windows_only/proto_files %%a)