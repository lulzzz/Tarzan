import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { Capture } from '../../api/index';

const TEST_DATA = [{ "id": 1, "name": "testbed-11jun.pcap", "type": "pcap", "size": 17306938543, "createdOn": new Date("2016-01-21T18:57:51"), "uploadOn": new Date("2018-07-11T10:09:47.9398066+02:00"), "hash": "", "author": "Alice Smith", "notes": "", "tags": [] }];

@Component
export default class CapturesComponent extends Vue {
    dataSource: Capture[] = TEST_DATA;
}
