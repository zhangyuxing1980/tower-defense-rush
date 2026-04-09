#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Unity PlayMode Test Runner - Full Suite"""

import http.client
import json
import time
import sys

class MCPUnityClient:
    def __init__(self, host='127.0.0.1', port=8080):
        self.session_id = None
        self.request_id = 0
        self.conn = http.client.HTTPConnection(host, port, timeout=30)

    def _make_request(self, method, params):
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

    def enter_play_mode(self, instance_id):
        result = self._make_request('tools/call', {
            'name': 'manage_editor',
            'arguments': {'unity_instance': instance_id, 'action': 'play'}
        })
        return result and 'result' in result

    def exit_play_mode(self, instance_id):
        result = self._make_request('tools/call', {
            'name': 'manage_editor',
            'arguments': {'unity_instance': instance_id, 'action': 'stop'}
        })
        return result and 'result' in result

    def read_console(self, instance_id, count=100):
        result = self._make_request('tools/call', {
            'name': 'read_console',
            'arguments': {'unity_instance': instance_id, 'count': count}
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
    print('='*60)
    print('Unity PlayMode Test Runner - Full Suite')
    print('='*60)

    client = MCPUnityClient()

    if not client.initialize():
        print('FAIL: MCP init failed')
        sys.exit(1)

    instances = client.get_instances()
    if not instances:
        print('FAIL: No Unity instance found')
        client.close()
        sys.exit(1)

    instance_id = instances[0].get('id')
    print(f'Found Unity instance: {instance_id}')

    # Enter play mode
    if not client.enter_play_mode(instance_id):
        print('FAIL: Cannot enter Play Mode')
        client.close()
        sys.exit(1)
    print('Entered Play Mode')

    # Wait and poll for test results
    print()
    print('Waiting for tests to complete (up to 4 minutes)...')
    print('-'*60)

    all_logs = []
    last_count = 0
    start_time = time.time()
    max_duration = 240  # 4 minutes

    # Track test completion
    test_results = {
        'SYNERGY_001': None,
        'JIMMY_001': None,
        'COMBAT_001': None,
        'PERF_001': None,
        'WAVE_001': None,
        'VICTORY_001': None
    }

    test_complete = False

    try:
        while time.time() - start_time < max_duration and not test_complete:
            time.sleep(2)
            logs = client.read_console(instance_id, 200)

            if logs:
                new_logs = logs[last_count:]
                for log in new_logs:
                    # Check for individual test results
                    for test_id in test_results.keys():
                        if test_id in log:
                            if 'Passed' in log or '通过' in log or '测试通过' in log:
                                if test_results[test_id] != 'PASSED':
                                    test_results[test_id] = 'PASSED'
                                    print(f'  [PASS] {test_id}')
                            elif 'Failed' in log or '失败' in log or '测试失败' in log:
                                if test_results[test_id] != 'FAILED':
                                    test_results[test_id] = 'FAILED'
                                    print(f'  [FAIL] {test_id}')

                    # Check for summary completion
                    if '==========' in log and ('摘要' in log or 'Summary' in log):
                        test_complete = True

                    # Print important logs
                    if any(k in log for k in ['测试', 'Test', '通过', '失败', 'Error', '摘要', '==========']):
                        try:
                            clean_log = log.encode('utf-8', errors='ignore').decode('utf-8')
                            if len(clean_log) > 5:
                                print(f'  {clean_log[:100]}')
                        except:
                            pass

                all_logs.extend(new_logs)
                last_count = len(logs)

                # Show progress every 30 seconds
                elapsed = time.time() - start_time
                if int(elapsed) % 30 < 2:
                    completed = sum(1 for v in test_results.values() if v is not None)
                    print(f'  ... {elapsed:.0f}s elapsed, {completed}/6 tests completed')

    except KeyboardInterrupt:
        print('\nInterrupted')

    print('-'*60)
    print()

    # Get final logs
    final_logs = client.read_console(instance_id, 300)
    if final_logs and len(final_logs) > last_count:
        new_logs = final_logs[last_count:]
        for log in new_logs:
            for test_id in test_results.keys():
                if test_id in log:
                    if ('Passed' in log or '通过' in log) and test_results[test_id] != 'PASSED':
                        test_results[test_id] = 'PASSED'
                    elif ('Failed' in log or '失败' in log) and test_results[test_id] != 'FAILED':
                        test_results[test_id] = 'FAILED'
        all_logs.extend(new_logs)

    # Exit play mode
    client.exit_play_mode(instance_id)
    print('Exited Play Mode')

    # Final analysis
    print()
    print('='*60)
    print('TEST RESULTS SUMMARY')
    print('='*60)

    for test_id, result in test_results.items():
        status = result if result else 'NOT RUN'
        print(f'  {test_id}: {status}')

    passed = sum(1 for v in test_results.values() if v == 'PASSED')
    failed = sum(1 for v in test_results.values() if v == 'FAILED')
    not_run = sum(1 for v in test_results.values() if v is None)

    print()
    print(f'Total: {passed} passed, {failed} failed, {not_run} not run')

    if passed == 6:
        print('Result: ALL TESTS PASSED')
    elif failed > 0:
        print('Result: SOME TESTS FAILED')
    else:
        print('Result: INCOMPLETE - may need longer timeout')

    print('='*60)

    # Save report
    report = {
        'timestamp': time.strftime('%Y-%m-%d %H:%M:%S'),
        'results': test_results,
        'passed': passed,
        'failed': failed,
        'not_run': not_run,
        'total_tests': 6
    }

    with open('FullTestReport.json', 'w', encoding='utf-8') as f:
        json.dump(report, f, indent=2)
    print('Report saved to FullTestReport.json')

    client.close()
    sys.exit(0 if passed == 5 else 1)


if __name__ == '__main__':
    main()
