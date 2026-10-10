namespace Tebing.Constants;

public static class FileUploadSettings
{
    // Batas ukuran bukti transfer, dicek di PaymentService (pesan error rapi ke user).
    public const long PaymentProofMaxBytes = 2 * 1024 * 1024;

    // Batas body request di level server — sengaja lebih longgar dari batas file di atas, biar file
    // yang kegedean dikit masih nyampe ke service dan dapet pesan "maksimal 2 MB", bukan error 413 mentah.
    public const long UploadRequestMaxBytes = 10 * 1024 * 1024;
}
