echo off
echo "ȷ��SMT_FB_EFModel.ssdl�е�Schema='dbo'�Ѿ��滻Ϊ��"

Edmgen2.exe /toedmx SMT_FB_EFModel.csdl SMT_FB_EFModel.msl SMT_FB_EFModel.ssdl

echo "�����ʵ��ģ������"

pause