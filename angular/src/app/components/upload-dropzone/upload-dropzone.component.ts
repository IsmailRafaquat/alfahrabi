import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';

@Component({
  selector: 'app-upload-dropzone',
  templateUrl: './upload-dropzone.component.html',
  styleUrls: ['./upload-dropzone.component.scss'],
})
export class UploadDropzoneComponent {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  @Input() accept: string | undefined;
  @Input() disabled = false;

  // Sirf display ke liye file name parent se ayega
  @Input() fileName: string | null = null;

  @Output() fileSelected = new EventEmitter<File | null>();

  isDragOver = false;

  onZoneClick(): void {
    if (this.disabled) return;
    this.fileInput.nativeElement.click();
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files[0];
    this.fileSelected.emit(file ?? null);
  }

  onDragOver(event: DragEvent): void {
    if (this.disabled) return;
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    if (this.disabled) return;
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    if (this.disabled) return;

    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    if (event.dataTransfer?.files?.length) {
      const file = event.dataTransfer.files[0];
      this.fileSelected.emit(file);

      // optional: sync hidden input
      if (this.fileInput?.nativeElement) {
        const dt = new DataTransfer();
        dt.items.add(file);
        this.fileInput.nativeElement.files = dt.files;
      }

      event.dataTransfer.clearData();
    }
  }
}
