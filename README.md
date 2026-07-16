# fennec
a two-vehicle sounding rocket program (goddard, 2024–2026). transonic flight, custom payload avionics, planned live telemetry that didn't reach the ground on either flight. 
this repo covers the software, telemetry rig, recovered flight data, and analysis work.

## the story in one paragraph
 
the Fennec II mission was the only vehicle that flew in 2026, as Fennec I expirineced pressurization errors that faulted our propulsion system on range in 2025. neither established a live telemetry link to the pi ground station despite extensive bench testing — the suspected cause is frequency interference or insufficient rocket-side transmit power at the range achieved (a separate team at the same range had identical symptoms). 
the data we have came from onboard recorders: altus metrum telemetrum + easymini on the flight-safety chain, and a custom payload logging baro / imu / oxidizer pressure / environmentals. on fennec ii, the two independent altimeters disagree on peak velocity by ~24% — a transonic-region artifact that motivates the ukf reconciliation work currently in progress.
