import { saveAs } from 'file-saver';

// Narrow Web-Share/wa.me-fallback logic extracted from the html2canvas->jsPDF WhatsApp-share flow
// that already existed independently on Cash Closing, Customer Ledger, and Supplier Ledger
// (message text/filename stay page-local business logic - this only handles the share mechanics,
// which are identical across all three). Sale Invoice never used this and still doesn't.
export async function shareOrOpenWhatsApp(blob: Blob, fileName: string, message: string): Promise<void> {
  const file = new File([blob], fileName, { type: 'application/pdf' });
  const nav = navigator as Navigator & {
    canShare?: (data: { files: File[] }) => boolean;
    share?: (data: { files: File[]; text?: string }) => Promise<void>;
  };

  if (nav.canShare && nav.canShare({ files: [file] }) && nav.share) {
    try {
      await nav.share({ files: [file], text: message });
      return;
    } catch {
      // User cancelled or share failed - fall through to the download + wa.me fallback below.
    }
  }

  saveAs(blob, fileName);
  window.open(`https://wa.me/?text=${encodeURIComponent(message)}`, '_blank');
}
