using System;
using System.Collections.Generic;
using GradeReview.Data;
using GradeReview.Services;
using GradeReview.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GradeReview.DragDrop
{
    /// <summary>
    /// Builds the drag &amp; drop screen from the students it receives and verifies the final classification.
    /// </summary>
    public class DragDropPanelController : MonoBehaviour
    {
        [SerializeField] private StudentDragItem cardPrefab;
        [SerializeField] private RectTransform dragLayer;
        [SerializeField] private StudentDropZone unassignedZone;
        [SerializeField] private StudentDropZone approvedZone;
        [SerializeField] private StudentDropZone failedZone;
        [SerializeField] private TMP_Text studentCountText;
        [SerializeField] private Button verifyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private ResultBannerView resultBanner;

        private readonly List<StudentDragItem> cards = new List<StudentDragItem>();

        /// <summary>Raised when the teacher asks to clear the classifications of every screen.</summary>
        public event Action ResetAllRequested;

        public IReadOnlyList<StudentDragItem> Cards => cards;

        private void Awake()
        {
            verifyButton.onClick.AddListener(Verify);
            resetButton.onClick.AddListener(RequestResetAll);
            foreach (StudentDropZone zone in Zones())
                zone.CardPlaced += HandleCardPlaced;
        }

        private void OnDestroy()
        {
            verifyButton.onClick.RemoveListener(Verify);
            resetButton.onClick.RemoveListener(RequestResetAll);
            foreach (StudentDropZone zone in Zones())
                zone.CardPlaced -= HandleCardPlaced;
        }

        /// <summary>Recreates every card as unclassified in the "Sin clasificar" strip.</summary>
        public void Build(IReadOnlyList<StudentData> students)
        {
            ClearCards();

            if (students != null)
            {
                foreach (StudentData student in students)
                {
                    if (student == null) continue;

                    StudentDragItem card = Instantiate(cardPrefab, unassignedZone.CardsContainer);
                    card.Setup(student, dragLayer);
                    cards.Add(card);
                }
            }

            studentCountText.text = cards.Count == 1 ? "1 estudiante" : $"{cards.Count} estudiantes";
            ResetBoardState();
        }

        /// <summary>Moves every card back to "Sin clasificar", in the original order, keeping the same cards.</summary>
        public void ClearClassifications()
        {
            foreach (StudentDragItem card in cards)
                card.PlaceIn(unassignedZone.CardsContainer, StudentClassification.Unclassified);

            ResetBoardState();
        }

        public void Verify()
        {
            var entries = new List<(StudentData, StudentClassification)>(cards.Count);
            foreach (StudentDragItem card in cards)
                entries.Add((card.Student, card.Classification));

            resultBanner.ShowResult(ClassificationValidator.Validate(entries));
        }

        private void ResetBoardState()
        {
            resultBanner.Hide();
            foreach (StudentDropZone zone in Zones())
                zone.ResetScroll();
            UpdateResetButton();
        }

        // A previous result no longer describes the board once a card moves.
        private void HandleCardPlaced(StudentDragItem _)
        {
            resultBanner.Hide();
            UpdateResetButton();
        }

        private void RequestResetAll() => ResetAllRequested?.Invoke();

        // Only offered once every card has left "Sin clasificar".
        private void UpdateResetButton()
        {
            bool allClassified = cards.Count > 0;
            foreach (StudentDragItem card in cards)
            {
                if (card.Classification != StudentClassification.Unclassified) continue;
                allClassified = false;
                break;
            }

            resetButton.gameObject.SetActive(allClassified);
        }

        private void ClearCards()
        {
            foreach (StudentDragItem card in cards)
            {
                if (card == null) continue;

                // Destroy is deferred to the end of the frame; deactivating first removes the card from the layout right away.
                card.gameObject.SetActive(false);
                Destroy(card.gameObject);
            }

            cards.Clear();
        }

        private IEnumerable<StudentDropZone> Zones()
        {
            yield return unassignedZone;
            yield return approvedZone;
            yield return failedZone;
        }
    }
}
