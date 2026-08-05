import { Component } from '@angular/core';

@Component({
  selector: 'app-system-guide-button',
  standalone: false,
  templateUrl: './system-guide-button.component.html',
  styleUrl: './system-guide-button.component.scss',
})
export class SystemGuideButtonComponent {
  open = false;

  toggle(): void {
    this.open = !this.open;
  }

  close(): void {
    this.open = false;
  }
}
