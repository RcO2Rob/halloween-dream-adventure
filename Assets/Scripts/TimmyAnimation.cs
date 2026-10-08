using UnityEngine;

namespace LostDream
{
    public enum JumpAnimationStage { None, Takeoff, Rising, Falling, Landing }
    // Humanoid locomotion and masked upper-body actions leave movement to PlayerMotor.
    public class TimmyAnimation : MonoBehaviour
    {
        public Animator animator;
        public Transform bow;
        public PlayerMotor player;
        public Quaternion chestBindRotation = Quaternion.identity;
        public Quaternion headBindRotation = Quaternion.identity;
        public Transform LeftHand => animator.GetBoneTransform(HumanBodyBones.LeftHand);
        public Transform RightHand => animator.GetBoneTransform(HumanBodyBones.RightHand);
        float aimingUntil;
        bool aiming;
        float bowPoseWeight;
        Vector3 shotTarget;
        float jumpStartedAt = -100, landingUntil, jumpWeight, throwingUntil;
        public JumpAnimationStage JumpStage => player == null ? JumpAnimationStage.None :
            !player.IsGrounded ? (player.IsJumping && Time.time - jumpStartedAt < .12f ? JumpAnimationStage.Takeoff :
                player.VerticalSpeed > 0 ? JumpAnimationStage.Rising : JumpAnimationStage.Falling) :
            Time.time < landingUntil ? JumpAnimationStage.Landing : JumpAnimationStage.None;
        public Vector3 BowGripPosition => LeftHand.position + transform.forward * .06f;

        public void Move(float speed) { animator.SetFloat("Speed", player != null && !player.IsGrounded ? 0 : speed, .08f, Time.deltaTime); }
        public void BeginJump() { jumpStartedAt = Time.time; landingUntil = 0; }
        public void Land() { landingUntil = Time.time + .22f; }
        public void ResetJumpPose() { jumpStartedAt = -100; landingUntil = 0; jumpWeight = 0; }
        public void Shoot(Vector3 target)
        {
            if (!aiming) animator.CrossFadeInFixedTime("BowAim", .04f, 1, 0);
            aiming = true; aimingUntil = Time.time + .4f;
            shotTarget = target;
            // Place the grip before spawning the arrow, including the first shot from idle.
            bowPoseWeight = 1;
            ApplyBowPose(1);
        }
        public void Throw()
        {
            aiming = false;
            bowPoseWeight = 0;
            throwingUntil = Time.time + .55f;
            animator.CrossFadeInFixedTime("Throw", .03f, 1, .12f);
        }
        void Update()
        {
            if (aiming && Time.time > aimingUntil)
            {
                aiming = false;
                animator.CrossFadeInFixedTime("Relax", .12f, 1);
            }
        }
        void LateUpdate()
        {
            if (player == null) return;
            if (DreamGame.Instance != null && DreamGame.Instance.Phase != GamePhase.Menu) ApplyJumpPose();
            bowPoseWeight = Mathf.MoveTowards(bowPoseWeight, aiming ? 1 : 0, Time.deltaTime / .12f);
            if (bowPoseWeight > 0) ApplyBowPose(bowPoseWeight);
            if (player.CarryingCandy && !aiming)
            {
                // Keep the right palm forward and clear of the jacket while carrying.
                var shoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                var elbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                Vector3 target = transform.TransformPoint(new Vector3(.32f, 1.1f, .42f));
                for (int i = 0; i < 3; i++)
                {
                    elbow.rotation = Quaternion.FromToRotation(RightHand.position - elbow.position, target - elbow.position) * elbow.rotation;
                    shoulder.rotation = Quaternion.FromToRotation(RightHand.position - shoulder.position, target - shoulder.position) * shoulder.rotation;
                }
            }
            // Maintain readable prop orientation without inheriting the rig's wrist axes or scale.
            if (bow != null)
            {
                bow.position = BowGripPosition;
                bow.rotation = transform.rotation;
                var stringLine = bow.GetChild(0).GetComponent<LineRenderer>();
                stringLine.SetPosition(1, Vector3.Lerp(Vector3.zero,
                    bow.InverseTransformPoint(RightHand.position), bowPoseWeight));
            }
            if (player.heldCandy != null)
            {
                player.heldCandy.transform.position = RightHand.position + transform.forward * .1f;
                player.heldCandy.transform.rotation = transform.rotation;
            }
        }

