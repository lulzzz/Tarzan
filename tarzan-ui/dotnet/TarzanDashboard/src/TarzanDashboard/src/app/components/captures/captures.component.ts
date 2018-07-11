import { Component, OnInit } from '@angular/core';
import { Capture, CapturesService } from 'src/api';


const TEST_DATA = [{"id":1,"name":"testbed-11jun.pcap","type":"pcap","size":17306938543,"createdOn": new Date("2016-01-21T18:57:51"),"uploadOn": new Date("2018-07-11T10:09:47.9398066+02:00"),"hash":null,"author":"Alice Smith","notes":"","tags":[]}];

@Component({
  selector: 'app-captures',
  templateUrl: './captures.component.html',
  styleUrls: ['./captures.component.css']
})
export class CapturesComponent implements OnInit {
  dataSource : Capture[] = TEST_DATA;
  current : Capture = null;
  constructor(private capturesService: CapturesService) 
  { 
  }

  ngOnInit() {
    console.log("CaptureComponent.ngOnInit");
    this.capturesService.apiCapturesGet("body", false).subscribe(
      (data:Capture[]) => {
        this.dataSource = data;
        console.log(`CaptureComponent.apiCapturesGet: Completed, ${ data.length }.`);  
      });
  }

  displayedColumns: string[] = ['id', 'name', 'type', 'size', 'createdOn', 'uploadedOn', 'hash'];
}
 