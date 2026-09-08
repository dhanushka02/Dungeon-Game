using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine; 

namespace SG
{
    public class FakeWall:MonoBehaviour
    {
        public bool ishit;
        public Material FWall;
        public float alpha;
        public float fadeTimer = 2.5f;
        public BoxCollider wallColider;

        public AudioSource audioSource;
        public AudioClip FWS;
        private void Update()
        {
            if(ishit)
            {
                Fading();
            }
        }
        private void Trigger(Collider other)
        {
            if (other.tag == "Character_Main") //Set This one as the player who control - Change after Merge
            {
                ishit = true;
            }
        }

        public void Fading()
        {
            alpha =  FWall.color.a;
            alpha = alpha - Time.deltaTime /fadeTimer;
            Color faded = new Color(1,1,1, alpha);
            FWall.color = faded;

            if (wallColider.enabled)
            {
                wallColider.enabled = false;
                audioSource.PlayOneShot(FWS);
            }

            if (alpha <= 0)
            {
                Destroy(this);
            }
        }
    }
}////DEMO SAMPLE
////Contribute Link https://www.youtube.com/watch?v=37LJow8pJFg&list=PLD_vBJjpCwJtrHIW1SS5_BNRk6KZJZ7_d&index=63