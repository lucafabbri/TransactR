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
    private readonly Mock<IMementoStore<TestStep, SingleStepTestContext>> _mementoStoreMock;
    private readonly Mock<IStateRestorer<TestStep, SingleStepTestContext>> _stateRestorerMock;
    private readonly TestableTransactionalBehavior<SingleStepTestContext> _sut;

    public DisasterRecoveryTests()
    {
        _mementoStoreMock = new Mock<IMementoStore<TestStep, SingleStepTestContext>>();
        _stateRestorerMock = new Mock<IStateRestorer<TestStep, SingleStepTestContext>>();
        var contextProvider = new TransactionContextProvider<TestStep, SingleStepTestContext>();
        var loggerMock = new Mock<ILogger<TransactionalBehaviorBase<TestRequest<SingleStepTestContext>, TestResponse, TestStep, SingleStepTestContext>>>();

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
        var request = new TestRequest<SingleStepTestContext> { TransactionId = "trx-disaster-1" };
        var exception = new InvalidOperationException("Something went wrong");
        var originalState = new SingleStepTestContext { Value = 1 };

        _mementoStoreMock.Setup(x => x.GetLatestAsync(request.TransactionId, default)).ReturnsAsync((Memento<TestStep, SingleStepTestContext>?)null);
        _mementoStoreMock.Setup(x => x.RetrieveAsync(request.TransactionId, TestStep.StepOne, default)).ReturnsAsync(originalState);

        // Act
        var action = () => _sut.Execute(request, (context) => throw exception);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
        _stateRestorerMock.Verify(x => x.RestoreAsync(originalState, It.IsAny<CancellationToken>()), Times.Once);
    }
}

