# payload avionics

custom sensor suite carried on both fennec i and fennec ii. baro, 3-axis accelerometer, 3-axis gyro, temperature, humidity, oxidizer pressure, voltage. logged to onboard storage; xbee downlink to the ground station was intended but did not receive in flight.

## the files here

- `omega_transducer.ino` — arduino sketch for the omega px1600 pressure transducer (0.5–4.5v analog, 1000 psi range). handles adc read at 14-bit resolution, voltage → psi conversion, and a first-order temperature-drift correction term (0.05% FS/°C on both zero and span). ran on both fennec i and fennec ii for oxidizer tank pressure sensing.
- `fennec_i_payload_data.xlsx` — recovered payload log from fennec i (4,335 samples, 17 columns). schema: timestamp fields (day/hour/minute/second/microsecond), baro pressure (hpa), accel (x/y/z), gyro (x/y/z), humidity, temperature, oxidizer pressure (psi), voltage, altitude (m).

## why the temperature-drift correction matters

the omega px1600 spec sheet gives per-degree drift coefficients but they aren't applied inside the sensor — you get raw voltage out and any drift shows up as apparent pressure error at temperature extremes. on a rocket, the payload bay goes from ground temp to substantially colder in tens of seconds. the sketch computes a `drift_psi` estimate alongside each reading so downstream analysis can bound the sensor error rather than treat readings as ground-truth.

## known limitations

- 4 hz sample rate is fine for tank-pressure monitoring but too slow for combustion-transient work
- serial output at 9600 baud, not designed to be a live-downlink source
- no in-flight calibration — the `NOMINAL_TEMP_C` constant is a compile-time assumption
