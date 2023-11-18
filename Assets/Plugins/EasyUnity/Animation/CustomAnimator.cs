using NoxLibrary;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using MoreLinq;
using UnityEditor;
using Sirenix.OdinInspector;

/// <summary>
/// Handles sprite animations automagically
/// </summary>
public class CustomAnimator : MonoBehaviour
{
    /// <summary>
    /// A single sprite animation (with its multiple frames)
    /// </summary>
    [Serializable]
    public class CustomAnimation
    {
        /// <summary>
        /// The sprite frames that compose the animation
        /// </summary>
        [SerializeField] public List<Sprite> GetSprites;

        /// <summary>
        /// The Category component of the animation name.
        /// </summary>
        [SerializeField] public string Category;

        /// <summary>
        /// The Action component of the animation name.
        /// </summary>
        [SerializeField] public string Action;

        /// <summary>
        /// The Cardinal Direction of the animation.
        /// </summary>
        [SerializeField] public AbsoluteDirection Direction;

        /// <summary>
        /// The Frames Per Second count of the animation. More = Faster.
        /// </summary>
        [SerializeField] public int FPS;

        /// <summary>
        /// How many frames this animation has.
        /// </summary>
        public int FrameCount => GetSprites?.Count ?? 0;

        /// <summary>
        /// Creates a new CustomAnimation.
        /// </summary>
        /// <param name="ssi">The SpriteSheetInfo containing all the necessary information.</param>
        public CustomAnimation(SpriteSheetInfo ssi)
        {
            GetSprites = new List<Sprite>();
            Category = ssi.CategoryName;
            Action = ssi.ActionName;
            Direction = new AbsoluteDirection(ssi.Direction);
            FPS = ssi.FPS;
        }

        /// <summary>
        /// Creates a new CustomAnimation.
        /// </summary>
        /// <param name="category">The Category component of the animation name.</param>
        /// <param name="action">The Action component of the animation name.</param>
        /// <param name="direction">The Cardinal Direction of the animation.</param>
        /// <param name="fps">The Frames Per Second count of the animation. More = Faster.</param>
        public CustomAnimation(string category, string action, AbsoluteDirection direction, int fps)
        {
            GetSprites = new List<Sprite>();
            Category = category;
            Action = action;
            Direction = direction;
            FPS = fps;
        }


        /// <summary>
        /// Creates a new CustomAnimation.
        /// </summary>
        /// <param name="ssi">The SpriteSheetInfo containing all the necessary information.</param>
        /// <param name="sprites">The actual sprites that compose the animation.</param>
        public CustomAnimation(SpriteSheetInfo ssi, IEnumerable<Sprite> sprites)
        {
            GetSprites = new List<Sprite>(sprites);
            Category = ssi.CategoryName;
            Action = ssi.ActionName;
            Direction = new AbsoluteDirection(ssi.Direction);
            FPS = ssi.FPS;
        }

        /// <summary>
        /// Creates a new CustomAnimation.
        /// </summary>
        /// <param name="category">The Category component of the animation name.</param>
        /// <param name="action">The Action component of the animation name.</param>
        /// <param name="direction">The Cardinal Direction of the animation.</param>
        /// <param name="fps">The Frames Per Second count of the animation. More = Faster.</param>
        /// <param name="sprites">The actual sprites that compose the animation.</param>
        public CustomAnimation(string category, string action, AbsoluteDirection direction, int fps, IEnumerable<Sprite> sprites)
        {
            GetSprites = new List<Sprite>(sprites);
            Category = category;
            Action = action;
            Direction = direction;
            FPS = fps;
        }

        /// <summary>
        /// Adds another sprite frame to the animation.
        /// </summary>
        /// <param name="sprite"></param>
        public void Add(Sprite sprite) => GetSprites.Add(sprite);

        /// <summary>
        /// Gets the qualified name of this animation.
        /// </summary>
        /// <returns><code>$"{Category}_{Action}_{Direction}_{FPS}F"</code></returns>
        public override string ToString() => $"{Category}_{Action}_{Direction}_{FPS}F";
    }

    [Serializable]
    protected class CustomAnimationsDictionary
    {
        [Serializable]
        public class InnerDictionaryClass : SerializableDictionary<string, AnimationDirectionsDictionary> { }
        public InnerDictionaryClass InnerDictionary = new InnerDictionaryClass();
        public Dictionary<string, AnimationDirectionsDictionary>.KeyCollection Keys => InnerDictionary.Keys;
        public Dictionary<string, AnimationDirectionsDictionary>.ValueCollection Values => InnerDictionary.Values;

        public bool TryGetValue(string key, out AnimationDirectionsDictionary value) => InnerDictionary.TryGetValue(key.ToLower(), out value);
        public int Count => InnerDictionary.Count;
        public void Add(string key, AnimationDirectionsDictionary value) => InnerDictionary.Add(key.ToLower(), value);
        public bool ContainsKey(string key) => InnerDictionary.ContainsKey(key.ToLower());
    }

