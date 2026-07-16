# ground station

raspberry pi + xbee receiver rig for the fennec program. built for live downlink of the custom payload's telemetry stream; bench-tested extensively; **did not receive in flight** on either fennec i or fennec ii.

## what was here

- raspberry pi 4 (ubuntu server on ssd), hostname `bravob`
- xbee module on `/dev/ttyUSB0`, 115.2k baud
- python receiver script running as a systemd service under a dedicated user
- direct-ethernet connection to the analyst laptop with static ip on both ends (no wireless, no wan-side exposure during operations)

## the files here

- `xbee_receiver.py.template` — reference implementation of the receiver script. **not** the original in-service code (which wasn't preserved); this reconstructs the intended behavior for anyone rebuilding a similar rig.
- `fennec-receiver.service.template` — the systemd unit that ran the receiver.

## what went wrong

live reception failed on both fennec i and fennec ii despite passing every ground/bench test we could design. suspected causes:

1. **frequency interference** at the launch site
2. **insufficient transmit power** on the rocket-side xbee for the range achieved

we did not fully isolate the cause. a separate team at the same range experienced identical symptoms with independently-verified bench-testing — suggesting the failure mode was environmental rather than specific to our rig.

data recovery came from the **onboard flight recorders** (altus metrum telemetrum + easymini for the flight-safety chain, and the custom payload avionics for sensor data), not from the ground station. see `../flight_data/` and `../payload/` for the recovered datasets.

## how to reproduce the rig

1. flash ubuntu server 22.04 to an ssd, boot the pi from it
2. install `pyserial`: `sudo apt install python3-serial`
3. create a dedicated user (`fennec`), place the receiver script at `/opt/fennec/xbee_receiver.py`
4. drop the systemd unit into `/etc/systemd/system/`, enable and start it
5. verify with `journalctl -u fennec-receiver -f`
6. **before every launch:** run an end-to-end test with the rocket-side transmitter at full range. we passed short-range tests every time and still lost the flight downlink — do more than we did.
