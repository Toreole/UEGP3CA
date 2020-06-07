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
        protected Image buttonPress;

        float remainingCooldown = 0;
        float cooldown = 0;

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

        IEnumerator HoldShout()
        {
            float timeHeld = 0f;
            float castTime = shout.FullCastTime;
            progCdFill.color = shoutColor;

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
            //TODO: handle the shouting.
            //figure out the strength of the shout.
            int words = 0; //default 0
            for(int i = 1; i < shout.words.Length; i++)
            {
                if (timeHeld < shout.castTime[i])
                    break;
                words = i;
            }

            //TODO: Actually do the shouting part.
            Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * 4, transform.rotation, int.MaxValue);
            Vector3 origin = transform.position;

            float strength = shout.strength[words];
            foreach(var hit in hits)
            {
                var rb = hit.GetComponent<Rigidbody>();
                if(rb)
                {
                    var dir = hit.transform.position - origin;
                    dir.Normalize();
                    rb.AddForce(dir * strength, ForceMode.Impulse);
                }
            }
            //set the cooldown.
            remainingCooldown = (cooldown = shout.cooldown[words]);
            progCdFill.color = cooldownColor;
        }

        void UpdateWordText(float timeHeld)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{shout.words[0]} ");

            float cTime = 0f;
            for(int i = 1; i < shout.words.Length; i++)
            {
                cTime += shout.castTime[i];
                bool valid = timeHeld >= cTime;
                Debug.Log($"{i} valid: {valid}");
                //change the color via the rich text tags.
                builder.Append($"<color=#{(valid? activeHexCol : inactiveHexCol)}>{shout.words[i]}</color> ");
            }
            wordsText.text = builder.ToString();
        }
    }
}