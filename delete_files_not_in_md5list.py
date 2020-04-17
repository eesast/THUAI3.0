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


url = client.get_presigned_url(
    Bucket=bucket_upload, Key='md5list.json', Method='GET')
cloud_list = json.loads(request.urlopen(url).read())

response = client.list_objects_versions(
    Bucket=bucket_upload,
    Prefix=''
)
for file_version in response['Version']:
    filename = file_version['Key']
    if not filename in cloud_list:
        client.delete_object(
            Bucket=bucket_upload,
            Key=filename
        )
