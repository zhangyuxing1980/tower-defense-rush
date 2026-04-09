#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Check Unity console for errors"""

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
        result = self._make_request("initialize", {
            "protocolVersion": "2024-11-05",
            "capabilities": {"tools": {}},
            "clientInfo": {"name": "CheckConsole", "version": "1.0"}
        })
        return result and "result" in result

    def get_instances(self):
        result = self._make_request("resources/read", {"uri": "mcpforunity://instances"})
        if result and "result" in result:
            try:
                content = result["result"]["contents"][0]["text"]
                data = json.loads(content)
                return data.get("instances", [])
            except:
                pass
        return None

    def read_console(self, count=100):
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

    def close(self):
        self.conn.close()


def main():
    print("Checking Unity console for errors...\n")

    client = MCPUnityClient()
    if not client.initialize():
        print("[FAIL] MCP init failed")
        return

    instances = client.get_instances()
    if not instances:
        print("[FAIL] No Unity instance found")
        client.close()
        return

    instance = instances[0]
    client.instance_id = instance.get("id")

    logs = client.read_console(100)

    errors = [log for log in logs if "[Error]" in log or "error" in log.lower() or "exception" in log.lower()]
    warnings = [log for log in logs if "[Warning]" in log or "warning" in log.lower()]

    print(f"Found {len(errors)} error(s):")
    for err in errors[:10]:
        print(f"  [ERROR] {err[:150]}")

    print(f"\nFound {len(warnings)} warning(s):")
    for warn in warnings[:5]:
        print(f"  [WARNING] {warn[:150]}")

    client.close()


if __name__ == "__main__":
    main()
