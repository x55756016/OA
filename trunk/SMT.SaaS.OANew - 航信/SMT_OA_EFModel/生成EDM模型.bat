echo off
echo "ȷ��SMT_OA_EFModel.ssdl�е�Schema='dbo'�Ѿ��滻Ϊ��"

Edmgen2.exe /toedmx SMT_OA_EFModel.csdl SMT_OA_EFModel.msl SMT_OA_EFModel.ssdl

echo "�����ʵ��ģ������"

pause