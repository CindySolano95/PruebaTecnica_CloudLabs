using System;
using UnityEngine;

namespace GradeReview.Data
{
    /// <summary>
    /// A single student as defined in estudiantes.json.
    /// Serialized field names must match the JSON keys exactly for JsonUtility.
    /// </summary>
    [Serializable]
    public class StudentData
    {
        public const float PassingGrade = 3f;

        [SerializeField] private string nombre;
        [SerializeField] private string apellido;
        [SerializeField] private string codigo;
        [SerializeField] private string correo;
        // NaN survives deserialization when the key is missing, so a missing grade is not mistaken for 0.0.
        [SerializeField] private float notaFinal = float.NaN;

        // Required by JsonUtility.
        public StudentData() { }

        public StudentData(string firstName, string lastName, string code, string email, float finalGrade)
        {
            nombre = firstName;
            apellido = lastName;
            codigo = code;
            correo = email;
            notaFinal = finalGrade;
        }

        public string FirstName => nombre;
        public string LastName => apellido;
        public string Code => codigo;
        public string Email => correo;
        public float FinalGrade => notaFinal;

        public string FullName => $"{nombre} {apellido}".Trim();

        /// <summary>Single source of truth for the approval rule.</summary>
        public bool IsApproved => notaFinal >= PassingGrade;
    }
}
