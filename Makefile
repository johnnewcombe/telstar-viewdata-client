VERSION := $(shell cat VERSION)
APPNAME := TelstarClient
APPBUNDLE-ARM := $(APPNAME)-arm64.app
APPBUNDLE-X64 := $(APPNAME)-x64.app

build:
	# This will fail if the version already exists, this is by design to prevent overwriting previos versions
	mkdir ../releases/$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os linux --arch x64   -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os linux --arch arm64 -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os osx --arch x64     -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os osx --arch arm64   -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os win --arch x64     -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os win --arch arm64   -p:Version=$(VERSION) -p:InformationalVersion=$(VERSION)

	$(MAKE) linux-arm64
	$(MAKE) linux-x64
	$(MAKE) win-arm64
	$(MAKE) win-x64
	$(MAKE) macos-arm64
	$(MAKE) macos-x64

linux-arm64:	
	# copy install script and icon to publish folder
	cp Assets/linux-install.sh ./bin/Release/net9.0/linux-arm64/publish/install.sh
	chmod +x ./bin/Release/net9.0/linux-arm64/publish/install.sh
	cp Assets/icon.png ./bin/Release/net9.0/linux-arm64/publish/icon.png

	# create zipfile	
	-cd ./bin/Release/net9.0/linux-arm64/publish;rm ./TelstarClient-linux-arm64.zip
	cd ./bin/Release/net9.0/linux-arm64/publish;zip -v ./TelstarClient-linux-arm64.zip *
	cp ./bin/Release/net9.0/linux-arm64/publish/TelstarClient-linux-arm64.zip ../releases/$(VERSION)

linux-x64:
	# copy install script and icon to publish folder
	cp Assets/linux-install.sh ./bin/Release/net9.0/linux-x64/publish/install.sh
	chmod +x ./bin/Release/net9.0/linux-x64/publish/install.sh
	cp Assets/icon.png ./bin/Release/net9.0/linux-x64/publish/icon.png

	-cd ./bin/Release/net9.0/linux-x64/publish;rm ./TelstarClient-linux-x64.zip
	cd ./bin/Release/net9.0/linux-x64/publish;zip -v ./TelstarClient-linux-x64.zip *
	cp ./bin/Release/net9.0/linux-x64/publish/TelstarClient-linux-x64.zip ../releases/$(VERSION)

win-arm64:
	makensis -DVERSION=$(VERSION) installer-win-arm64.nsi

win-x64:
	makensis -DVERSION=$(VERSION) installer-win-x64.nsi

macos-arm64:
	mkdir -p $(APPBUNDLE-ARM)/Contents/MacOS
	mkdir -p $(APPBUNDLE-ARM)/Contents/Resources
	cp ./bin/Release/net9.0/osx-arm64/publish/* $(APPBUNDLE-ARM)/Contents/MacOS/
	chmod +x $(APPBUNDLE-ARM)/Contents/MacOS/$(APPNAME)
	cp Assets/icon.icns $(APPBUNDLE-ARM)/Contents/Resources/icon.icns
	sed 's/VERSION_PLACEHOLDER/$(VERSION)/g' Assets/TelstarClient.app.plist > $(APPBUNDLE-ARM)/Contents/Info.plist
	cp -r $(APPBUNDLE-ARM) ../releases/$(VERSION)/	

macos-x64:
	mkdir -p $(APPBUNDLE-X64)/Contents/MacOS
	mkdir -p $(APPBUNDLE-X64)/Contents/Resources
	cp ./bin/Release/net9.0/osx-x64/publish/* $(APPBUNDLE-X64)/Contents/MacOS/
	chmod +x $(APPBUNDLE-X64)/Contents/MacOS/$(APPNAME)
	cp Assets/icon.icns $(APPBUNDLE-X64)/Contents/Resources/icon.icns
	sed 's/VERSION_PLACEHOLDER/$(VERSION)/g' Assets/TelstarClient.app.plist > $(APPBUNDLE-X64)/Contents/Info.plist
	cp -r $(APPBUNDLE-X64) ../releases/$(VERSION)/

