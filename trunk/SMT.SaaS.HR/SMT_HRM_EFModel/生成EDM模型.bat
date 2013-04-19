echo off
echo "确保SMT_HRM_EFModel.ssdl中的Schema='dbo'已经替换为空"

Edmgen2.exe /toedmx SMT_HRM_EFModel.csdl SMT_HRM_EFModel.msl SMT_HRM_EFModel.ssdl

echo "已完成实体模型生成"

pause