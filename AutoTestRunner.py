#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""MCP Unity Auto Test Framework"""

import http.client
import json
import sys
import time
import re
from datetime import datetime
from typing import Dict, List, Optional
from dataclasses import dataclass, field
from enum import Enum


class MCPUnityClient:
    def __init__(self, host="127.0.0.1", port=8080):
        self.session_id = None
        self.instance_id = None
        self.request_id = 0
        self.conn = http.client.HTTPConnection(host, port, timeout=30)

    def _make_request(self, method: str, params: dict):
        self.request_id += 1
        payload = {
            "jsonrpc": "2.0",
            "method": method,
            "params": params,
            "id": self.request_id
        }
        headers = {
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream"
        }
        if self.session_id:
            headers["Mcp-Session-Id"] = self.session_id
        
        try:
            self.conn.request("POST", "/mcp", body=json.dumps(payload), headers=headers)
            response = self.conn.getresponse()
            new_session = response.getheader('Mcp-Session-Id')
            if new_session:
                self.session_id = new_session
            content = response.read().decode('utf-8')
            result = None
            for line in content.split("\n"):
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
            print(f"[ERROR] Request failed: {e}")
            return None

    def initialize(self):
        print("[*] Initializing MCP session...")
        result = self._make_request("initialize", {
            "protocolVersion": "2024-11-05",
            "capabilities": {"tools": {}},
            "clientInfo": {"name": "AutoTestRunner", "version": "1.0"}
        })
        if result and "result" in result:
            print("[OK] MCP session initialized")
            return True
        return False

    def get_instances(self):
        result = self._make_request("resources/read", {"uri": "mcpforunity://instances"})
        if result and "result" in result:
            try:
                content = result["result"]["contents"][0]["text"]
                data = json.loads(content)
                return data.get("instances", [])
            except Exception as e:
                print(f"[ERROR] Parsing instances: {e}")
        return None

    def enter_play_mode(self):
        print("[*] Entering Play Mode...")
        result = self._make_request("tools/call", {
            "name": "manage_editor",
            "arguments": {"unity_instance": self.instance_id, "action": "play"}
        })
        if result and "result" in result:
            print("[OK] Entered Play Mode")
            return True
        return False

    def exit_play_mode(self):
        print("[*] Exiting Play Mode...")
        result = self._make_request("tools/call", {
            "name": "manage_editor",
            "arguments": {"unity_instance": self.instance_id, "action": "stop"}
        })
        if result and "result" in result:
            print("[OK] Exited Play Mode")
            return True
        return False

    def read_console(self, count: int = 100):
        result = self._make_request("tools/call", {
            "name": "read_console",
            "arguments": {"unity_instance": self.instance_id, "count": count}
        })
        if result and "result" in result:
            try:
                content = result["result"]["content"][0]["text"]
                data = json.loads(content)
                return data.get("data", [])
            except:
                pass
        return []

    def read_file(self, file_path: str):
        """读取 Unity 中的文件"""
        result = self._make_request("resources/read", {"uri": f"mcpforunity://path/{file_path}"})
        if result and "result" in result:
            try:
                content = result["result"]["contents"][0]["text"]
                return content
            except:
                pass
        return None

    def close(self):
        self.conn.close()


