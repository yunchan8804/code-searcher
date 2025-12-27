; Inno Setup Script for Unicode Searcher
; https://jrsoftware.org/isinfo.php

#define MyAppName "Unicode Searcher"
#define MyAppNameKo "유니코드 검색기"
#define MyAppVersion "1.3.0"
#define MyAppPublisher "UnicodeSearcher Team"
#define MyAppURL "https://github.com/yunchan8804/code-searcher"
#define MyAppExeName "UnicodeSearcher.exe"

[Setup]
; 앱 정보
AppId={{B8F3D2E1-4A5C-6D7E-8F9A-0B1C2D3E4F5A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; 권한 설정 (관리자 권한 없이 설치 가능)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; 출력 설정
OutputDir=..\installer-output
OutputBaseFilename=UnicodeSearcher-{#MyAppVersion}-Setup
; 압축 설정
Compression=lzma2
SolidCompression=yes
; UI 설정
WizardStyle=modern
SetupIconFile=..\src\UnicodeSearcher\Resources\Icons\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
; 기타
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "Windows 시작 시 자동 실행"; GroupDescription: "추가 옵션:"

[Files]
; 메인 실행 파일 및 의존성
Source: "..\publish\framework-dependent\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; 라이선스
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; 시작 메뉴
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppNameKo}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
; 바탕화면 (선택적)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "{#MyAppNameKo}"

[Registry]
; 시작 프로그램 등록 (선택적)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "UnicodeSearcher"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

[Run]
; 설치 완료 후 실행
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// 이전 버전 제거 확인
function InitializeSetup(): Boolean;
var
  UninstallKey: String;
  UninstallString: String;
  ResultCode: Integer;
begin
  Result := True;
  UninstallKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1';

  if RegQueryStringValue(HKLM, UninstallKey, 'UninstallString', UninstallString) or
     RegQueryStringValue(HKCU, UninstallKey, 'UninstallString', UninstallString) then
  begin
    if MsgBox('이전 버전의 Unicode Searcher가 설치되어 있습니다.' + #13#10 +
              '계속하기 전에 이전 버전을 제거하시겠습니까?',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      Exec(RemoveQuotes(UninstallString), '/SILENT', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
    end;
  end;
end;
