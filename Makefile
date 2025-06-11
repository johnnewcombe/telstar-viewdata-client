

build:
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os linux --arch x64
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os linux --arch arm64
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os osx --arch x64
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os osx --arch arm64
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os win --arch x64
	dotnet publish --verbosity detailed --self-contained --use-current-runtime --configuration Release --os win --arch arm64