    [Serializable]
    protected class AnimationDirectionsDictionary
    {
        [Serializable]
        public class InnerDictionaryClass : SerializableDictionary<AbsoluteDirection, CustomAnimation> { }
        public InnerDictionaryClass InnerDictionary = new InnerDictionaryClass();

        
        [Header("Helper-EditorOnly")] public Texture2D mainAnimationTex = null;

#if UNITY_EDITOR
        [ContextMenuItem("Helper",nameof(PopulateSpritesFromHelperSprite))]
#endif

        [Header("Helper-EditorOnly")] public AbsoluteDirection absoluteDirectionToPopulate;

#if UNITY_EDITOR
        [Button]
        public void PopulateSpritesFromHelperSprite()
        {
            if (mainAnimationTex == null) return;
            if(absoluteDirectionToPopulate == null) return;
            if (!InnerDictionary.Keys.Contains(absoluteDirectionToPopulate)) return;

            List<Sprite> loadedSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mainAnimationTex)).OfType<Sprite>().ToList();

            if (this.TryGetValue(absoluteDirectionToPopulate, out CustomAnimation val))
            {
                val.GetSprites = loadedSprites;
            }


        }
#endif

        public Dictionary<AbsoluteDirection, CustomAnimation>.KeyCollection Keys => InnerDictionary.Keys;
        public Dictionary<AbsoluteDirection, CustomAnimation>.ValueCollection Values => InnerDictionary.Values;
        public bool TryGetValue(AbsoluteDirection key, out CustomAnimation value) => InnerDictionary.TryGetValue(key, out value);
        public int Count => InnerDictionary.Count;
        public void Add(AbsoluteDirection key, CustomAnimation value) => InnerDictionary.Add(key, value);
        public bool ContainsKey(AbsoluteDirection key) => InnerDictionary.ContainsKey(key);
    }

    [Serializable]
    protected class CombinedAlternativeAnimations
    {
        public SerializableDictionary<string, List<CustomAnimation>> CombinedAlternativeAnimationsDictionary = new SerializableDictionary<string, List<CustomAnimation>>();

        public Dictionary<string, List<CustomAnimation>>.KeyCollection Keys => CombinedAlternativeAnimationsDictionary.Keys;
        public Dictionary<string, List<CustomAnimation>>.ValueCollection Values => CombinedAlternativeAnimationsDictionary.Values;
        public bool TryGetValue(string key, out List<CustomAnimation> value) => CombinedAlternativeAnimationsDictionary.TryGetValue(key, out value);
        public int Count => CombinedAlternativeAnimationsDictionary.Count;
        public void Add(string key, List<CustomAnimation> value) => CombinedAlternativeAnimationsDictionary.Add(key, value);
        public bool ContainsKey(string key) => CombinedAlternativeAnimationsDictionary.ContainsKey(key);


    }

    [Tooltip("If true, this animator will ignore time scaling")]
    [SerializeField] protected bool useUnscaledTime = false;
    /// <summary>
    /// If true, this animator will ignore time scaling
    /// </summary>
    public bool UseUnscaledTime { get => useUnscaledTime; set => useUnscaledTime = value; }

    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Image image;
    [SerializeField] protected SpriteMask spriteMask;
    protected bool NeedsRenderer => !image && !spriteRenderer;

    public SpriteRenderer GetSpriteRenderer => spriteRenderer;
    public Image GetImage => image;
    public SpriteMask GetSpriteMask => spriteMask;
    public Component GetRenderer => GetSpriteRenderer ? GetSpriteRenderer : (Component)GetImage;


    [Tooltip("Reference to all spritesheets this character can use")]
    [SerializeField] protected List<Texture2D> spriteSheets = new List<Texture2D>();

    [Serializable] public class ActionEventDictionary : SerializableDictionary<string, UnityEvent> { }
    [Tooltip("This event is invoked every time an action (animation/spritesheet) begins, even if it loops.\nIt passes as a string argument the name of the action that just began.")]
    public UnityEvent<string> onActionChanged;
    [Tooltip("This event is invoked every time an action (animation/spritesheet) begins, even if it loops.\nIt passes as a string argument the name of the action that just began.")]
    public UnityEvent<string> onAnyActionBegin;
    [Tooltip("This event is invoked every time an action (animation/spritesheet) is over, even if it loops.\nIt passes as a string argument the name of the action that just finished.")]
    public UnityEvent<string> onAnyActionOver;
    [Tooltip("The event is invoked every time the action (animation/spritesheet) begins, even if it loops.")]
    public ActionEventDictionary onSpecificActionBegin;
    [Tooltip("The event is invoked every time the action (animation/spritesheet) is over, even if it loops.")]
    public ActionEventDictionary onSpecificActionOver;

    [SerializeField] protected string playingAction = null;
    [SerializeField] protected string defaultDirection = string.Empty;
    [SerializeField] protected AbsoluteDirection playingDirection = default;
    [SerializeField] protected int playingFrame = 0;
    [SerializeField] protected float playingSpeed = 1;
    [SerializeField] protected float elapsedTime = 0f;
    [SerializeField] protected bool isPlaying = true;
    [SerializeField] protected bool playInReverse = false;
    [SerializeField] protected bool randomizeStartFrame = false;
    [SerializeField] protected CustomAnimationsDictionary customAnimationsDictionary = new CustomAnimationsDictionary();
    [Header("Alternate Animations"), SerializeField] protected CustomAnimationsDictionary alternateCustomAnimationsDictionary = new CustomAnimationsDictionary();
    [Header("Alternate Animations"), SerializeField, ReadOnly] protected CombinedAlternativeAnimations combinedAlternativeAnimations = new CombinedAlternativeAnimations();
    /// <summary>
    /// Used to track the animation used if it's using alternatives
    /// </summary>
    public int AlternativeAnimationIndex = 0;
    public int AlternativeAnimationCount = 0;


    public bool HasAction(string action) => customAnimationsDictionary != null && customAnimationsDictionary.TryGetValue(action, out _) ? true : false;

    /// <summary>
    /// If playing, returns the FPS of the current animation, otherwise 0
    /// </summary>
    protected int CurrentFrameRate => Application.isPlaying ? GetPlayingAnimation(Action, Direction)?.FPS ?? 0 : 0;

    public void ResetElapsedTime() => elapsedTime = 0;

    /// <summary>
    /// Immediately sets a new action to be played.
    /// </summary>
    /// <param name="action">The name of the new action</param>
    public void SetAction(string action) => Action = action;

    /// <summary>
    /// Immediately sets a new action to be played.
    /// </summary>
    /// <param name="action">The name of the new action</param>
    /// <param name="direction">The direction of the new action</param>
    /// <param name="updateSprite">If true (default), also immediately updates the sprite, otherwise only updates after the animation interval (1s/FPS)</param>
    public void SetAction(string action, string direction, bool updateSprite = true) { Action = action; SetDirection(direction, updateSprite); }

    /// <summary>
    /// Immediately sets a new action to be played.
    /// </summary>
    /// <param name="action">The name of the new action</param>
    /// <param name="direction">The direction of the new action</param>
    /// <param name="updateSprite">If true (default), also immediately updates the sprite, otherwise only updates after the animation interval (1s/FPS)</param>
    public void SetAction(string action, AbsoluteDirection direction, bool updateSprite = true) { Action = action; SetDirection(direction, updateSprite); }

    /// <summary>
    /// Immediately sets a new direction for the playing action.
    /// </summary>
    /// <param name="direction">The new direction of the current action</param>
    /// <param name="updateSprite">If true (default), also immediately updates the sprite, otherwise only updates after the animation interval (1s/FPS)</param>
    public void SetDirection(string direction, bool updateSprite = true) => SetDirection(new AbsoluteDirection(direction), updateSprite);

    /// <summary>
    /// Immediately sets a new direction for the playing action.
    /// </summary>
    /// <param name="direction">The new direction of the current action</param>
    /// <param name="flipX">If true, flips the provided direction horizontally before applying the resulting value</param>
    /// <param name="updateSprite">If true (default), also immediately updates the sprite, otherwise only updates after the animation interval (1s/FPS)</param>
    public void SetDirection(string direction, bool flipX, bool updateSprite = true) => SetDirection(flipX ? new AbsoluteDirection(direction).FlipX() : new AbsoluteDirection(direction), updateSprite);

    /// <summary>
    /// Immediately sets a new direction for the playing action.
    /// </summary>
    /// <param name="direction">The new direction of the current action</param>
    /// <param name="updateSprite">If true (default), also immediately updates the sprite, otherwise only updates after the animation interval (1s/FPS)</param>
    public void SetDirection(AbsoluteDirection direction, bool updateSprite = true)
    {
        IEnumerable<AbsoluteDirection> validDirections = GetActionDirections(Action);

        if (validDirections == null || !TryDirection(direction))
        {
            Log.Warning($"CustomAnimator {name} could not find valid animation direction for action {Action} while facing {direction}");
            return;
        }
        else if (updateSprite)
            UpdateSprite(GetPlayingAnimation(ref playingAction, ref playingDirection), invokeEvent: false);

        bool TryDirection(AbsoluteDirection dir)
        {
            if (validDirections.Contains(dir))
            {
                playingDirection = dir;
                if (spriteRenderer) spriteRenderer.flipX = false;
                return true;
            }
            else if (spriteRenderer && validDirections.Contains(dir.FlipX()))
            {
                playingDirection = dir.FlipX();
                spriteRenderer.flipX = true;
                return true;
            }
            else return false;
        }
    }

    public void FaceTo(Transform trans)
    {
        if (trans.position.x > this.transform.position.x) Direction = "E";
        else
        if (trans.position.x < this.transform.position.x) Direction = "W";
    }

    /// <summary>
    /// Immediately sets a new action to be played.
    /// Format: "category_action". Example: "default_walk"
    /// </summary>
    public string Action
    {
        get => playingAction;
        set
        {
            onActionChanged?.Invoke(playingAction = value);
            SetDirection(FacingDirection);
            playingFrame = 0;
            ActionQueue.Clear();




        }
    }


    /// <summary>
    /// Immediately sets a new direction for the playing action.
    /// </summary>
    public string Direction { get => playingDirection.ToString(); set => SetDirection(value); }

    /// <summary>
    /// Immediately sets a new direction for the playing action.
    /// </summary>
    public AbsoluteDirection CardinalDirection { get => playingDirection; set => SetDirection(value); }

    /// <summary>
    /// Gets the currently effective Facing Direction (flipped from the Cardinal Direction if necessary)
    /// </summary>
    public AbsoluteDirection FacingDirection => spriteRenderer && spriteRenderer.flipX ? CardinalDirection.FlipX() : CardinalDirection;

    /// <summary>
    /// The current frame we're playing on the current Action (animation)
    /// </summary>
    public int Frame { get => playingFrame; set => playingFrame = value; }
    /// <summary>
    /// The current frame we're playing on the current Action (animation)
    /// </summary>
    public float Speed { get => playingSpeed; set => playingSpeed = value; }

    /// <summary>
    /// Indicates whether we're currently actively playing an animation.
    /// </summary>
    public virtual bool IsPlaying { get => isPlaying; set => isPlaying = value; }

    /// <summary>
    /// Indicates whether we're playing the animation in reverse (from last to first frame)
    /// </summary>
    public bool PlayInReverse { get => playInReverse; set => playInReverse = value; }

    /// <summary>
    /// Indicates whether we're playing the animation in reverse (from last to first frame)
    /// </summary>
    public bool RandomizeStartFrame { get => randomizeStartFrame; set => randomizeStartFrame = value; }

    /// <summary>
    /// Toggles reverse playing
    /// </summary>
    public void TogglePlayReverse() => playInReverse = !playInReverse;

    /// <summary>
    /// Gets all the qualified action names available
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetActionNames() => customAnimationsDictionary?.Keys;

    /// <summary>
    /// Gets all valid directions for a given action
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    /// <returns>All the available directions for this action</returns>
    public IEnumerable<AbsoluteDirection> GetActionDirections(string action)
        => (customAnimationsDictionary != null && customAnimationsDictionary.TryGetValue(action, out AnimationDirectionsDictionary directions)) ? directions.Keys : null;

    /// <summary>
    /// Allows queued actions to be played after the current one ends
    /// </summary>
    public Queue<string> ActionQueue { get; } = new Queue<string>();

    /// <summary>
    /// Gets a single comma(and space)-separated string composed of all the queued actions
    /// </summary>
    /// <returns>"action1, action2, action3..."</returns>
    public string GetQueuedNames() => ActionQueue.SelectToString(v => v).Aggregate((a, b) => $"{a}, {b}");

    /// <summary>
    /// All the registered events to be played on specific frames of specific animations(actions)
    /// </summary>
    public Dictionary<(string Action, AbsoluteDirection Direction, int Frame), UnityAction> FrameEvents { get; } = new Dictionary<(string Action, AbsoluteDirection Direction, int Frame), UnityAction>();

    public void FaceTowards(Transform target)
    {
        if (target.position.x > this.transform.position.x)
            Direction = "E";
        else
            Direction = "W";
    }

    public void ClearFrameEvents()
    {
        Log.Warning($"Frame Events Cleared in {name}");
        FrameEvents?.Clear();
    }
    /// <summary>
    /// Registers an event to be fired whenever a specific action reaches a specific frame.
    /// </summary>
    /// <param name="Action">The qualified name of the action</param>
    /// <param name="Frame">The zero-based index of the frame</param>
    /// <param name="unityAction">The event to be fired when the frame is played</param>
    public void OnFrame(string Action, int Frame, UnityAction unityAction)
    {
        if (customAnimationsDictionary.TryGetValue(Action, out AnimationDirectionsDictionary directionsDictionary))
            foreach (AbsoluteDirection direction in directionsDictionary.Keys)
                OnFrame(Action, direction, Frame, unityAction);
        else Log.Error($"CustomAnimator.OnFrame({Action},{Frame},{unityAction}): Could not find Action {Action}");
    }

    /// <summary>
    /// Registers an event to be fired whenever a specific action reaches a specific frame with a specific direction.
    /// </summary>
    /// <param name="Action">The qualified name of the action</param>
    /// <param name="Direction">The only direction valid for this event</param>
    /// <param name="Frame">The zero-based index of the frame</param>
    /// <param name="unityAction">The event to be fired when the frame is played</param>
    public void OnFrame(string Action, AbsoluteDirection Direction, int Frame, UnityAction unityAction)
    {
        if (customAnimationsDictionary.TryGetValue(Action, out AnimationDirectionsDictionary directionsDictionary) &&
            directionsDictionary.TryGetValue(Direction, out CustomAnimation framesList))
        {
            (string, AbsoluteDirection, int) key = (Action, Direction, Frame);
            if (FrameEvents.ContainsKey(key))
                FrameEvents.Remove(key);
            FrameEvents.Add(key, unityAction);
        }
        else throw new ArgumentException("Could not find Action/Direction");
    }

    /// <summary>
    /// Registers an event to be fired whenever a specific action reaches a specific frame.
    /// </summary>
    /// <param name="Action">The qualified name of the action</param>
    /// <param name="Frame">The zero-based index of the frame</param>
    /// <param name="unityAction">The event to be fired when the frame is played</param>
    public void OnFrameExtend(string Action, int Frame, UnityAction unityAction)
    {
        if (customAnimationsDictionary.TryGetValue(Action, out AnimationDirectionsDictionary directionsDictionary))
            foreach (AbsoluteDirection direction in directionsDictionary.Keys)
                OnFrameExtend(Action, direction, Frame, unityAction);
        else Log.Error($"CustomAnimator.OnFrame({Action},{Frame},{unityAction}): Could not find Action {Action}");
    }

    /// <summary>
    /// Registers an event to be fired whenever a specific action reaches a specific frame with a specific direction.
    /// </summary>
    /// <param name="Action">The qualified name of the action</param>
    /// <param name="Direction">The only direction valid for this event</param>
    /// <param name="Frame">The zero-based index of the frame</param>
    /// <param name="unityAction">The event to be fired when the frame is played</param>
    public void OnFrameExtend(string Action, AbsoluteDirection Direction, int Frame, UnityAction unityAction)
    {
        if (customAnimationsDictionary.TryGetValue(Action, out AnimationDirectionsDictionary directionsDictionary) &&
            directionsDictionary.TryGetValue(Direction, out CustomAnimation framesList))
        {
            (string, AbsoluteDirection, int) key = (Action, Direction, Frame);
            if (FrameEvents.ContainsKey(key))
                FrameEvents[key] += unityAction;
            else
                FrameEvents.Add(key, unityAction);
        }
        else throw new ArgumentException("Could not find Action/Direction");
    }

    /// <summary>
    /// Registers an event to be fired whenever a specific action reaches its last frame.
    /// </summary>
    /// <param name="Action">The qualified name of the action</param>
    /// <param name="unityAction">The event to be fired when the frame is played</param>
    public void OnLastFrame(string Action, UnityAction unityAction)
    {
        if (customAnimationsDictionary.TryGetValue(Action, out AnimationDirectionsDictionary directionsDictionary))
            foreach (AbsoluteDirection direction in directionsDictionary.Keys)
                OnLastFrame(Action, direction, unityAction);
        else Log.Error($"CustomAnimator.OnLastFrame: Could not find Action: {Action}");
    }

    /// <summary>
    /// Registers an event to be fired whenever a specific action reaches its last frame with a specific direction.
    /// </summary>
    /// <param name="Action">The qualified name of the action</param>
    /// <param name="Direction">The only direction valid for this event</param>
    /// <param name="unityAction">The event to be fired when the frame is played</param>
    public void OnLastFrame(string Action, AbsoluteDirection Direction, UnityAction unityAction)
    {
        if (customAnimationsDictionary.TryGetValue(Action, out AnimationDirectionsDictionary directionsDictionary) &&
            directionsDictionary.TryGetValue(Direction, out CustomAnimation customAnimation))
        {
            (string, AbsoluteDirection, int) key = (Action, Direction, customAnimation.FrameCount - 1);
            if (FrameEvents.ContainsKey(key))
                FrameEvents.Remove(key);
            FrameEvents.Add(key, unityAction);
        }
        else Log.Error($"CustomAnimator.OnLastFrame({Action}, {Direction}, UnityAction): Could not find Action/Direction");
    }

    /// <summary>
    /// Enqueues an action to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    public void Enqueue(string action) => ActionQueue.Enqueue(action);
    public void EnqueueCurrentDirection(string action) => ActionQueue.Enqueue(action);
    /// <summary>
    /// Enqueues an action to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    /// <param name="direction">The new direction to be set once this new action starts playing</param>
    public void Enqueue(string action, string direction) => ActionQueue.Enqueue(action);

    /// <summary>
    /// Enqueues an action to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    /// <param name="direction">The new direction to be set once this new action starts playing</param>
    public void Enqueue(string action, AbsoluteDirection direction) => ActionQueue.Enqueue(action);

    /// <summary>
    /// Enqueues an action to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="directionalAction">The qualified name of the action *and* The new direction to be set once this new action starts playing</param>
    public void Enqueue((string action, string direction) directionalAction) => ActionQueue.Enqueue(directionalAction.action);

    /// <summary>
    /// Enqueues an action to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="directionalAction">The qualified name of the action *and* The new direction to be set once this new action starts playing</param>
    public void Enqueue((string action, AbsoluteDirection direction) directionalAction) => ActionQueue.Enqueue(directionalAction.action);

    /// <summary>
    /// Enqueues multiple actions to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="actions">The qualified names of the actions</param>
    public void EnqueueMultiple(params string[] actions) => actions?.ForEach(a => Enqueue(a));

    /// <summary>
    /// Enqueues multiple actions to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="directionalActions">The qualified names of the actions *and* The new direction to be set once each action starts playing</param>
    public void EnqueueMultiple(params (string action, string direction)[] directionalActions) => directionalActions?.ForEach(da => Enqueue(da));

    /// <summary>
    /// Enqueues multiple actions to be played after the current and other already queued ones.
    /// </summary>
    /// <param name="directionalActions">The qualified names of the actions *and* The new direction to be set once each action starts playing</param>
    public void EnqueueMultiple(params (string action, AbsoluteDirection direction)[] directionalActions) => directionalActions?.ForEach(da => Enqueue(da));

    protected virtual void Awake()
    {
        if (playingDirection == default)
            playingDirection = global::AbsoluteDirection.South;

        if (randomizeStartFrame) Frame = Random.Range(0, GetActionFrameCount(playingAction, playingDirection));
    }

    protected virtual (string category, string action, AbsoluteDirection direction) GetSpriteData(string spriteName, bool ignoreLastPart)
    {
        string[] values = spriteName.Split('_');
        return (ignoreLastPart ? values.Length - 1 : values.Length) switch
        {
            1 => (null, values[0], global::AbsoluteDirection.South),
            2 => (null, values[1], global::AbsoluteDirection.South),
            4 => (values[1], values[2], new AbsoluteDirection(values[3])),
            _ => ((string)null, (string)null, (AbsoluteDirection)default),
        };
    }

    protected static string GetAnimationKey(SpriteSheetInfo ssi) => $"{ssi.CategoryName ?? "Default"}_{ssi.ActionName ?? ssi.ObjectName}";

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        ValidateSprites();
        ValidateRenderer();

        if (customAnimationsDictionary.Keys.Count == 1 || ((string.IsNullOrEmpty(playingAction) || !customAnimationsDictionary.ContainsKey(playingAction)) && customAnimationsDictionary.Keys.Count > 0))
            playingAction = customAnimationsDictionary.Keys.First();
        else if (customAnimationsDictionary.TryGetValue(playingAction, out var validDirections) && validDirections.Count > 0 && !validDirections.ContainsKey(playingDirection))
            SetDirection(validDirections.Keys.First(), updateSprite: false);
        else SetDirection(playingDirection == default ? global::AbsoluteDirection.South : playingDirection, updateSprite: false);

        if (!Application.isPlaying && defaultDirection != string.Empty)
            SetDirection(defaultDirection, false);
    }

    protected virtual void ValidateSprites()
    {
        HashSet<Texture2D> seenSpriteSheets = new HashSet<Texture2D>();
        customAnimationsDictionary = new CustomAnimationsDictionary();

        for (int i = 0; i < spriteSheets?.Count; i++)
        {
            Texture2D texture = spriteSheets[i];
            if (texture == null || !seenSpriteSheets.Add(texture))
            { spriteSheets.RemoveAt(i--); continue; }

            string path = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (SpriteSheetInfo.TryLoadFrom(path, out SpriteSheetInfo ssi))
                AddFromSSI(texture, ssi);
            else if (path.Contains("frame"))
                AddAsMulti(texture, path);
            else AddAsSingle(texture, path);
        }
    }

    protected virtual void AddFromSSI(Texture2D texture, SpriteSheetInfo ssi)
    {
        if (ssi.Direction == default) ssi.Direction = global::AbsoluteDirection.South;
        AbsoluteDirection direction = new AbsoluteDirection(ssi.Direction);

        string key = GetAnimationKey(ssi);
        if (string.IsNullOrEmpty(key))
        { Log.Error($"{nameof(CustomAnimator)} could not get info from texture named {texture.name} at path {ssi.AssetPath}"); return; }

        if (!customAnimationsDictionary.TryGetValue(key, out AnimationDirectionsDictionary directionsDictionary))
            customAnimationsDictionary.Add(key, directionsDictionary = new AnimationDirectionsDictionary());
        else if (directionsDictionary.ContainsKey(direction))
        {
            Log.Error($"{nameof(CustomAnimator)} rejecting texture because onther one already had the same key and direction found on {texture.name} at path {ssi.AssetPath}");
            return;
        }

        ssi.FPS = ssi.FPS.AtLeast(1);
        CustomAnimation customAnimation = new CustomAnimation(ssi, UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(ssi.AssetPath).OfType<Sprite>());
        directionsDictionary.Add(direction, customAnimation);
    }

    protected virtual void AddAsMulti(Texture2D texture, string path)
    {
        (string category, string action, AbsoluteDirection direction) = GetSpriteData(texture.name, true);
        string key = string.IsNullOrEmpty(category) ? action : $"{category}_{action}";

        if (string.IsNullOrEmpty(key))
        { Log.Error($"{nameof(CustomAnimator)} could not get info from texture named {texture.name} at path {path}"); return; }

        if (!int.TryParse(texture.name.Split('_').Last(), out int index))
        { Log.Error($"{nameof(CustomAnimator)} could not get info from texture named {texture.name} at path {path}"); return; }

        if (!customAnimationsDictionary.TryGetValue(key, out AnimationDirectionsDictionary directionsDictionary))
            customAnimationsDictionary.Add(key, directionsDictionary = new AnimationDirectionsDictionary());

        if (!directionsDictionary.TryGetValue(direction, out CustomAnimation customAnimation))
            directionsDictionary.Add(direction, customAnimation = new CustomAnimation(category, action, direction, 10));

        customAnimation.Add(UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().Single());
    }

    protected virtual void AddAsSingle(Texture2D texture, string path)
    {
        (string category, string action, AbsoluteDirection direction) = GetSpriteData(texture.name, false);
        string key = string.IsNullOrEmpty(category) ? action : $"{category}_{action}";

        if (string.IsNullOrEmpty(key))
        { Log.Error($"{nameof(CustomAnimator)} could not get info from texture named {texture.name} at path {path}"); return; }

        if (!customAnimationsDictionary.TryGetValue(key, out AnimationDirectionsDictionary directionsDictionary))
            customAnimationsDictionary.Add(key, directionsDictionary = new AnimationDirectionsDictionary());

        if (!directionsDictionary.TryGetValue(direction, out CustomAnimation customAnimation))
            directionsDictionary.Add(direction, customAnimation = new CustomAnimation(category, action, direction, 10, UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>()));
    }

    protected virtual void ValidateRenderer()
    {
        if (!image && !spriteRenderer && !spriteMask)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (!spriteRenderer)
                image = GetComponentInChildren<Image>();
            if (!image)
                spriteMask = GetComponentInChildren<SpriteMask>();
        }
    }

    protected virtual void LoadAllFrom(string folderPath)
    {
        HashSet<Texture2D> hashSet = Enumerable.ToHashSet(spriteSheets);
        foreach (string path in System.IO.Directory.EnumerateFiles(folderPath, "*.png", System.IO.SearchOption.AllDirectories))
            UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path)
                .ForEach(o =>
                {
                    if (o is Texture2D t)
                        hashSet.Add(t);
                });
        spriteSheets = hashSet.ToList();
    }
