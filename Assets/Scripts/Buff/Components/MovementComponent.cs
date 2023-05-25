using UnityEngine;
using NaughtyAttributes;
public class MovementComponent : MonoBehaviour
{
    public float MovementSpeed { get; set; } = 1f;

#if UNITY_EDITOR
    [SerializeField][ReadOnly]
    private float speed;

    public void Update()
    {
        speed = MovementSpeed ;
    }
#endif

}
