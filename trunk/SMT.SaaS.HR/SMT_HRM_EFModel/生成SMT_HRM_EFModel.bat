echo 如果是64位机器，须安装64位oracle客户端，以及修改64位。netframework machineconfig
cd C:\WINDOWS\Microsoft.NET\Framework\v3.5
c:
Edmgen.exe /provider:EFOracleProvider /mode:fullgeneration /connectionstring:"data source=SMTSAAS94;user id=smthrm; password=test" /project:SMT_HRM_EFModel

cd C:\WINDOWS\Microsoft.NET\Framework\v3.5
c:

start C:\WINDOWS\Microsoft.NET\Framework\v3.5

pause