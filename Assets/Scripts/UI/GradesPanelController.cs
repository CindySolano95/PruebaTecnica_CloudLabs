using System;
using System.Collections.Generic;
using GradeReview.Data;
using GradeReview.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GradeReview.UI
{
    /// <summary>
    /// Builds the grades table from the students it receives and validates the teacher's classifications.
    /// It never loads data by itself.
    /// </summary>
    public class GradesPanelController : MonoBehaviour
    {
        [SerializeField] private StudentRowView rowPrefab;
        [SerializeField] private RectTransform rowsContainer;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_Text studentCountText;
        [SerializeField] private Button validateButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button reloadButton;
        [SerializeField] private ResultBannerView resultBanner;

        private readonly List<StudentRowView> rows = new List<StudentRowView>();

        /// <summary>Raised when the teacher asks to clear the classifications of every screen.</summary>
        public event Action ResetAllRequested;

        /// <summary>Raised when the teacher asks to read estudiantes.json again.</summary>
        public event Action ReloadRequested;

        public IReadOnlyList<StudentRowView> Rows => rows;

        private void Awake()
        {
            validateButton.onClick.AddListener(Validate);
            resetButton.onClick.AddListener(RequestResetAll);
            reloadButton.onClick.AddListener(RequestReload);
        }

        private void OnDestroy()
        {
            validateButton.onClick.RemoveListener(Validate);
            resetButton.onClick.RemoveListener(RequestResetAll);
            reloadButton.onClick.RemoveListener(RequestReload);
        }

        public void Build(IReadOnlyList<StudentData> students)
        {
            ClearRows();
            resultBanner.Hide();

            if (students != null)
            {
                foreach (StudentData student in students)
                {
                    if (student == null) continue;

                    StudentRowView row = Instantiate(rowPrefab, rowsContainer);
                    row.Setup(student);
                    row.ClassificationChanged += HandleClassificationChanged;
                    rows.Add(row);
                }
            }

            UpdateStudentCount();
            UpdateResetButton();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        public void Validate()
        {
            var entries = new List<(StudentData, StudentClassification)>(rows.Count);
            foreach (StudentRowView row in rows)
                entries.Add((row.Student, row.Classification));

            ValidationResult result = ClassificationValidator.Validate(entries);

            for (int i = 0; i < rows.Count; i++)
                rows[i].ShowValidationOutcome(result.Outcomes[i]);

            resultBanner.ShowResult(result);
        }

        public void ShowLoadError(string message) => resultBanner.ShowError(message);

        public void ShowReloadSuccess(int studentCount) =>
            resultBanner.ShowSuccess(studentCount == 1 ? "Datos recargados: 1 estudiante." : $"Datos recargados: {studentCount} estudiantes.");

        /// <summary>Leaves every row unclassified, without validation feedback.</summary>
        public void ClearClassifications()
        {
            foreach (StudentRowView row in rows)
                row.ClearClassification();

            resultBanner.Hide();
            UpdateResetButton();
        }

        private void HandleClassificationChanged()
        {
            resultBanner.Hide();
            UpdateResetButton();
        }

        private void RequestResetAll() => ResetAllRequested?.Invoke();

        private void RequestReload() => ReloadRequested?.Invoke();

        // Only offered once every student has been classified.
        private void UpdateResetButton()
        {
            bool allClassified = rows.Count > 0;
            foreach (StudentRowView row in rows)
            {
                if (row.Classification != StudentClassification.Unclassified) continue;
                allClassified = false;
                break;
            }

            resetButton.gameObject.SetActive(allClassified);
        }

        private void ClearRows()
        {
            foreach (StudentRowView row in rows)
            {
                if (row == null) continue;

                row.ClassificationChanged -= HandleClassificationChanged;
                // Destroy is deferred to the end of the frame; deactivating first removes the row from the layout right away.
                row.gameObject.SetActive(false);
                Destroy(row.gameObject);
            }

            rows.Clear();
        }

        private void UpdateStudentCount()
        {
            studentCountText.text = rows.Count == 1 ? "1 estudiante" : $"{rows.Count} estudiantes";
        }
    }
}
