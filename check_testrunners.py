#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Check for multiple TestRunner instances in Unity scene"""

import http.client
import json

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
            "clientInfo": {"name": "CheckTestRunners", "version": "1.0"}
        })
        if result and "result" in result:
            print("[OK] MCP session initialized")
            return True
        return False

    def find_gameobjects(self, search_term, search_method="by_name"):
        result = self._make_request("tools/call", {
            "name": "find_gameobjects",
            "arguments": {"search_term": search_term, "search_method": search_method}
        })
        if result and "result" in result:
            return result["result"]
        return None

    def read_console(self, count=50):
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

    def close(self):
        self.conn.close()


def main():
    print("\n" + "=" * 60)
    print("Checking for TestRunner instances in Unity scene")
    print("=" * 60 + "\n")

    client = MCPUnityClient()

    if not client.initialize():
        print("[[FAIL]] MCP init failed")
        client.close()
        return

    instances = client.get_instances()
    if not instances:
        print("[[FAIL]] No Unity instance found")
        client.close()
        return

    instance = instances[0]
    instance_id = instance.get("id")
    print(f"[[OK]] Found Unity instance: {instance_id}")
    client.instance_id = instance_id

    # Find TestRunner objects by name
    print("\n[*] Searching for TestRunner objects by name...")
    result = client.find_gameobjects("TestRunner", "by_name")
    if result:
        gameobjects = result.get("game_objects", [])
        print(f"[INFO] Found {len(gameobjects)} TestRunner object(s) by name:")
        for go in gameobjects:
            print(f"  - {go.get('name', 'Unknown')} (ID: {go.get('instance_id', 'N/A')})")
    else:
        print("[WARNING] Could not find TestRunner objects by name")

    # Find TestRunner objects by component
    print("\n[*] Searching for objects with TestRunner component...")
    result = client.find_gameobjects("TestRunner", "by_component")
    if result:
        gameobjects = result.get("game_objects", [])
        print(f"[INFO] Found {len(gameobjects)} object(s) with TestRunner component:")
        for go in gameobjects:
            print(f"  - {go.get('name', 'Unknown')} (ID: {go.get('instance_id', 'N/A')})")
    else:
        print("[WARNING] Could not find objects with TestRunner component")

    # Find VisualTestRunner objects by component
    print("\n[*] Searching for objects with VisualTestRunner component...")
    result = client.find_gameobjects("VisualTestRunner", "by_component")
    if result:
        gameobjects = result.get("game_objects", [])
        print(f"[INFO] Found {len(gameobjects)} object(s) with VisualTestRunner component:")
        for go in gameobjects:
            print(f"  - {go.get('name', 'Unknown')} (ID: {go.get('instance_id', 'N/A')})")
    else:
        print("[WARNING] Could not find objects with VisualTestRunner component")

    # Check console for warnings about duplicate tests
    print("\n[*] Checking console for duplicate test warnings...")
    logs = client.read_console(100)
    if logs:
        duplicate_warnings = [log for log in logs if "测试已经在运行中" in log or "测试已经执行完毕" in log]
        if duplicate_warnings:
            print(f"[INFO] Found {len(duplicate_warnings)} duplicate prevention warning(s):")
            for warning in duplicate_warnings[:5]:
                print(f"  {warning[:100]}")
        else:
            print("[INFO] No duplicate prevention warnings found")

        # Check for any errors
        errors = [log for log in logs if "[Error]" in log or "error" in log.lower()]
        if errors:
            print(f"\n[ERROR] Found {len(errors)} error(s) in console:")
            for err in errors[:10]:
                print(f"  {err[:150]}")
        else:
            print("[OK] No errors found in console")

    client.close()
    print("\n" + "=" * 60)


if __name__ == "__main__":
    main()
