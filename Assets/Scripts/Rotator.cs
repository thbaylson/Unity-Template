using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template
{
    [MovedFrom(true, null, "Assembly-CSharp", "Rotator")]
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 0f, 0f);

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
