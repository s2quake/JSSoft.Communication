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

    

# 간단한 예제를 작성해보면서 무엇인지 알아보기

## 1. Visual Studio Code 폴더 열기

Visual Studio Code 를 실행후 폴더 열기로 소스 위치를 선택합니다.

| 폴더 열기 | macOS       | Windows     |
| ------- | ----------- | ----------- |
| 단축키   | <kdb>&#8984;</kdb> + <kbd>O</kbd> | <kbd>Ctrl</kbd> + <kbd>K</kbd>  <kbd>Ctrl</kbd> + <kbd>O</kbd>       |


## 2. 서버 프로젝트 만들기

상단의 보기 메뉴에서 터미널을 선택하여 터미널 창을 띄웁니다.

| 터미널 | macOS       | Windows     |
| ------- | ----------- | ----------- |
| 단축키   | <kdb>⌃</kdb> + <kbd>~</kbd> | <kbd>Ctrl</kbd> + <kbd>~</kbd> |

> 아래 명령을 실행하여 프로젝트를 생성하고 필요한 설정을 수행합니다.

    mkdir Server-Test
    dotnet new console -o Server-Test
    dotnet sln add Server-Test
    dotnet add Server-Test reference JSSoft.Communication

* Server-Test 경로를 생성합니다.
* Server-Test 경로에 콘솔 프로젝트를 생성합니다.
* Server-Test 프로젝트를 솔루션에 추가합니다.
* Server-Test 프로젝트에 JSSoft.Communication 프로젝트를 추가합니다.

## 3. 나만의 서비스 정의하기

서버를 구축하기 전에 클라이언트에서 사용할 간단한 서비스를 제작해봅니다.

> Server-Test 경로내에 `IMyService.cs` 파일을 만들고 아래와 같은 인터페이스를 정의합니다.

```csharp
using System.Threading.Tasks;

namespace Server_Test
{
    interface IMyService
    {
        string Login(string userID);

        Task<(string product, string version)> GetVersionAsync();
    }
}
```

아주 간단한 Login 메소드와 복잡해보이고 웬지 코딩 숙련도가 높아질것 같은 비동기 메소드 GetVersionAsync 를 정의하였습니다.

## 4. 나만의 서비스 구현하기

이제 인터페이스를 정의했으니 실제 작업을 구현해봅니다.

> Server-Test 경로내에 `MyService.cs` 파일을 만들고 아래와 같이 작업을 구현합니다.

```csharp
using System;
using System.Threading.Tasks;

namespace Server_Test
{
    class MyService : IMyService
    {
        public string Login(string userID)
        {
            Console.WriteLine($"logged in: '{userID}'");
            return $"{Guid.NewGuid()}";
        }

        public Task<(string product, string version)> GetVersionAsync()
        {
            return Task.Run(() =>
            {
                return ("MyServer", "1.0.0.0");
            });
        }
    }
}
```

## 5. 나만의 서비스 준비하기

이제 구현된 서비스를 서버에 사용할 준비를 해야 합니다.

서버에는 다수의 서비스를 사용할 수 있기 때문에 이를 관리하기 위하여

`IServiceHost` 라는 인터페이스를 구현해야 합니다.

`IServiceHost` 은(는) 서비스의 주인 역할을 하며 서버에서 잘 사용할 수 있게 여러 제반 사항을 만들어줍니다.

IServiceHost 은(는) 직접 구현하기 힘들기 때문에 구현된 기본 클래스인 `ServerServiceHostBase` 을(를) 상속받아 정의합니다.

> Server-Test 경로내에 `MyServiceHost.cs` 파일을 만들고 아래와 같이 내용을 구현합니다.

```csharp
using System;
using JSSoft.Communication;

namespace Server_Test
{
    class MyServiceHost : ServerServiceHostBase<IMyService>
    {
        protected override IMyService CreateService()
        {
            return new MyService();
        }

        protected override void DestroyService(IMyService service)
        {

        }
    }
}
```

## 6. 서버 실행하기

이제 준비된 서비스를 사용하여 서버를 실행해봅니다.

> Server-Test 경로내에 `ServerContext.cs` 파일을 만들고 아래와 같이 내용을 구현합니다.

```csharp
using JSSoft.Communication;

namespace Server_Test
{
    class ServerContext : ServerContextBase
    {
        public ServerContext()
            : base(new MyServiceHost())
        {

        }
    }
}
```

> Server-Test 경로내에 Program.cs 내용을 다음과 같이 작성합니다.

```csharp
using System;
using System.Threading.Tasks;
using JSSoft.Communication.Services;

namespace Server_Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var service = new MyService();
            var serviceHost = new MyServiceHost();
            var serviceContext = new ServerContext(serviceHost);

            var token = await serviceContext.OpenAsync();

            Console.WriteLine("서버가 시작되었습니다.");
            Console.WriteLine("종료하려면 아무 키나 누르세요.");
            Console.ReadKey();

            await serviceContext.CloseAsync(token);
        }
    }
}
```

## 7. 클라이언트 프로젝트 만들기

서버 프로젝트를 생성할때와 마찬가지로 터미널 창을 열고 아래 명령을 실행합니다.

    mkdir Client-Test
    dotnet new console -o Client-Test
    dotnet sln add Client-Test
    dotnet add Client-Test reference JSSoft.Communication

## 8. 클라이언트 구현하기

```csharp
using System;
using JSSoft.Communication;

namespace Client_Test
{
    class MyServiceHost : ClientServiceHostBase<IMyService>
    {

    }
}
```

> Server-Test 경로내에 Program.cs 내용을 다음과 같이 작성합니다.
```csharp
using JSSoft.Communication;

namespace Client_Test
{
    class ClientContext : ClientContextBase
    {
        public ClientContext()
            : base(new MyServiceHost())
        {

        }
    }
}
```

> Client-Test 경로내에 Program.cs 내용을 다음과 같이 작성합니다.
```csharp
using System;
using System.Threading.Tasks;
using JSSoft.Communication.Services;

namespace Client_Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var service = new MyService();
            var serviceHost = new MyServiceHost();
            var serviceContext = new ClientContext(serviceHost);

            var token = await serviceContext.OpenAsync();

            Console.WriteLine("서버가 시작되었습니다.");
            Console.WriteLine("종료하려면 아무 키나 누르세요.");
            Console.ReadKey();

            await serviceContext.CloseAsync(token);
        }
    }
}
```