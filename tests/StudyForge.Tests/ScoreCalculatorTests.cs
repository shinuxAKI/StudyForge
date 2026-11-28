using StudyForge.Domain.Assessment;
using Xunit;
namespace StudyForge.Tests;
public sealed class ScoreCalculatorTests
{
 [Theory]
 [InlineData(4,4,100)]
 [InlineData(3,4,75)]
 [InlineData(1,3,33.33)]
 [InlineData(0,5,0)]
 public void Percentage_ReturnsExpectedRoundedScore(int correct,int total,decimal expected)
 { Assert.Equal(expected,ScoreCalculator.Percentage(correct,total)); }
 [Fact]
 public void Percentage_RejectsZeroQuestions()
 { Assert.Throws<ArgumentOutOfRangeException>(()=>ScoreCalculator.Percentage(0,0)); }
 [Fact]
 public void Percentage_RejectsMoreCorrectThanTotal()
 { Assert.Throws<ArgumentOutOfRangeException>(()=>ScoreCalculator.Percentage(3,2)); }
}