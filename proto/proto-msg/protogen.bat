
protoc -I=.\proto\ --descriptor_set_out=.\pb\common.pb .\proto\common.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\account.pb .\proto\account.proto 
protoc -I=.\proto\ --descriptor_set_out=.\pb\battlearena.pb .\proto\battlearena.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\battledungeon.pb .\proto\battledungeon.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\battleground.pb .\proto\battleground.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\battlescene.pb .\proto\battlescene.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\battlesync.pb .\proto\battlesync.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\character.pb .\proto\character.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\friend.pb .\proto\friend.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\inventory.pb .\proto\inventory.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\mail.pb .\proto\mail.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\mall.pb .\proto\mall.proto
protoc -I=.\proto\ --descriptor_set_out=.\pb\team.pb .\proto\team.proto

xcopy .\pb\*.pb ..\..\client\assets\lua\3rd\pbc /y


protogen -i:.\proto\common.proto -o:.\cs\common.cs

protogen -i:.\proto\battlearena.proto -o:.\cs\battlearena.cs
protogen -i:.\proto\battleground.proto -o:.\cs\battleground.cs
protogen -i:.\proto\battlescene.proto -o:.\cs\battlescene.cs
protogen -i:.\proto\battledungeon.proto -o:.\cs\battledungeon.cs
protogen -i:.\proto\battlesync.proto -o:.\cs\battlesync.cs

protogen -i:.\proto\account.proto -o:.\cs\account.cs
protogen -i:.\proto\character.proto -o:.\cs\character.cs
protogen -i:.\proto\inventory.proto -o:.\cs\inventory.cs
protogen -i:.\proto\mall.proto -o:.\cs\mall.cs
protogen -i:.\proto\mail.proto -o:.\cs\mail.cs
protogen -i:.\proto\friend.proto -o:.\cs\friend.cs
protogen -i:.\proto\team.proto -o:.\cs\team.cs

xcopy .\cs\*.cs ..\..\server\proto-msg\cs /y

pause


