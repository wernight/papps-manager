Name "PApps Manager"
OutFile "PAppsManager-Setup.exe"

; The default installation directory
InstallDir "\"

; Allow (and should) install at the drive's root directory
AllowRootDirInstall true

; Pages
Page directory
Page instfiles

; Request no privileges
RequestExecutionLevel user

Section
    SetOutPath $INSTDIR
    File /r /x "*.nsi" "PortableDriveRoot\"

    SetOutPath $INSTDIR\PAppsManager
    File /r /x "*.vshost.*" "PAppsManager\bin\Release\*.exe" "PAppsManager\bin\Release\*.dll" "PAppsManager\bin\Release\*.config"

    CreateDirectory "$INSTDIR\PortableApps"
SectionEnd