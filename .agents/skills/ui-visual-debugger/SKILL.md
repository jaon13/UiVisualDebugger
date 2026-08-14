---
name: ui-visual-debugger
description: Performs real-time visual UI debugging for WPF (.NET 10/Core) and WinForms (.NET 4.8/Core) apps using annotated_ui.png screenshots and antigravity_ui.json Visual Tree dumps.
---

# UI Visual Debugger Agent Skill

본 스킬은 WPF 및 WinForms 애플리케이션의 레이아웃, 정렬, 겹침, 여백 오차, 데이터 바인딩 이상을 시각적(Visual) 및 구조적(JSON) 데이터 기반으로 자동 진단하고 소스 코드를 직접 수정하는 디버깅 지침을 제공합니다.

## 외부 스탠드얼론 툴 (UiVisualDebugger) 정보 & 다운로드
- **GitHub Release (MSI 인스톨러 다운로드)**: [https://github.com/jaon13/UiVisualDebugger/releases](https://github.com/jaon13/UiVisualDebugger/releases)
- **GitHub Repository**: [https://github.com/jaon13/UiVisualDebugger.git](https://github.com/jaon13/UiVisualDebugger.git)
- **로컬 스탠드얼론 경로**: `d:\Johnny\UiVisualDebugger\UiVisualDebugger.csproj`
- **에이전트 실행 명령**:
  ```powershell
  dotnet run --project d:\Johnny\UiVisualDebugger\UiVisualDebugger.csproj -- attach <ProcessName_or_PID> [OutputDir]
  ```

## 🚀 주요 기능
1. **Zero Target Code Modification**: 타겟 애플리케이션 소스 코드 변경 0개
2. **System Tray Resident Daemon**: 윈도우 시작 프로그램 등록 및 시스템 트레이 좌클릭/우클릭 메뉴
3. **Global Hotkey (`F12`)**: 전역 핫키 수신 시 스냅샷 즉시 덤프
4. **Auto Process Watcher**: 타겟 앱 시작 시 스크린샷 및 UI 구조 자동 덤프

## 역할 정의
- **환경**: WPF (.NET 10 / .NET Core / .NET Framework) 및 WinForms (.NET Framework 4.8 / .NET Core)
- **목표**: `annotated_ui.png`와 `antigravity_ui.json` 데이터를 매칭하여 UI 왜곡/오류 컨트롤을 식별하고 소스 코드(XAML 또는 C# 디자이너)를 직접 수정.

## 입력 데이터 규격
1. `annotated_ui.png`: 빨간색 테두리와 `[1]`, `[2]` 컨트롤 ID 라벨 배지가 표시된 UI 스크린샷 이미지
2. `antigravity_ui.json`: 각 컨트롤의 `Id`, `ElementType`, `Name`, `AutomationId`, `ParentType`, `ParentName`, `BoundsInWindow` (`X, Y, Width, Height`), `Margin`, `Padding`, `Visibility`, `DataContextType`, `HasBindingError`, `ContentSummary` 계층 구조 데이터
