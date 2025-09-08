using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using TransactR.Behaviors;
using TransactR.Tests.TestDoubles;
using Xunit;

namespace TransactR.Tests;

public class InteractiveSagaTests
{
    private readonly Mock<IMementoStore<TestState>> _mementoStoreMock;
    private readonly Mock<IStateRestorer<TestState>> _stateRestorerMock;
    private readonly TransactionContextProvider<TestSagaContext> _contextProvider;
    private readonly TestableTransactionalBehavior<TestSagaContext> _sut;

    public InteractiveSagaTests()
    {
        _mementoStoreMock = new Mock<IMementoStore<TestState>>();
        _stateRestorerMock = new Mock<IStateRestorer<TestState>>();
        _contextProvider = new TransactionContextProvider<TestSagaContext>();
        var loggerMock = new Mock<ILogger<TransactionalBehaviorBase<TestRequest, TestResponse, TestSagaContext, TestState>>>();
        _sut = new TestableTransactionalBehavior<TestSagaContext>(
            _mementoStoreMock.Object,
            _stateRestorerMock.Object,
            _contextProvider,
            loggerMock.Object);
    }

    //[Fact]
    //public async Task ExecuteAsync_WhenStartingSaga_ShouldSaveInitialStateAndPreserveMemento()
    //{
    //    // Arrange
    //    var request = new TestRequest { TransactionId = "trx-saga-1" };
    //    var response = new TestResponse { Success = true };

    //    _mementoStoreMock.Setup(x => x.GetLatestAsync(request.TransactionId, default))
    //        .ReturnsAsync((Memento<TestState>?)null);

    //    // Act
    //    await _sut.Execute(request, (context) =>
    //    {
    //        context.State.Value = 1;
    //        // Do NOT advance the step here. The context's evaluation logic
    //        // will correctly return InProgress based on the current step (StepOne).
    //        return Task.FromResult(response);
    //    });

    //    // Assert
    //    _mementoStoreMock.Verify(x => x.SaveAsync(request.TransactionId, It.IsAny<TestState>(), default), Times.Once);
    //    _mementoStoreMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<TestStep>(), default), Times.Never);
    //    _mementoStoreMock.Verify(x => x.RemoveTransactionAsync(It.IsAny<string>(), default), Times.Never);
    //    _contextProvider.Context.State.Step.Should().Be(TestStep.StepOne);
    //}

    //[Fact]
    //public async Task ExecuteAsync_WhenContinuingSaga_ShouldLoadLatestStateAndComplete()
    //{
    //    // Arrange
    //    var request = new TestRequest { TransactionId = "trx-saga-2" };
    //    var response = new TestResponse { Success = true };
    //    var existingState = new TestState(TestStep.StepOne) { Value = 1 };
    //    var latestMemento = new Memento<TestState>(existingState);

    //    _mementoStoreMock.Setup(x => x.GetLatestAsync(request.TransactionId, default))
    //        .ReturnsAsync(latestMemento);

    //    // Act
    //    await _sut.Execute(request, (context) =>
    //    {
    //        // Verify the context was hydrated correctly
    //        context.State.Step.Should().Be(TestStep.StepOne);
    //        context.State.Value.Should().Be(1);

    //        // Simulate the work of the final step
    //        context.State.Value = 2;
    //        context.AdvanceToStep(TestStep.StepTwo);

    //        return Task.FromResult(response);
    //    });

    //    // Assert
    //    // Verify it loaded the state
    //    _mementoStoreMock.Verify(x => x.GetLatestAsync(request.TransactionId, default), Times.Once);

    //    // Verify it saved the state for the current step (StepOne) before executing the handler, for rollback protection
    //    _mementoStoreMock.Verify(x => x.SaveAsync(request.TransactionId, existingState, default), Times.Once);

    //    // Verify that because the saga completed, the entire transaction was cleaned up
    //    _mementoStoreMock.Verify(x => x.RemoveTransactionAsync(request.TransactionId, default), Times.Once);

    //    // Verify a memento for the final, transient step was NOT saved
    //    _mementoStoreMock.Verify(x => x.SaveAsync(request.TransactionId, It.IsAny<TestState>(), default), Times.Never);

    //    // Verify the context reflects the final state
    //    _contextProvider.Context.State.Step.Should().Be(TestStep.StepTwo);
    //    _contextProvider.Context.State.Value.Should().Be(2);
    //}
}

