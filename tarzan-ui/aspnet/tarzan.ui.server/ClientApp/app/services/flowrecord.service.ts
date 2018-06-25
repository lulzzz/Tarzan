import { Injectable, Inject } from '@angular/core';  
import { Http, Response } from '@angular/http';  
import { Observable } from 'rxjs/Observable';   
import 'rxjs/add/operator/map';  
import 'rxjs/add/operator/catch';  
import 'rxjs/add/observable/throw';  
  
@Injectable()  
export class FlowRecordService {  
    myAppUrl: string = "";  
  
    constructor(private _http: Http, @Inject('BASE_URL') baseUrl: string) {  
        this.myAppUrl = baseUrl;  
    }  
  
    getFlowRecords() {  
        return this._http.get(this.myAppUrl + 'api/flows/index')  
            .map((response: Response) => response.json())  
            .catch(this.errorHandler);  
    }  
  
    getFlowRecordById(id: number) {  
        return this._http.get(this.myAppUrl + "api/flows/item/" + id)  
            .map((response: Response) => response.json())  
            .catch(this.errorHandler)  
    }  
    
    errorHandler(error: Response) {  
        console.log(error);  
        return Observable.throw(error);  
    }  
} 