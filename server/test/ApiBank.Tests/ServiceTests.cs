using Xunit;
using Moq;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using ApiBank.Core.Abstracts;
using ApiBank.Core.Attributes;
using ApiBank.Core.Interfaces;
using ApiBank.Infrastructure.ApiActions;
using ApiBank.Infrastructure.ApiServices;
using ApiBank.Infrastructure.Registries;
using ApiBank.Infrastructure.Services;

public class ServiceTests
{
    [Fact]
    public void GetAllServiceNames_ShouldReturnAllRegisteredServices()
    {
        // Arrange
        var actions = new List<IApiAction>
        {
            new Action1(),
            new Action2(),
            new Action3()
        };

        var mockTaskManager = new Mock<ITaskManager>();
        var mockLoggingService = new Mock<ILoggingService>();
        var serviceRegistry = new ServiceRegistry();

        var serviceA = new ApiServiceA(actions, mockTaskManager.Object, mockLoggingService.Object, serviceRegistry);
        var serviceB = new ApiServiceB(actions, mockTaskManager.Object, mockLoggingService.Object, serviceRegistry);

        serviceRegistry.RegisterService(serviceA);
        serviceRegistry.RegisterService(serviceB);

        // Act
        var serviceNames = serviceRegistry.GetAllServiceNames();

        // Assert
        var expectedServiceNames = new List<string> { "ServiceA", "ServiceB" };
        Assert.Equal(expectedServiceNames, serviceNames);
    }

    [Fact]
    public void GetServiceActions_ShouldReturnAllActionsForGivenService()
    {
        // Arrange
        var actions = new List<IApiAction>
        {
            new Action1(),
            new Action2(),
            new Action3()
        };

        var mockTaskManager = new Mock<ITaskManager>();
        var mockLoggingService = new Mock<ILoggingService>();
        var serviceRegistry = new ServiceRegistry();

        var serviceA = new ApiServiceA(actions, mockTaskManager.Object, mockLoggingService.Object, serviceRegistry);
        var serviceB = new ApiServiceB(actions, mockTaskManager.Object, mockLoggingService.Object, serviceRegistry);

        serviceRegistry.RegisterService(serviceA);
        serviceRegistry.RegisterService(serviceB);

        // Act
        var actionsForServiceA = serviceRegistry.GetServiceActions("ServiceA");
        var actionsForServiceB = serviceRegistry.GetServiceActions("ServiceB");

        // Assert
        var expectedActionsForServiceA = new List<string> { "Action1", "Action2" };
        var expectedActionsForServiceB = new List<string> { "Action3" };

        Assert.Equal(expectedActionsForServiceA, actionsForServiceA);
        Assert.Equal(expectedActionsForServiceB, actionsForServiceB);
    }

    [Fact]
    public async Task ExecuteServiceAction_ShouldExecuteActionAndReturnTaskId()
    {
        // Arrange
        var actionsA = new List<IApiAction>
        {
            new Action1(),
            new Action2()
        };

        var mockTaskManager = new Mock<ITaskManager>();
        var mockLoggingService = new Mock<ILoggingService>();
        var serviceRegistry = new ServiceRegistry();

        Guid expectedTaskId = Guid.NewGuid();  

        mockTaskManager.Setup(tm => tm.EnqueueTaskAsync(It.IsAny<Func<CancellationToken, Task>>()))
            .Callback<Func<CancellationToken, Task>>(func => func(CancellationToken.None))  
            .Returns(Task.FromResult(expectedTaskId));  

        var serviceA = new ApiServiceA(actionsA, mockTaskManager.Object, mockLoggingService.Object, serviceRegistry);
        serviceRegistry.RegisterService(serviceA);

        // Act
        var service = serviceRegistry.GetService("ServiceA");
        var taskId = await service.ExecuteActionAsync("Action1");  

        // Assert
        Assert.Equal(service, serviceRegistry.GetServiceByTaskId(taskId));  

        mockLoggingService.Verify(ls => ls.Log(taskId, It.IsAny<string>()), Times.AtLeastOnce);  
    }



}
