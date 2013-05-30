set binDir=%~dp0

echo %binDir% 

cd C:\Program Files\Microsoft SDKs\Windows\v7.0A\bin
c:
echo %binDir%\EFOracleProvider.dll

gacutil /i  %binDir%\EFOracleProvider.dll

pause