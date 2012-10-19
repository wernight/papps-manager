OutFile "nsis.exe"

SetCompressor /SOLID lzma

Section
    SetOutPath $EXEDIR
    File /oname=example.txt nsis.nsi
SectionEnd
