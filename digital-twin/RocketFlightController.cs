using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// RocketFlightController
// Drives the Fennec 1 digital twin from TeleMetrum CSV telemetry.
// All live telemetry is exposed as public properties for UI scripts to read.
// ─────────────────────────────────────────────────────────────────────────────
public class RocketFlightController : MonoBehaviour
{
    // ── Data point ────────────────────────────────────────────────────────────
    [System.Serializable]
    public class DataPoint
    {
        public float  time;
        public float  altitude;
        public float  pitch;
        public float  velocity;
        public float  acceleration;
        public float  accelerationG;
        public float  pressure;
        public float  temperature;
        public float  drogueVoltage;
        public float  mainVoltage;
        public float  batteryVoltage;
        public float  latitude;
        public float  longitude;
        public string state;
    }

    // ── Flight phase ──────────────────────────────────────────────────────────
    public enum Phase { PreLaunch, Boost, Fast, Coast, Drogue, Main, Landed }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("References")]
    public Transform rocket;

    [Header("Playback")]
    [Range(0.1f, 100f)]
    public float playbackSpeed = 1f;
    public bool  pauseOnAwake  = false;
    public bool  loop          = false;

    [Header("Rotation")]
    public Vector3 rotationAxis   = new Vector3(0f, 0f, 1f);
    public float   rotationOffset = 90f;

    [Header("Deployment Times (s)")]
    public float drogueTime = 48.84f;
    public float mainTime   = 326.15f;

    [Header("Debug")]
    public bool logEvents = true;

    // ── Live telemetry ────────────────────────────────────────────────────────
    public float  FlightTime      { get; private set; }
    public float  Altitude        { get; private set; }
    public float  Pitch           { get; private set; }
    public float  Velocity        { get; private set; }
    public float  Acceleration    { get; private set; }
    public float  AccelerationG   { get; private set; }
    public float  Pressure        { get; private set; }
    public float  Temperature     { get; private set; }
    public float  DrogueVoltage   { get; private set; }
    public float  MainVoltage     { get; private set; }
    public float  BatteryVoltage  { get; private set; }
    public float  Latitude        { get; private set; }
    public float  Longitude       { get; private set; }
    public string State           { get; private set; }
    public Phase  CurrentPhase    { get; private set; }
    public bool   IsPlaying       { get; private set; }

    // ── Flight stats ──────────────────────────────────────────────────────────
    public float Duration         { get; private set; }
    public float PeakAltitude     { get; private set; }
    public float PeakVelocity     { get; private set; }
    public float PeakAcceleration { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────
    public System.Action OnLaunch;
    public System.Action OnBurnout;
    public System.Action OnApogee;
    public System.Action OnDrogue;
    public System.Action OnMain;
    public System.Action OnLanded;

    // ── Private ───────────────────────────────────────────────────────────────
    private List<DataPoint> data          = new List<DataPoint>();
    private float           apogeeAlt     = 0f;
    private bool            drogueFired   = false;
    private bool            mainFired     = false;
    private bool            apogeePassed  = false;
    private bool            burnoutPassed = false;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        Application.targetFrameRate = 60;
        CurrentPhase = Phase.PreLaunch;
        State        = "preflight";
        IsPlaying    = false;

        LoadCSV("filtered");

        if (data.Count > 0)
        {
            Duration = data[data.Count - 1].time;
            foreach (var pt in data)
            {
                if (pt.altitude     > PeakAltitude)     PeakAltitude     = pt.altitude;
                if (pt.velocity     > PeakVelocity)     PeakVelocity     = pt.velocity;
                if (pt.acceleration > PeakAcceleration) PeakAcceleration = pt.acceleration;
            }

            if (logEvents)
                Debug.Log($"[Fennec1] {data.Count} pts | " +
                          $"T={Duration:F1}s | Alt={PeakAltitude:F0}m | " +
                          $"Vel={PeakVelocity:F0}m/s | " +
                          $"Accel={PeakAcceleration:F1}m/s² ({PeakAcceleration / 9.80665f:F1}G)");
        }

        if (!pauseOnAwake)
            StartCoroutine(Fly());
    }

    // ── CSV ───────────────────────────────────────────────────────────────────
    void LoadCSV(string file)
    {
        TextAsset csv = Resources.Load<TextAsset>(file);
        if (csv == null) { Debug.LogError($"[Fennec1] CSV not found: {file}"); return; }

        // Columns (0-indexed):
        // 0:time  1:altitude  2:pitch  3:velocity  4:acceleration  5:acceleration_g
        // 6:pressure  7:temperature  8:drogue_voltage  9:main_voltage
        // 10:battery_voltage  11:latitude  12:longitude  13:state

        var    reader  = new StringReader(csv.text);
        string line;
        bool   header  = true;
        int    skipped = 0;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (header) { header = false; continue; }

            string[] c = line.Split(',');
            if (c.Length < 14) { skipped++; continue; }

            try
            {
                data.Add(new DataPoint
                {
                    time           = F(c[0]),
                    altitude       = F(c[1]),
                    pitch          = F(c[2]),
                    velocity       = F(c[3]),
                    acceleration   = F(c[4]),
                    accelerationG  = F(c[5]),
                    pressure       = F(c[6]),
                    temperature    = F(c[7]),
                    drogueVoltage  = F(c[8]),
                    mainVoltage    = F(c[9]),
                    batteryVoltage = F(c[10]),
                    latitude       = F(c[11]),
                    longitude      = F(c[12]),
                    state          = c[13].Trim(),
                });
            }
            catch { skipped++; }
        }

        if (skipped > 0 && logEvents)
            Debug.LogWarning($"[Fennec1] Skipped {skipped} bad rows.");

        Debug.Log($"[Fennec1] Loaded {data.Count} rows.");
    }

    static float F(string s) =>
        float.Parse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

    // ── Controls ──────────────────────────────────────────────────────────────
    public void Play()                    { if (!IsPlaying) StartCoroutine(Fly()); }
    public void Pause()                   => IsPlaying = false;
    public void SetSpeed(float s)         => playbackSpeed = Mathf.Clamp(s, 0.1f, 100f);

    // ── Flight loop ───────────────────────────────────────────────────────────
    IEnumerator Fly()
    {
        if (data.Count == 0) { Debug.LogError("[Fennec1] No data."); yield break; }

        drogueFired = mainFired = apogeePassed = burnoutPassed = false;
        apogeeAlt   = 0f;
        IsPlaying   = true;
        CurrentPhase = Phase.Boost;
        OnLaunch?.Invoke();
        if (logEvents) Debug.Log("[Fennec1] LAUNCH");

        for (int i = 0; i < data.Count - 1; i++)
        {
            if (!IsPlaying) yield return new WaitUntil(() => IsPlaying);
            if (rocket == null)
            {
                Debug.LogError("[Fennec1] rocket Transform not assigned!");
                yield break;
            }

            DataPoint a = data[i];
            DataPoint b = data[i + 1];

            float step    = Mathf.Max(b.time - a.time, 0.001f) / Mathf.Max(playbackSpeed, 0.01f);
            float elapsed = 0f;

            while (elapsed < step)
            {
                float t = Mathf.Clamp01(elapsed / step);

                // Position — vertical only
                rocket.position = new Vector3(0f, Mathf.Lerp(a.altitude, b.altitude, t), 0f);

                // Rotation
                float p = Mathf.LerpAngle(a.pitch, b.pitch, t);
                rocket.rotation = Quaternion.AngleAxis(rotationOffset - p, rotationAxis);

                // Telemetry
                FlightTime     = Mathf.Lerp(a.time,           b.time,           t);
                Altitude       = Mathf.Lerp(a.altitude,       b.altitude,       t);
                Pitch          = p;
                Velocity       = Mathf.Lerp(a.velocity,       b.velocity,       t);
                Acceleration   = Mathf.Lerp(a.acceleration,   b.acceleration,   t);
                AccelerationG  = Mathf.Lerp(a.accelerationG,  b.accelerationG,  t);
                Pressure       = Mathf.Lerp(a.pressure,       b.pressure,       t);
                Temperature    = Mathf.Lerp(a.temperature,    b.temperature,    t);
                DrogueVoltage  = Mathf.Lerp(a.drogueVoltage,  b.drogueVoltage,  t);
                MainVoltage    = Mathf.Lerp(a.mainVoltage,    b.mainVoltage,    t);
                BatteryVoltage = Mathf.Lerp(a.batteryVoltage, b.batteryVoltage, t);
                Latitude       = Mathf.Lerp(a.latitude,       b.latitude,       t);
                Longitude      = Mathf.Lerp(a.longitude,      b.longitude,       t);
                State          = a.state;

                if (Altitude > apogeeAlt) apogeeAlt = Altitude;

                CheckEvents();

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        Apply(data[data.Count - 1]);
        CurrentPhase = Phase.Landed;
        IsPlaying    = false;
        OnLanded?.Invoke();
        if (logEvents) Debug.Log("[Fennec1] LANDED.");

        if (loop) { yield return new WaitForSeconds(2f); Start(); }
    }

    void Apply(DataPoint pt)
    {
        if (rocket != null)
        {
            rocket.position = new Vector3(0f, pt.altitude, 0f);
            rocket.rotation = Quaternion.AngleAxis(rotationOffset - pt.pitch, rotationAxis);
        }
        FlightTime     = pt.time;
        Altitude       = pt.altitude;
        Pitch          = pt.pitch;
        Velocity       = pt.velocity;
        Acceleration   = pt.acceleration;
        AccelerationG  = pt.accelerationG;
        Pressure       = pt.pressure;
        Temperature    = pt.temperature;
        DrogueVoltage  = pt.drogueVoltage;
        MainVoltage    = pt.mainVoltage;
        BatteryVoltage = pt.batteryVoltage;
        Latitude       = pt.latitude;
        Longitude      = pt.longitude;
        State          = pt.state;
    }

    void CheckEvents()
    {
        switch (State)
        {
            case "boost":  CurrentPhase = Phase.Boost;  break;
            case "fast":   CurrentPhase = Phase.Fast;   break;
            case "coast":  CurrentPhase = Phase.Coast;  break;
            case "drogue": CurrentPhase = Phase.Drogue; break;
            case "main":   CurrentPhase = Phase.Main;   break;
        }

        if (!burnoutPassed &&
            (CurrentPhase == Phase.Coast || CurrentPhase == Phase.Fast))
        {
            burnoutPassed = true;
            OnBurnout?.Invoke();
            if (logEvents) Debug.Log($"[Fennec1] BURNOUT T+{FlightTime:F1}s");
        }

        if (!drogueFired && FlightTime >= drogueTime)
        {
            drogueFired = true;
            OnDrogue?.Invoke();
            if (logEvents) Debug.Log($"[Fennec1] DROGUE T+{FlightTime:F2}s Alt:{Altitude:F0}m");
        }

        if (!apogeePassed && drogueFired && Altitude < apogeeAlt - 50f)
        {
            apogeePassed = true;
            OnApogee?.Invoke();
            if (logEvents) Debug.Log($"[Fennec1] APOGEE {apogeeAlt:F0}m");
        }

        if (!mainFired && FlightTime >= mainTime)
        {
            mainFired = true;
            OnMain?.Invoke();
            if (logEvents) Debug.Log($"[Fennec1] MAIN T+{FlightTime:F2}s Alt:{Altitude:F0}m");
        }
    }
}
