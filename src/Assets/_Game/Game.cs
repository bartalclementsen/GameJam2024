using Core.Containers;
using Core.Loggers;
using Core.Mediators;
using System;
using UnityEngine;

public class Game
{
    public static IContainer Container { get; private set; }

    private static Core.Loggers.ILogger _logger;

    [RuntimeInitializeOnLoadMethod]
    private static void Main()
    {
        Debug.Log("Startup");

        Debug.Log("Starting bootstrap");

        Bootstrap();

        UnityEngine.Random.InitState(Convert.ToInt32(DateTime.Now.Ticks % int.MaxValue));

        _logger = Container.Resolve<ILoggerFactory>().Create(null);

        _logger.Log("Done bootstrap");
    }

    private static void Bootstrap()
    {
        ContainerBuilder containerBuilder = new ContainerBuilder();

        containerBuilder.Register<ILoggerFactory, LoggerFactory>();
        containerBuilder.RegisterSingleton<IMessenger, Messenger>();

        Container = containerBuilder.Build();
    }
}
