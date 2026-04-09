#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Restore test case container to scene file"""

import re

# Read the scene file
with open("Assets/Scenes/PrototypeScene.unity", "r", encoding="utf-8") as f:
    content = f.read()

# Check if test case container already exists
if "1722620442" in content:
    print("[INFO] Test case container already exists")
else:
    # Add test case container at the end
    test_container = """
--- !u!1 &1722620442
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 1722620449}
  - component: {fileID: 1722620443}
  - component: {fileID: 1722620444}
  - component: {fileID: 1722620445}
  - component: {fileID: 1722620446}
  - component: {fileID: 1722620447}
  m_Layer: 0
  m_Name: TestCaseContainer
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!114 &1722620443
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1722620442}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: ac49426fb84471340b388346c31205dc, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: SYNERGY_001
  testName:
  description:
  category: Combat
  priority: 1
  timeout: 90
  enemyPrefab: {fileID: 0}
  spawnPoint: {fileID: 0}
--- !u!114 &1722620444
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1722620442}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 4e3d07f3259427d488744966159d88b4, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: JIMMY_001
  testName:
  description:
  category: AI
  priority: 1
  timeout: 60
--- !u!114 &1722620445
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1722620442}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: e0714319ba4f25148b8439719b1981fe, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: COMBAT_001
  testName:
  description:
  category: Combat
  priority: 1
  timeout: 60
  testEnemyPrefab: {fileID: 0}
  testSpawnPoint: {fileID: 0}
--- !u!114 &1722620446
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1722620442}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 37a13647f83dde3499a0ee9a679b9f7e, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: PERF_001
  testName:
  description:
  category: Performance
  priority: 2
  timeout: 60
  targetFrameRate: 60
  minAcceptableFPS: 30
  maxMemoryMB: 512
  maxDrawCalls: 100
--- !u!114 &1722620447
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1722620442}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 4a1a0bb40aaa2f94397c66d22695a5fb, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  testId: WAVE_001
  testName:
  description:
  category: Systems
  priority: 2
  timeout: 60
  testEnemyPrefab: {fileID: 0}
  testSpawnPoint: {fileID: 0}
--- !u!4 &1722620449
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1722620442}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
"""
    content = content + test_container

    with open("Assets/Scenes/PrototypeScene.unity", "w", encoding="utf-8") as f:
        f.write(content)

    print("[OK] Test case container restored")
