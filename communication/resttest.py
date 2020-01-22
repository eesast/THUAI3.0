from requests import get, post
from json import loads, dumps
from time import sleep
from base64 import b64encode

# login test
result = loads(get('http://localhost:8080/login?username=admin&password=admin').content.decode('utf-8'))
print(result)

# register test
token = result['result']
result = loads(get('http://localhost:8080/register?username=test&password=test&token={}'.format(token)).content.decode('utf-8'))
print(result)
result = loads(get('http://localhost:8080/login?username=test&password=test').content.decode('utf-8'))
print(result)

# create room test
token = result['result']
result = loads(get('http://localhost:8080/create?token={}'.format(token)).content.decode('utf-8'))
print(result)

# query room test
sleep(3)
room = result['result']
result = loads(get('http://localhost:8080/query?token={}'.format(token)).content.decode('utf-8'))
print(result)

# room status test
for _ in range(0, 5):
    result = loads(get('http://localhost:8080/status?token={}&room={}'.format(token, room)).content.decode('utf-8'))
    print(result)
    sleep(0.5)

# upload file test & post test
data = {
    'token' : token,
    'sharing' : True,
    'name' : 'Hello World',
    'data' : b64encode(open('HelloWorld.exe', 'rb').read()).decode('utf-8')
}
result = loads(post('http://localhost:8080/upload', data = dumps(data)).content.decode('utf-8'))
print(result)

#join room test
result = loads(get('http://localhost:8080/join?token={}&name=Hello+World&room={}'.format(token, room)).content.decode('utf-8'))
print(result)