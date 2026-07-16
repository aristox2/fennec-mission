# flight data — fennec ii

two independent altus metrum recorders on the flight-safety chain. both recovered post-flight. **live telemetry via telemetrum's radio did not reach the ground station** — the data here came from onboard storage.

## the files here

- `fennec_ii_telemetrum.csv` — altus metrum telemetrum, primary altimeter with gps and telemetry radio. 10,835 samples, ~355 s covered. includes gps fix (lat/lon/altitude), satellite visibility, and pad-relative range/azimuth/elevation in addition to baro/accel.
- `fennec_ii_easymini.csv` — altus metrum easymini, backup altimeter. baro-only, 8,181 samples, ~354 s.

## headline numbers

| metric | telemetrum | easymini |
|---|---|---|
| apogee (ft agl) | 34,755 | 35,050 |
| peak velocity (m/s) | 510 | 636 |
| peak velocity (mach @ sl) | 1.49 | 1.85 |
| peak acceleration (m/s²) | 196 | 244 |

**the two altimeters disagree on peak velocity by ~24%.** this is a reconciliation problem. see `fennec-mission/README.md` for what's known about the source of the disagreement and the planned ukf work to resolve it.

## column reference

altus metrum's own documentation covers the full schema. quick notes on what's used most:

- `time` — seconds from ignition (negative values are pre-launch)
- `state_name` — flight state machine (pad, boost, coast, drogue, main, landed)
- `height` — altitude above pad, meters
- `altitude` — msl altitude, meters
- `speed` — vertical velocity (baro-derived on easymini; baro + accel-integrated on telemetrum), m/s
- `acceleration` — vertical acceleration, m/s²
