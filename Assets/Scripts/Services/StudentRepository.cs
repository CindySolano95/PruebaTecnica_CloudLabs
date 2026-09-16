using System;
using System.Collections.Generic;
using System.IO;
using GradeReview.Data;
using UnityEngine;

namespace GradeReview.Services
{
    /// <summary>
    /// Loads students from StreamingAssets and returns only valid entries.
    /// Never throws: every failure is reported through the TryLoad result.
    /// </summary>
    public class StudentRepository
    {
        public const string DefaultFileName = "estudiantes.json";

        private const float MinGrade = 0f;
        private const float MaxGrade = 5f;
        private const string LogPrefix = "[StudentRepository]";

        private readonly string fileName;

        public StudentRepository(string fileName = DefaultFileName)
        {
            this.fileName = fileName;
        }

        public string FilePath => Path.Combine(Application.streamingAssetsPath, fileName);

        /// <param name="students">Valid students, or an empty list when loading fails.</param>
        /// <param name="error">User-facing error message, or null on success.</param>
        public bool TryLoad(out IReadOnlyList<StudentData> students, out string error)
        {
            students = Array.Empty<StudentData>();

            if (!TryReadCollection(out StudentCollection collection, out error))
            {
                Debug.LogError($"{LogPrefix} {error} Path: {FilePath}");
                return false;
            }

            List<StudentData> validStudents = SanitizeStudents(collection.Students);
            if (validStudents.Count == 0)
            {
                error = "El archivo no contiene estudiantes válidos.";
                Debug.LogWarning($"{LogPrefix} {error} Path: {FilePath}");
                return false;
            }

            students = validStudents;
            return true;
        }

        private bool TryReadCollection(out StudentCollection collection, out string error)
        {
            collection = null;

            if (!File.Exists(FilePath))
            {
                error = $"No se encontró el archivo {fileName}.";
                return false;
            }

            string json;
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = $"No se pudo leer el archivo {fileName}.";
                Debug.LogException(exception);
                return false;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                error = $"El archivo {fileName} está vacío.";
                return false;
            }

            try
            {
                collection = JsonUtility.FromJson<StudentCollection>(json);
            }
            catch (ArgumentException exception)
            {
                error = $"El archivo {fileName} no tiene un formato JSON válido.";
                Debug.LogWarning($"{LogPrefix} JSON parse error: {exception.Message}");
                return false;
            }

            if (collection == null || collection.Students == null)
            {
                error = $"El archivo {fileName} no contiene la lista \"estudiantes\".";
                return false;
            }

            error = null;
            return true;
        }

        private static List<StudentData> SanitizeStudents(IReadOnlyList<StudentData> rawStudents)
        {
            var validStudents = new List<StudentData>(rawStudents.Count);

            for (int i = 0; i < rawStudents.Count; i++)
            {
                if (TrySanitize(rawStudents[i], i, out StudentData student))
                    validStudents.Add(student);
            }

            return validStudents;
        }

        private static bool TrySanitize(StudentData raw, int index, out StudentData student)
        {
            student = null;

            if (raw == null)
            {
                Debug.LogWarning($"{LogPrefix} Entry #{index} is null. Skipped.");
                return false;
            }

            string firstName = Clean(raw.FirstName);
            string lastName = Clean(raw.LastName);
            string code = Clean(raw.Code);
            string email = Clean(raw.Email);

            // JsonUtility turns a null array element into an empty object, so this also catches that case.
            if (firstName.Length == 0 && lastName.Length == 0 && code.Length == 0)
            {
                Debug.LogWarning($"{LogPrefix} Entry #{index} has no name, last name or code. Skipped.");
                return false;
            }

            string label = code.Length > 0 ? $"'{code}'" : $"#{index}";

            if (float.IsNaN(raw.FinalGrade))
            {
                Debug.LogWarning($"{LogPrefix} Student {label} is missing notaFinal. Skipped.");
                return false;
            }

            if (!IsGradeInRange(raw.FinalGrade))
            {
                Debug.LogWarning($"{LogPrefix} Student {label} has an out-of-range notaFinal ({raw.FinalGrade}). " +
                                 $"Expected {MinGrade}-{MaxGrade}. Skipped.");
                return false;
            }

            if (firstName.Length == 0 || lastName.Length == 0 || code.Length == 0 || email.Length == 0)
                Debug.LogWarning($"{LogPrefix} Student {label} has incomplete text fields. Loaded anyway.");

            student = new StudentData(firstName, lastName, code, email, raw.FinalGrade);
            return true;
        }

        private static bool IsGradeInRange(float grade) => grade >= MinGrade && grade <= MaxGrade;

        private static string Clean(string value) => value?.Trim() ?? string.Empty;
    }
}
