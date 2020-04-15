# -*- coding=utf-8
# appid 已在配置中移除,请在参数 Bucket 中带上 appid。Bucket 由 BucketName-APPID 组成
# 1. 设置用户配置, 包括 secretId，secretKey 以及 Region
import hmac
import os
import sys
import logging
import hashlib
import json
import getopt
import urllib.request as request
from qcloud_cos import CosConfig
from qcloud_cos import CosS3Client

logging.basicConfig(level=logging.INFO, stream=sys.stdout)
logger = logging.getLogger()

# 'AKIDRn0ygI5kmY4IYYLDfaF7nDNGtFK9KYrt'      # 替换为用户的 secretId
secret_id = ''
secret_key = ''      # 替换为用户的 secretKey
region = 'ap-beijing'     # 替换为用户的 Region
token = None
scheme = 'https'
bucket_upload = 'thuai-1255334966'  # 替换为用户的存储桶

try:
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

response = client.list_objects_versions(
    Bucket=bucket_upload,
    Prefix='CAPI/windows_only'
)
for file in response['Version']:
    s = file['Key']
    if not os.path.exists(os.path.dirname(s)):
        os.makedirs(os.path.dirname(s))

    response = client.get_object(
        Bucket=bucket_upload,
        Key=s)
    response['Body'].get_stream_to_file(s)