#endif

    [Button]
    protected virtual void InitializeAlternativeAnimations()
    {
        combinedAlternativeAnimations.CombinedAlternativeAnimationsDictionary.Clear();
        foreach(var customAnimationKey in customAnimationsDictionary.Keys)
        {
            List<string> tempAnimationKeys = alternateCustomAnimationsDictionary.Keys.Where(x=> x.Contains(customAnimationKey)).ToList();

            if (tempAnimationKeys == null || tempAnimationKeys.Count == 0) continue;

            List<CustomAnimation> customAnimations = new List<CustomAnimation>();
            for(int i = -1; i < tempAnimationKeys.Count; i++)
            {
                if(i == -1)
                {
                    customAnimations.Add(customAnimationsDictionary.InnerDictionary[customAnimationKey].InnerDictionary.First().Value);
                    continue;
                }
                customAnimations.Add(alternateCustomAnimationsDictionary.InnerDictionary[customAnimationKey+$"-{i}"].InnerDictionary.First().Value);
            }


            combinedAlternativeAnimations.CombinedAlternativeAnimationsDictionary.Add(customAnimationKey, customAnimations);

        }
    }
    [Button]
    protected virtual void ClearAllAlternativeAnimationsCache()
    {
        combinedAlternativeAnimations.CombinedAlternativeAnimationsDictionary.Clear();
    }

    protected virtual void OnEnable()
    {
        if (defaultDirection != string.Empty)
            SetDirection(defaultDirection);
        else
            SetDirection(playingDirection);
        InitializeAlternativeAnimations();
        UpdateSprite(GetPlayingAnimation(ref playingAction, ref playingDirection));
    }

    protected virtual void Update()
    {
        elapsedTime += (UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * Speed;
        if (isPlaying && elapsedTime > 1f / CurrentFrameRate)
        {
            elapsedTime -= 1f / CurrentFrameRate;
            PlayNextFrame();
        }
    }

    protected virtual void PlayNextFrame()
    {
        CustomAnimation customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
        Frame = customAnimation == null ? 0 : playInReverse ? Frame - 1 : Frame + 1;

        if (customAnimation != null && !Frame.IsIndexOf(customAnimation.GetSprites))
        {
            onAnyActionOver?.Invoke(playingAction);
            int rand = Random.Range(0, AlternativeAnimationCount);
            AlternativeAnimationIndex = rand;
            if (onSpecificActionOver != null && onSpecificActionOver.TryGetValue(playingAction, out UnityEvent onSpecificActionOverEvent))
                onSpecificActionOverEvent?.Invoke();
        }

        if ((customAnimation == null || !Frame.IsIndexOf(customAnimation.GetSprites)) && ActionQueue.Count > 0)
        {
            DequeueAction();
            customAnimation = GetPlayingAnimation(ref playingAction, ref playingDirection);
            Frame = customAnimation != null && playInReverse ? customAnimation.FrameCount - 1 : 0;
        }
        else if (customAnimation != null)
        {
            if (Frame < 0) Frame = customAnimation.FrameCount - 1;
            else if (Frame >= customAnimation.FrameCount) Frame = 0;
        }
        else Frame = 0;

        if (customAnimation != null && (Frame == 0 || (playInReverse && (Frame == customAnimation.FrameCount - 1))))
        {
            onAnyActionBegin?.Invoke(playingAction);
            if (onSpecificActionBegin != null && onSpecificActionBegin.TryGetValue(playingAction, out UnityEvent onSpecificActionBeginEvent))
                onSpecificActionBeginEvent?.Invoke();
        }

        UpdateSprite(customAnimation);
    }

    protected virtual void DequeueAction()
    {
        onActionChanged?.Invoke(playingAction = ActionQueue.Dequeue());
        SetDirection(FacingDirection);
        // SetDirection(direction != default ? direction : CardinalDirection);
        // No need to set direction for now...
    }

    /// <summary>
    /// Immediately updates the current sprite in the renderer and optionally fires any registered events
    /// </summary>
    /// <param name="invokeEvent">If true, this will fire any events that listen to this frame</param>
    public virtual void UpdateSprite(bool invokeEvent = false) => UpdateSprite(GetPlayingAnimation(playingAction, playingDirection), invokeEvent);

    /// <summary>
    /// Immediately updates the current sprite in the renderer and optionally fires any registered events
    /// </summary>
    /// <param name="customAnimation">The currently playing animation from which to extract the sprite</param>
    /// <param name="invokeEvent">If true, this will fire any events that listen to this frame</param>
    protected virtual void UpdateSprite(CustomAnimation customAnimation, bool invokeEvent = true)
    {
        if (!Frame.IsIndexOf(customAnimation?.GetSprites)) return;

        SetSprite(customAnimation.GetSprites[Frame]);

        if (invokeEvent && FrameEvents.TryGetValue((Action, customAnimation.Direction, Frame), out UnityAction unityAction))
            unityAction.Invoke();
    }

    protected virtual void SetSprite(Sprite sprite)
    {
        if (spriteRenderer) spriteRenderer.sprite = sprite;
        if (image) image.sprite = sprite;
        if (spriteMask) spriteMask.sprite = sprite;
    }

    protected virtual CustomAnimation GetPlayingAnimation(string action, string direction)
        => string.IsNullOrEmpty(direction) ? null : GetPlayingAnimation(action, new AbsoluteDirection(direction));

    protected virtual CustomAnimation GetPlayingAnimation(ref string action, ref string direction)
    {
        if (string.IsNullOrEmpty(direction)) return null;
        AbsoluteDirection cardinalDirection = new AbsoluteDirection(direction);
        CustomAnimation customAnimation = GetPlayingAnimation(ref action, ref cardinalDirection);
        direction = cardinalDirection.ToString();
        return customAnimation;
    }

    protected virtual CustomAnimation GetPlayingAnimation(string action, AbsoluteDirection direction) => GetPlayingAnimation(ref action, ref direction);

    protected virtual CustomAnimation GetPlayingAnimation(ref string action, ref AbsoluteDirection direction)
    {
        if (string.IsNullOrEmpty(action)) return null;

        if (combinedAlternativeAnimations.ContainsKey(action))
        {
            AlternativeAnimationCount = combinedAlternativeAnimations.CombinedAlternativeAnimationsDictionary[action].Count();
            return combinedAlternativeAnimations.CombinedAlternativeAnimationsDictionary[action][AlternativeAnimationIndex];
        }
        if (!customAnimationsDictionary.TryGetValue(action, out AnimationDirectionsDictionary directionsDictionary))
        {
            Log.Warning($"CustomAnimator ({gameObject.name}) could not find Action: ({action})", this);
            action = null;
            return null;
        }

        if (!directionsDictionary.TryGetValue(new AbsoluteDirection(direction), out CustomAnimation customAnimation))
        {
            if (!directionsDictionary.TryGetValue(new AbsoluteDirection(direction.FlipX()), out customAnimation))
            {
                Log.Warning($"CustomAnimator ({gameObject.name}) could not find Direction: ({direction}) for Action: ({action})", this);
                direction = default;
                return null;
            }
        }

        return customAnimation;
    }

    /// <summary>
    /// Gets how many frames a given action has
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    /// <param name="direction">The qualified direction</param>
    /// <returns>How many frames the action has if it's found, otherwise zero</returns>
    public virtual int GetActionFrameCount(string action, string direction) => GetPlayingAnimation(action, direction)?.FrameCount ?? 0;

    /// <summary>
    /// Gets how many frames a given action has
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    /// <param name="direction">The qualified direction</param>
    /// <returns>How many frames the action has if it's found, otherwise zero</returns>
    public virtual int GetActionFrameCount(string action, AbsoluteDirection direction) => GetPlayingAnimation(action, direction)?.FrameCount ?? 0;

    /// <summary>
    /// Gets the estimated duration of an animation based on frame count and frame rate
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    /// <param name="direction">The qualified direction</param>
    /// <returns>How long in seconds it should take to fully play this action(animation). (FrameCount/FrameRate)</returns>
    public virtual float GetActionPlayTime(string action, string direction) => GetActionFrameCount(action, direction) / CurrentFrameRate;

    /// <summary>
    /// Gets the estimated duration of an animation based on frame count and frame rate
    /// </summary>
    /// <param name="action">The qualified name of the action</param>
    /// <param name="direction">The qualified direction</param>
    /// <returns>How long in seconds it should take to fully play this action(animation). (FrameCount/FrameRate)</returns>
    public virtual float GetActionPlayTime(string action, AbsoluteDirection direction) => GetActionFrameCount(action, direction) / CurrentFrameRate;

    /// <summary>
    /// Gets the estimated duration of the currently playing animation based on frame count and frame rate
    /// </summary>
    /// <returns>How long in seconds it should take to fully play the currently playing action(animation). (FrameCount/FrameRate)</returns>
    public virtual float GetRunningActionPlayTime() => GetActionFrameCount(Action, Direction) / (float)CurrentFrameRate;

    /// <summary>
    /// Gets the estimated remaining duration of the currently playing animation based on frame count and frame rate
    /// </summary>
    /// <returns>How long in seconds it should take to finish the currently playing action(animation).</returns>
    public virtual float GetRemainingActionPlayTime() => ((GetActionFrameCount(Action, Direction) - Frame) / CurrentFrameRate) - elapsedTime;
}
