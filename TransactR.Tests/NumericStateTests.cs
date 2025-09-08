using System;
using System.Linq;
using TransactR;
using Xunit;

namespace TransactR.Tests;

/// <summary>
/// Contains all unit tests for the State management classes within the TransactR namespace.
/// Tests are organized into nested classes corresponding to the class under test.
/// </summary>
public class StateTests
{
    /// <summary>
    /// Tests for the abstract NumericState class, using a concrete implementation.
    /// </summary>
    public class NumericStateTests
    {
        private class TestNumericState : NumericState
        {
            public TestNumericState() { }
            public TestNumericState(int step) : base(step) { }
        }

        [Fact]
        public void Constructor_Default_InitializesStepToZero()
        {
            // Arrange & Act
            var state = new TestNumericState();

            // Assert
            Assert.Equal(0, state.InnerStep);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-10)]
        [InlineData(100)]
        public void Constructor_WithInitialValue_SetsStepCorrectly(int initialStep)
        {
            // Arrange & Act
            var state = new TestNumericState(initialStep);

            // Assert
            Assert.Equal(initialStep, state.InnerStep);
        }

        [Fact]
        public void TryIncrementStep_Always_ReturnsTrueAndIncrementsStep()
        {
            // Arrange
            var state = new TestNumericState(5);

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal(6, state.InnerStep);
        }

        [Fact]
        public void TryIncrementStep_FromMaxValue_WrapsAroundToIntMinValue()
        {
            // Arrange
            var state = new TestNumericState(int.MaxValue);

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal(int.MinValue, state.InnerStep); // Documents the current behavior of integer overflow.
        }

        [Fact]
        public void TryDecrementStep_WhenStepIsPositive_ReturnsTrueAndDecrementsStep()
        {
            // Arrange
            var state = new TestNumericState(1);

            // Act
            var result = state.TryDecrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal(0, state.InnerStep);
        }

        [Fact]
        public void TryDecrementStep_WhenStepIsZero_ReturnsFalseAndStepRemainsZero()
        {
            // Arrange
            var state = new TestNumericState(0);

            // Act
            var result = state.TryDecrementStep();

            // Assert
            Assert.False(result);
            Assert.Equal(0, state.InnerStep);
        }

        [Fact]
        public void TrySetStep_WithValidInt_ReturnsTrueAndSetsStep()
        {
            // Arrange
            var state = new TestNumericState(10);
            const int newStep = 42;

            // Act
            var result = state.TrySetStep(newStep);

            // Assert
            Assert.True(result);
            Assert.Equal(newStep, state.InnerStep);
        }

