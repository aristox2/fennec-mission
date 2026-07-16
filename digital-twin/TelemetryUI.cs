using UnityEngine;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
// TelemetryUI
// Single script that drives ALL telemetry text panels.
// Replaces AltitudeUIController, VelocityUIController, DriftUIController.
// Assign the labels you want in the Inspector — leave others blank to skip.
// ─────────────────────────────────────────────────────────────────────────────
public class TelemetryUI : MonoBehaviour
{
    [Header("Flight Controller")]
    public RocketFlightController fc;

    [Header("Primary Flight Data")]
    public TMP_Text altitudeText;
    public TMP_Text velocityText;
    public TMP_Text accelerationText;
    public TMP_Text pitchText;
    public TMP_Text flightTimeText;
    public TMP_Text phaseText;

    [Header("Environment")]
    public TMP_Text pressureText;
    public TMP_Text temperatureText;

    [Header("Avionics")]
    public TMP_Text drogueVoltageText;
    public TMP_Text mainVoltageText;
    public TMP_Text batteryVoltageText;

    [Header("GPS")]
    public TMP_Text latitudeText;
    public TMP_Text longitudeText;

    [Header("Peak Stats")]
    public TMP_Text peakAltitudeText;
    public TMP_Text peakVelocityText;

    // ── Phase colors ──────────────────────────────────────────────────────────
    private static readonly Color ColBoost  = new Color(1.00f, 0.42f, 0.21f); // orange
    private static readonly Color ColCoast  = new Color(0.22f, 0.74f, 0.97f); // blue
    private static readonly Color ColDrogue = new Color(0.96f, 0.28f, 0.71f); // pink
    private static readonly Color ColMain   = new Color(0.29f, 0.87f, 0.50f); // green
    private static readonly Color ColLanded = new Color(0.58f, 0.58f, 0.58f); // grey

    void Update()
    {
        if (fc == null) return;

        Set(altitudeText,      $"ALT   {fc.Altitude:F1} m  ({fc.Altitude * 3.28084f:F0} ft)");
        Set(velocityText,      $"VEL   {fc.Velocity:F1} m/s");
        Set(accelerationText,  $"ACCEL {fc.Acceleration:F1} m/s²  ({fc.AccelerationG:F2} G)");
        Set(pitchText,         $"PITCH {fc.Pitch:F1}°");
        Set(flightTimeText,    $"T+    {fc.FlightTime:F1} s");
        Set(pressureText,      $"PRESS {fc.Pressure:F1} hPa");
        Set(temperatureText,   $"TEMP  {fc.Temperature:F1} °C");
        Set(drogueVoltageText, $"DROGUE  {fc.DrogueVoltage:F2} V");
        Set(mainVoltageText,   $"MAIN    {fc.MainVoltage:F2} V");
        Set(batteryVoltageText,$"BATT    {fc.BatteryVoltage:F2} V");
        Set(latitudeText,      $"LAT  {fc.Latitude:F6}°");
        Set(longitudeText,     $"LON  {fc.Longitude:F6}°");
        Set(peakAltitudeText,  $"PEAK ALT  {fc.PeakAltitude:F0} m");
        Set(peakVelocityText,  $"PEAK VEL  {fc.PeakVelocity:F0} m/s");

        // Phase label + color
        if (phaseText != null)
        {
            phaseText.text = fc.CurrentPhase.ToString().ToUpper();
            phaseText.color = fc.CurrentPhase switch
            {
                RocketFlightController.Phase.Boost  => ColBoost,
                RocketFlightController.Phase.Fast   => ColBoost,
                RocketFlightController.Phase.Coast  => ColCoast,
                RocketFlightController.Phase.Drogue => ColDrogue,
                RocketFlightController.Phase.Main   => ColMain,
                RocketFlightController.Phase.Landed => ColLanded,
                _                                   => Color.white,
            };
        }
    }

    static void Set(TMP_Text label, string value)
    {
        if (label != null) label.text = value;
    }
}
