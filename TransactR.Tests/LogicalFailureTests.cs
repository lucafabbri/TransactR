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
    private readonly Mock<IMementoStore<TestStep, TestSagaContext>> _mementoStoreMock;
    private readonly Mock<IStateRestorer<TestStep, TestSagaContext>> _stateRestorerMock;
    private readonly TestableTransactionalBehavior<TestSagaContext> _sut;

    public LogicalFailureTests()
    {
        _mementoStoreMock = new Mock<IMementoStore<TestStep, TestSagaContext>>();
        _stateRestorerMock = new Mock<IStateRestorer<TestStep, TestSagaContext>>();
        var contextProvider = new TransactionContextProvider<TestStep, TestSagaContext>();
        var loggerMock = new Mock<ILogger<TransactionalBehaviorBase<TestRequest<TestSagaContext>, TestResponse, TestStep, TestSagaContext>>>();

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
        var request = new TestRequest<TestSagaContext> { TransactionId = "trx-logic-fail-1" };
        var failedResponse = new TestResponse { Success = false }; // This will trigger the failure
        var originalState = new TestSagaContext { Value = 1 };

        _mementoStoreMock.Setup(x => x.GetLatestAsync(request.TransactionId, default)).ReturnsAsync((Memento<TestStep, TestSagaContext>?)null);
        _mementoStoreMock.Setup(x => x.RetrieveAsync(request.TransactionId, TestStep.StepOne, default)).ReturnsAsync(originalState);

        // Act
        var action = async () => await _sut.Execute(request, (context) => Task.FromResult(failedResponse));

        // Assert
        await action.Should().ThrowAsync<TransactionEvaluationFailedException>();
        _stateRestorerMock.Verify(x => x.RestoreAsync(originalState, It.IsAny<CancellationToken>()), Times.Once);
    }
}

