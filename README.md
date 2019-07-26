# 개요

https://github.com/grpc/grpc 를 사용하여 구현한 서버와 클라이언트입니다.

https://github.com/s2quake/Crema 프로젝트에서 WCF 대체할 목적으로 작업하고 있습니다.

# 도구

[.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)

[Visual Studio Code](https://code.visualstudio.com/)

# 빌드

    git clone https://github.com/s2quake/grpc-sample.git --recursive
    cd grpc-sample
    dotnet restore
    dotnet build --framework netcoreapp3.0

# 실행

    dotnet run --project Server --framework netcoreapp3.0
    
    dotnet run --project Client --framework netcoreapp3.0
