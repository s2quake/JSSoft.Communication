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
