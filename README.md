# 개요

https://github.com/s2quake/Crema 프로젝트에서 WCF 대체할 목적으로 Grpc 를 사용한 서버 및 클라이언트 예제입니다.

# 도구

    .NET Core 3.0

    Visual Studio Code

# 빌드

    git clone https://github.com/s2quake/grpc-sample.git --recursive
    cd grpc-sample
    dotnet restore
    dotnet build


# 실행

    dotnet run --project Server --framework netcoreapp3.0
    
    dotnet run --project Client --framework netcoreapp3.0
