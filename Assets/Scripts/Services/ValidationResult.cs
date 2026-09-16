using System.Collections.Generic;
using GradeReview.Data;

namespace GradeReview.Services
{
    /// <summary>
    /// Outcome of validating a set of classifications.
    /// <see cref="Outcomes"/> keeps the same order as the validated entries.
    /// </summary>
    public class ValidationResult
    {
        public ValidationResult(
            IReadOnlyList<ClassificationOutcome> outcomes,
            IReadOnlyList<StudentData> incorrect,
            IReadOnlyList<StudentData> unclassified,
            int correctCount)
        {
            Outcomes = outcomes;
            Incorrect = incorrect;
            Unclassified = unclassified;
            CorrectCount = correctCount;
        }

        public IReadOnlyList<ClassificationOutcome> Outcomes { get; }
        public IReadOnlyList<StudentData> Incorrect { get; }
        public IReadOnlyList<StudentData> Unclassified { get; }
        public int CorrectCount { get; }

        public int Total => Outcomes.Count;
        public bool IsAllCorrect => Total > 0 && CorrectCount == Total;
    }
}
