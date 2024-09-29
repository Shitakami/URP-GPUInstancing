using UnityEngine;

namespace AnimationSettings
{
    [CreateAssetMenu(fileName = "AnimationSettings", menuName = "AnimationSettings", order = 0)]
    public class CubeAnimationSettings : ScriptableObject
    {
        [SerializeField] private float _moveVelocity;
        [SerializeField] private float _rotationVelocity;

        public float MoveVelocity => _moveVelocity;
        public float RotationVelocity => _rotationVelocity;
    }
}