using System.Collections.Generic;
using GradeReview.Data;

namespace GradeReview.Services
{
    /// <summary>
    /// Compares teacher classifications against <see cref="StudentData.IsApproved"/>.
    /// Shared by every screen that needs to validate, so the comparison lives in one place.
    /// </summary>
    public static class ClassificationValidator
    {
        public static ClassificationOutcome Evaluate(StudentData student, StudentClassification classification)
        {
            if (classification == StudentClassification.Unclassified)
                return ClassificationOutcome.Unclassified;

            bool markedApproved = classification == StudentClassification.Approved;
            return markedApproved == student.IsApproved ? ClassificationOutcome.Correct : ClassificationOutcome.Incorrect;
        }

        public static ValidationResult Validate(IReadOnlyList<(StudentData Student, StudentClassification Classification)> entries)
        {
            var outcomes = new List<ClassificationOutcome>(entries.Count);
            var incorrect = new List<StudentData>();
            var unclassified = new List<StudentData>();
            int correctCount = 0;

            foreach ((StudentData student, StudentClassification classification) in entries)
            {
                ClassificationOutcome outcome = Evaluate(student, classification);
                outcomes.Add(outcome);

                switch (outcome)
                {
                    case ClassificationOutcome.Correct:
                        correctCount++;
                        break;
                    case ClassificationOutcome.Incorrect:
                        incorrect.Add(student);
                        break;
                    default:
                        unclassified.Add(student);
                        break;
                }
            }

            return new ValidationResult(outcomes, incorrect, unclassified, correctCount);
        }
    }
}