        [Fact]
        public void TrySetStep_WithInvalidType_ReturnsFalseAndStepIsUnchanged()
        {
            // Arrange
            var state = new TestNumericState(10);
            const string newStep = "not-an-int";

            // Act
            var result = state.TrySetStep(newStep);

            // Assert
            Assert.False(result);
            Assert.Equal(10, state.InnerStep);
        }
    }

    /// <summary>
    /// Tests for the abstract StringState class, using a concrete implementation.
    /// </summary>
    public class StringStateTests
    {
        private class TestStringState : StringState
        {
            public override string[] Steps => new[] { "Initial", "Processing", "Completed", "Failed" };

            public TestStringState(string step) : base(step) { }
        }

        private class UnorderedStringState : StringState
        {
            // States are not in alphabetical order to test against IComparable's natural sort order.
            public override string[] Steps => new[] { "Zebra", "Apple", "Mango" };
            public UnorderedStringState(string step) : base(step) { }
        }


        [Fact]
        public void TryIncrementStep_FromMiddleOfSequence_MovesToNextState()
        {
            // Arrange
            var state = new TestStringState("Processing");

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal("Completed", state.InnerStep);
        }

        [Fact]
        public void TryIncrementStep_FromLastState_ReturnsFalseAndStateIsUnchanged()
        {
            // Arrange
            var state = new TestStringState("Failed");

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.False(result);
            Assert.Equal("Failed", state.InnerStep);
        }

        [Fact]
        public void TryIncrementStep_WithNonAlphabeticalOrder_FollowsArrayIndexOrderNotComparableLogic()
        {
            // Arrange
            var state = new UnorderedStringState("Apple"); // "Apple" is at index 1

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.True(result);
            // Should move to "Mango" (index 2), not "Zebra" which would be next alphabetically.
            Assert.Equal("Mango", state.InnerStep);
        }

        [Fact]
        public void TryDecrementStep_FromMiddleOfSequence_MovesToPreviousState()
        {
            // Arrange
            var state = new TestStringState("Completed");

            // Act
            var result = state.TryDecrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal("Processing", state.InnerStep);
        }

        [Fact]
        public void TryDecrementStep_FromFirstState_ReturnsFalseAndStateIsUnchanged()
        {
            // Arrange
            var state = new TestStringState("Initial");

            // Act
            var result = state.TryDecrementStep();

            // Assert
            Assert.False(result);
            Assert.Equal("Initial", state.InnerStep);
        }

        [Theory]
        [InlineData("Processing")]
        [InlineData("Completed")]
        public void TrySetStep_WithValidStateAtIndexGreaterThanZero_ReturnsTrueAndSetsState(string validStep)
        {
            // Arrange
            var state = new TestStringState("Initial");

            // Act
            var result = state.TrySetStep(validStep);

            // Assert
            Assert.True(result);
            Assert.Equal(validStep, state.InnerStep);
        }

        [Fact]
        public void TrySetStep_WithStateAtZeroIndex_ShouldReturnTrueAndSetState()
        {
            // Arrange
            var state = new TestStringState("Completed");
            const string zeroIndexState = "Initial";

            // Act
            var result = state.TrySetStep(zeroIndexState);

            // Assert
            // This test defines the future expected behavior that setting the state to any valid step,
            // including the one at index 0, should be a successful operation.
            // It is expected to FAIL until the logic in `StringState.SetStepInternal` is updated from `> 0` to `>= 0`.
            Assert.True(result);
            Assert.Equal(zeroIndexState, state.InnerStep);
        }

        [Fact]
        public void TrySetStep_WithNonExistentState_ReturnsFalseAndStateIsUnchanged()
        {
            // Arrange
            var state = new TestStringState("Initial");

            // Act
            var result = state.TrySetStep("NonExistentState");

            // Assert
            Assert.False(result);
            Assert.Equal("Initial", state.InnerStep);
        }
    }

    /// <summary>
    /// Tests for the abstract EnumState class, using a concrete implementation.
    /// </summary>
    public class EnumStateTests
    {
        public enum TestEnum { StateA, StateB, StateC }

        private class TestEnumState : EnumState<TestEnum>
        {
            public TestEnumState(TestEnum step) : base(step) { }
        }



        [Fact]
        public void TryIncrementStep_FromMiddleOfSequence_MovesToNextState()
        {
            // Arrange
            var state = new TestEnumState(TestEnum.StateA);

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal(TestEnum.StateB, state.InnerStep);
        }

        [Fact]
        public void TryIncrementStep_FromLastState_ReturnsFalseAndStateIsUnchanged()
        {
            // Arrange
            var state = new TestEnumState(TestEnum.StateC);

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.False(result);
            Assert.Equal(TestEnum.StateC, state.InnerStep);
        }

        [Fact]
        public void TryDecrementStep_FromMiddleOfSequence_MovesToPreviousState()
        {
            // Arrange
            var state = new TestEnumState(TestEnum.StateC);

            // Act
            var result = state.TryDecrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal(TestEnum.StateB, state.InnerStep);
        }

        [Fact]
        public void TryDecrementStep_FromFirstState_ReturnsFalseAndStateIsUnchanged()
        {
            // Arrange
            var state = new TestEnumState(TestEnum.StateA);

            // Act
            var result = state.TryDecrementStep();

            // Assert
            Assert.False(result);
            Assert.Equal(TestEnum.StateA, state.InnerStep);
        }

        [Fact]
        public void TrySetStep_WithValidStateAtIndexGreaterThanZero_ReturnsTrueAndSetsState()
        {
            // Arrange
            var state = new TestEnumState(TestEnum.StateA);

            // Act
            var result = state.TrySetStep(TestEnum.StateB);

            // Assert
            Assert.True(result);
            Assert.Equal(TestEnum.StateB, state.InnerStep);
        }

        [Fact]
        public void TrySetStep_WithStateAtZeroIndex_ShouldReturnTrueAndSetState()
        {
            // Arrange
            var state = new TestEnumState(TestEnum.StateB);
            const TestEnum zeroIndexState = TestEnum.StateA;

            // Act
            var result = state.TrySetStep(zeroIndexState);

            // Assert
            // This test defines the future expected behavior that setting the state to any valid step,
            // including the one at index 0, should be a successful operation.
            // It is expected to FAIL until the logic in `EnumState.SetStepInternal` is updated from `> 0` to `>= 0`.
            Assert.True(result);
            Assert.Equal(zeroIndexState, state.InnerStep);
        }
    }

    /// <summary>
    /// Contains tests for constructor edge cases and behavior with invalid initial data.
    /// </summary>
    public class StateConstructorEdgeCasesTests
    {
        private class TestStringState : StringState
        {
            public override string[] Steps => new[] { "Initial", "Processing", "Completed" };
            public TestStringState(string step) : base(step) { }
        }

        private class TestStringStateWithEmptySteps : StringState
        {
            public override string[] Steps => Array.Empty<string>();
            public TestStringStateWithEmptySteps(string step) : base(step) { }
        }

        public enum TestEnum { StateA = 0, StateB = 1, StateC = 2 }

        private class TestEnumState : EnumState<TestEnum>
        {
            public TestEnumState(TestEnum step) : base(step) { }
        }

        [Fact]
        public void StringState_Constructor_WithNull_InitializesStepToNull()
        {
            // Arrange & Act
            var state = new TestStringState(null!);

            // Assert
            Assert.Null(state.InnerStep);
        }

        [Fact]
        public void StringState_Constructor_WithOutOfRangeString_ShouldInitializeToFirstStep()
        {
            // Arrange
            const string outOfRangeState = "NonExistentState";

            // Act
            var state = new TestStringState(outOfRangeState);
            var expectedFirstStep = state.Steps.First();

            // Assert
            // This test defines the future behavior that an invalid initial state should result in the first valid step.
            // It is expected to FAIL until the StringState constructor logic is updated.
            Assert.Equal(expectedFirstStep, state.InnerStep);
        }

        [Fact]
        public void StringState_Constructor_WithEmptyStepsAndOutOfRangeString_ShouldInitializeStepToNull()
        {
            // Arrange
            const string outOfRangeState = "NonExistentState";

            // Act
            var state = new TestStringStateWithEmptySteps(outOfRangeState);

            // Assert
            // This test defines that if Steps is empty, an invalid initial state should result in null.
            // It is expected to FAIL until the StringState constructor logic is updated.
            Assert.Null(state.InnerStep);
        }

        [Fact]
        public void StringState_TryIncrementStep_WhenInitialStateIsNull_MovesToFirstState()
        {
            // Arrange
            var state = new TestStringState(null!);
            var expectedFirstState = state.Steps.First();

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.True(result);
            Assert.Equal(expectedFirstState, state.InnerStep);
        }

        [Fact]
        public void EnumState_Constructor_WithUndefinedEnumValue_InitializesStepToThatValue()
        {
            // Arrange
            var undefinedValue = (TestEnum)99;

            // Act
            var state = new TestEnumState(undefinedValue);

            // Assert
            Assert.Equal(undefinedValue, state.InnerStep);
        }

        [Fact]
        public void EnumState_TryIncrementStep_WhenInitialStateIsUndefined_MovesToDefaultState()
        {
            // Arrange
            var undefinedValue = (TestEnum)99;
            var state = new TestEnumState(undefinedValue);

            // Act
            var result = state.TryIncrementStep();

            // Assert
            Assert.True(result);
            // The current logic sets InnerStep to default!, which is TestEnum.StateA (value 0).
            Assert.Equal(TestEnum.StateA, state.InnerStep);
        }
    }
}

