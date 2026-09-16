using System.Globalization;
using GradeReview.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GradeReview.DragDrop
{
    /// <summary>
    /// Draggable student card. Owns its current classification; drop zones only move it and assign a new one.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class StudentDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const string EmptyValue = "—";

        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text gradeText;
        [SerializeField] private Image background;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite draggingSprite;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private LayoutElement layoutElement;

        private RectTransform dragLayer;
        private Transform originalParent;
        private int originalSiblingIndex;
        private bool isDragging;
        private bool placedByDropZone;

        public StudentData Student { get; private set; }
        public StudentClassification Classification { get; private set; } = StudentClassification.Unclassified;

        private RectTransform RectTransform => (RectTransform)transform;

        /// <param name="dragLayer">Unmasked, layout-free container the card lives in while it is dragged.</param>
        public void Setup(StudentData student, RectTransform dragLayer)
        {
            Student = student;
            this.dragLayer = dragLayer;
            Classification = StudentClassification.Unclassified;

            nameText.text = string.IsNullOrEmpty(student.FullName) ? EmptyValue : student.FullName;
            gradeText.text = $"Nota: {student.FinalGrade.ToString("0.0#", CultureInfo.InvariantCulture)}";
            background.sprite = normalSprite;
        }

        /// <summary>Called by a drop zone: moves the card into the zone's layout and records the new classification.</summary>
        public void PlaceIn(RectTransform container, StudentClassification classification)
        {
            transform.SetParent(container, false);
            transform.SetAsLastSibling();
            Classification = classification;
            placedByDropZone = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || dragLayer == null) return;

            isDragging = true;
            placedByDropZone = false;
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            // Leaving the layout group and the scroll mask keeps the card on top and fully visible.
            transform.SetParent(dragLayer, false);
            transform.SetAsLastSibling();
            RectTransform.anchorMin = RectTransform.anchorMax = RectTransform.pivot = new Vector2(0.5f, 0.5f);
            RectTransform.sizeDelta = new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight);

            // Lets the raycast reach the drop zone under the pointer.
            canvasGroup.blocksRaycasts = false;
            background.sprite = draggingSprite;
            FollowPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging) FollowPointer(eventData);
        }

        // Unity calls OnDrop on the target before OnEndDrag, so placedByDropZone is already known here.
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;
            canvasGroup.blocksRaycasts = true;
            background.sprite = normalSprite;

            if (!placedByDropZone) ReturnToOrigin();
        }

        private void FollowPointer(PointerEventData eventData)
        {
            // Converting through the drag layer accounts for the CanvasScaler and the canvas camera.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                RectTransform.localPosition = localPoint;
        }

        private void ReturnToOrigin()
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
}
