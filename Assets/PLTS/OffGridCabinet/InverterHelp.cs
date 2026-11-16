using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public sealed class InverterHelp : MonoBehaviour
{
    public GameObject explanationPanel;
    public TMP_Text explanation;

    public void ShowOverview(SelectEnterEventArgs args)
    {
        Show("PETUNJUK PANEL INVERTER",
            "ON/OFF\nTahan 1 detik untuk menyalakan atau mematikan inverter.\n\n"
            + "UP / DOWN\nPada simulasi ini, pilih UP untuk menambah beban 50 W dan DOWN untuk mengurangi beban 50 W.\n\n"
            + "FUNCTION\nPada simulasi ini, pilih FUNCTION untuk mengganti halaman LCD antara kWh, tegangan, arus, dan RPM motor.");
    }

    public void ShowFunction(SelectEnterEventArgs args)
    {
        Show("FUNCTION", "Petunjuk pada perangkat\n\nTahan 1 detik untuk mengaktifkan atau menonaktifkan alarm.\n\n"
            + "Tahan 3 detik untuk masuk ke pengaturan AC charge, working mode, dan battery type. Gunakan UP / DOWN untuk menyesuaikan nilai.");
    }

    public void ShowUp(SelectEnterEventArgs args)
    {
        Show("UP", "Petunjuk pada perangkat\n\nTahan 1 detik untuk berpindah halaman parameter, misalnya AC output atau AC input.\n\n"
            + "Saat berada di pengaturan, tombol UP digunakan untuk menyesuaikan nilai parameter.");
    }

    public void ShowDown(SelectEnterEventArgs args)
    {
        Show("DOWN", "Petunjuk pada perangkat\n\nTahan 1 detik untuk berpindah halaman parameter, misalnya AC output atau AC input.\n\n"
            + "Saat berada di pengaturan, tombol DOWN digunakan untuk menyesuaikan nilai parameter.");
    }

    public void Close(SelectEnterEventArgs args)
    {
        explanationPanel.SetActive(false);
    }

    private void Show(string heading, string body)
    {
        explanation.text = heading + "\n\n" + body
            + "\n\nFUNCTION dan UP/DOWN mengoperasikan tampilan serta beban simulasi. "
            + "Pengaturan parameter dan alarm belum dijalankan.\n\nPilih TUTUP untuk kembali.";
        explanationPanel.SetActive(true);
    }
}

