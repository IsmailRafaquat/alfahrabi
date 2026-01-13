import { Rest, RestService } from "@abp/ng.core";
import { Injectable } from "@angular/core";
import { StaffDocumentDto } from "src/app/proxy/staff-documents";

@Injectable({
    providedIn: 'root'
})
export class CustomStaffDocumentService {
    apiName = 'Default';

    constructor(private restService: RestService) {}

    uploadFormData = (formData: FormData, config?: Partial<Rest.Config>) => {
        return this.restService.request<any, StaffDocumentDto>(
            {
                method: 'POST',
                url: '/api/app/staff-documents/upload',
                body: formData,
            },
            { apiName: this.apiName, ...config }
        );
    }

}