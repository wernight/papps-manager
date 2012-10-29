Name "PApps Manager"
OutFile "PAppsManager-Setup.exe"

; The default installation directory
InstallDir "C:\"

; Allow (and should) install at the drive's root directory
AllowRootDirInstall true

; Pages
Page directory
Page instfiles

; Request no privileges
RequestExecutionLevel user

Section
    SetOutPath $INSTDIR
    File "PortableDriveRoot\Start.exe"
    File /r /x "Start.nsi" "PortableDriveRoot\"

    SetOutPath $INSTDIR\Applications\PAppsManager
    File /r /x "*.vshost.*" /x "*.json" "PAppsManager\bin\Release\*.exe" "PAppsManager\bin\Release\*.dll" "PAppsManager\bin\Release\*.config"

    CreateDirectory "$INSTDIR\Applications"
    CreateDirectory "$INSTDIR\PortableApps"

    ; Start it
    ExecShell "" '"$INSTDIR\Start.exe"'
SectionEnd
