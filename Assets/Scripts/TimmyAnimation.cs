using UnityEngine;

namespace LostDream
{
    public enum JumpAnimationStage { None, Takeoff, Rising, Falling, Landing }
    // Humanoid locomotion and masked upper-body actions leave movement to PlayerMotor.
    [DefaultExecutionOrder(0)]
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
        public bool IsAiming => aiming;

        public void Move(float speed) { animator.SetFloat("Speed", player != null && !player.IsGrounded ? 0 : speed, .08f, Time.deltaTime); }
        public void BeginJump() { jumpStartedAt = Time.time; landingUntil = 0; }
        public void Land() { landingUntil = Time.time + .22f; }
        public void ResetJumpPose() { jumpStartedAt = -100; landingUntil = 0; jumpWeight = 0; }
        public void BeginVictory()
        {
            aiming = false; aimingUntil = 0; bowPoseWeight = 0; throwingUntil = 0;
            ResetJumpPose();
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetFloat("Speed", 0);
            animator.CrossFadeInFixedTime("Locomotion", .12f, 0);
            animator.CrossFadeInFixedTime("Relax", .12f, 1);
        }
        public void Shoot(Vector3 target)
        {
            if (!aiming) animator.CrossFadeInFixedTime("BowAim", .04f, 1, 0);
            aiming = true; aimingUntil = Time.time + .4f;
            shotTarget = target;
            // Place the grip before spawning the arrow, including the first shot from idle.
            bowPoseWeight = 1;
            ApplyBowPose(1);
            RefreshProps();
        }
        public void TrackAim(Vector3 target) { if (aiming) { shotTarget = target; ApplyBowPose(bowPoseWeight); RefreshProps(); } }
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
            if (DreamGame.Instance != null && DreamGame.Instance.Phase != GamePhase.Menu &&
                DreamGame.Instance.Phase != GamePhase.Victory) ApplyJumpPose();
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
            RefreshProps();
        }
        void RefreshProps()
        {
            // The bow and newly spawned arrow share the grip and aiming pitch.
            if (bow != null)
            {
                bow.position = BowGripPosition;
                Vector3 direction = shotTarget - BowGripPosition;
                bow.rotation = direction.sqrMagnitude > .001f ? Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction, Vector3.up), bowPoseWeight) : transform.rotation;
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
            // Sweep low, bent arms through takeoff, flight and landing rather than
            // holding both hands beside the head. Upper-body actions blend over this.
            float armWeight = jumpWeight * (1 - Mathf.SmoothStep(0, 1, bowPoseWeight)) *
                Mathf.Clamp01((Time.time - throwingUntil) / .12f);
            if (armWeight > 0)
            {
                float falling = airborne ? Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.5f, -4, player.VerticalSpeed)) : 0;
                ApplyJumpArm(true, airborne, falling, takeoff, armWeight);
                RelaxJumpWrist(true, armWeight);
                // The carry solver owns the right arm, including while airborne.
                if (!player.CarryingCandy)
                {
                    ApplyJumpArm(false, airborne, falling, takeoff, armWeight);
                    RelaxJumpWrist(false, armWeight);
                }
            }
        }

        void ApplyJumpArm(bool left, bool airborne, float falling, float takeoff, float weight)
        {
            var upperBone = left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm;
            var lowerBone = left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm;
            Transform shoulder = animator.GetBoneTransform(upperBone), elbow = animator.GetBoneTransform(lowerBone);
            Transform hand = left ? LeftHand : RightHand;
            float reach = Vector3.Distance(shoulder.position, elbow.position) + Vector3.Distance(elbow.position, hand.position);
            Vector3 outward = transform.right * (left ? -1 : 1);
            // Timmy has a large head and short torso. Use his actual shoulder and
            // arm length instead of a fixed height that raises hands to his neck.
            float drop = airborne ? Mathf.Lerp(.80f, .76f, falling) : .66f;
            float spread = airborne ? Mathf.Lerp(.16f, .25f, falling) : .14f;
            float forward = airborne ? Mathf.Lerp(.10f, .14f, falling) - takeoff * .18f : .20f;
            Vector3 target = shoulder.position + reach * (outward * spread - Vector3.up * drop + transform.forward * forward);
            Vector3 hint = shoulder.position + reach * (outward * .38f - Vector3.up * .48f - transform.forward * .16f);
            SolveArm(upperBone, lowerBone, hand, target, hint, weight);
        }

        void RelaxJumpWrist(bool left, float weight)
        {
            Transform hand = left ? LeftHand : RightHand;
            var forearm = animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);
            var middle = animator.GetBoneTransform(left ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);
            var index = animator.GetBoneTransform(left ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal);
            var little = animator.GetBoneTransform(left ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal);
            if (middle == null || index == null || little == null) return;
            // Derive the axes from this rig's knuckles, avoiding Mixamo's mirrored
            // wrist axes. Fingers follow the forearm and both thumbs face forward.
            Vector3 fingers = middle.position - hand.position;
            Vector3 across = index.position - little.position;
            Vector3 relaxed = (hand.position - forearm.position).normalized;
            Vector3 thumbSide = Vector3.ProjectOnPlane(transform.forward, relaxed);
            if (fingers.sqrMagnitude < .000001f || across.sqrMagnitude < .000001f || thumbSide.sqrMagnitude < .01f) return;
            Quaternion from = Quaternion.LookRotation(fingers, across);
            Quaternion to = Quaternion.LookRotation(relaxed, thumbSide);
            Quaternion target = to * Quaternion.Inverse(from) * hand.rotation;
            hand.rotation = Quaternion.Slerp(hand.rotation, target, weight);
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
