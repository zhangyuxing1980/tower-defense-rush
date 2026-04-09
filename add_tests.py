#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Add test case components to TestRunner using MCP"""

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
            "clientInfo": {"name": "AddTests", "version": "1.0"}
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

    def find_gameobjects(self, search_term, search_method="by_name"):
        result = self._make_request("tools/call", {
            "name": "find_gameobjects",
            "arguments": {"search_term": search_term, "search_method": search_method}
        })
        if result and "result" in result:
            return result["result"]
        return None

    def modify_gameobject(self, target, components_to_add=None):
        params = {"action": "modify", "target": target}
        if components_to_add:
            params["components_to_add"] = components_to_add
        result = self._make_request("tools/call", {
            "name": "manage_gameobject",
            "arguments": params
        })
        return result

    def close(self):
        self.conn.close()


def main():
    print("\n" + "=" * 60)
    print("Adding test cases to TestRunner")
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

    # Find TestRunner object
    print("\n[*] Searching for TestRunner...")
    result = client.find_gameobjects("TestRunner", "by_name")
    if not result or not result.get("game_objects"):
        print("[[FAIL]] TestRunner not found")
        client.close()
        return

    testrunner = result["game_objects"][0]
    testrunner_id = testrunner.get("instance_id")
    print(f"[OK] Found TestRunner (ID: {testrunner_id})")

    # Add test case components
    test_cases = [
        "TowerDefenseRush.Testing.SynergySystemTest",
        "TowerDefenseRush.Testing.JimmyAITest",
        "TowerDefenseRush.Testing.CombatSystemTest",
        "TowerDefenseRush.Testing.PerformanceTest",
        "TowerDefenseRush.Testing.WaveSystemTest"
    ]

    print(f"\n[*] Adding {len(test_cases)} test case components...")
    result = client.modify_gameobject(testrunner_id, components_to_add=test_cases)
    if result and "result" in result:
        print("[OK] Test cases added successfully")
        print("\n[IMPORTANT] Please wait for Unity to compile scripts, then run tests again.")
    else:
        print(f"[FAIL] Could not add test cases: {result}")

    client.close()
    print("\n" + "=" * 60)


if __name__ == "__main__":
    main()
