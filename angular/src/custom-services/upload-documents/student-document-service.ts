import { Rest, RestService } from "@abp/ng.core";
import { Injectable } from "@angular/core";
import { StudentDocumentDto } from "../../app/proxy/student-documents/models";

@Injectable({
    providedIn: 'root'
})
export class CustomStudentDocumentService {
    apiName = 'Default';

    constructor(private restService: RestService) {}

    uploadFormData = (formData: FormData, config?: Partial<Rest.Config>) => {
        return this.restService.request<any, StudentDocumentDto>(
            {
                method: 'POST',
                url: '/api/app/student-documents/upload',
                body: formData,
            },
            { apiName: this.apiName, ...config }
        );
    }

}