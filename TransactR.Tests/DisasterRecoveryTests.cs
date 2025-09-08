using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Behaviors;
using TransactR.Tests.TestDoubles;
using Xunit;

namespace TransactR.Tests;

public class DisasterRecoveryTests
{
    private readonly Mock<IMementoStore<TestState>> _mementoStoreMock;
    private readonly Mock<IStateRestorer<TestState>> _stateRestorerMock;
    private readonly TestableTransactionalBehavior<SingleStepTestContext> _sut;

    public DisasterRecoveryTests()
    {
        _mementoStoreMock = new Mock<IMementoStore<TestState>>();
        _stateRestorerMock = new Mock<IStateRestorer<TestState>>();
        var contextProvider = new TransactionContextProvider<SingleStepTestContext>();
        var loggerMock = new Mock<ILogger<TransactionalBehaviorBase<TestRequest, TestResponse, SingleStepTestContext, TestState>>>();

        _sut = new TestableTransactionalBehavior<SingleStepTestContext>(
            _mementoStoreMock.Object,
            _stateRestorerMock.Object,
            contextProvider,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionIsThrown_ShouldRollbackToCurrentStepByDefault()
    {
        // Arrange
        var request = new TestRequest { TransactionId = "trx-disaster-1" };
        var exception = new InvalidOperationException("Something went wrong");
        var originalState = new TestState { Value = 1 };

        _mementoStoreMock.Setup(x => x.GetLatestAsync(request.TransactionId, default)).ReturnsAsync((Memento<TestState>?)null);
        _mementoStoreMock.Setup(x => x.RetrieveAsync(request.TransactionId, TestStep.StepOne, default)).ReturnsAsync(originalState);

        // Act
        var action = () => _sut.Execute(request, (context) => throw exception);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
        _stateRestorerMock.Verify(x => x.RestoreAsync(originalState, It.IsAny<CancellationToken>()), Times.Once);
    }
}

