Name "PApps Manager Launcher"
OutFile "Start.exe"
Icon "..\PAppsManager\Resources\Icons\AppIcon.ico"

WindowIcon Off ; show no icon of the launcher
SilentInstall Silent ; start as launcher, not as installer
AutoCloseWindow True ; automatically close when finished

!include LogicLib.nsh
!include FileFunc.nsh

Section
    Call CheckAndInstallDotNet
    SetOutPath "Applications\PAppsManager"
    Exec "PAppsManager.exe"
SectionEnd

; Check that .NET Framework 4.5 is installed.
Function CheckAndInstallDotNet
    ; Magic numbers from http://msdn.microsoft.com/en-us/library/ee942965.aspx
    ClearErrors
    ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"

    IfErrors NotDetected

    ${If} $0 >= 378389
        DetailPrint "Microsoft .NET Framework 4.5 is installed ($0)"
    ${Else}
    NotDetected:
        DetailPrint "Installing Microsoft .NET Framework 4.5"
        SetDetailsPrint listonly
        ExecWait '"$INSTDIR\Tools\dotNetFx45_Full_setup.exe" /passive /norestart' $0
        ${If} $0 == 3010 
        ${OrIf} $0 == 1641
            DetailPrint "Microsoft .NET Framework 4.5 installer requested reboot"
            SetRebootFlag true
        ${EndIf}
        SetDetailsPrint lastused
        DetailPrint "Microsoft .NET Framework 4.5 installer returned $0"
    ${EndIf}
FunctionEnd