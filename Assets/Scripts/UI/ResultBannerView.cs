using System.Collections.Generic;
using System.Linq;
using GradeReview.Data;
using GradeReview.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GradeReview.UI
{
    /// <summary>
    /// Success / error banner. Turns a <see cref="ValidationResult"/> into a readable message.
    /// </summary>
    public class ResultBannerView : MonoBehaviour
    {
        // Keeps long lists inside the fixed-height banner.
        private const int MaxNamesListed = 4;

        [SerializeField] private Image background;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Sprite successSprite;
        [SerializeField] private Sprite errorSprite;
        [SerializeField] private Color successTextColor = new Color32(0x1E, 0x6B, 0x3A, 0xFF);
        [SerializeField] private Color errorTextColor = new Color32(0x9B, 0x2C, 0x2C, 0xFF);

        public void ShowResult(ValidationResult result)
        {
            if (result.Total == 0)
            {
                ShowError("No hay estudiantes para validar.");
                return;
            }

            if (result.IsAllCorrect)
            {
                ShowSuccess($"¡Todo correcto! {result.CorrectCount} de {result.Total} estudiantes clasificados correctamente.");
                return;
            }

            var sentences = new List<string>();
            if (result.Incorrect.Count > 0)
            {
                string noun = result.Incorrect.Count == 1 ? "clasificación incorrecta" : "clasificaciones incorrectas";
                sentences.Add($"Hay {result.Incorrect.Count} {noun}: {JoinNames(result.Incorrect)}.");
            }
            if (result.Unclassified.Count > 0)
            {
                string noun = result.Unclassified.Count == 1 ? "Falta 1 estudiante" : $"Faltan {result.Unclassified.Count} estudiantes";
                sentences.Add($"{noun} por clasificar: {JoinNames(result.Unclassified)}.");
            }
            ShowError(string.Join(" ", sentences));
        }

        public void ShowSuccess(string message) => Show(message, successSprite, successTextColor);

        public void ShowError(string message) => Show(message, errorSprite, errorTextColor);

        public void Hide() => gameObject.SetActive(false);

        private void Show(string message, Sprite sprite, Color textColor)
        {
            background.sprite = sprite;
            messageText.text = message;
            messageText.color = textColor;
            gameObject.SetActive(true);
        }

        private static string JoinNames(IReadOnlyList<StudentData> students)
        {
            List<string> names = students.Take(MaxNamesListed).Select(DisplayName).ToList();
            int remaining = students.Count - names.Count;
            if (remaining > 0)
                return $"{string.Join(", ", names)} y {remaining} más";

            return names.Count == 1 ? names[0] : $"{string.Join(", ", names.Take(names.Count - 1))} y {names[names.Count - 1]}";
        }

        private static string DisplayName(StudentData student)
        {
            if (!string.IsNullOrEmpty(student.FullName)) return student.FullName;
            return string.IsNullOrEmpty(student.Code) ? "Estudiante sin nombre" : $"Código {student.Code}";
        }
    }
}
