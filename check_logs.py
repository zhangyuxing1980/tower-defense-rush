#!/usr/bin/env python3
import http.client
import json

conn = http.client.HTTPConnection('127.0.0.1', 8080, timeout=30)

# Initialize
payload = {'jsonrpc': '2.0', 'method': 'initialize', 'params': {'protocolVersion': '2024-11-05', 'capabilities': {'tools': {}}, 'clientInfo': {'name': 'LogChecker', 'version': '1.0'}}, 'id': 1}
conn.request('POST', '/mcp', body=json.dumps(payload), headers={'Content-Type': 'application/json'})
resp = conn.getresponse()
session_id = resp.getheader('Mcp-Session-Id')
resp.read()

# Get instances
payload = {'jsonrpc': '2.0', 'method': 'resources/read', 'params': {'uri': 'mcpforunity://instances'}, 'id': 2}
headers = {'Content-Type': 'application/json'}
if session_id:
    headers['Mcp-Session-Id'] = session_id
conn.request('POST', '/mcp', body=json.dumps(payload), headers=headers)
resp = conn.getresponse()
content = resp.read().decode('utf-8')

for line in content.split('\n'):
    if line.startswith('data:'):
        try:
            data = json.loads(line[5:].strip())
            if 'result' in data and 'contents' in data['result']:
                instances = json.loads(data['result']['contents'][0]['text'])['instances']
                instance_id = instances[0]['id']
                print(f'Instance: {instance_id}')

                # Read console
                payload = {'jsonrpc': '2.0', 'method': 'tools/call', 'params': {'name': 'read_console', 'arguments': {'unity_instance': instance_id, 'count': 300}}, 'id': 3}
                conn.request('POST', '/mcp', body=json.dumps(payload), headers=headers)
                resp = conn.getresponse()
                content2 = resp.read().decode('utf-8')

                for line2 in content2.split('\n'):
                    if line2.startswith('data:'):
                        try:
                            data2 = json.loads(line2[5:].strip())
                            if 'result' in data2 and 'content' in data2['result']:
                                logs = json.loads(data2['result']['content'][0]['text'])['data']
                                print(f'Total logs: {len(logs)}')
                                print()
                                print('=== Recent Test Logs ===')
                                for log in logs[-100:]:
                                    if any(k in log for k in ['测试', 'Test', '通过', '失败', 'Error', 'SYNERGY', 'JIMMY', 'COMBAT', 'PERF', 'WAVE', '==========']):
                                        print(log[:120])
                        except Exception as e:
                            print(f'Error parsing logs: {e}')
        except Exception as e:
            print(f'Error: {e}')

conn.close()
