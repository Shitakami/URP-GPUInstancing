using UnityEngine;

public class SampleMeshRenderer : MonoBehaviour
{
    [SerializeField] private int _count;
    [SerializeField] private GameObject _prefab;
    
    void Start()
    {
        var maxPosition = transform.position + transform.localScale / 2;
        var minPosition = transform.position - transform.localScale / 2;
        var transformMatrixArray = TransformMatrixArrayFactory.Create(_count, maxPosition, minPosition);

        var parentObject = new GameObject();
        var parentTransform = parentObject.transform;
        
        for (var i = 0; i < _count; i++)
        {
            var position = transformMatrixArray[i].GetPosition();
            var rotation = transformMatrixArray[i].rotation;
            var scale = transformMatrixArray[i].lossyScale;
            
            var instance = Instantiate(_prefab, position, rotation, parentTransform);
            instance.transform.localScale = scale;
        }
    }
}
