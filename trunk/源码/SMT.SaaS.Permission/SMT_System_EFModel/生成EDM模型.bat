echo off
echo "ȷ��SMT_HRM_EFModel.ssdl�е�Schema='dbo'�Ѿ��滻Ϊ��"

Edmgen2.exe /toedmx SMT_System_EFModel.csdl SMT_System_EFModel.msl SMT_System_EFModel.ssdl

echo "�����ʵ��ģ������"

pause