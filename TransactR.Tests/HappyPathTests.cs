using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Behaviors;
using TransactR.Tests.TestDoubles;
using Xunit;

namespace TransactR.Tests;

public class HappyPathTests
{
    private readonly Mock<IMementoStore<TestState>> _mementoStoreMock;
    private readonly TransactionContextProvider<SingleStepTestContext> _contextProvider;
    private readonly TestableTransactionalBehavior<SingleStepTestContext> _sut;

    public HappyPathTests()
    {
        _mementoStoreMock = new Mock<IMementoStore<TestState>>();
        var stateRestorerMock = new Mock<IStateRestorer<TestState>>();
        _contextProvider = new TransactionContextProvider<SingleStepTestContext>();
        var loggerMock = new Mock<ILogger<TransactionalBehaviorBase<TestRequest, TestResponse, SingleStepTestContext, TestState>>>();

        _sut = new TestableTransactionalBehavior<SingleStepTestContext>(
            _mementoStoreMock.Object,
            stateRestorerMock.Object,
            _contextProvider,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionIsNewAndCompletes_ShouldSaveAndThenRemoveMemento()
    {
        // Arrange
        var request = new TestRequest { TransactionId = "trx-happy-1" };
        var response = new TestResponse { Success = true };

        _mementoStoreMock.Setup(x => x.GetLatestAsync(request.TransactionId, default))
            .ReturnsAsync((Memento<TestState>?)null);

        // Act
        await _sut.Execute(request, (context) =>
        {
            context.State.Value = 123;
            return Task.FromResult(response);
        });

        // Assert
        _mementoStoreMock.Verify(x => x.SaveAsync(
            request.TransactionId,
            It.IsAny<TestState>(), // Verify that *any* state object was saved.
            It.IsAny<CancellationToken>()),
            Times.Once);

        _mementoStoreMock.Verify(x => x.RemoveTransactionAsync(
            request.TransactionId,
            It.IsAny<CancellationToken>()),
            Times.Once);

        _contextProvider.Context.State.Value.Should().Be(123);
    }
}

