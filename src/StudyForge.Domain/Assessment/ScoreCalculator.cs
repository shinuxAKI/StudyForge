namespace StudyForge.Domain.Assessment;
public static class ScoreCalculator
{
 public static decimal Percentage(int correct,int total)
 {
  if(total<=0)throw new ArgumentOutOfRangeException(nameof(total),"Total questions must be positive.");
  if(correct<0||correct>total)throw new ArgumentOutOfRangeException(nameof(correct));
  return Math.Round((decimal)correct/total*100m,2,MidpointRounding.AwayFromZero);
 }
}