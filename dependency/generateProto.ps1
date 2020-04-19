$windows_protopath = "../CAPI/windows_only/proto_files"

foreach ($o in Get-ChildItem -Recurse -Include *.proto) {
    Write-Output ($o.Name)
    protoc --csharp_out=../communication/Proto $o.Name
    protoc --cpp_out=$windows_protopath $o.Name
}

(Get-Content $windows_protopath/MessageToClient.pb.h).replace("// source: MessageToClient.proto", "// source: MessageToClient.proto`n`n#undef min`n#undef max") | Set-Content $windows_protopath/MessageToClient.pb.h
(Get-Content $windows_protopath/MessageToServer.pb.h).replace("// source: MessageToServer.proto", "// source: MessageToServer.proto`n`n#undef min`n#undef max") | Set-Content $windows_protopath/MessageToServer.pb.h