using System;
using System.Collections.Generic;
using UnityEngine;

namespace GradeReview.Data
{
    /// <summary>
    /// Root object of estudiantes.json: { "estudiantes": [ ... ] }.
    /// JsonUtility cannot deserialize a top-level array, so the list needs this wrapper.
    /// </summary>
    [Serializable]
    public class StudentCollection
    {
        [SerializeField] private List<StudentData> estudiantes = new List<StudentData>();

        /// <summary>Raw deserialized list. May contain null entries or be null if the JSON says so.</summary>
        public IReadOnlyList<StudentData> Students => estudiantes;
    }
}
