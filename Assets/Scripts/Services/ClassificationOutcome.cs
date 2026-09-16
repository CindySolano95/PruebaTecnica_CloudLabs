namespace GradeReview.Services
{
    /// <summary>Result of comparing one teacher classification against the real grade.</summary>
    public enum ClassificationOutcome
    {
        Unclassified,
        Correct,
        Incorrect
    }
}
