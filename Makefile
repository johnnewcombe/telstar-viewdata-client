VERSION := $(shell cat VERSION)

build:
	# This will fail if the version already exists, this is by design
	mkdir ../releases/$(VERSION)

	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os linux --arch x64   -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os linux --arch arm64 -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os osx --arch x64     -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os osx --arch arm64   -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os win --arch x64     -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os win --arch arm64   -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)

	# Linux
	-cd ./bin/Release/net9.0/linux-arm64/publish;rm ./TelstarClient-linux-arm64.zip
	cd ./bin/Release/net9.0/linux-arm64/publish;zip -v ./TelstarClient-linux-arm64.zip *
	cp ./bin/Release/net9.0/linux-arm64/publish/TelstarClient-linux-arm64.zip ../releases/$(VERSION)
	-cd ./bin/Release/net9.0/linux-x64/publish;rm ./TelstarClient-linux-x64.zip
	cd ./bin/Release/net9.0/linux-x64/publish;zip -v ./TelstarClient-linux-x64.zip *
	cp ./bin/Release/net9.0/linux-x64/publish/TelstarClient-linux-x64.zip ../releases/$(VERSION)
	# OSX
	-cd ./bin/Release/net9.0/osx-arm64/publish;rm ./TelstarClient-osx-arm64.zip
	cd ./bin/Release/net9.0/osx-arm64/publish;zip -v ./TelstarClient-osx-arm64.zip *
	cp ./bin/Release/net9.0/osx-arm64/publish/TelstarClient-osx-arm64.zip ../releases/$(VERSION)
	-cd ./bin/Release/net9.0/osx-x64/publish;rm ./TelstarClient-osx-x64.zip
	cd ./bin/Release/net9.0/osx-x64/publish;zip -v ./TelstarClient-osx-x64.zip *
	cp ./bin/Release/net9.0/osx-x64/publish/TelstarClient-osx-x64.zip ../releases/$(VERSION)
	# Windows
	-cd ./bin/Release/net9.0/win-arm64/publish;rm ./TelstarClient-win-arm64.zip
	cd ./bin/Release/net9.0/win-arm64/publish;zip -v ./TelstarClient-win-arm64.zip *
	cp ./bin/Release/net9.0/win-arm64/publish/TelstarClient-win-arm64.zip ../releases/$(VERSION)
	-cd ./bin/Release/net9.0/win-x64/publish;rm ./TelstarClient-win-x64.zip
	cd ./bin/Release/net9.0/win-x64/publish;zip -v ./TelstarClient-win-x64.zip *
	cp ./bin/Release/net9.0/win-x64/publish/TelstarClient-win-x64.zip ../releases/$(VERSION)

installer:
	makensis -DVERSION=$(VERSION) installer.nsi