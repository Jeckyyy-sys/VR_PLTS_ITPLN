using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public sealed class InverterSimulation : MonoBehaviour
{
    [Header("Assumed source availability for the learning simulation")]
    public bool batteryAvailable = true;
    public bool pvAvailable = true;
    public bool externalAcAvailable;
    public TMP_Text lcd;
    public TMP_Text statusBoard;
    public TMP_Text[] switchLabels;
    public Transform[] switchHandles;
    public GameObject loadIndicator;
    public Transform motorRotor;
    [Header("Illustrative model values, not device specifications")]
    [Min(1f)] public float nominalVoltage = 230f;
    [Min(1f)] public float frequencyHz = 50f;
    [Min(2)] public int motorPoles = 4;
    [Range(50f, 1000f)] public float requestedLoadWatts = 100f;
    [Range(0.1f, 1f)] public float powerFactor = 0.85f;

    public float OutputVoltage { get; private set; }
    public float OutputCurrent { get; private set; }
    public float OutputWatts { get; private set; }
    public double EnergyKwh { get; private set; }
    public float MotorRpm { get; private set; }
    public int DisplayPage { get; private set; }

    public bool BatteryConnected { get; private set; }
    public bool PvConnected { get; private set; }
    public bool AcOutputConnected { get; private set; }
    public bool AcInputConnected { get; private set; }
    public bool Powered { get; private set; }
    public bool OutputActive { get; private set; }

    private bool powerHeld;
    private float powerPressedAt;
    private bool previousSourceAvailable;
    private float nextDisplayTime;
    private string message = "Aktifkan BATT, tahan POWER 1 detik, lalu aktifkan AC OUT.";

    private bool SourceAvailable =>
        (BatteryConnected && batteryAvailable) || (AcInputConnected && externalAcAvailable);

    private void Start()
    {
        RefreshOutputs();
    }

    private void Update()
    {
        if (SourceAvailable != previousSourceAvailable)
            RefreshOutputs();

        if (powerHeld && Time.unscaledTime - powerPressedAt >= 1f)
        {
            powerHeld = false;
            Powered = !Powered && SourceAvailable;
            message = SourceAvailable
                ? (Powered ? "Inverter aktif. AC OUT menghubungkan beban." : "Inverter dimatikan.")
                : "Sumber belum tersedia. Aktifkan BATT pada simulasi ini.";
            RefreshOutputs();
        }

        UpdateMeasurements(Time.deltaTime);
        if (motorRotor != null)
            motorRotor.Rotate(0f, 0f, MotorRpm * 6f * Time.deltaTime, Space.Self);
        if (Time.unscaledTime >= nextDisplayTime)
        {
            UpdateDisplay();
            nextDisplayTime = Time.unscaledTime + 0.1f;
        }
    }

    public void NextDisplay(SelectEnterEventArgs args)
    {
        DisplayPage = (DisplayPage + 1) % 4;
        UpdateDisplay();
    }

    public void IncreaseLoad(SelectEnterEventArgs args)
    {
        requestedLoadWatts = Mathf.Clamp(requestedLoadWatts + 50f, 50f, 1000f);
        RefreshOutputs();
    }

    public void DecreaseLoad(SelectEnterEventArgs args)
    {
        requestedLoadWatts = Mathf.Clamp(requestedLoadWatts - 50f, 50f, 1000f);
        RefreshOutputs();
    }

    private void UpdateMeasurements(float seconds)
    {
        requestedLoadWatts = Mathf.Clamp(requestedLoadWatts, 50f, 1000f);
        OutputVoltage = OutputActive ? Mathf.Max(1f, nominalVoltage) : 0f;
        OutputWatts = OutputActive ? requestedLoadWatts : 0f;
        OutputCurrent = OutputActive
            ? OutputWatts / (OutputVoltage * Mathf.Clamp(powerFactor, 0.1f, 1f))
            : 0f;
        EnergyKwh += OutputWatts * (double)seconds / 3600000d;
        float slip = Mathf.Lerp(0.01f, 0.05f, requestedLoadWatts / 1000f);
        float targetRpm = OutputActive
            ? 120f * Mathf.Max(1f, frequencyHz) / Mathf.Max(2, motorPoles) * (1f - slip)
            : 0f;
        MotorRpm = Mathf.MoveTowards(MotorRpm, targetRpm, 600f * seconds);
    }

    public void BeginPowerHold(SelectEnterEventArgs args)
    {
        powerHeld = true;
        powerPressedAt = Time.unscaledTime;
    }

    public void EndPowerHold(SelectExitEventArgs args)
    {
        powerHeld = false;
    }

    public void ToggleBattery(SelectEnterEventArgs args)
    {
        BatteryConnected = !BatteryConnected;
        RefreshOutputs();
    }

    public void TogglePv(SelectEnterEventArgs args)
    {
        PvConnected = !PvConnected;
        RefreshOutputs();
    }

    public void ToggleAcOutput(SelectEnterEventArgs args)
    {
        AcOutputConnected = !AcOutputConnected;
        RefreshOutputs();
    }

    public void ToggleAcInput(SelectEnterEventArgs args)
    {
        AcInputConnected = !AcInputConnected;
        RefreshOutputs();
    }

    public void RefreshOutputs()
    {
        if (!SourceAvailable && Powered)
        {
            Powered = false;
            message = "Sumber terputus. Aktifkan sumber lalu tahan POWER untuk mulai kembali.";
        }

        previousSourceAvailable = SourceAvailable;
        OutputActive = Powered && AcOutputConnected;
        if (loadIndicator != null)
            loadIndicator.SetActive(OutputActive);

        SetSwitch(0, "BATT", BatteryConnected);
        SetSwitch(1, "PV", PvConnected);
        SetSwitch(2, "AC OUT", AcOutputConnected);
        SetSwitch(3, "AC IN", AcInputConnected);

        UpdateMeasurements(0f);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (lcd != null)
        {
            switch (DisplayPage)
            {
                case 0: lcd.text = EnergyKwh.ToString("F6") + "\nkWh"; break;
                case 1: lcd.text = OutputVoltage.ToString("F1") + "\nV AC"; break;
                case 2: lcd.text = OutputCurrent.ToString("F2") + "\nA AC"; break;
                default: lcd.text = MotorRpm.ToString("F0") + "\nRPM"; break;
            }
        }

        if (statusBoard != null)
        {
            string pv = PvConnected && pvAvailable ? "TERSEDIA" : "TERPUTUS";
            string ac = AcInputConnected && externalAcAvailable ? "TERSEDIA" : "TIDAK TERSEDIA";
            statusBoard.text = "SIMULASI INVERTER\n\n"
                + "Inverter  " + (Powered ? "ON" : "OFF") + "\n"
                + "Baterai  " + (BatteryConnected && batteryAvailable ? "TERHUBUNG" : "TERPUTUS") + "\n"
                + "PV  " + pv + "\nAC eksternal  " + ac + "\n"
                + "Keluaran AC  " + (OutputActive ? "AKTIF" : "MATI") + "\n\n"
                + $"{OutputVoltage:F1} V   |   {OutputCurrent:F2} A\n"
                + $"{OutputWatts:F0} W   |   {EnergyKwh:F6} kWh\n"
                + $"Motor {MotorRpm:F0} RPM\nBeban dipilih {requestedLoadWatts:F0} W\n\n"
                + "UP/DOWN: beban +/-50 W\nFUNCTION: ganti halaman LCD\n\n"
                + message + "\n\nAngka model belajar. "
                + $"Asumsi {nominalVoltage:F0} V, {frequencyHz:F0} Hz, {motorPoles} kutub, PF {powerFactor:F2}. "
                + "RPM milik motor simulasi. kWh dihitung selama Play Mode. "
                + "Pengisian baterai dan proteksi perangkat belum dimodelkan.";
        }
    }

    private void SetSwitch(int index, string label, bool enabled)
    {
        if (switchLabels != null && index < switchLabels.Length && switchLabels[index] != null)
            switchLabels[index].text = label + "\n" + (enabled ? "ON" : "OFF");
        if (switchHandles != null && index < switchHandles.Length && switchHandles[index] != null)
            switchHandles[index].localRotation = Quaternion.Euler(0f, 0f, enabled ? 28f : -28f);
    }

    private void OnDisable()
    {
        powerHeld = false;
        Powered = false;
        OutputActive = false;
        OutputVoltage = 0f;
        OutputCurrent = 0f;
        OutputWatts = 0f;
        MotorRpm = 0f;
        if (loadIndicator != null)
            loadIndicator.SetActive(false);
    }
}
