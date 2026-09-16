using System;
using System.Globalization;
using GradeReview.Data;
using GradeReview.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GradeReview.UI
{
    /// <summary>
    /// One row of the grades table: shows a student and captures the teacher's classification.
    /// It displays validation feedback but never decides whether a classification is correct.
    /// </summary>
    public class StudentRowView : MonoBehaviour
    {
        private const string EmptyValue = "—";

        [Header("Texts")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text codeText;
        [SerializeField] private TMP_Text emailText;
        [SerializeField] private TMP_Text gradeText;

        [Header("Classification")]
        [SerializeField] private Toggle approvedToggle;
        [SerializeField] private Toggle failedToggle;

        [Header("Status")]
        [SerializeField] private Image statusIcon;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private Sprite pendingSprite;
        [SerializeField] private Sprite approvedSprite;
        [SerializeField] private Sprite failedSprite;
        [SerializeField] private Color pendingColor = new Color32(0x6B, 0x77, 0x85, 0xFF);
        [SerializeField] private Color approvedColor = new Color32(0x2E, 0x9E, 0x52, 0xFF);
        [SerializeField] private Color failedColor = new Color32(0xD6, 0x45, 0x45, 0xFF);

        [Header("Validation feedback")]
        [SerializeField] private Image gradePill;
        [SerializeField] private Color neutralPillColor = new Color32(0xDD, 0xE4, 0xEC, 0xFF);
        [SerializeField] private Color correctPillColor = new Color32(0x2E, 0x9E, 0x52, 0xFF);
        [SerializeField] private Color incorrectPillColor = new Color32(0xD6, 0x45, 0x45, 0xFF);
        [SerializeField] private Color neutralGradeTextColor = new Color32(0x1F, 0x3A, 0x5F, 0xFF);
        [SerializeField] private Color feedbackGradeTextColor = Color.white;

        /// <summary>Raised when the teacher changes this row's classification.</summary>
        public event Action ClassificationChanged;

        public StudentData Student { get; private set; }

        public StudentClassification Classification
        {
            get
            {
                if (approvedToggle.isOn) return StudentClassification.Approved;
                if (failedToggle.isOn) return StudentClassification.Failed;
                return StudentClassification.Unclassified;
            }
        }

        private void Awake()
        {
            approvedToggle.onValueChanged.AddListener(HandleToggleChanged);
            failedToggle.onValueChanged.AddListener(HandleToggleChanged);
        }

        private void OnDestroy()
        {
            approvedToggle.onValueChanged.RemoveListener(HandleToggleChanged);
            failedToggle.onValueChanged.RemoveListener(HandleToggleChanged);
        }

        public void Setup(StudentData student)
        {
            Student = student;

            nameText.text = ValueOrPlaceholder(student.FullName);
            codeText.text = ValueOrPlaceholder(student.Code);
            emailText.text = ValueOrPlaceholder(student.Email);
            // Invariant culture keeps "4.5" on systems that use a decimal comma.
            // "0.0#" avoids rounding 2.99 up to "3.0", which would contradict a failing grade.
            gradeText.text = student.FinalGrade.ToString("0.0#", CultureInfo.InvariantCulture);

            ClearClassification();
        }

        public void ClearClassification()
        {
            approvedToggle.SetIsOnWithoutNotify(false);
            failedToggle.SetIsOnWithoutNotify(false);
            RefreshStatus();
            ClearValidationFeedback();
        }

        public void ShowValidationOutcome(ClassificationOutcome outcome)
        {
            switch (outcome)
            {
                case ClassificationOutcome.Correct:
                    SetGradePill(correctPillColor, feedbackGradeTextColor);
                    break;
                case ClassificationOutcome.Incorrect:
                    SetGradePill(incorrectPillColor, feedbackGradeTextColor);
                    break;
                default:
                    ClearValidationFeedback();
                    break;
            }
        }

        public void ClearValidationFeedback() => SetGradePill(neutralPillColor, neutralGradeTextColor);

        private void HandleToggleChanged(bool _)
        {
            RefreshStatus();
            // A previous validation no longer describes this row.
            ClearValidationFeedback();
            ClassificationChanged?.Invoke();
        }

        private void RefreshStatus()
        {
            switch (Classification)
            {
                case StudentClassification.Approved:
                    ShowStatus(approvedSprite, "Aprobado", approvedColor);
                    break;
                case StudentClassification.Failed:
                    ShowStatus(failedSprite, "Reprobado", failedColor);
                    break;
                default:
                    ShowStatus(pendingSprite, "Pendiente", pendingColor);
                    break;
            }
        }

        private void ShowStatus(Sprite sprite, string label, Color color)
        {
            statusIcon.sprite = sprite;
            statusLabel.text = label;
            statusLabel.color = color;
        }

        private void SetGradePill(Color pillColor, Color textColor)
        {
            gradePill.color = pillColor;
            gradeText.color = textColor;
        }

        private static string ValueOrPlaceholder(string value) =>
            string.IsNullOrEmpty(value) ? EmptyValue : value;
    }
}
