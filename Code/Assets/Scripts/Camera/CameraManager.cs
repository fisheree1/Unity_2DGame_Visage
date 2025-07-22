using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;

    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallPanTime = 0.35f;
    public float _fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYDampingCoroutine;
    private Coroutine _panCameraCoroutine;

    [SerializeField]
    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;

    private float _normYPanAmount;

    private Vector2 _startingTrackedObjectOffset;

    private CinemachineVirtualCamera respawnCamera;


    private void Awake()
    {
        Debug.Log("CameraManager Awake called");
        if (instance == null)
        {
            instance = this;
        }

        for (int i = 0; i < _allVirtualCameras.Length; i++)
        {
            if (_allVirtualCameras[i].enabled)
            {
                _currentCamera = _allVirtualCameras[i];

                _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        SetRespawnCamera(_currentCamera);

        _normYPanAmount = _framingTransposer.m_ScreenY;

        _startingTrackedObjectOffset = _framingTransposer.m_TrackedObjectOffset;
    }


    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft, CinemachineVirtualCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        Debug.Log("Current Left camera: " + cameraFromLeft.name);
        Debug.Log("Current Right camera: " + cameraFromRight.name);
        Debug.Log("Current camera: " + _currentCamera.name);
        Debug.Log("Trigger exit direction: " + triggerExitDirection);
        if (_currentCamera == cameraFromLeft && (triggerExitDirection.x > 0f))
        {
            Debug.Log("change to right camera");

            cameraFromRight.enabled = true;
            cameraFromLeft.enabled = false;
            _currentCamera = cameraFromRight;
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        if (_currentCamera == cameraFromRight && (triggerExitDirection.x < 0f))
        {
            Debug.Log("change to left camera");
            cameraFromLeft.enabled = true;
            cameraFromRight.enabled = false;
            _currentCamera = cameraFromLeft;
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }


    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        if (!panToStartingPos)
        {
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.right;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.left;
                    break;
                default:
                    break;
            }

            endPos *= panDistance;
            startingPos = _startingTrackedObjectOffset;
            endPos += startingPos;
        }
        else
        {
            startingPos = _framingTransposer.m_TrackedObjectOffset;
            endPos = _startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while (elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, elapsedTime / panTime);
            _framingTransposer.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        _lerpYDampingCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = _fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = _normYPanAmount;
        }

        float elapsedTime = 0f;
        while (elapsedTime < _fallPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / _fallPanTime);
            _framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }

    public void SetRespawnCamera(CinemachineVirtualCamera camera)
    {
        respawnCamera = camera;
    }

    public CinemachineVirtualCamera GetRespawnCamera()
    {
        return respawnCamera;
    }
    
    public CinemachineVirtualCamera GetCurrentCamera()
    {
        return _currentCamera;
    }

    public void RespawnCamera()
    {
        if (respawnCamera != null)
        {
            _currentCamera.enabled = false;
            respawnCamera.enabled = true;

            for (int i = 0; i < _allVirtualCameras.Length; i++)
            {
                if (_allVirtualCameras[i].enabled)
                {
                    _currentCamera = _allVirtualCameras[i];

                    _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                }
            }

            _normYPanAmount = _framingTransposer.m_ScreenY;

            _startingTrackedObjectOffset = _framingTransposer.m_TrackedObjectOffset;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
