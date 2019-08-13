# 개요

https://github.com/grpc/grpc 를 사용하여 구현한 서버와 클라이언트입니다.

https://github.com/s2quake/Crema 프로젝트에서 WCF 대체할 목적으로 작업하고 있습니다.

# 도구

[.NET Core 3.0.100-preview7-012821](https://dotnet.microsoft.com/download/dotnet-core/3.0)

[Visual Studio Code](https://code.visualstudio.com/)

# 빌드

    git clone https://github.com/s2quake/grpc-sample.git --recursive
    cd grpc-sample
    dotnet restore
    dotnet build --framework netcoreapp3.0

# 실행

    dotnet run --project Server --framework netcoreapp3.0
    
    dotnet run --project Client --framework netcoreapp3.0

# 솔루션 구성

솔루션은 3개의 서버와 3개의 클라이언트 예제로 그리고 통신을 담당하는 1개의 라이브러리로 구성되어 있습니다.

## JSSoft.Communication

gRPC 을(를) 사용하여 서버와 통신을 할 수 있는 라이브러리 입니다.

서버와 클라이언트 간의 동일한 인터페이스(C#)를 사용하여 기능을 구현할 목적이기 때문에

gRPC 은(는)낮은 수준으로만 사용되었습니다.

따라서 gRPC 은(는) 내부에 감추어져 있으며 직접적으로 사용 하지는 못합니다.

gPRC 을(를) 사용하기 위한 프로토콜은 [adaptor.proto](JSSoft.Communication/Grpc/adaptor.proto) 에 정의되어 있습니다.

## Server-MEF, Client-MEF

[MEF](https://blog.powerumc.kr/189) 을 사용하여 서버와 클라이언트를 사용할 수 있도록 예제를 구성하였습니다.

여러 예제에서 같은 코드를 사용하기 때문에 #if MEF 을(를) 사용하였습니다.

    #if MEF
    ...
    #endif

## Server, Client

MEF 을(를) 사용하지 않고 필요한 인스턴스를 직접 생성하여 서버와 클라이언트를 구동할 수 있는 예제입니다.

인스턴스 생성 [Container.cs](JSSoft.Communication.ConsoleApp.Sharing/Container.cs#L102) 에 구현되어 있습니다.


## Server-Simple, Client-Simple

위 2개의 예제는 서버와 클라이언트를 구동후 능동적으로 기능을 사용할 수 있는 간단한 기본 기능들이 내재 되어 있습니다.

이 예제는 그러한 기능들을 제외하고 서버와 클라이언트만 구동하는 가장 간단한 방법을 구현해 놓았습니다.

# 처음부터 실행까지

## 1. 도구 설치

아래의 링크로 이동하여 .NET Core 3.0과 Visual Studio Code를 설치합니다.

본 예제 작성시에 .NET Core 버전은 3.0.100-preview7-012821 입니다.

[.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)

[Visual Studio Code](https://code.visualstudio.com/)

만약 설치가 이미 되어 있다면 이 과정을 건너 뛰셔도 됩니다.

## 2. 소스코드 받기

git 이 설치되어 있지 않다면 https://git-scm.com/ 에서 git을 설치하시기 바랍니다.

macOS 또는 linux 운영체제에서는 **terminal**을

Windows 에서는 **PowerShell**을 실행합니다.

    git clone https://github.com/s2quake/grpc-sample.git --recursive

> 해당 소스는 서브모듈을 포함하고 있기 때문에 --recursive 스위치를 사용합니다.

## 3. 소스 경로로 이동

    cd grpc-sample

## 4. 소스 빌드

    dotnet build --framework netcoreapp3.0

## 5. 서버 실행

    dotnet run --project Server --framework netcoreapp3.0

## 6. 클라이언트 실행

새로운 terminal이나 PowerShell을 실행하여 소스 경로로 이동하여 아래의 명령을 실행합니다.

    dotnet run --project Client --framework netcoreapp3.0

