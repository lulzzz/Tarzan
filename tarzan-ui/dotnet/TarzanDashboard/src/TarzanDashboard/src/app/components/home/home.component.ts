import { Component, OnInit } from '@angular/core';
import { FlowRecord } from 'src/api/index';
import { ITEMS } from 'src/app/mock/mock_data';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})

export class HomeComponent implements OnInit {
  data : FlowRecord;

  onSelect(item: FlowRecord): void {
    this.data = item;
  }

  items : FlowRecord[] = ITEMS;
  displayedColumns: string[] = ['id', 'protocol', 'sourceAddress', 'sourcePort'];

  constructor() { }

  ngOnInit() {
  }

}
