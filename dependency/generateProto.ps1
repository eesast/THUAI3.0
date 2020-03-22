foreach ($o in Get-ChildItem -Recurse -Include *.proto) {
    Write-Output ($o.Name)
    protoc --csharp_out=../communication/Proto $o.Name
    protoc --cpp_out=../CAPI/windows_only/proto_files $o.Name
}