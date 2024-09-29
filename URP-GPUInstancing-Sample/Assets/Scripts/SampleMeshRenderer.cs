using UnityEngine;

public class SampleMeshRenderer : MonoBehaviour
{
    [SerializeField] private int _count;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Bounds _bounds;
    
    void Start()
    {
        var maxPosition = transform.position + _bounds.size / 2;
        var minPosition = transform.position - _bounds.size / 2;
        var matrix4x4Array = CreateMatrix4x4.CreateMatrix4x4Array(_count, maxPosition, minPosition);
        
        for (var i = 0; i < _count; i++)
        {
            var position = matrix4x4Array[i].GetPosition();
            var rotation = matrix4x4Array[i].rotation;
            var scale = matrix4x4Array[i].lossyScale;
            
            var instance = Instantiate(_prefab, position, rotation, transform);
            instance.transform.localScale = scale;
        }
    }
}