        void ApplyJumpPose()
        {
            bool airborne = !player.IsGrounded;
            float landing = Mathf.Clamp01((landingUntil - Time.time) / .22f);
            jumpWeight = Mathf.MoveTowards(jumpWeight, airborne || landing > 0 ? 1 : 0, Time.deltaTime * 14);
            if (jumpWeight <= 0) return;
            float takeoff = airborne ? Mathf.Clamp01(1 - (Time.time - jumpStartedAt) / .12f) : 0;
            float tuck = airborne ? (player.VerticalSpeed > 0 ? .32f : .20f) : 0;
            float crouch = airborne ? .07f + takeoff * .1f : Mathf.Sin(landing * Mathf.PI) * .14f;
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            hips.position -= Vector3.up * (crouch * jumpWeight);
            var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            spine.rotation = Quaternion.AngleAxis((airborne ? 8 : landing * 6) * jumpWeight, transform.right) * spine.rotation;
            SolveArm(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
                animator.GetBoneTransform(HumanBodyBones.LeftFoot),
                transform.TransformPoint(new Vector3(-.15f, .2f + tuck, .10f)),
                transform.TransformPoint(new Vector3(-.15f, .65f, .55f)), jumpWeight);
            SolveArm(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
                animator.GetBoneTransform(HumanBodyBones.RightFoot),
                transform.TransformPoint(new Vector3(.15f, .2f + tuck * .9f, -.04f)),
                transform.TransformPoint(new Vector3(.15f, .65f, .5f)), jumpWeight);
            // Upper-body bow/throw actions take precedence; the legs keep their jump pose.
            if (bowPoseWeight < .1f && Time.time > throwingUntil)
            {
                SolveArm(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, LeftHand,
                    transform.TransformPoint(new Vector3(-.4f, 1.18f, .15f)),
                    transform.TransformPoint(new Vector3(-.5f, .9f, -.1f)), jumpWeight);
                SolveArm(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, RightHand,
                    transform.TransformPoint(new Vector3(.4f, 1.22f, .12f)),
                    transform.TransformPoint(new Vector3(.5f, .9f, -.1f)), jumpWeight);
            }
        }

        void ApplyBowPose(float weight)
        {
            Vector3 origin = player.transform.position + Vector3.up * 1.3f;
            Vector3 direction = (shotTarget - origin).normalized;
            if (direction.sqrMagnitude < .1f) direction = transform.forward;
            var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            // The source clip faces across its left shoulder. Square the chest with
            // the gameplay heading before solving the hands in world space.
            chest.rotation = Quaternion.Slerp(chest.rotation, transform.rotation * chestBindRotation, weight);
            head.rotation = Quaternion.Slerp(head.rotation,
                Quaternion.LookRotation(direction, Vector3.up) * headBindRotation, weight);
            Vector3 leftTarget = origin + direction * .62f - transform.right * .035f;
            Vector3 rightTarget = origin - direction * .02f + transform.right * .25f + Vector3.up * .12f;
            SolveArm(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, LeftHand,
                leftTarget, origin - transform.right * .35f - Vector3.up * .12f + direction * .28f, weight);
            SolveArm(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, RightHand,
                rightTarget, origin + transform.right * .6f - direction * .28f, weight);
        }

        void SolveArm(HumanBodyBones upperBone, HumanBodyBones lowerBone, Transform hand,
            Vector3 target, Vector3 elbowHint, float weight)
        {
            Transform upper = animator.GetBoneTransform(upperBone);
            Transform lower = animator.GetBoneTransform(lowerBone);
            Quaternion upperBefore = upper.localRotation, lowerBefore = lower.localRotation;
            Vector3 shoulder = upper.position;
            float upperLength = Vector3.Distance(shoulder, lower.position);
            float lowerLength = Vector3.Distance(lower.position, hand.position);
            Vector3 direction = (target - shoulder).normalized;
            float reach = Mathf.Clamp(Vector3.Distance(shoulder, target),
                Mathf.Abs(upperLength - lowerLength) + .001f, upperLength + lowerLength - .003f);
            Vector3 reachable = shoulder + direction * reach;
            float along = (upperLength * upperLength + reach * reach - lowerLength * lowerLength) / (2 * reach);
            float bend = Mathf.Sqrt(Mathf.Max(0, upperLength * upperLength - along * along));
            Vector3 bendDirection = Vector3.ProjectOnPlane(elbowHint - shoulder, direction).normalized;
            if (bendDirection.sqrMagnitude < .1f) bendDirection = Vector3.ProjectOnPlane(transform.right, direction).normalized;
            Vector3 elbow = shoulder + direction * along + bendDirection * bend;
            upper.rotation = Quaternion.FromToRotation(lower.position - shoulder, elbow - shoulder) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(hand.position - lower.position, reachable - lower.position) * lower.rotation;
            Quaternion upperSolved = upper.localRotation, lowerSolved = lower.localRotation;
            upper.localRotation = Quaternion.Slerp(upperBefore, upperSolved, weight);
            lower.localRotation = Quaternion.Slerp(lowerBefore, lowerSolved, weight);
        }
    }
}
