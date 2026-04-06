import { CoreModule } from '@abp/ng.core';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { NgModule } from '@angular/core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzStepsModule } from 'ng-zorro-antd/steps';
import { UploadDropzoneComponent } from '../components/upload-dropzone/upload-dropzone.component';
import { NzTagModule } from 'ng-zorro-antd/tag';

@NgModule({
  declarations: [],
  imports: [
    CoreModule,
    ThemeSharedModule,
    NgbDropdownModule,
    NgxValidateCoreModule,  
    NzSelectModule,
    NzStepsModule,
    UploadDropzoneComponent,
    NzTableModule,
    NzTagModule
  ],
  exports: [
    CoreModule,
    ThemeSharedModule,
    NgbDropdownModule,
    NgxValidateCoreModule,
    NzSelectModule,
    NzStepsModule,
    UploadDropzoneComponent,
    NzTableModule,
    NzTagModule
    
  ],
  providers: []
})
export class SharedModule {}
