using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

namespace UEGP3CA.Shouts
{
    public class ShoutBehaviour : MonoBehaviour
    {
        [Header("Shout")]
        [SerializeField]
        protected ShoutData shout;
        [SerializeField]
        protected string shoutButton;
        [SerializeField]
        protected Vector3 boxOffset, boxHalfSize;
        [SerializeField]
        protected Animator anim;

        [Header("UI")]
        [SerializeField]
        protected Slider progCooldown;
        [SerializeField]
        protected Image progCdFill;
        [SerializeField]
        protected Color shoutColor, cooldownColor;
        [SerializeField]
        protected Text wordsText;
        [SerializeField]
        protected string inactiveHexCol, activeHexCol;
        [SerializeField]
        protected float fadeOutTime = 2f;
        [SerializeField]
        protected Image buttonPress;

        readonly int shoutTrigger = Animator.StringToHash("Yeet");

        float remainingCooldown = 0;
        float cooldown = 0;

        private void Start()
        {
            wordsText.text = "";
        }

        private void Update()
        {
            if(Input.GetButtonDown(shoutButton) && remainingCooldown <= 0)
            {
                StartCoroutine(HoldShout());
            } 
            else if (remainingCooldown > 0)
            {
                remainingCooldown -= Time.deltaTime;
                progCooldown.value = remainingCooldown / cooldown;
            }
        }

        //easy editor visualization.
        private void OnDrawGizmos()
        {
            var tempMatrix = transform.localToWorldMatrix;
            var worldMatrix = Gizmos.matrix;
            Gizmos.matrix = tempMatrix;

            Gizmos.color = new Color(0.3f, 0.2f, 0.8f, 0.5f);
            Gizmos.DrawCube(boxOffset, boxHalfSize * 2);

            Gizmos.matrix = worldMatrix;
        }

        IEnumerator HoldShout()
        {
            //setup some stuff.
            float timeHeld = 0f;
            float castTime = shout.FullCastTime;
            progCdFill.color = shoutColor;

            //correct colors.
            StartFadeAlpha(buttonPress, 0.5f, 0.05f);
            wordsText.CrossFadeAlpha(1, 0.01f, true);

            //as long as the button is held, update some UI and wait
            while (Input.GetButton(shoutButton))
            {
                timeHeld += Time.deltaTime;
                //normalized progress 0-1
                float progress = timeHeld / castTime;
                progCooldown.value = progress;
                UpdateWordText(timeHeld);

                if (timeHeld >= castTime)
                    break;

                yield return null;
            }

            DoShoutEffects(timeHeld);
            //reset stuff.
            StartFadeAlpha(buttonPress, 0.0f, 0.05f);
            progCdFill.color = cooldownColor;
            wordsText.CrossFadeAlpha(0, fadeOutTime, true);
        }

        //literally just a small helper because im lazy
        void StartFadeAlpha(Image img, float alpha, float t) => StartCoroutine(FadeAlpha(img, alpha, t));
        IEnumerator FadeAlpha(Image img, float targetAlpha, float time)
        {
            Color col = img.color;
            float startAlpha = col.a;
            for(float t = 0; t < time; t+= Time.deltaTime)
            {
                col.a = Mathf.Lerp(startAlpha, targetAlpha, t / time);
                img.color = col;
                yield return null;
            }
        }

        /// <summary>
        /// Update the word word word text underneat the progress/cd slider.
        /// Corrects for active / inactive word (color tag)
        /// </summary>
        /// <param name="timeHeld"></param>
        void UpdateWordText(float timeHeld)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{shout[0].textWord} ");
            ShoutData.ShoutWord current = default;

            for(int i = 1; i < shout.Size; i++)
            {
                current = shout[i];
                bool valid = timeHeld >= current.castTime; 

                //change the color via the rich text tags.
                builder.Append($"<color=#{(valid? activeHexCol : inactiveHexCol)}>{current.textWord}</color> ");
            }
            wordsText.text = builder.ToString();
        }

        /// <summary>
        /// Apply the effects of the shout to objects in the scene.
        /// </summary>
        void DoShoutEffects(float timeHeld)
        {
            //figure out the strength of the shout.
            int words = 0; //default 0
            for (int i = 1; i < shout.Size; i++)
            {
                if (timeHeld < shout[i].castTime)
                    break;
                words = i;
            }

            //get all colliders.
            Collider[] hits = Physics.OverlapBox(transform.position + transform.TransformVector(boxOffset), boxHalfSize, transform.rotation, int.MaxValue);
            Vector3 origin = transform.position;
            anim.SetTrigger(shoutTrigger);

            ShoutData.ShoutWord finalShout = shout[words];
            float strength = finalShout.strength;
            foreach (var hit in hits)
            {
                var rb = hit.GetComponent<Rigidbody>();
                if (rb)
                {
                    var dir = hit.transform.position - origin;
                    dir.Normalize();
                    rb.AddForce(dir * strength, ForceMode.Impulse);
                }
            }
            //set the cooldown.
            remainingCooldown = (cooldown = finalShout.cooldown);
        }
    }
}