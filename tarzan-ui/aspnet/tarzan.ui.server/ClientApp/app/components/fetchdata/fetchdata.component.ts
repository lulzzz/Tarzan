import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';  
import { FlowRecordService } from '../../services/flowrecord.service';

@Component({
    selector: 'fetchdata',
    templateUrl: './fetchdata.component.html'
})
export class FetchDataComponent {
    public flowRecords: FlowRecord[] | undefined;
    constructor(public http: Http, private _router: Router, private _flowRecordService: FlowRecordService) {  
        this.getFlowRecords();  
    }  
  
    getFlowRecords() {  
        this._flowRecordService.getFlowRecords().subscribe(  
            data => this.flowRecords = (data as FlowRecord[])
        )  
    }  
}

interface FlowRecord {
    id: number;
    protocol: string;
    sourceAddress: string;
    sourcePort: number;
    destinationAddress: string;
    destinationPort: number;
    firstSeen: string;
    lastSeen: string;
    packets: number;
    octets: string;
}
