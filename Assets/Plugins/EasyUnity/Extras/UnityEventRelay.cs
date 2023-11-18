using System;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventRelay : MonoBehaviour
{
    [Serializable] public class AwakeRelay : UnityEvent { }
    [SerializeField] private AwakeRelay awakeRelay;
    private void Awake() => awakeRelay.Invoke();

    [Serializable] public class OnEnableRelay : UnityEvent { }
    [SerializeField] private OnEnableRelay onEnableRelay;
    private void OnEnable() => onEnableRelay.Invoke();

    [Serializable] public class StartRelay : UnityEvent { }
    [SerializeField] private StartRelay startRelay;
    private void Start() => startRelay.Invoke();

    [Serializable] public class OnDisableRelay : UnityEvent { }
    [SerializeField] private OnDisableRelay onDisableRelay;
    private void OnDisable() => onDisableRelay.Invoke();

    [Serializable] public class OnDestroyRelay : UnityEvent { }
    [SerializeField] private OnDestroyRelay onDestroyRelay;
    private void OnDestroy() => onDestroyRelay.Invoke();
}
