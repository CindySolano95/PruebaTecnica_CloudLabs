using System.Collections.Generic;
using GradeReview.Data;
using GradeReview.DragDrop;
using GradeReview.Services;
using UnityEngine;
using UnityEngine.UI;

namespace GradeReview.UI
{
    /// <summary>
    /// Scene entry point: loads the students, hands them to the panels and coordinates navigation, reload and the global reset.
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
            gradesPanel.ReloadRequested += ReloadStudents;
            dragDropPanel.ResetAllRequested += ResetAll;
        }

        private void OnDestroy()
        {
            continueButton.onClick.RemoveListener(ShowDragDrop);
            backButton.onClick.RemoveListener(ShowGrades);
            gradesPanel.ResetAllRequested -= ResetAll;
            gradesPanel.ReloadRequested -= ReloadStudents;
            dragDropPanel.ResetAllRequested -= ResetAll;
        }

        private void Start()
        {
            LoadStudents(announceSuccess: false);
            ShowGrades();
        }

        private void ReloadStudents() => LoadStudents(announceSuccess: true);

        private void LoadStudents(bool announceSuccess)
        {
            // On failure the repository logs the cause and returns an empty list; the app keeps running.
            bool loaded = new StudentRepository().TryLoad(out students, out string loadError);
            gradesPanel.Build(students);
            // The drag & drop board is rebuilt from the new data the next time it opens.
            isDragDropBuilt = false;

            if (!loaded) gradesPanel.ShowLoadError(loadError);
            else if (announceSuccess) gradesPanel.ShowReloadSuccess(students.Count);
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
