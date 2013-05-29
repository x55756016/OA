echo off
echo "确保SMT_FB_EFModel.ssdl中的Schema='dbo'已经替换为空"

Edmgen2.exe /toedmx SMT_FB_EFModel.csdl SMT_FB_EFModel.msl SMT_FB_EFModel.ssdl

echo "已完成实体模型生成"

pause