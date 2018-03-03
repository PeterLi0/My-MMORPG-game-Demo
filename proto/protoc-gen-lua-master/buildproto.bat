
cd  protobuf\luascript  
for %%i in (*.proto) do (    
echo %%i  
"..\..\protoc.exe" --plugin=protoc-gen-lua="..\..\plugin\protoc-gen-lua.bat" --lua_out=. %%i  
  
)  
echo end  

cd ..\..\
xcopy .\protobuf\luascript\*.lua ..\..\client\assets\lua\3rd\pblua /y
xcopy .\protobuf\luascript\*.proto ..\..\client\assets\lua\3rd\pblua /y
pause  