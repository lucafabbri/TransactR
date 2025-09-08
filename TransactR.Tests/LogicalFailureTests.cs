using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Behaviors;
using TransactR.Exceptions;
using TransactR.Tests.TestDoubles;
using Xunit;

namespace TransactR.Tests;

public class LogicalFailureTests
{
    private readonly Mock<IMementoStore<TestState>> _mementoStoreMock;
    private readonly Mock<IStateRestorer<TestState>> _stateRestorerMock;
    private readonly TestableTransactionalBehavior<TestSagaContext> _sut;

    public LogicalFailureTests()
    {
        _mementoStoreMock = new Mock<IMementoStore<TestState>>();
        _stateRestorerMock = new Mock<IStateRestorer<TestState>>();
        var contextProvider = new TransactionContextProvider<TestSagaContext>();
        var loggerMock = new Mock<ILogger<TransactionalBehaviorBase<TestRequest, TestResponse, TestSagaContext, TestState>>>();

        _sut = new TestableTransactionalBehavior<TestSagaContext>(
            _mementoStoreMock.Object,
            _stateRestorerMock.Object,
            contextProvider,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEvaluationIsFailed_ShouldRollbackAndThrow()
    {
        // Arrange
        var request = new TestRequest { TransactionId = "trx-logic-fail-1" };
        var failedResponse = new TestResponse { Success = false }; // This will trigger the failure
        var originalState = new TestState { Value = 1 };

        _mementoStoreMock.Setup(x => x.GetLatestAsync(request.TransactionId, default)).ReturnsAsync((Memento<TestState>?)null);
        _mementoStoreMock.Setup(x => x.RetrieveAsync(request.TransactionId, TestStep.StepOne, default)).ReturnsAsync(originalState);

        // Act
        var action = () => _sut.Execute(request, (context) => Task.FromResult(failedResponse));

        // Assert
        await action.Should().ThrowAsync<TransactionEvaluationFailedException>();
        _stateRestorerMock.Verify(x => x.RestoreAsync(originalState, It.IsAny<CancellationToken>()), Times.Once);
    }
}

