// Omega Transducer 1600

#include "Wire.h"
#include "math.h"
#include "stdint.h"

#define ANALOG_READ_RESOLUTION_BITS 14
#define BAUD_RATE 9600
#define SENSOR_DELAY_MS 250

#define NOMINAL_TEMP_C 25.0     // Calibration temp
#define TEMP_COEFF_ZERO 0.0005  // 0.05% FS/°C
#define TEMP_COEFF_SENS 0.0005  // 0.05% FS/°C

float TEMP = 20.5;  // Room temperature in Celsius (e.g., 70°F ≈ 21.1°C)
typedef struct {
  float V_LOW;
  float V_HI;
  float PRESSURE_RANGE;
  int AI_PIN;
} PRESSURE_TRANSDUCER;

PRESSURE_TRANSDUCER pt1 = {
  0.5,   // V_LOW
  4.5,   // V_HI
  1000,  // PRESSURE_RANGE in PSI
  A0     // AI_PIN
};

typedef struct {
  uint16_t raw;
  float voltage;
  float pressure_psi;
  float drift_psi;
} DATA;

DATA readData(PRESSURE_TRANSDUCER pt) {
  uint16_t raw = analogRead(pt.AI_PIN);
  float voltage = raw * (5.0 / pow(2, ANALOG_READ_RESOLUTION_BITS));

  float pressure_psi = pt.PRESSURE_RANGE * ((voltage - pt.V_LOW) / (pt.V_HI - pt.V_LOW));
  if (pressure_psi < 0) pressure_psi = 0;

  float temp_delta = TEMP - NOMINAL_TEMP_C;
  float drift_psi = pt.PRESSURE_RANGE * (fabs(temp_delta) * (TEMP_COEFF_ZERO + TEMP_COEFF_SENS));

  return {
    raw,
    voltage,
    pressure_psi,
    drift_psi
  };
}

void printData(DATA data) {
  Serial.print("Raw: ");
  Serial.print(data.raw);
  Serial.print(" | Voltage: ");
  Serial.print(data.voltage, 3);
  Serial.print(" V | Pressure: ");
  Serial.print(data.pressure_psi, 2);
  Serial.print(" PSI ±");
  Serial.print(data.drift_psi, 2);
  Serial.println(" PSI (temp error)");
}

void setup() {
  Serial.begin(BAUD_RATE);
}

void loop() {
  analogReadResolution(ANALOG_READ_RESOLUTION_BITS);
  delay(10);

  DATA data_1 = readData(pt1);

  Serial.print("Sensor 1 -> ");
  printData(data_1);

  Serial.println("------------------");
  delay(SENSOR_DELAY_MS);
}

