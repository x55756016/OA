echo off
echo "ȷ��SMT_HRM_EFModel.ssdl�е�Schema='dbo'�Ѿ��滻Ϊ��"

Edmgen2.exe /toedmx SMT_HRM_EFModel.csdl SMT_HRM_EFModel.msl SMT_HRM_EFModel.ssdl

echo "�����ʵ��ģ������"

pause