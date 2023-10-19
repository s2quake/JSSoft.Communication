// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Google.Protobuf.WellKnownTypes;

namespace JSSoft.Communication.Logging;

public static class LogUtility
{
    private static ILogger logger = EmptyLogger.Default;
    private static LogLevel logLevel = LogLevel.Fatal;

    public static ILogger Logger
    {
        get => logger;
        set => logger = value;
    }

    public static LogLevel LogLevel
    {
        get => logLevel;
        set => logLevel = value;
    }

    public static void Debug(object message)
    {
        if (logLevel >= LogLevel.Debug)
            LoggerInternal.Debug(message);
    }

    public static void Info(object message)
    {
        if (logLevel >= LogLevel.Info)
            LoggerInternal.Info(message);
    }

    public static void Error(object message)
    {
        if (logLevel >= LogLevel.Error)
            LoggerInternal.Error(message);
    }

    public static void Warn(object message)
    {
        if (logLevel >= LogLevel.Warn)
            LoggerInternal.Warn(message);
    }

    public static void Fatal(object message)
    {
        if (logLevel >= LogLevel.Fatal)
            LoggerInternal.Fatal(message);
    }

    private static ILogger LoggerInternal => logger ?? EmptyLogger.Default;
}
