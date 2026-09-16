using System;
using GradeReview.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GradeReview.DragDrop
{
    /// <summary>
    /// Area that accepts student cards and assigns them its classification.
    /// It decides where a card goes, never whether the classification is correct.
    /// </summary>
    public class StudentDropZone : MonoBehaviour, IDropHandler
    {
        [SerializeField] private StudentClassification classification;
        [SerializeField] private RectTransform cardsContainer;
        [SerializeField] private ScrollRect scrollRect;

        /// <summary>Raised after a card has been placed in this zone.</summary>
        public event Action<StudentDragItem> CardPlaced;

        public RectTransform CardsContainer => cardsContainer;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null || !eventData.pointerDrag.TryGetComponent(out StudentDragItem card)) return;

            card.PlaceIn(cardsContainer, classification);
            ScrollToEnd();
            CardPlaced?.Invoke(card);
        }

        public void ResetScroll()
        {
            scrollRect.StopMovement();
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
        }

        // New cards are appended, so show the end of the list where the card landed.
        private void ScrollToEnd()
        {
            Canvas.ForceUpdateCanvases();
            if (scrollRect.vertical)
                scrollRect.verticalNormalizedPosition = 0f;
            else
                scrollRect.horizontalNormalizedPosition = 1f;
        }
    }
}
