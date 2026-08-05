import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoreModule } from '@abp/ng.core';
import { SystemGuideButtonComponent } from './system-guide-button.component';
import { SystemGuideModalComponent } from './system-guide-modal.component';

@NgModule({
  declarations: [SystemGuideButtonComponent, SystemGuideModalComponent],
  imports: [CommonModule, FormsModule, CoreModule],
  exports: [SystemGuideButtonComponent],
})
export class SystemGuideModule {}
