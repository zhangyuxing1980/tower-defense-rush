#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Unity PlayMode Test Runner - Extended Timeout (4 min)"""

import http.client
import json
import time
import sys

class MCPUnityClient:
    def __init__(self, host='127.0.0.1', port=8080):
        self.session_id = None
        self.instance_id = None
        self.request_id = 0
        self.conn = http.client.HTTPConnection(host, port, timeout=30)

    def _make_request(self, method: str, params: dict):
        self.request_id += 1
        payload = {
            'jsonrpc': '2.0',
            'method': method,
            'params': params,
            'id': self.request_id
        }
        headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json, text/event-stream'
        }
        if self.session_id:
            headers['Mcp-Session-Id'] = self.session_id

        try:
            self.conn.request('POST', '/mcp', body=json.dumps(payload), headers=headers)
            response = self.conn.getresponse()
            new_session = response.getheader('Mcp-Session-Id')
            if new_session:
                self.session_id = new_session
            content = response.read().decode('utf-8')
            result = None
            for line in content.split('\n'):
                if line.startswith('data:'):
                    try:
                        data = json.loads(line[5:].strip())
                        if 'result' in data:
                            result = data
                        elif result is None:
                            result = data
                    except:
                        pass
            return result
        except Exception as e:
            print(f'[ERROR] Request failed: {e}')
            return None

    def initialize(self):
        result = self._make_request('initialize', {
            'protocolVersion': '2024-11-05',
            'capabilities': {'tools': {}},
            'clientInfo': {'name': 'TestRunner', 'version': '1.0'}
        })
        return result and 'result' in result

    def get_instances(self):
        result = self._make_request('resources/read', {'uri': 'mcpforunity://instances'})
        if result and 'result' in result:
            try:
                content = result['result']['contents'][0]['text']
                data = json.loads(content)
                return data.get('instances', [])
            except:
                pass
        return None

    def enter_play_mode(self):
        result = self._make_request('tools/call', {
            'name': 'manage_editor',
            'arguments': {'unity_instance': self.instance_id, 'action': 'play'}
        })
        return result and 'result' in result

    def exit_play_mode(self):
        result = self._make_request('tools/call', {
            'name': 'manage_editor',
            'arguments': {'unity_instance': self.instance_id, 'action': 'stop'}
        })
        return result and 'result' in result

    def read_console(self, count: int = 100):
        result = self._make_request('tools/call', {
            'name': 'read_console',
            'arguments': {'unity_instance': self.instance_id, 'count': count}
        })
        if result and 'result' in result:
            try:
                content = result['result']['content'][0]['text']
                data = json.loads(content)
                return data.get('data', [])
            except:
                pass
        return []

    def close(self):
        self.conn.close()


def main():
    print('=' * 60)
    print('Unity PlayMode Test Runner - Extended Timeout (4 min)')
    print('=' * 60)

    client = MCPUnityClient()

    if not client.initialize():
        print('[FAIL] MCP init failed')
        sys.exit(1)

    instances = client.get_instances()
    if not instances:
        print('[FAIL] No Unity instance found')
        client.close()
        sys.exit(1)

    instance = instances[0]
    client.instance_id = instance.get('id')
    print(f'[OK] Found Unity instance: {client.instance_id}')

    if not client.enter_play_mode():
        print('[FAIL] Cannot enter Play Mode')
        client.close()
        sys.exit(1)
    print('[OK] Entered Play Mode')

    print()
    print('Waiting for tests to complete (up to 4 minutes)...')
    print('-' * 60)

    all_logs = []
    last_log_count = 0
    test_completed = False
    max_wait = 240  # 4 minutes
    start_time = time.time()

    while time.time() - start_time < max_wait and not test_completed:
        time.sleep(3)
        logs = client.read_console(count=100)
        if logs:
            new_logs = logs[last_log_count:]
            for log in new_logs:
                # Print key test events
                if any(k in log for k in ['测试', 'Test', '>>', '<<', '通过', '失败', '完成', '摘要']):
                    print(f'  {log[:120]}')
                # Check for completion
                if '========== 测试执行完成 ==========' in log:
                    test_completed = True
                    print('[OK] Test completion detected!')
            all_logs.extend(new_logs)
            last_log_count = len(logs)

    print('-' * 60)

    if not test_completed:
        print('[WARNING] Timeout - tests did not complete within 4 minutes')

    client.exit_play_mode()
    print('[OK] Exited Play Mode')

    # Analyze results
    print()
    print('=' * 60)
    print('Test Results Summary')
    print('=' * 60)

    # Count pass/fail
    passed = [log for log in all_logs if '<<< 测试通过' in log]
    failed = [log for log in all_logs if '<<< 测试失败' in log]
    errors = [log for log in all_logs if '[Error]' in log and '测试' not in log]

    for log in passed:
        test_name = log.split('<<< 测试通过:')[1].split('(')[0].strip() if '<<< 测试通过:' in log else 'Unknown'
        print(f'[PASS] {test_name}')

    for log in failed:
        test_name = log.split('<<< 测试失败:')[1].split('-')[0].strip() if '<<< 测试失败:' in log else 'Unknown'
        print(f'[FAIL] {test_name}')

    # Look for summary
    summary_logs = [log for log in all_logs if '测试摘要' in log or '通过:' in log or '失败:' in log]
    for log in summary_logs[-5:]:  # Last 5 summary lines
        print(f'  {log[:100]}')

    print()
    print(f'Total Passed: {len(passed)}')
    print(f'Total Failed: {len(failed)}')
    print(f'Unity Errors: {len(errors)}')

    # Check for JIMMY_001 specifically
    jimmy_passed = any('JIMMY_001' in log or '吉米AI行为测试' in log for log in passed)
    jimmy_failed = any('JIMMY_001' in log or '吉米AI行为测试' in log for log in failed)

    print()
    print('=' * 60)
    if jimmy_passed:
        print('JIMMY_001: PASSED')
    elif jimmy_failed:
        print('JIMMY_001: FAILED')
    else:
        print('JIMMY_001: Status unknown (may have timed out)')
    print('=' * 60)

    # Save detailed report
    report = {
        'timestamp': time.strftime('%Y%m%d_%H%M%S'),
        'jimmy_001_passed': jimmy_passed,
        'jimmy_001_failed': jimmy_failed,
        'total_passed': len(passed),
        'total_failed': len(failed),
        'unity_errors': len(errors),
        'all_logs': all_logs
    }

    filename = f"ExtendedTestReport_{time.strftime('%Y%m%d_%H%M%S')}.json"
    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(report, f, ensure_ascii=False, indent=2)
    print(f'\n[OK] Detailed report saved: {filename}')

    client.close()


if __name__ == '__main__':
    main()