def main():
    test_duration = 30
    if len(sys.argv) > 1:
        try:
            test_duration = int(sys.argv[1])
        except:
            pass

    print("\n" + "=" * 60)
    print("MCP Unity Auto Test Framework")
    print("=" * 60 + "\n")

    client = MCPUnityClient()

    if not client.initialize():
        print("[[FAIL]] MCP init failed")
        client.close()
        sys.exit(1)

    instances = client.get_instances()
    if not instances:
        print("[[FAIL]] No Unity instance found")
        client.close()
        sys.exit(1)

    instance = instances[0]
    instance_id = instance.get("id")
    print(f"[[OK]] Found Unity instance: {instance_id}")
    client.instance_id = instance_id

    if not client.enter_play_mode():
        print("[[FAIL]] Cannot enter Play Mode")

    print(f"\n[*] Collecting logs ({test_duration}s)...")
    print("-" * 60)

    all_logs = []
    start_time = time.time()
    last_log_count = 0

    try:
        while time.time() - start_time < test_duration:
            logs = client.read_console(count=50)
            if logs is None:
                logs = []
            new_logs = logs[last_log_count:]
            for log in new_logs:
                # 打印所有错误
                if "[Error]" in log or "Exception" in log or "NullReference" in log:
                    print(f"  [ERROR] {log[:200]}")
                # 打印测试相关日志
                elif any(k in log for k in [">>", "<<", "测试", "Test", "通过", "失败"]):
                    print(f"  {log[:120]}")
            all_logs.extend(new_logs)
            last_log_count = len(logs)
            time.sleep(1)
    except KeyboardInterrupt:
        print("\n[!] Interrupted")

    print("-" * 60)

    # 等待测试完全完成
    print("\n[*] Waiting for tests to complete...")

    # 持续等待直到看到测试完成标识或超时
    wait_start = time.time()
    max_wait = 90  # 最多额外等待90秒
    test_completed = False
    final_logs = None

    while time.time() - wait_start < max_wait and not test_completed:
        time.sleep(2)
        try:
            final_logs = client.read_console(count=100)
            if final_logs:
                new_logs = final_logs[last_log_count:]
                for log in new_logs:
                    if any(k in log for k in [">>", "<<", "[Error]", "测试", "Exception", "摘要", "通过", "失败", "完成"]):
                        print(f"  {log[:120]}")
                    # 检测测试完成标识
                    if any(k in log for k in ["测试执行完成", "========== 测试执行完成 ==========", "<<< 测试通过", "<<< 测试失败"]):
                        test_completed = True
                        print(f"  [OK] Tests completed signal detected: {log[:60]}...")
                all_logs.extend(new_logs)
                last_log_count = len(final_logs)
        except Exception as e:
            print(f"  [!] Error reading console: {e}")
            break

    if not test_completed:
        print("  [!] Wait timeout - proceeding with available results")

    client.exit_play_mode()

    # STRICT error analysis
    print("\n[*] Analyzing test results...")

    # 测试通过/失败检测 - 支持中英文
    passed_tests = sum(1 for log in all_logs if "测试通过" in log or "Test Passed" in log)
    failed_tests = sum(1 for log in all_logs if "测试失败" in log or "Test Failed" in log)

    # 断言失败检测
    assertion_failures = [log for log in all_logs if "断言失败" in log or "Assertion Failed" in log]
    failed_tests += len(assertion_failures)

    # Unity 错误检测 - 更全面的关键词
    error_keywords = [
        "[Error]", "NullReferenceException", "UnassignedReferenceException",
        "MissingReferenceException", "ArgumentNullException", "Exception:",
        "IndexOutOfRangeException", "KeyNotFoundException", "InvalidOperationException",
        "UnityException", "MissingComponentException", "AssertionException"
    ]
    errors = [log for log in all_logs if any(k in log for k in error_keywords)]
    warnings = [log for log in all_logs if "[Warning]" in log]

    # 收集所有测试相关的日志用于调试
    test_logs = [log for log in all_logs if "[Test]" in log]

    total_issues = failed_tests + len(errors)

    print("\n" + "=" * 60)
    print("MCP Unity Auto Test Report")
    print("=" * 60)
    print(f"Test Time: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"Tests Passed: {passed_tests}")
    print(f"Tests Failed: {failed_tests}")
    print(f"Assertion Failures: {len(assertion_failures)}")
    print(f"Unity Errors: {len(errors)}")
    print(f"Warnings: {len(warnings)}")
    print("-" * 60)
    if total_issues == 0:
        print("Result: [[PASS]] All tests passed!")
    else:
        print(f"Result: [[FAIL]] {total_issues} issue(s) found!")
    print("=" * 60)

    # 显示测试相关日志（用于调试）
    if test_logs:
        print("\n[TEST LOGS] 测试执行日志:")
        for log in test_logs[:20]:  # 限制显示前20条
            print(f"  {log[:120]}")
        if len(test_logs) > 20:
            print(f"  ... 还有 {len(test_logs) - 20} 条日志")

    # 显示断言失败详情
    if assertion_failures:
        print("\n[ASSERTION FAILURES] 测试断言失败:")
        for i, failure in enumerate(assertion_failures[:10], 1):
            print(f"  {i}. {failure[:120]}")

    # 显示 Unity 错误
    if errors:
        print("\n[ERRORS] Unity Errors Found:")
        for i, err in enumerate(errors[:10], 1):
            print(f"  {i}. {err[:120]}")
        if len(errors) > 10:
            print(f"  ... and {len(errors) - 10} more errors")

    report_data = {
        "timestamp": datetime.now().strftime('%Y%m%d_%H%M%S'),
        "tests_passed": passed_tests,
        "tests_failed": failed_tests,
        "assertion_failures": len(assertion_failures),
        "unity_errors": len(errors),
        "warnings": len(warnings),
        "result": "PASS" if total_issues == 0 else "FAIL",
        "error_logs": errors,
        "assertion_logs": assertion_failures[:10],
        "test_logs": test_logs[:30]
    }

    filename = f"TestReport_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(report_data, f, ensure_ascii=False, indent=2)
    print(f"\n[OK] Test report saved: {filename}")

    client.close()
    sys.exit(0 if total_issues == 0 else 1)


if __name__ == "__main__":
    main()
