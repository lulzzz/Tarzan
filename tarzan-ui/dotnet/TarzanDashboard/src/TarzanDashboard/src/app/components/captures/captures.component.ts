import { Component, OnInit } from '@angular/core';
import { Capture } from 'src/api';

@Component({
  selector: 'app-captures',
  templateUrl: './captures.component.html',
  styleUrls: ['./captures.component.css']
})
export class CapturesComponent implements OnInit {
  dataSource : Capture[];
  current : Capture = null;
  constructor() { }

  ngOnInit() {
  }

  displayedColumns: string[] = ['position', 'name', 'type', 'size', 'createdOn', 'uploadedOn', 'hash'];
}
 