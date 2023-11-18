 using NoxLibrary;

using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class VFXPlayer : CustomAnimator
{
    [Tooltip("If true, continues playing animations after they're over and the queue is empty")]
    [SerializeField] protected bool playInLoop = false; public bool PlayerInLoop => playInLoop;
    [Tooltip("If true, plays a random animation from the available list when Play() is invoked")]
    [SerializeField] protected bool playRandomSheet = false;
    [Tooltip("Sets a range for a random interval between triggers of the animation when looping")]
    [SerializeField] protected FloatRange loopRandomInterval;
    [Tooltip("If true, disables the game object while not playing, and re-enables upon Play()")]
    [SerializeField] protected bool hideWhenNotPlaying = true;

    /// <summary>
    /// Immediately starts playing an animation according to pre-specified variables
    /// </summary>
    public virtual void Play()
    {
        //Resets the current Frame and elapsedTime
        Frame = PlayInReverse ? GetActionFrameCount(playingAction, playingDirection) - 1 : 0;
        elapsedTime = 0;

        //Invokes appropriate custom events
        onAnyActionBegin?.Invoke(playingAction);
        if (onSpecificActionBegin != null && onSpecificActionBegin.TryGetValue(playingAction ?? "", out UnityEvent onSpecificActionBeginEvent))
            onSpecificActionBeginEvent?.Invoke();

        //Immediately updates the sprite, which also sets "IsPlaying = true" if everything goes correctly
        UpdateSprite(GetPlayingAnimation(playingAction, playingDirection));
    }

    public void SetPlayInReverse(bool flag)
    {
        playInReverse = flag;
    }
    /// <summary>
    /// Immediately starts playing an animation based on provided action name
    /// </summary>
    /// <param name="action">The name of the action to be played</param>
    public virtual void Play(string action)
    {
        SetAction(action);
        SetDirection(FacingDirection);
        Play();
    }

    /// <summary>
    /// Immediately starts playing an animation based on provided action name
    /// </summary>
    /// <param name="action">The name of the action to be played</param>
    public virtual void PlayRetainFrame(string action)
    {
        int frame = Frame;
        SetAction(action);
        SetDirection(FacingDirection);

        Frame = frame;
        elapsedTime = 0;

        //Invokes appropriate custom events
        onAnyActionBegin?.Invoke(playingAction);
        if (onSpecificActionBegin != null && onSpecificActionBegin.TryGetValue(playingAction ?? "", out UnityEvent onSpecificActionBeginEvent))
            onSpecificActionBeginEvent?.Invoke();

        //Immediately updates the sprite, which also sets "IsPlaying = true" if everything goes correctly
        UpdateSprite(GetPlayingAnimation(playingAction, playingDirection));
    }

    /// <summary>    /// 
    /// Immediately starts playing a random animation from the available list
    /// </summary>
    public virtual void PlayRandom()
    {
        //Invokes appropriate custom events
        onActionChanged?.Invoke(playingAction = customAnimationsDictionary.Keys.Random());

        //Resets the current Frame and elapsedTime
        Frame = PlayInReverse ? GetActionFrameCount(playingAction, playingDirection) - 1 : 0;
        elapsedTime = 0;

        //Invokes appropriate custom events
        onAnyActionBegin?.Invoke(playingAction);
        if (onSpecificActionBegin != null && onSpecificActionBegin.TryGetValue(playingAction, out UnityEvent onSpecificActionBeginEvent))
            onSpecificActionBeginEvent?.Invoke();

        //Immediately updates the sprite, which also sets "IsPlaying = true" if everything goes correctly
        UpdateSprite(GetPlayingAnimation(playingAction, playingDirection));
    }

    /// <summary>
    /// Toggles VFX Player to play current animation in a loop
    /// </summary>
    public virtual void SetPlayInLoop(bool flag) => playInLoop = flag;

    /// <summary>
    /// Alias for SetIsPlaying(false);
    /// </summary>
    public virtual void StopPlaying() => SetIsPlaying(false);
    public virtual void SetHideWhileNotPlaying(bool flag) => hideWhenNotPlaying = flag;

    public virtual void TogglePlayInLoop() => playInLoop = !playInLoop;

    /// <summary>
    /// Whenever re-enabled, resets and restarts functionality
    /// </summary>
    protected override void OnEnable()
    {
        if (IsPlaying)
        {
            base.OnEnable();
            Frame = randomizeStartFrame ? Random.Range(0, GetActionFrameCount(playingAction, playingDirection)) : 0;
            PlayNextFrame();
        }
        else SetIsPlaying(IsPlaying);

        if (playInLoop && !IsPlaying && loopRandomInterval.Max > 0)
            this.InvokeDelayed(loopRandomInterval.Random(), SetFirstFrame);
    }

    protected override void PlayNextFrame()
    {
        //Gets the currently playing animation and calculates what the next Frame index should be
        CustomAnimation customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
        Frame = customAnimation == null ? 0 : playInReverse ? Frame - 1 : Frame + 1;

        //If animation is just ending, invoke appropriate events
        if (customAnimation != null && !Frame.IsIndexOf(customAnimation.GetSprites))
        {
            onAnyActionOver?.Invoke(playingAction);
            if (onSpecificActionOver != null && onSpecificActionOver.TryGetValue(playingAction, out UnityEvent onSpecificActionOverEvent))
                onSpecificActionOverEvent?.Invoke();
        }

        //If animation is just ending but another is queued, dequeue and replace.
        if ((customAnimation == null || !Frame.IsIndexOf(customAnimation.GetSprites)) && ActionQueue.Count > 0)
        {
            DequeueAction();
            customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
            Frame = customAnimation != null && playInReverse ? customAnimation.FrameCount - 1 : 0;
        }
        //Otherwise if animation is just ending but it's set to loop, prepares the loop
        else if (customAnimation != null && !Frame.IsIndexOf(customAnimation.GetSprites) && playInLoop)
        {
            if (!combinedAlternativeAnimations.ContainsKey(playingAction))
            {
                float interval = loopRandomInterval.Random();
                if (interval == 0) SetFirstFrame();
                else this.InvokeDelayed(interval, SetFirstFrame);
            }
            else
            {
                int rand = Random.Range(0, AlternativeAnimationCount);
                AlternativeAnimationIndex = rand;
                if (Frame < 0) Frame = customAnimation.FrameCount - 1;
                else if (Frame >= customAnimation.FrameCount) Frame = 0;
            }
        }
        //Otherwise no animation is playing, reset the Frame index
        else if (customAnimation == null) Frame = 0;

        //If an animation is just beginning, invoke appropriate events
        if (customAnimation != null && (Frame == 0 || (playInReverse && (Frame == customAnimation.FrameCount - 1))))
        {
            onAnyActionBegin?.Invoke(playingAction);
            if (onSpecificActionBegin != null && onSpecificActionBegin.TryGetValue(playingAction, out UnityEvent onSpecificActionBeginEvent))
                onSpecificActionBeginEvent?.Invoke();
        }

        //Immediately updates the sprite, and also resets IsPlaying as appropriate
        UpdateSprite(customAnimation);
    }

    /// <summary>
    /// Resets the currently playing animation back to the first frame and immediately updates the sprite
    /// </summary>
    protected void SetFirstFrame()
    {
        CustomAnimation customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
        if (customAnimation?.FrameCount > 0)
        {
            if (playRandomSheet) onActionChanged?.Invoke(playingAction = customAnimationsDictionary.Keys.Random());
            if (Frame < 0) Frame = customAnimation.FrameCount - 1;
            else if (Frame >= customAnimation.FrameCount) Frame = 0;
            UpdateSprite(customAnimation);
        }
    }

    protected override void UpdateSprite(CustomAnimation customAnimation, bool invokeEvent = true)
    {
        //If we're in an invalid state for whatever reason, stop playing and return
        if (customAnimation == null || !Frame.IsIndexOf(customAnimation.GetSprites))
        {
            SetIsPlaying(false);
            return;
        }

        //If we've reached this far, we can start playing
        SetIsPlaying(true);

        //Immediately updates the current sprite
        SetSprite(customAnimation.GetSprites[Frame]);

        //Invokes frame event if any
        if (invokeEvent && FrameEvents.TryGetValue((Action, customAnimation.Direction, Frame), out UnityAction unityAction))
        {
            Debug.Log("UpdateSprite frame event invoked: " + unityAction);/////
            unityAction.Invoke();
        }
    }

    public override bool IsPlaying { get => base.IsPlaying; set => SetIsPlaying(value); }

    public void SetRandomFrame()
    {
        CustomAnimation customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
        if (customAnimation?.FrameCount > 0)
        {
            Frame = Random.Range(0, customAnimation.FrameCount);
            UpdateSprite(customAnimation);
        }
    }
    /// <summary>
    /// Sets the information on whether we're currently playing and also performs some important updates
    /// </summary>
    /// <param name="enabled">Whether we should be set to be currently playing or stopped</param>
    protected virtual void SetIsPlaying(bool enabled)
    {
        if (enabled && !isPlaying) elapsedTime = 0;
        isPlaying = enabled;
        if (hideWhenNotPlaying)
        {
            if (spriteRenderer) spriteRenderer.enabled = isPlaying;
            if (image) image.enabled = isPlaying;
        }
    }


    #region High Level Functions

    public void SetFrameAndStop(int index)
    {
        CustomAnimation customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
        if (customAnimation?.FrameCount > 0)
        {
            if (playRandomSheet) onActionChanged?.Invoke(playingAction = customAnimationsDictionary.Keys.Random());
            if (index < 0) Frame = customAnimation.FrameCount - 1;
            else Frame = index;
            UpdateSprite(customAnimation);
            StopPlaying();
        }
    }

    public void SetFrameAndPlay(int index)
    {
        CustomAnimation customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
        if (customAnimation?.FrameCount > 0)
        {
            if (playRandomSheet) onActionChanged?.Invoke(playingAction = customAnimationsDictionary.Keys.Random());
            if (index < 0) Frame = customAnimation.FrameCount - 1;
            else Frame = index;
            UpdateSprite(customAnimation);
        }
    }

    #endregion
}
