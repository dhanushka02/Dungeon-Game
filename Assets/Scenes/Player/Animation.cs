using UnityEngine;
using Test2;
using Unity.Mathematics;
public class Animation : MonoBehaviour
{
    public Animator anim;
    int vertical;
    int horizontal;
    public bool canRotate;
    int isJumping;

    public void Initialize()
    {
        anim = GetComponent<Animator>();
        vertical = Animator.StringToHash("Vertical");
        horizontal = Animator.StringToHash("Horizontal");
        isJumping = Animator.StringToHash("IsJumping");

    }
    public void SetJumping(bool jumping)
    {
        anim.SetBool(isJumping, jumping);
    }
    public void UpdateAnimValue(float VerticalMove,float HorizontalMove)
    {
        #region Vertical
        float v = 0;
        if (VerticalMove > 0 && VerticalMove < 0.55f)
        {
            v = 0.5f;
        }
        else if(VerticalMove > 0.55f)
        {
            v = 1;
        }
        else if (VerticalMove < 0 && VerticalMove > -0.55f)
        {
            v = -0.5f;
        }
        else if (VerticalMove < -0.55f)
        {
            v = -1;
        }
        else v = 0;
        #endregion

        #region Horizontal
        float h = 0;
        if (HorizontalMove > 0 && HorizontalMove < 0.55f)
        {
            h = 0.5f;
        }
        else if(HorizontalMove > 0.55f)
        {
            h = 1;
        }
        else if (HorizontalMove < 0 && HorizontalMove > -0.55f)
        {
            h = -0.5f;
        }
        else if (HorizontalMove < -0.55f)
        {
            h = -1;
        }
        else h = 0;
        #endregion

        anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
        anim.SetFloat(horizontal,h, 0.1f,Time.deltaTime);

    }
    public void CanRotate()
    {
        canRotate = true;
    }
    public void StopRotate()
    {
        canRotate = false;
    }
}
