using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
// PlaybackController
// Optional UI script for play/pause and speed control buttons.
// Wire up buttons in the Inspector if you want playback controls in-scene.
// ─────────────────────────────────────────────────────────────────────────────
public class PlaybackController : MonoBehaviour
{
    [Header("Flight Controller")]
    public RocketFlightController fc;

    [Header("Buttons (optional)")]
    public Button playPauseButton;
    public Button speed1x;
    public Button speed5x;
    public Button speed10x;
    public Button speed25x;
    public Button speed50x;

    [Header("Labels (optional)")]
    public TMP_Text playPauseLabel;
    public TMP_Text speedLabel;

    void Start()
    {
        playPauseButton?.onClick.AddListener(TogglePlayPause);
        speed1x?.onClick.AddListener(()  => SetSpeed(1f));
        speed5x?.onClick.AddListener(()  => SetSpeed(5f));
        speed10x?.onClick.AddListener(() => SetSpeed(10f));
        speed25x?.onClick.AddListener(() => SetSpeed(25f));
        speed50x?.onClick.AddListener(() => SetSpeed(50f));
    }

    void Update()
    {
        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Space)) TogglePlayPause();
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpeed(1f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpeed(5f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpeed(10f);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetSpeed(25f);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetSpeed(50f);

        // Update labels
        if (playPauseLabel != null && fc != null)
            playPauseLabel.text = fc.IsPlaying ? "⏸" : "▶";

        if (speedLabel != null && fc != null)
            speedLabel.text = $"{fc.playbackSpeed:F0}x";
    }

    void TogglePlayPause()
    {
        if (fc == null) return;
        if (fc.IsPlaying) fc.Pause();
        else              fc.Play();
    }

    void SetSpeed(float s)
    {
        if (fc == null) return;
        fc.SetSpeed(s);
        if (speedLabel != null) speedLabel.text = $"{s:F0}x";
    }
}
