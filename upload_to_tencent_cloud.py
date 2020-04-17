# -*- coding=utf-8
# appid 已在配置中移除,请在参数 Bucket 中带上 appid。Bucket 由 BucketName-APPID 组成
# 1. 设置用户配置, 包括 secretId，secretKey 以及 Region
import os
import sys
import logging
import hashlib
import json
import getopt
import urllib.request as request
from qcloud_cos import CosConfig
from qcloud_cos import CosS3Client
from fnmatch import fnmatch

logging.basicConfig(level=logging.INFO, stream=sys.stdout)
logger = logging.getLogger()

secret_id = ''      # 替换为用户的 secretId
secret_key = ''      # 替换为用户的 secretKey
region = 'accelerate'     # 替换为用户的 Region
token = None
scheme = 'https'
dirs = []   # 替换为需要上传的文件夹
bucket_upload = 'thuai-1255334966'  # 替换为用户的存储桶


try:
    print(sys.argv[1:])
    opts, dirs = getopt.getopt(sys.argv[1:], "i:k:", ["id=", "key="])
except getopt.GetoptError:
    print('Error: --id <secret_id> --key <secret_key>')
    sys.exit(2)

for opt, arg in opts:
    if opt in ("-i", "--id"):
        secret_id = arg
    elif opt in ("-k", "--key"):
        secret_key = arg


config = CosConfig(Region=region, SecretId=secret_id,
                   SecretKey=secret_key, Token=token, Scheme=scheme)

client = CosS3Client(config)

clientNative = CosS3Client(CosConfig(
    Region="ap-beijing", SecretId=secret_id, SecretKey=secret_key, Token=token, Scheme=scheme))

md5list = {}

try:
    url = client.get_presigned_url(
        Bucket=bucket_upload, Key='md5list.json', Method='GET')
    cloud_list = json.loads(request.urlopen(url).read())
except:
    logger.info("cloud_list could not be load !")
    cloud_list = None

res = clientNative.list_objects_versions(
    Bucket=bucket_upload,
    Prefix="CAPI/windows_only/"
)
if 'Version' in res:
    for item in res['Version']:
        if (("CAPI/windows_only/include" in item['Key']) or ("CAPI/windows_only/dll" in item['Key']) or ("CAPI/windows_only/lib" in item['Key'])):
            logger.info(
                item['Key'] + " is not in local but in need to be in cloud")
            md5list[item['Key']] = item['ETag'].strip('"')


def upload_local_file(client, src, archivename):
    if os.path.isfile(src):
        with open(src, 'rb') as f:
            md5 = hashlib.md5()
            md5.update(f.read())
            md5list[src] = md5.hexdigest()
        if cloud_list != None:
            if archivename in cloud_list:
                if md5list[src] == cloud_list[archivename]:
                    logger.info(
                        "existed in md5list and didnot change , skip " + src)
                    return
        res = clientNative.list_objects_versions(
            Bucket=bucket_upload,
            Prefix=archivename
        )
        if 'Version' in res:
            for file in res['Version']:
                if file['ETag'].strip('"') == md5list[src]:
                    logger.info(
                        "Not existed in md5list but in cloud , skip " + src)
                    return
                break
        logger.info("uploading file " + src)
        response = client.put_object_from_local_file(
            Bucket=bucket_upload,
            LocalFilePath=src,
            Key=archivename)
    elif os.path.isdir(src):
        logger.info("uploading folder "+src)
        ignorelist = []
        if (os.path.exists(src + "/.uploadignore")):
            ignorelist = open(src + "/.uploadignore").read().splitlines()
            logger.info(ignorelist)
        for filename in os.listdir(src):
            isContinue = False
            for ig in ignorelist:
                if (fnmatch(filename, ig)):
                    print("ignore list match : " + filename + " , " + ig)
                    isContinue = True
                    break
            if (isContinue):
                continue
            upload_local_file(client, src + "/" + filename,
                              archivename + "/" + filename)
    else:
        logger.info("upload fail")


logger.info("start to upload")
for dir_ in dirs:
    upload_local_file(client, dir_, dir_)
md5list_path = 'md5list.json'

with open(md5list_path, 'w') as f:
    json.dump(md5list, f, indent=4)

client.put_object_from_local_file(
    Bucket=bucket_upload,
    LocalFilePath=md5list_path,
    Key=md5list_path
)
