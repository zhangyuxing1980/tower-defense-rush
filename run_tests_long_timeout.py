#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Unity PlayMode Test Runner - Very Long Timeout (6 min)"""

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

    def read_console(self, count: int = 200):
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
    print('=' * 70)
    print('Unity PlayMode Test Runner - Very Long Timeout (6 min)')
    print('=' * 70)

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

    # Clear console before starting
    print('[*] Clearing console...')
    client._make_request('tools/call', {
        'name': 'read_console',
        'arguments': {'unity_instance': client.instance_id, 'action': 'clear'}
    })

    if not client.enter_play_mode():
        print('[FAIL] Cannot enter Play Mode')
        client.close()
        sys.exit(1)
    print('[OK] Entered Play Mode')

    print()
    print('Waiting for tests to complete (up to 6 minutes)...')
    print('Test sequence: SYNERGY_001 -> JIMMY_001 -> COMBAT_001 -> PERF_001 -> WAVE_001')
    print('-' * 70)

    all_logs = []
    last_log_count = 0
    test_completed = False
    max_wait = 360  # 6 minutes
    start_time = time.time()
    last_status_time = start_time

    # Track which tests we've seen
    seen_tests = {
        'SYNERGY_001': {'started': False, 'passed': False, 'failed': False},
        'JIMMY_001': {'started': False, 'passed': False, 'failed': False},
        'COMBAT_001': {'started': False, 'passed': False, 'failed': False},
        'PERF_001': {'started': False, 'passed': False, 'failed': False},
        'WAVE_001': {'started': False, 'passed': False, 'failed': False},
    }

    while time.time() - start_time < max_wait and not test_completed:
        time.sleep(5)
        logs = client.read_console(count=200)
        if logs:
            new_logs = logs[last_log_count:]
            for log in new_logs:
                # Track test progress
                for test_id in seen_tests:
                    if test_id in log:
                        if '开始测试' in log or '>>> 开始' in log:
                            seen_tests[test_id]['started'] = True
                            print(f'[{test_id}] Started')
                        elif '测试通过' in log or '<<< 测试通过' in log:
                            seen_tests[test_id]['passed'] = True
                            print(f'[{test_id}] PASSED')
                        elif '测试失败' in log or '<<< 测试失败' in log:
                            seen_tests[test_id]['failed'] = True
                            print(f'[{test_id}] FAILED')

                # Print key test events
                if any(k in log for k in ['测试', 'Test', '>>', '<<', '通过', '失败', '完成', '摘要', '超时']):
                    print(f'  {log[:120]}')

                # Check for completion
                if '========== 测试执行完成 ==========' in log:
                    test_completed = True
                    print('[OK] All tests completed!')

            all_logs.extend(new_logs)
            last_log_count = len(logs)

        # Print status every 30 seconds
        if time.time() - last_status_time > 30:
            elapsed = int(time.time() - start_time)
            print(f'[*] Still running... ({elapsed}s elapsed)')
            last_status_time = time.time()

    print('-' * 70)

    if not test_completed:
        print('[WARNING] Timeout - tests did not complete within 6 minutes')

    client.exit_play_mode()
    print('[OK] Exited Play Mode')

    # Analyze results
    print()
    print('=' * 70)
    print('Test Results Summary')
    print('=' * 70)

    # Count pass/fail from logs
    passed = [log for log in all_logs if '<<< 测试通过' in log]
    failed = [log for log in all_logs if '<<< 测试失败' in log]
    timeouts = [log for log in all_logs if '超时' in log or 'timeout' in log.lower()]
    errors = [log for log in all_logs if '[Error]' in log and '测试' not in log]

    print()
    print('Individual Test Results:')
    for test_id, status in seen_tests.items():
        if status['passed']:
            print(f'  {test_id}: PASSED')
        elif status['failed']:
            print(f'  {test_id}: FAILED')
        elif status['started']:
            print(f'  {test_id}: STARTED (no result)')
        else:
            print(f'  {test_id}: NOT STARTED')

    # Look for summary
    print()
    summary_logs = [log for log in all_logs if '测试摘要' in log or '通过:' in log or '失败:' in log or '总计:' in log]
    if summary_logs:
        print('Summary from logs:')
        for log in summary_logs[-10:]:
            print(f'  {log[:100]}')

    print()
    print(f'Total Passed (from pass messages): {len(passed)}')
    print(f'Total Failed (from fail messages): {len(failed)}')
    print(f'Timeouts detected: {len(timeouts)}')
    print(f'Unity Errors: {len(errors)}')

    # Check for JIMMY_001 specifically
    print()
    print('=' * 70)
    jimmy_status = seen_tests['JIMMY_001']
    if jimmy_status['passed']:
        print('JIMMY_001: PASSED')
    elif jimmy_status['failed']:
        print('JIMMY_001: FAILED')
    elif jimmy_status['started']:
        print('JIMMY_001: STARTED but no result (may have timed out)')
    else:
        print('JIMMY_001: NOT STARTED (previous test may have blocked)')
    print('=' * 70)

    # Save detailed report
    report = {
        'timestamp': time.strftime('%Y%m%d_%H%M%S'),
        'test_status': seen_tests,
        'total_passed': len(passed),
        'total_failed': len(failed),
        'timeouts': len(timeouts),
        'unity_errors': len(errors),
        'all_logs': all_logs
    }

    filename = f"TestReport_Long_{time.strftime('%Y%m%d_%H%M%S')}.json"
    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(report, f, ensure_ascii=False, indent=2)
    print(f'\n[OK] Detailed report saved: {filename}')

    client.close()


if __name__ == '__main__':
    main()
