# -*- coding=utf-8
# appid 已在配置中移除,请在参数 Bucket 中带上 appid。Bucket 由 BucketName-APPID 组成
# 1. 设置用户配置, 包括 secretId，secretKey 以及 Region
import os
import sys
import logging
import hashlib
import json
import getopt
from qcloud_cos import CosConfig
from qcloud_cos import CosS3Client
from fnmatch import fnmatch

logging.basicConfig(level=logging.INFO, stream=sys.stdout)
logger = logging.getLogger()

secret_id = ''      # 替换为用户的 secretId
secret_key = ''      # 替换为用户的 secretKey
region = 'ap-beijing'     # 替换为用户的 Region
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

md5list = {}


def upload_local_file(client, src, archivename):
    logger.info("uploading "+src)
    if os.path.isfile(src):
        with open(src, 'rb') as f:
            md5 = hashlib.md5()
            md5.update(f.read())
            md5list[src] = md5.hexdigest()
        logger.info("filename is [%s]", src)
        response = client.put_object_from_local_file(
            Bucket=bucket_upload,
            LocalFilePath=src,
            Key=archivename)
    elif os.path.isdir(src):
        ignorelist = []
        if(os.path.exists(src+"/.uploadignore")):
            ignorelist = open(src+"/.uploadignore").readlines()
        for filename in os.listdir(src):
            isContinue = False
            for ig in ignorelist:
                if (fnmatch(filename, ig)):
                    isContinue = True
                    break
            if (isContinue):
                continue
            upload_local_file(client, src+"/"+filename,
                              archivename+"/"+filename)
    else:
        logger.info("upload fail")


logger.info("start to upload")
for dir_ in dirs:
    upload_local_file(client, dir_, dir_)
md5list_path = 'md5list.json'

with open(md5list_path, 'w') as f:
    json.dump(md5list, f, indent=4)

client.delete_object(
    Bucket=bucket_upload,
    Key=md5list_path
)
client.put_object_from_local_file(
    Bucket=bucket_upload,
    LocalFilePath=md5list_path,
    Key=md5list_path
)
