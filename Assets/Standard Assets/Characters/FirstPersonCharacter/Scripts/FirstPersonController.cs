using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityEngine.UI;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
		[SerializeField] private float m_StepInterval;
		[SerializeField] private AudioMixer m_Mixer;
		[SerializeField] private AudioSource m_JumpAudioSource;
		[SerializeField] private AudioSource m_WalkAudioSource;
		[SerializeField] private AudioSource m_DashAudioSource;
		[SerializeField] private AudioSource m_LandAudioSource;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
		[SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.
		[SerializeField] private AudioClip m_AirjumpSound;
		[SerializeField] private AudioClip m_AirdashSound;
		[SerializeField] private AudioMixerGroup m_JumpGroup;
		[SerializeField] private AudioMixerGroup m_WalkGroup;
		[SerializeField] private AudioMixerGroup m_DashGroup;
		[SerializeField] private AudioMixerGroup m_LandGroup;
        [SerializeField] private Image m_JumpCircle_TR;
        [SerializeField] private Image m_JumpCircle_BR;
        [SerializeField] private Image m_JumpCircle_TL;
        [SerializeField] private Image m_JumpCircle_BL;

        private Camera m_Camera;
        private bool m_Jump;
		private int m_Charges;
		private float m_CurrentSpeed;
		private float m_MaxInfluence;
		private float m_AirdashMulti;
		private float m_AirdashTime;
		private bool m_Dashing;
		private bool m_Dash;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
		private bool m_Jumping;

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
			m_Charges = 2;
			m_MaxInfluence = 10f;
			m_AirdashMulti = 2f;
			m_AirdashTime = 0f;
			m_Dashing = false;
			m_Dash = false;
			m_MouseLook.Init(transform , m_Camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

			// Dash state
			if (!m_Dash)
			{
				m_Dash = CrossPlatformInputManager.GetButtonDown("Dash");

				if (m_CharacterController.isGrounded || m_Dashing || m_Charges == 0)
				{
					m_Dash = false;
				}
			}

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
				m_Charges = 2;
                m_JumpCircle_TL.GetComponent<UICircle>().setCharged(true);
                m_JumpCircle_TR.GetComponent<UICircle>().setCharged(true);
                m_JumpCircle_BL.GetComponent<UICircle>().setCharged(true);
                m_JumpCircle_BR.GetComponent<UICircle>().setCharged(true);
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        private void FixedUpdate()
        {
            GetInput();
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			if (m_CharacterController.isGrounded)
			{
				m_MoveDir.x = desiredMove.x*m_CurrentSpeed;
				m_MoveDir.z = desiredMove.z*m_CurrentSpeed;
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
				float resistance = 0.04f;
				float projX = Math.Abs(m_MoveDir.x + desiredMove.x*m_CurrentSpeed*resistance);
				float projZ = Math.Abs(m_MoveDir.z + desiredMove.z*m_CurrentSpeed*resistance);
				float infX = Math.Abs(m_MaxInfluence*desiredMove.x);
				float infZ = Math.Abs(m_MaxInfluence*desiredMove.z);

				// Allow air resistant movement, only if counterproductive to current movement
				if (projX < Math.Abs(m_MoveDir.x) || projX < infX)
				{
					m_MoveDir.x += desiredMove.x*m_CurrentSpeed*resistance;
				}
				if (projZ < Math.Abs(m_MoveDir.z) || projZ < infZ)
				{
					m_MoveDir.z += desiredMove.z*m_CurrentSpeed*resistance;
				}
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
			if (!m_CharacterController.isGrounded && m_Charges > 0 && m_Jump)
			{
				m_MoveDir.x = desiredMove.x*m_CurrentSpeed;
				m_MoveDir.z = desiredMove.z*m_CurrentSpeed;

                // update circle UI
                if (m_Charges == 2)
                {
                    m_JumpCircle_TL.GetComponent<UICircle>().setCharged(false);
                    m_JumpCircle_TR.GetComponent<UICircle>().setCharged(false);
                }
                else
                {
                    m_JumpCircle_BL.GetComponent<UICircle>().setCharged(false);
                    m_JumpCircle_BR.GetComponent<UICircle>().setCharged(false);
                }

				m_MoveDir.y = m_JumpSpeed;
				PlayJumpSound();
				m_Jump = false;
				m_Jumping = true;
				m_Charges--;

				PlayAirjumpSound();
			}

			// Dashing
			if (m_Dashing)
			{
				m_AirdashTime +=Time.fixedDeltaTime;
				if (m_AirdashTime >= 0.26f)
				{
					m_AirdashTime = 0f;
					m_Dashing = false;
				}
			}
			else if (m_Dash)
			{
				// update circle UI
				if (m_Charges == 2)
				{
					m_JumpCircle_TL.GetComponent<UICircle>().setCharged(false);
					m_JumpCircle_TR.GetComponent<UICircle>().setCharged(false);
				}
				else
				{
					m_JumpCircle_BL.GetComponent<UICircle>().setCharged(false);
					m_JumpCircle_BR.GetComponent<UICircle>().setCharged(false);
				}

				m_Dash = false;
				m_Dashing = true;
				m_Charges--;
				m_CurrentSpeed = m_CurrentSpeed * m_AirdashMulti;

				if (m_CurrentSpeed < m_RunSpeed)
				{
					m_CurrentSpeed = m_RunSpeed;
				}
				if (m_MoveDir.y < 0)
				{
					m_MoveDir.y = 0;
				}

				m_MoveDir.x = transform.forward.x*m_CurrentSpeed;
				m_MoveDir.z = transform.forward.z*m_CurrentSpeed;
				m_MoveDir.y = m_MoveDir.y*m_AirdashMulti;

				PlayAirdashSound();
			}

            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

			ProgressStepCycle(m_CurrentSpeed);
			UpdateCameraPosition(m_CurrentSpeed);

            m_MouseLook.UpdateCursorLock();
		}
			
		private void PlayLandingSound()
		{
			m_LandAudioSource.outputAudioMixerGroup = m_LandGroup;
			m_LandAudioSource.clip = m_LandSound;
			m_LandAudioSource.Play();
			m_NextStep = m_StepCycle + .5f;
		}

        private void PlayJumpSound()
		{
			m_JumpAudioSource.outputAudioMixerGroup = m_JumpGroup;
			m_JumpAudioSource.clip = m_JumpSound;
			m_JumpAudioSource.Play();
		}

		private void PlayAirjumpSound()
		{
			m_JumpAudioSource.outputAudioMixerGroup = m_JumpGroup;
			m_JumpAudioSource.clip = m_AirjumpSound;
			m_JumpAudioSource.Play();
		}

		private void PlayAirdashSound()
		{
			m_DashAudioSource.outputAudioMixerGroup = m_DashGroup;
			m_DashAudioSource.clip = m_AirdashSound;
			m_DashAudioSource.Play();
		}
			
        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

			if (m_CharacterController.isGrounded)
			{
            	PlayFootStepAudio();
			}
        }


        private void PlayFootStepAudio()
        {
			int n = (int)(Math.Floor((decimal)(Random.Range(0, m_FootstepSounds.Length))));

			m_WalkAudioSource.outputAudioMixerGroup = m_WalkGroup;
			m_WalkAudioSource.clip = m_FootstepSounds[n];
			m_WalkAudioSource.volume = 0.2f;
			m_WalkAudioSource.Play();
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput()
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            //m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            //speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
			//speed = m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

			// Only allow movement acceleration/deceleration on ground (except air resistance)
			if (m_CharacterController.isGrounded)
			{
				if (m_Input.sqrMagnitude > 0)
				{
					// Acceleration
					if (m_CurrentSpeed <= 0.05f)
					{
						m_CurrentSpeed = m_RunSpeed*0.05f;
					}
					else if (m_CurrentSpeed < m_RunSpeed)
					{
						m_CurrentSpeed += (m_RunSpeed - m_CurrentSpeed)*0.1f;
					}
					else
					{
						m_CurrentSpeed = m_RunSpeed;
					}
				}
				else
				{
					// Deceleration
					if (m_CurrentSpeed > 0.05f)
					{
						m_CurrentSpeed = m_CurrentSpeed*0.8f;
					}
					else
					{
						m_CurrentSpeed = 0;
					}
				}
			}

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            /*if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }*/
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
