#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Add test case components to TestRunner in scene file"""

import re

# Read the scene file
with open("Assets/Scenes/PrototypeScene.unity", "r", encoding="utf-8") as f:
    content = f.read()

# Find the TestRunner GameObject section and add new components
testrunner_section = """  m_Component:
  - component: {fileID: 874813247}
  - component: {fileID: 874813246}
  - component: {fileID: 874813248}"""

new_section = """  m_Component:
  - component: {fileID: 874813247}
  - component: {fileID: 874813246}
  - component: {fileID: 874813248}
  - component: {fileID: 874813250}
  - component: {fileID: 874813251}
  - component: {fileID: 874813252}
  - component: {fileID: 874813253}
  - component: {fileID: 874813254}"""

content = content.replace(testrunner_section, new_section)

# Add test case components at the end of the file
new_components = """
--- !u!114 &874813250
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 874813245}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: ac49426fb84471340b388346c31205dc, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: SYNERGY_001
  testName:
  description:
  category: General
  priority: 3
  timeout: 30
  enemyPrefab: {fileID: 0}
  spawnPoint: {fileID: 0}
--- !u!114 &874813251
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 874813245}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 4a1a0bb40aaa2f94397c66d22695a5fb, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: JIMMY_001
  testName:
  description:
  category: General
  priority: 3
  timeout: 30
--- !u!114 &874813252
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 874813245}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: e0714319ba4f25148b8439719b1981fe, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: COMBAT_001
  testName:
  description:
  category: General
  priority: 3
  timeout: 30
  testEnemyPrefab: {fileID: 0}
  testSpawnPoint: {fileID: 0}
--- !u!114 &874813253
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 874813245}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 37a13647f83dde3499a0ee9a679b9f7e, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: PERF_001
  testName:
  description:
  category: General
  priority: 3
  timeout: 30
  targetFrameRate: 60
  minAcceptableFPS: 30
  maxMemoryMB: 512
  maxDrawCalls: 100
--- !u!114 &874813254
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 874813245}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 7027a2293f9ad5e4bb15f03b5d8cf0d5, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: WAVE_001
  testName:
  description:
  category: General
  priority: 3
  timeout: 30
  testEnemyPrefab: {fileID: 0}
  testSpawnPoint: {fileID: 0}
"""

content = content + new_components

# Write the modified content back
with open("Assets/Scenes/PrototypeScene.unity", "w", encoding="utf-8") as f:
    f.write(content)

print("[OK] Added test case components to TestRunner")
print("Components added:")
print("  - SynergySystemTest (fileID: 874813250)")
print("  - JimmyAITest (fileID: 874813251)")
print("  - CombatSystemTest (fileID: 874813252)")
print("  - PerformanceTest (fileID: 874813253)")
print("  - WaveSystemTest (fileID: 874813254)")
