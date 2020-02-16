for /F %%a in ('dir/b^|find /i ".proto"') do (protoc --csharp_out=. %%a)
pause