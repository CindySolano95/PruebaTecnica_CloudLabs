using System.Collections.Generic;
using GradeReview.Data;
using GradeReview.DragDrop;
using GradeReview.Services;
using UnityEngine;
using UnityEngine.UI;

namespace GradeReview.UI
{
    /// <summary>
    /// Scene entry point: loads the students once, hands them to the panels and coordinates navigation and the global reset.
    /// </summary>
    public class AppController : MonoBehaviour
    {
        [SerializeField] private GradesPanelController gradesPanel;
        [SerializeField] private DragDropPanelController dragDropPanel;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button backButton;

        private IReadOnlyList<StudentData> students;
        private bool isDragDropBuilt;

        private void Awake()
        {
            continueButton.onClick.AddListener(ShowDragDrop);
            backButton.onClick.AddListener(ShowGrades);
            gradesPanel.ResetAllRequested += ResetAll;
            dragDropPanel.ResetAllRequested += ResetAll;
        }

        private void OnDestroy()
        {
            continueButton.onClick.RemoveListener(ShowDragDrop);
            backButton.onClick.RemoveListener(ShowGrades);
            gradesPanel.ResetAllRequested -= ResetAll;
            dragDropPanel.ResetAllRequested -= ResetAll;
        }

        private void Start()
        {
            // On failure the repository already logs the cause and returns an empty list.
            new StudentRepository().TryLoad(out students, out _);
            gradesPanel.Build(students);
            ShowGrades();
        }

        private void ShowDragDrop()
        {
            gradesPanel.gameObject.SetActive(false);
            dragDropPanel.gameObject.SetActive(true);

            // Built once with the same StudentData as the grades table; afterwards each screen keeps its own board.
            if (isDragDropBuilt) return;
            dragDropPanel.Build(students);
            isDragDropBuilt = true;
        }

        private void ShowGrades()
        {
            dragDropPanel.gameObject.SetActive(false);
            gradesPanel.gameObject.SetActive(true);
        }

        private void ResetAll()
        {
            gradesPanel.ClearClassifications();
            if (isDragDropBuilt) dragDropPanel.ClearClassifications();
        }
    }
}
