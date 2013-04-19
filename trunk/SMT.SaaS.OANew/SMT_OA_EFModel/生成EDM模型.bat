echo off
echo "确保SMT_OA_EFModel.ssdl中的Schema='dbo'已经替换为空"

Edmgen2.exe /toedmx SMT_OA_EFModel.csdl SMT_OA_EFModel.msl SMT_OA_EFModel.ssdl

echo "已完成实体模型生成"

pause